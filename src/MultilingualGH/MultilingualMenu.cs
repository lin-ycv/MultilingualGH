﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace MultilingualGH
{
    public class MultilingualMenu : GH_AssemblyPriority
    {
        static MultilingualInstance mgh;
        static internal ToolStripMenuItem mghDropdown = new ToolStripMenuItem();
        static private ToolStripItemCollection options = null;

        public override GH_LoadingInstruction PriorityLoad()
        {
            Grasshopper.Instances.CanvasCreated += Setup;
            return GH_LoadingInstruction.Proceed;
        }
        static void Setup(GH_Canvas canvas)
        {
            Grasshopper.Instances.CanvasCreated -= Setup;
            mgh = new MultilingualInstance();
            canvas.CanvasPrePaintObjects += Translation.Paint;
            var docServer = Grasshopper.Instances.DocumentServer;
            docServer.DocumentAdded += (s, e) =>
            {
                mgh = new MultilingualInstance();
                MultilingualInstance.documents.Add(e.DocumentID, mgh);
                mghDropdown.Enabled = true;
                mghDropdown.ToolTipText = "";
            };
            docServer.DocumentRemoved += (s, e) =>
            {
                MultilingualInstance.documents.Remove(e.DocumentID);
                if (docServer.DocumentCount == 0)
                {
                    mghDropdown.Enabled = false;
                    mghDropdown.ToolTipText = "No GH document opened";
                }
            };
            GH_DocumentEditor docEditor = Grasshopper.Instances.DocumentEditor;
            if (docEditor != null)
            {
                Translation.GetFiles();
                CreateMenu(docEditor);
            }
            canvas.DocumentChanged += (s, e) =>
            {
                if (canvas.Document == null) return;
                MultilingualInstance.documents.TryGetValue(canvas.Document.DocumentID, out mgh);
                bool noComp = mgh.compGuid == Guid.Empty;
                mghDropdown.Enabled = noComp;
                if (noComp)
                {
                    UpdateMenu(mgh);
                }

            };
        }
        static void CreateMenu(GH_DocumentEditor docEditor)
        {
            SetupMenu();
            docEditor.MainMenuStrip.SuspendLayout();
            docEditor.MainMenuStrip.Items.AddRange(new ToolStripItem[] { mghDropdown });
            docEditor.MainMenuStrip.Renderer = new CutomToolStripMenuRenderer();
            docEditor.MainMenuStrip.ShowItemToolTips = true;
            docEditor.MainMenuStrip.ResumeLayout(false);
            docEditor.MainMenuStrip.PerformLayout();
        }
        static void SetupMenu()
        {
            var canvas = Grasshopper.Instances.ActiveCanvas;
            ToolStripMenuItem toggle = new ToolStripMenuItem { Name = "Version", Text = "Version " + MultilingualGHInfo.Ver, ToolTipText = "Click to enable/disable", Checked = mgh.enabled };
            toggle.Click += (s, e) =>
            {
                mgh.enabled = !mgh.enabled;
                toggle.Checked = mgh.enabled;
                MultilingualInstance.EventHandler(canvas, mgh);
                if (mgh.enabled && !mgh.textLabel && mgh.language != mgh.prevLang)
                {
                    Translation.Clear(Grasshopper.Instances.ActiveCanvas.Document);
                    MultilingualInstance.EventHandler(canvas, mgh);
                    Grasshopper.Instances.ActiveCanvas.Refresh();
                    mgh.prevLang = mgh.language;
                }
            };
            ToolStripMenuItem keepOption = new ToolStripMenuItem { Name = "Keep", Text = "Keep Annotations", Checked = mgh.keep };
            keepOption.Click += (s, e) =>
            {
                mgh.keep = !mgh.keep;
                keepOption.Checked = mgh.keep;
            };
            ToolStripComboBox translationMethod = new ToolStripComboBox { Name = "Method", DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
            translationMethod.Items.AddRange(new object[] {
            "Bubble Label",
            "Text Label"});
            translationMethod.SelectedIndex = 1;
            translationMethod.SelectedIndexChanged += (s, e) =>
            {
                bool basic = translationMethod.SelectedIndex == 0;
                keepOption.Enabled = basic;
                if (!basic)
                {
                    mgh.keep = false;
                    keepOption.Checked = false;
                }
                mgh.textLabel = !basic;
                if (mgh.compGuid == Guid.Empty) MultilingualInstance.EventHandler(canvas, mgh);
            };
            ToolStripMenuItem defaultOption = new ToolStripMenuItem { Name = "Default", Text = "Use Default Exclusions", Checked = mgh.excludeDefault };
            defaultOption.Click += (s, e) =>
            {
                mgh.excludeDefault = !mgh.excludeDefault;
                MultilingualInstance.EventHandler(canvas, mgh);
                canvas.Refresh();
                defaultOption.Checked = mgh.excludeDefault;
            };
            ToolStripMenuItem userOption = new ToolStripMenuItem { Name = "User", Text = "Custom Exclusions", Checked = mgh.excludeUser != string.Empty };
            userOption.Click += (s, e) =>
            {
                UserForm form = new UserForm();
                form.ShowDialog();
                if (form.DialogResult == DialogResult.OK)
                {
                    MultilingualInstance.EventHandler(canvas, mgh);
                    canvas.Refresh();
                }
                userOption.Checked = mgh.excludeUser != string.Empty;
            };
            ToolStripMenuItem saveSettings = new ToolStripMenuItem { Name = "Save", Text = "Save As Default", ToolTipText = "Save current settings as the default values for new documents" };
            saveSettings.Click += (s, e) =>
            {
                MultilingualInstance.SaveSettings(mgh);
                MessageBox.Show($"Settings saved as default\r\n" +
                    $"Enabled: {mgh.enabled}\r\n" +
                    $"Text Label: {mgh.textLabel}\r\n" +
                    $"Default Exclusions: {mgh.excludeDefault}\r\n" +
                    $"User Exclusions: {mgh.excludeUser.Length > 0}\r\n" +
                    $"Keep Annotations: {mgh.keep}\r\n" +
                    $"Language: {mgh.language}");
            };
            ToolStripMenuItem langOption = new ToolStripMenuItem { Name = "English", Text = "English", Checked = "English" == mgh.language };
            langOption.Click += new EventHandler(LanguageSelection_Click);

            List<ToolStripItem> menuOptions = new List<ToolStripItem> {
                toggle,
                translationMethod,
                new ToolStripSeparator(),
                saveSettings,
                new ToolStripSeparator(),
                defaultOption,
                userOption,
                keepOption,
                new ToolStripSeparator(),
                langOption
            };

            if (Translation.noRoot || Translation.files.Count == 0)
            {
                ToolStripMenuItem help = new ToolStripMenuItem
                {
                    Text = "Help"
                };
                help.Click += (s, e) =>
                {
                    MessageBox.Show($"To add translation languages, copy translations files to\r\n{Translation.folder}\r\nIf the folder does not exisit, create it\r\n\r\n*Restarting Rhino is required to load new files", "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    string target = "https://github.com/v-xup6/MultilingualGH/tree/main/Languages";
                    try
                    {
                        System.Diagnostics.Process.Start(target);
                    }
                    catch { }
                };
                menuOptions.Add(help);
            }
            else
            {
                foreach (var lang in Translation.files)
                {
                    Translation.translations[lang].TryGetValue("*Translator*", out string credit);
                    langOption = new ToolStripMenuItem { Name = lang, Text = lang, ToolTipText = "Translation by " + credit, Checked = lang == mgh.language };
                    langOption.Click += new EventHandler(LanguageSelection_Click);
                    menuOptions.Add(langOption);
                }
            }
            if (Translation.extraFiles.Count > 0)
            {
                menuOptions.Add(new ToolStripSeparator());
                foreach (var fileName in Translation.extraFiles)
                {
                    var plugin = fileName.Split('_')[0];
                    Translation.extraTranslations[plugin].TryGetValue("*Translator*", out string credit);
                    var pluginTranslations = new ToolStripMenuItem { Name = fileName, Text = fileName, ToolTipText = "Translation by " + credit, Checked = mgh.extras.Contains($"*{plugin}*") };
                    pluginTranslations.Click += new EventHandler(PluginSelection_Click);
                    menuOptions.Add(pluginTranslations);
                }
            }
            mghDropdown.DropDownItems.AddRange(menuOptions.ToArray());
            options = mghDropdown.DropDownItems;
            mghDropdown.Enabled = false;
            mghDropdown.ToolTipText = "No GH document opened";
            mghDropdown.Name = "MultilingualGH";
            mghDropdown.Text = "MultilingualGH";
        }
        static void LanguageSelection_Click(object sender, EventArgs e)
        {
            mgh.language = sender.ToString();
            ToolStripMenuItem lang = (ToolStripMenuItem)options["English"];
            lang.Checked = mgh.language == lang.Text;
            foreach (var langs in Translation.files)
            {
                lang = (ToolStripMenuItem)options[langs];
                lang.Checked = mgh.language == lang.Text;
            }
            if (mgh.enabled)
            {
                if (!mgh.textLabel && (mgh.language != sender.ToString() || mgh.prevLang != sender.ToString()))
                {
                    Translation.Clear(Grasshopper.Instances.ActiveCanvas.Document);
                    MultilingualInstance.EventHandler(Grasshopper.Instances.ActiveCanvas, mgh);
                    mgh.prevLang = sender.ToString();
                }
                Grasshopper.Instances.ActiveCanvas.Refresh();
            }
            if (mgh.language == "English")
            {
                foreach (var plugin in Translation.extraFiles)
                {
                    ((ToolStripMenuItem)options[plugin]).Checked = false;
                    ((ToolStripMenuItem)options[plugin]).Enabled = false;
                }
            }
            else
            {
                foreach (var plugin in Translation.extraFiles)
                {
                    ((ToolStripMenuItem)options[plugin]).Checked = mgh.extras.Contains(plugin);
                    ((ToolStripMenuItem)options[plugin]).Enabled = true;
                }
            }
        }
        static void PluginSelection_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem pOption = (ToolStripMenuItem)options[sender.ToString()];
            string plugin = sender.ToString().Split('_')[0];
            if (mgh.extras.Contains($"*{plugin}*"))
            {
                pOption.Checked = false;
                mgh.extras = mgh.extras.Replace($"*{plugin}*", "");
            }
            else
            {
                mgh.extras += $"*{plugin}*";
                pOption.Checked = true;
                if (!mgh.textLabel)
                {
                    Translation.Clear(Grasshopper.Instances.ActiveCanvas.Document);
                    MultilingualInstance.EventHandler(Grasshopper.Instances.ActiveCanvas, mgh);
                }
                Grasshopper.Instances.ActiveCanvas.Refresh();
            }
        }
        static internal void UpdateMenu(MultilingualInstance mgh)
        {
            ((ToolStripMenuItem)options["Version"]).Checked = mgh.enabled;
            ((ToolStripComboBox)options["Method"]).SelectedIndex = mgh.textLabel ? 1 : 0;
            if (mgh.textLabel)
            {
                ((ToolStripMenuItem)options["Keep"]).Checked = false;
                ((ToolStripMenuItem)options["Keep"]).Enabled = false;
            }
            else
            {
                ((ToolStripMenuItem)options["Keep"]).Enabled = true;
                ((ToolStripMenuItem)options["Keep"]).Checked = mgh.keep;
            }
            ((ToolStripMenuItem)options["Default"]).Checked = mgh.excludeDefault;
            ((ToolStripMenuItem)options["User"]).Checked = mgh.excludeUser != string.Empty;
            bool isEng = mgh.language == "English";
            ((ToolStripMenuItem)options["English"]).Checked = isEng;
            foreach (var lang in Translation.files)
            {
                ((ToolStripMenuItem)options[lang]).Checked = mgh.language == lang;
            }
            foreach (var plugin in Translation.extraFiles)
            {
                if (isEng)
                {
                    ((ToolStripMenuItem)options[plugin]).Checked = false;
                    ((ToolStripMenuItem)options[plugin]).Enabled = false;
                }
                else
                {
                    ((ToolStripMenuItem)options[plugin]).Checked = mgh.extras.Contains(plugin);
                    ((ToolStripMenuItem)options[plugin]).Enabled = true;
                }
            }
        }
        class CutomToolStripMenuRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Enabled)
                    base.OnRenderMenuItemBackground(e);
            }
            protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Enabled)
                    base.OnRenderMenuItemBackground(e);
            }
        }
    }
}