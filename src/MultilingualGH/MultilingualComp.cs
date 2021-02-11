using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Special;

namespace MultilingualGH
{
    public class MultilingualComp : GH_Component
    {
        MultilingualInstance mgh;

        public MultilingualComp()
          : base("MultilingualGH", "MGH",
              "Annotates components with desired language.",
              "Params", "Util")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Exclude", "X", "List of components to exclude from annotations", GH_ParamAccess.list);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter(" ", " ", "Info", GH_ParamAccess.list);
        }

        public override void AddedToDocument(GH_Document document)
        {
            if (document != null)
            {
                MultilingualInstance.documents.TryGetValue(document.DocumentID, out mgh);
                if (mgh != null)
                {
                    if (mgh.compGuid != Guid.Empty && mgh.compGuid != InstanceGuid)
                    {
                        MessageBox.Show("There is already an instance of MultilingualGH on the canvas. No need for another one.", "MultilingualGH", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        document.RemoveObject(this, false);
                    }
                    else
                    {
                        mgh.compGuid = InstanceGuid;
                        MultilingualMenu.mghDropdown.Enabled = false;
                        MultilingualMenu.mghDropdown.ToolTipText = "Menu disabled when MGH component is in use";
                        document.ObjectsDeleted += RemovedMe;
                        IGH_Param inputParam = base.Params.Input[0];
                        if (mgh.excludeUser != string.Empty && inputParam.SourceCount == 0)
                        {
                            var exPanel = new GH_Panel();
                            exPanel.CreateAttributes();
                            exPanel.UserText = mgh.excludeUser;
                            exPanel.Properties.Multiline = false;
                            exPanel.Attributes.Bounds = new RectangleF(
                                new Point(0, 0),
                                new SizeF((float)this.Attributes.Bounds.Width, (float)this.Attributes.Bounds.Height));
                            exPanel.Attributes.Pivot = new PointF(
                                this.Attributes.Pivot.X - exPanel.Attributes.Bounds.Width - this.Attributes.Bounds.Width,
                                this.Attributes.Pivot.Y - exPanel.Attributes.Bounds.Height / 2);
                            document.AddObject(exPanel, false);
                            inputParam.AddSource(exPanel);
                            inputParam.CollectData();
                        }
                    }
                }
                base.AddedToDocument(document);
            }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            var canvas = Grasshopper.Instances.ActiveCanvas;
            var items = MultilingualMenu.mghDropdown.DropDownItems;
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendItem(menu, "Use Text Label", (s, e) =>
            {
                mgh.textLabel = !mgh.textLabel;
                ((ToolStripComboBox)items["Method"]).SelectedIndex = mgh.textLabel ? 1 : 0;
                MultilingualInstance.EventHandler(canvas, mgh);
            }, true, mgh.textLabel);
            Menu_AppendItem(menu, "Use Default Exclusions", (s, e) =>
            {
                mgh.excludeDefault = !mgh.excludeDefault;
                ((ToolStripMenuItem)items["Default"]).Checked = mgh.excludeDefault;
                MultilingualInstance.EventHandler(canvas, mgh);
            }, true, mgh.excludeDefault);
            Menu_AppendItem(menu, "Keep annotations", (s, e) =>
            {
                mgh.keep = !mgh.keep;
                ((ToolStripMenuItem)items["Keep"]).Checked = mgh.keep;
            }, !mgh.textLabel, mgh.keep);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "English", (s, e) => LangSelection("English"), true, mgh.language == "English");
            foreach (var lang in Translation.files)
            {
                Menu_AppendItem(menu, lang, (s, e) => LangSelection(lang), true, mgh.language == lang);
            }
            if(Translation.extraFiles.Count>0)
                Menu_AppendSeparator(menu);
            foreach (var plugin in Translation.extraFiles)
            {
                string shorten = $"*{plugin.Split('_')[0]}*";
                Menu_AppendItem(menu, plugin, (s, e) => PluginSelection(shorten), mgh.language!="English", mgh.language != "English" && mgh.extras.Contains(shorten));
            }
        }
        internal void LangSelection(string lang)
        {
            mgh.language = lang;
            if (mgh.enabled)
            {
                Message = lang;
                if (mgh.prevLang != lang)
                {
                    Translation.Clear(Grasshopper.Instances.ActiveCanvas.Document);
                    mgh.prevLang = lang;
                    ExpireSolution(true);
                }
                MultilingualInstance.EventHandler(Grasshopper.Instances.ActiveCanvas, mgh);                
            }
        }
        internal void PluginSelection(string plugin)
        {
            bool clean = false;
            if (mgh.extras.Contains($"*{plugin}*"))
                mgh.extras = mgh.extras.Replace($"*{plugin}*", "");
            else
            {
                mgh.extras += $"*{plugin}*";
                clean = true;
            }
            if (mgh.enabled)
            {
                if(clean)
                    Translation.Clear(Grasshopper.Instances.ActiveCanvas.Document);
                ExpireSolution(true);
                MultilingualInstance.EventHandler(Grasshopper.Instances.ActiveCanvas, mgh);
            }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Version " + MultilingualGHInfo.Ver);
            var canvas = Grasshopper.Instances.ActiveCanvas;
            if (mgh == null) AddedToDocument(canvas.Document);
            if (Translation.noRoot) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $" Folder '{Translation.folder}' not found");
            var userExclusion = new List<string>();
            DA.GetDataList(0, userExclusion);
            if (userExclusion.Count > 0)
            {
                mgh.excludeUser = string.Join(Environment.NewLine, userExclusion);
                ((ToolStripMenuItem)MultilingualMenu.mghDropdown.DropDownItems["User"]).Checked = true;
            }
            else
            {
                mgh.excludeUser = string.Empty;
                ((ToolStripMenuItem)MultilingualMenu.mghDropdown.DropDownItems["User"]).Checked = false;
            }
            Message = mgh.enabled ? mgh.language : "Disabled";

