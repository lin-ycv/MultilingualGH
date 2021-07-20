using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Expressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultilingualGH
{
    public class Menu : GH_AssemblyPriority
    {
#pragma warning disable IDE0044
        static internal string EN = "English";
        static internal ToolStripMenuItem MGHMenu = new ToolStripMenuItem();
        static internal ToolStripMenuItem Version = new ToolStripMenuItem { Name = "Version", Text = UI.Version + MultilingualGHInfo.Ver, ToolTipText = UI.TipEnable, Checked = MGH.Enabled };
        static internal ToolStripMenuItem languageUI = new ToolStripMenuItem { Name = "LanguageUI", Text = UI.LanguageUI };
        static internal ToolStripMenuItem langUIEng = new ToolStripMenuItem { Name = EN, Text = EN, Checked = MGH.LangUI == EN };
        static internal ToolStripMenuItem method = new ToolStripMenuItem { Name = "Methods", Text = UI.Methods };
        static internal ToolStripMenuItem methodBubble = new ToolStripMenuItem { Name = "MBubble", Text = UI.MBubble, Checked = !MGH.TextDisplay };
        static internal ToolStripMenuItem methodText = new ToolStripMenuItem { Name = "MText", Text = UI.MText, Checked = MGH.TextDisplay };
        static internal ToolStripMenuItem textSize = new ToolStripMenuItem { Name = "TextSize", Text = UI.TextSize, Visible = MGH.TextDisplay };
        static internal ToolStripTextBox tSizeInput = new ToolStripTextBox { Text = MGH.Size.ToString() };
        static internal ToolStripMenuItem save = new ToolStripMenuItem { Name = "Save", Text = UI.Save };
        static internal ToolStripMenuItem reload = new ToolStripMenuItem { Name = "Reload", Text = UI.Reload };
        static internal ToolStripMenuItem displayName = new ToolStripMenuItem { Name = "DisplayName", Text = UI.DisplayName };
        static internal ToolStripMenuItem disFullName = new ToolStripMenuItem { Name = "DFull", ToolTipText = ((int)MGH.DisplayType.full).ToString(), Text = UI.DFull, Checked = MGH.DisplayName == MGH.DisplayType.full };
        static internal ToolStripMenuItem disNickname = new ToolStripMenuItem { Name = "DNick", ToolTipText = ((int)MGH.DisplayType.nickname).ToString(), Text = UI.DNick, Checked = MGH.DisplayName == MGH.DisplayType.nickname };
        static internal ToolStripMenuItem disCustom = new ToolStripMenuItem { Name = "DCustom", ToolTipText = ((int)MGH.DisplayType.custom).ToString(), Text = UI.DCustom, Checked = MGH.DisplayName == MGH.DisplayType.custom };
        static internal ToolStripMenuItem disCustFull = new ToolStripMenuItem { Name = "DCustomFull", ToolTipText = ((int)MGH.DisplayType.customFull).ToString(), Text = UI.DCustomFull, Checked = MGH.DisplayName == MGH.DisplayType.customFull };
        static internal ToolStripMenuItem disCustNick = new ToolStripMenuItem { Name = "DCustomNick", ToolTipText = ((int)MGH.DisplayType.customNick).ToString(), Text = UI.DCustomNick, Checked = MGH.DisplayName == MGH.DisplayType.customNick };
        static internal ToolStripMenuItem excludeDe = new ToolStripMenuItem { Name = "ExcludeDefault", Text = UI.ExcludeDefault, Checked = MGH.ExcludeDefault };
        static internal ToolStripMenuItem excludeUs = new ToolStripMenuItem { Name = "ExcludeUser", Text = UI.ExcludeUser, Checked = MGH.ExcludeUser.Length != 0 };
        static internal ToolStripMenuItem showEng = new ToolStripMenuItem { Name = "ShowEng", Text = UI.ShowEng, Checked = MGH.ShowEng, Visible = MGH.LangAnno != EN };
        static internal ToolStripMenuItem language = new ToolStripMenuItem { Name = "EndOfDefaultMenu", Text = MGH.LangAnno };
        static internal ToolStripMenuItem langEng = new ToolStripMenuItem { Name = EN, Text = EN, Checked = MGH.LangAnno == EN };
        static internal GH_Canvas Canvas;
#pragma warning restore IDE0044


        public override GH_LoadingInstruction PriorityLoad()
        {
            Grasshopper.Instances.CanvasCreated += Setup;
            return GH_LoadingInstruction.Proceed;
        }
        static void Setup(GH_Canvas canvas)
        {
            Grasshopper.Instances.CanvasCreated -= Setup;
            canvas.CanvasPrePaintObjects += Translation.Paint;
            GH_DocumentEditor docEditor = Grasshopper.Instances.DocumentEditor;
            if (docEditor != null)
            {
                Translation.GetFiles();
                CreateMenu(docEditor);
            }
            canvas.DocumentChanged += (s, e) =>
            {
                var ghDoc = s.Document;
                if (ghDoc == null) return;

                if (!(ghDoc.ConstantServer.ContainsKey("MGH_Enabled") && MGH.Enabled == canvas.Document.ConstantServer["MGH_Enabled"]._Bool &&
                    MGH.TextDisplay == canvas.Document.ConstantServer["MGH_TextDisplay"]._Bool && MGH.Size == canvas.Document.ConstantServer["MGH_Size"]._Int &&
                    MGH.DisplayName == (MGH.DisplayType)canvas.Document.ConstantServer["MGH_DisplayName"]._Int &&
                    MGH.ExcludeDefault == canvas.Document.ConstantServer["MGH_ExcludeDefault"]._Bool &&
                    MGH.ExcludeUser == canvas.Document.ConstantServer["MGH_ExcludeUser"]._String && MGH.ShowEng == canvas.Document.ConstantServer["MGH_ShowEng"]._Bool &&
                    MGH.LangAnno == canvas.Document.ConstantServer["MGH_LangAnno"]._String))
                {
                    ghDoc.DefineConstant("MGH_Enabled", new GH_Variant(MGH.Enabled));
                    ghDoc.DefineConstant("MGH_TextDisplay", new GH_Variant(MGH.TextDisplay));
                    ghDoc.DefineConstant("MGH_Size", new GH_Variant(MGH.Size));
                    ghDoc.DefineConstant("MGH_DisplayName", new GH_Variant((int)MGH.DisplayName));
                    ghDoc.DefineConstant("MGH_ExcludeDefault", new GH_Variant(MGH.ExcludeDefault));
                    ghDoc.DefineConstant("MGH_ExcludeUser", new GH_Variant(MGH.ExcludeUser));
                    ghDoc.DefineConstant("MGH_ShowEng", new GH_Variant(MGH.ShowEng));
                    ghDoc.DefineConstant("MGH_LangAnno", new GH_Variant(MGH.LangAnno));
                    ghDoc.DefineConstant("MGH_LangAnnoPrev", new GH_Variant());
                    if (!MGH.TextDisplay) Translation.Clear(ghDoc);
                    MGH.EventHandler(canvas);
                }
            };
            Canvas = canvas;
        }
        static void CreateMenu(GH_DocumentEditor docEditor)
        {
            SetupMenu();
            docEditor.MainMenuStrip.SuspendLayout();
            docEditor.MainMenuStrip.Items.Add(MGHMenu);
            docEditor.MainMenuStrip.ShowItemToolTips = true;
            docEditor.MainMenuStrip.ResumeLayout(false);
            docEditor.MainMenuStrip.PerformLayout();
        }
        static void SetupMenu()
        {
            var canvas = Grasshopper.Instances.ActiveCanvas;

            MGHMenu.DropDownItems.AddRange((new List<ToolStripItem> {
                Version,
                languageUI,
                method,
                textSize,
                new ToolStripSeparator(),
                save,
                reload,
                new ToolStripSeparator(),
                displayName,
                excludeDe,
                excludeUs,
                showEng,
                new ToolStripSeparator(),
                language
            }).ToArray());

            Version.Click += (s, e) =>
            {
                MGH.Enabled = !MGH.Enabled;
                ((ToolStripMenuItem)s).Checked = MGH.Enabled;
                if (canvas.Document == null) return;
                canvas.Document.DefineConstant("MGH_Enabled", new GH_Variant(MGH.Enabled));
                if (MGH.Enabled && !MGH.TextDisplay && MGH.LangAnno != canvas.Document.ConstantServer["MGH_LangAnno"]._String)
                {
                    Translation.Clear(Grasshopper.Instances.ActiveCanvas.Document);
                    canvas.Document.DefineConstant("MGH_LangAnno", new GH_Variant(MGH.LangAnno));
                }
                MGH.EventHandler(canvas);
            };

            langUIEng.Click += UI.Update;
            languageUI.DropDownItems.Add(langUIEng);
            UI2Menu();

            method.DropDownItems.AddRange(new ToolStripItem[] {
                methodBubble,
                methodText
            });
            methodBubble.Click += MethodSwitch;
            methodText.Click += MethodSwitch;
            void MethodSwitch(object sender, EventArgs e)
            {
                MGH.TextDisplay = ((ToolStripMenuItem)sender).Name == "MText";
                methodBubble.Checked = !MGH.TextDisplay;
                methodText.Checked = MGH.TextDisplay;
                if (MGH.TextDisplay) textSize.Visible = true;
                else textSize.Visible = false;
                if (canvas.Document == null) return;
                canvas.Document.DefineConstant("MGH_TextDisplay", new GH_Variant(MGH.TextDisplay));
                MGH.EventHandler(canvas);
            }

            textSize.DropDownItems.Add(tSizeInput);
            tSizeInput.TextChanged += (s, e) =>
            {
                if (double.TryParse(((ToolStripTextBox)s).Text, out double input))
                {
                    double size = Math.Round(input, 3);
                    if (canvas.Document == null || size <= 0) return;
                    MGH.Size = size;
                    canvas.Document.DefineConstant("MGH_Size", new GH_Variant(MGH.Size));
                    MGH.EventHandler(canvas);
                }
            };

            save.Click += (s, e) => MGH.SaveSettings();
            reload.Click += Translation.Reload;

            displayName.DropDownItems.AddRange(new ToolStripItem[] {
                disFullName,
                disNickname,
                disCustom,
                disCustFull,
                disCustNick
            });
            disFullName.Click += DisplaySwitch;
            disNickname.Click += DisplaySwitch;
            disCustom.Click += DisplaySwitch;
            disCustFull.Click += DisplaySwitch;
            disCustNick.Click += DisplaySwitch;
            void DisplaySwitch(object sender, EventArgs e)
            {
                MGH.DisplayName = (MGH.DisplayType)int.Parse(((ToolStripMenuItem)sender).ToolTipText);
                foreach (ToolStripMenuItem i in displayName.DropDownItems)
                {
                    if ((MGH.DisplayType)int.Parse(i.ToolTipText) == MGH.DisplayName) i.Checked = true;
                    else i.Checked = false;
                }
                if (canvas.Document == null) return;
                if (!MGH.TextDisplay) Translation.Clear(canvas.Document);
                canvas.Document.DefineConstant("MGH_DisplayName", new GH_Variant((int)MGH.DisplayName));
                MGH.EventHandler(canvas);
            }

            excludeDe.Click += (s, e) =>
            {
                MGH.ExcludeDefault = !MGH.ExcludeDefault;
                excludeDe.Checked = MGH.ExcludeDefault;
                if (canvas.Document == null) return;
                canvas.Document.DefineConstant("MGH_ExcludeDefault", new GH_Variant(MGH.ExcludeDefault));
                MGH.EventHandler(canvas);
            };

            excludeUs.Click += (s, e) =>
            {
                UserForm form = new UserForm();
                form.ShowDialog();
                if (canvas.Document != null && form.DialogResult == DialogResult.OK)
                {
                    canvas.Document.DefineConstant("MGH_ExcludeUser", new GH_Variant(MGH.ExcludeUser));
                    MGH.EventHandler(canvas);
                }
                excludeUs.Checked = MGH.ExcludeUser.Length != 0;
            };

            showEng.Click += (s, e) =>
            {
                MGH.ShowEng = !MGH.ShowEng;
                showEng.Checked = MGH.ShowEng;
                if (canvas.Document != null)
                {
                    canvas.Document.DefineConstant("MGH_ShowEng", new GH_Variant(MGH.ShowEng));
                    MGH.EventHandler(canvas);
                }
            };

            List<ToolStripItem> languageOptions = new List<ToolStripItem> { langEng };
            langEng.Click += Translation.LangAnnoSwitch;
            language.DropDownItems.Add(langEng);
            Lang2Menu();

            if (Translation.extraFiles.Count == 0)
            {
                if (Translation.files.Count == 0 && Translation.uiFiles.Count == 0)
                {
                    ToolStripMenuItem help = new ToolStripMenuItem { Text = "Help..." };
                    help.Click += (s, e) =>
                    {
                        MessageBox.Show($"To add translation languages, copy translations files to\r\n{Translation.folder}\r\nand press Reload", "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        string target = "https://github.com/v-xup6/MultilingualGH/tree/main/Languages";
                        try
                        {
                            Process.Start(target);
                            System.IO.Directory.CreateDirectory(Translation.folder);
                            Process.Start(Translation.folder);
                        }
                        catch { }
                    };
                    MGHMenu.DropDownItems.Add(help);
                }
            }
            else
                Extra2Menu();

            MGHMenu.Name = "MultilingualGH";
            MGHMenu.Text = "MultilingualGH";
        }
        static internal void RemoveInvalid(object sender, ToolStripMenuItem menu)
        {
            MessageBox.Show($"{sender} is missing or is invalid");
            Translation.files.Remove(sender.ToString());
            menu.DropDownItems.RemoveByKey(sender.ToString());
        }
        static internal void Extra2Menu()
        {
            int index = MGHMenu.DropDownItems.IndexOfKey("EndOfDefaultMenu");
            if (index != -1)
                for (int i = MGHMenu.DropDownItems.Count - 1; i > index; i--)
                    MGHMenu.DropDownItems.RemoveAt(i);
            foreach (var file in Translation.extraFiles)
            {
                var plugin = new ToolStripMenuItem { Name = file.Key, Text = file.Key, Checked = MGH.Extras.Contains(file.Key) };
                plugin.Click += Translation.Extras;
                MGHMenu.DropDownItems.Add(plugin);
            }
        }
        static internal void Lang2Menu()
        {
            if (language.DropDownItems.Count > 1)
                for (int i = language.DropDownItems.Count - 1; i > 0; i--)
                    language.DropDownItems.RemoveAt(i);
            foreach (var lang in Translation.files)
            {
                var langOption = new ToolStripMenuItem { Name = lang.Key, Text = lang.Key, Checked = lang.Key == MGH.LangAnno };
                langOption.Click += Translation.LangAnnoSwitch;
                language.DropDownItems.Add(langOption);
            }
        }
        static internal void UI2Menu()
        {
            if (languageUI.DropDownItems.Count > 1)
                for (int i = languageUI.DropDownItems.Count - 1; i > 0; i--)
                    languageUI.DropDownItems.RemoveAt(i);
            foreach (var uiLang in Translation.uiFiles)
            {
                var UIoption = new ToolStripMenuItem { Name = uiLang.Key, Text = uiLang.Key, Checked = MGH.LangUI == uiLang.Key };
                UIoption.Click += UI.Update;
                languageUI.DropDownItems.Add(UIoption);
                if (MGH.LangUI != EN && MGH.LangUI == uiLang.Key)
                    UI.Update(languageUI.DropDownItems[uiLang.Key], null);
            }
        }
    }
}
