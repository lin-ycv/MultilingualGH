using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Expressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MultilingualGH
{
    public class Menu : GH_AssemblyPriority
    {
#pragma warning disable IDE0044
        static internal string EN = "English";
        static internal ToolStripMenuItem MGHMenu = new ToolStripMenuItem();
        static internal ToolStripMenuItem Version = new ToolStripMenuItem { Name = nameof(UI.Version), Text = UI.Version + MultilingualGHInfo.Ver, ToolTipText = UI.TipEnable, Checked = MGH.Enabled };
        static internal ToolStripMenuItem languageUI = new ToolStripMenuItem { Name = nameof(UI.LanguageUI), Text = UI.LanguageUI };
        static internal ToolStripMenuItem langUIEng = new ToolStripMenuItem { Name = EN, Text = EN, Checked = MGH.LangUI == EN };
        static internal ToolStripMenuItem method = new ToolStripMenuItem { Name = nameof(UI.Methods), Text = UI.Methods };
        static internal ToolStripMenuItem methodBubble = new ToolStripMenuItem { Name = nameof(UI.MBubble), Text = UI.MBubble, Checked = !MGH.TextDisplay };
        static internal ToolStripMenuItem methodText = new ToolStripMenuItem { Name = nameof(UI.MText), Text = UI.MText, Checked = MGH.TextDisplay };
        static internal ToolStripMenuItem textSize = new ToolStripMenuItem { Name = nameof(UI.TextSize), Text = UI.TextSize, Visible = MGH.TextDisplay };
        static internal ToolStripTextBox tSizeInput = new ToolStripTextBox { Text = MGH.Size.ToString() };
        static internal ToolStripMenuItem reAnnotate = new ToolStripMenuItem { Name = nameof(UI.ReAnnotate), Text = UI.ReAnnotate, Visible = !MGH.TextDisplay };
        static internal ToolStripMenuItem save = new ToolStripMenuItem { Name = nameof(UI.Save), Text = UI.Save };
        static internal ToolStripMenuItem reload = new ToolStripMenuItem { Name = nameof(UI.Reload), Text = UI.Reload };
        static internal ToolStripMenuItem displayName = new ToolStripMenuItem { Name = nameof(UI.DisplayName), Text = UI.DisplayName };
        static internal ToolStripMenuItem disFullName = new ToolStripMenuItem { Name = nameof(UI.DFull), ToolTipText = ((int)MGH.DisplayType.full).ToString(), Text = UI.DFull, Checked = MGH.DisplayName == MGH.DisplayType.full };
        static internal ToolStripMenuItem disNickname = new ToolStripMenuItem { Name = nameof(UI.DNick), ToolTipText = ((int)MGH.DisplayType.nickname).ToString(), Text = UI.DNick, Checked = MGH.DisplayName == MGH.DisplayType.nickname };
        static internal ToolStripMenuItem disCustom = new ToolStripMenuItem { Name = nameof(UI.DCustom), ToolTipText = ((int)MGH.DisplayType.custom).ToString(), Text = UI.DCustom, Checked = MGH.DisplayName == MGH.DisplayType.custom };
        static internal ToolStripMenuItem disCustFull = new ToolStripMenuItem { Name = nameof(UI.DCustomFull), ToolTipText = ((int)MGH.DisplayType.customFull).ToString(), Text = UI.DCustomFull, Checked = MGH.DisplayName == MGH.DisplayType.customFull };
        static internal ToolStripMenuItem disCustNick = new ToolStripMenuItem { Name = nameof(UI.DCustomNick), ToolTipText = ((int)MGH.DisplayType.customNick).ToString(), Text = UI.DCustomNick, Checked = MGH.DisplayName == MGH.DisplayType.customNick };
        static internal ToolStripMenuItem excludeDe = new ToolStripMenuItem { Name = nameof(UI.ExcludeDefault), Text = UI.ExcludeDefault, Checked = MGH.ExcludeDefault };
        static internal ToolStripMenuItem excludeUs = new ToolStripMenuItem { Name = nameof(UI.ExcludeUser), Text = UI.ExcludeUser, Checked = MGH.ExcludeUser.Length != 0 };
        static internal ToolStripMenuItem showEng = new ToolStripMenuItem { Name = nameof(UI.ShowEng), Text = UI.ShowEng, Checked = MGH.ShowEng, Visible = MGH.LangAnno != EN };
        static internal ToolStripMenuItem language = new ToolStripMenuItem { Name = "EndofDefaultMenu", Text = MGH.LangAnno };
        static internal ToolStripMenuItem langEng = new ToolStripMenuItem { Name = EN, Text = EN, Checked = MGH.LangAnno == EN };
#pragma warning restore IDE0044


        public override GH_LoadingInstruction PriorityLoad()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && typeof(Rhino.Geometry.Curve).Assembly.GetName().Version.Major == 6)
            {//issue with RH6 on MacOS
                MessageBox.Show($"You do not meet the minimum requirements for v{MultilingualGHInfo.Ver}\r\n\r\nPLEASE USE:\r\n v1.3.5", "MultilingualGH", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return GH_LoadingInstruction.Proceed;
            }
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

                if (!(ghDoc.ConstantServer.ContainsKey("MGH_" + nameof(MGH.Enabled)) && MGH.Enabled == canvas.Document.ConstantServer["MGH_" + nameof(MGH.Enabled)]._Bool &&
                    MGH.TextDisplay == canvas.Document.ConstantServer["MGH_" + nameof(MGH.TextDisplay)]._Bool && MGH.Size == canvas.Document.ConstantServer["MGH_" + nameof(MGH.Size)]._Int &&
                    MGH.DisplayName == (MGH.DisplayType)canvas.Document.ConstantServer["MGH_" + nameof(MGH.DisplayName)]._Int &&
                    MGH.ExcludeDefault == canvas.Document.ConstantServer["MGH_" + nameof(MGH.ExcludeDefault)]._Bool &&
                    MGH.ExcludeUser == canvas.Document.ConstantServer["MGH_" + nameof(MGH.ExcludeUser)]._String && MGH.ShowEng == canvas.Document.ConstantServer["MGH_" + nameof(MGH.ShowEng)]._Bool &&
                    MGH.LangAnno == canvas.Document.ConstantServer["MGH_" + nameof(MGH.LangAnno)]._String))
                {
                    ghDoc.DefineConstant("MGH_" + nameof(MGH.Enabled), new GH_Variant(MGH.Enabled));
                    ghDoc.DefineConstant("MGH_" + nameof(MGH.TextDisplay), new GH_Variant(MGH.TextDisplay));
                    ghDoc.DefineConstant("MGH_" + nameof(MGH.Size), new GH_Variant(MGH.Size));
                    ghDoc.DefineConstant("MGH_" + nameof(MGH.DisplayName), new GH_Variant((int)MGH.DisplayName));
                    ghDoc.DefineConstant("MGH_" + nameof(MGH.ExcludeDefault), new GH_Variant(MGH.ExcludeDefault));
                    ghDoc.DefineConstant("MGH_" + nameof(MGH.ExcludeUser), new GH_Variant(MGH.ExcludeUser));
                    ghDoc.DefineConstant("MGH_" + nameof(MGH.ShowEng), new GH_Variant(MGH.ShowEng));
                    ghDoc.DefineConstant("MGH_" + nameof(MGH.LangAnno), new GH_Variant(MGH.LangAnno));
                    if (!MGH.TextDisplay) Translation.Clear(ghDoc);
                    MGH.EventHandler(canvas);
                }
            };
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
            var Canvas = Grasshopper.Instances.ActiveCanvas;

            MGHMenu.DropDownItems.AddRange((new List<ToolStripItem> {
                Version,
                languageUI,
                method,
                textSize,
                reAnnotate,
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
                if (Canvas.Document == null) return;
                Canvas.Document.DefineConstant("MGH_" + nameof(MGH.Enabled), new GH_Variant(MGH.Enabled));
                if (MGH.Enabled && !MGH.TextDisplay && MGH.LangAnno != Canvas.Document.ConstantServer["MGH_" + nameof(MGH.LangAnno)]._String)
                {
                    Translation.Clear(Grasshopper.Instances.ActiveCanvas.Document);
                    Canvas.Document.DefineConstant("MGH_" + nameof(MGH.LangAnno), new GH_Variant(MGH.LangAnno));
                }
                MGH.EventHandler(Canvas);
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
                MGH.TextDisplay = ((ToolStripMenuItem)sender).Name == nameof(UI.MText);
                methodBubble.Checked = !MGH.TextDisplay;
                methodText.Checked = MGH.TextDisplay;
                if (MGH.TextDisplay)
                {
                    textSize.Visible = true;
                    reAnnotate.Visible = false;
                }
                else
                {
                    textSize.Visible = false;
                    reAnnotate.Visible = true;
                }
                if (Canvas.Document == null) return;
                Canvas.Document.DefineConstant("MGH_" + nameof(MGH.TextDisplay), new GH_Variant(MGH.TextDisplay));
                MGH.EventHandler(Canvas);
            }

            textSize.DropDownItems.Add(tSizeInput);
            textSize.MouseHover += (s, e) => tSizeInput.Text = MGH.Size.ToString();
            tSizeInput.TextChanged += (s, e) =>
            {
                if (double.TryParse(((ToolStripTextBox)s).Text, out double input))
                {
                    double size = Math.Round(input, 3);
                    if (Canvas.Document == null || size <= 0) return;
                    MGH.Size = size;
                    Canvas.Document.DefineConstant("MGH_" + nameof(MGH.Size), new GH_Variant(MGH.Size));
                    MGH.EventHandler(Canvas);
                }
            };

            reAnnotate.Click += (s, e) =>
            {
                Translation.Clear(Canvas.Document);
                MGH.EventHandler(Canvas);
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
                if (Canvas.Document == null) return;
                if (!MGH.TextDisplay) Translation.Clear(Canvas.Document);
                Canvas.Document.DefineConstant("MGH_" + nameof(MGH.DisplayName), new GH_Variant((int)MGH.DisplayName));
                MGH.EventHandler(Canvas);
            }

            excludeDe.Click += (s, e) =>
            {
                MGH.ExcludeDefault = !MGH.ExcludeDefault;
                excludeDe.Checked = MGH.ExcludeDefault;
                if (Canvas.Document == null) return;
                Canvas.Document.DefineConstant("MGH_" + nameof(MGH.ExcludeDefault), new GH_Variant(MGH.ExcludeDefault));
                MGH.EventHandler(Canvas);
            };

            excludeUs.Click += (s, e) =>
            {
                UserForm form = new UserForm();
                form.ShowDialog();
                if (Canvas.Document != null && form.DialogResult == DialogResult.OK)
                {
                    Canvas.Document.DefineConstant("MGH_" + nameof(MGH.ExcludeUser), new GH_Variant(MGH.ExcludeUser));
                    MGH.EventHandler(Canvas);
                    excludeUs.Checked = MGH.ExcludeUser.Length != 0;
                }
            };

            showEng.Click += (s, e) =>
            {
                MGH.ShowEng = !MGH.ShowEng;
                showEng.Checked = MGH.ShowEng;
                if (Canvas.Document != null)
                {
                    Canvas.Document.DefineConstant("MGH_" + nameof(MGH.ShowEng), new GH_Variant(MGH.ShowEng));
                    if (!MGH.TextDisplay) Translation.Clear(Canvas.Document);
                    MGH.EventHandler(Canvas);
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
                        string target = "https://github.com/lin-ycv/MultilingualGH/wiki";
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
            MessageBox.Show($"{sender} {UI.Missing}");
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