            if (mgh.language == "English")
                DA.SetData(0, "https://github.com/v-xup6/MultilingualGH");
            else
            {
                Translation.translations[mgh.language].TryGetValue("*Translator*", out string credit);
                StringBuilder eCredits = new StringBuilder();
                foreach(string plugin in mgh.extras.Split(new string[] { "**", "*" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Translation.extraTranslations[plugin].TryGetValue("*Translator*", out string cred);
                    eCredits.Append(", " + cred);
                }
                DA.SetData(0, "Translation(s) by " + credit+eCredits.ToString());
            }
            MultilingualInstance.EventHandler(canvas, mgh);
        }
        internal void RemovedMe(object sender, GH_DocObjectEventArgs e) //sender = document, e = List of component that invoked the call
        {
            GH_Document ghDoc = (GH_Document)sender;
            if (ghDoc == null) return;
            if (e.Objects.Contains(this))
            {
                ghDoc.ObjectsDeleted -= RemovedMe;
                ghDoc.ObjectsAdded -= Translation.CompAdded;
                if (mgh.enabled) mgh.enabled = false;
                mgh.compGuid = Guid.Empty;
                MultilingualMenu.mghDropdown.ToolTipText = string.Empty;
                MultilingualMenu.mghDropdown.Enabled = true;
                MultilingualMenu.UpdateMenu(mgh);
            }
        }

        public override void CreateAttributes()
        {
            m_attributes = new MultilingualAttributes(this);
        }
        private class MultilingualAttributes : GH_ComponentAttributes
        {
            public MultilingualAttributes(IGH_Component component) : base(component) { }

            public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                MultilingualInstance.documents.TryGetValue(sender.Document.DocumentID, out MultilingualInstance mgh);
                mgh.enabled = !mgh.enabled;
                base.Owner.Message = mgh.enabled ? mgh.language : "Disabled";
                if (mgh.enabled)
                {
                    if (!mgh.textLabel)
                    {
                        if (mgh.language != mgh.prevLang)
                        {
                            Translation.Clear(sender.Document);
                            mgh.prevLang = mgh.language;
                        }
                    }
                }
                base.Owner.ExpireSolution(true);
                return GH_ObjectResponse.Handled;
            }
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.MultilingualGH;
            }
        }
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetString("MGHLangSel", mgh.language);
            writer.SetBoolean("MGHenable", mgh.enabled);
            writer.SetBoolean("MGHKeepAnno", mgh.keep);
            writer.SetBoolean("MGHUseDe", mgh.excludeDefault);
            writer.SetBoolean("MGHLabelMethod", mgh.textLabel);
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            var ghDoc = Grasshopper.Instances.ActiveCanvas.Document;
            if (ghDoc != null)
            {
                MultilingualInstance.documents.TryGetValue(ghDoc.DocumentID, out mgh);
                reader.TryGetString("MGHLangSel", ref mgh.language);
                reader.TryGetBoolean("MGHenable", ref mgh.enabled);
                reader.TryGetBoolean("MGHKeepAnno", ref mgh.keep);
                reader.TryGetBoolean("MGHUseDe", ref mgh.excludeDefault);
                reader.TryGetBoolean("MGHLabelMethod", ref mgh.textLabel);
            }
            return base.Read(reader);
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("af2608bd-eb79-4f25-9fde-1610d9eb8451"); }
        }
    }

}
