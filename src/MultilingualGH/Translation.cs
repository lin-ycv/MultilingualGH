using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Expressions;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultilingualGH
{
    public class TranslationInfo
    {
        public string Name { get; set; }
        public string Translation { get; set; }
        public string Category { get; set; }
    }
    class Translation
    {
        // Path to translations, Win: Documents/GHLanguage, Mac: User/GHLanguage
        static internal readonly string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "GHLanguage");
        static internal readonly string[] exclusionDefault = new string[] { "MultilingualGH", "Scribble", "Panel", "Value List", "Button", "Boolean Toggle", "Number Slider" };
        static internal Dictionary<string, string> files = new Dictionary<string, string>();
        static internal Dictionary<string, string> extraFiles = new Dictionary<string, string>();
        static internal Dictionary<string, string> uiFiles = new Dictionary<string, string>();
        static internal Dictionary<string, string> translations = new Dictionary<string, string>();
        static internal Dictionary<string, Dictionary<string, string>> extraTranslations = new Dictionary<string, Dictionary<string, string>>();
        static byte temp;
        static private readonly JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        static private readonly GH_ComponentServer CompServer = Grasshopper.Instances.ComponentServer;

        static internal void GetFiles()
        {
            if (Directory.Exists(folder))
            {
                string[] inFolder = Directory.GetFiles(folder);
                foreach (var file in inFolder)
                {
                    if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        string nameOnly = Path.GetFileNameWithoutExtension(file);
                        if (nameOnly.StartsWith("UILang_"))
                        {
                            var lang = nameOnly.Substring(7, nameOnly.Length - 7);
                            uiFiles.Add(lang, file);
                        }
                        else
                        {
                            files.Add(nameOnly, file);
                            if (MGH.LangAnno != Menu.EN && MGH.LangAnno == nameOnly)
                                ParseFile(file, ref translations);
                        }
                    }
                }
                if (Directory.Exists(Path.Combine(folder, "Extras")))
                {
                    inFolder = Directory.GetFiles(Path.Combine(folder, "Extras"));
                    foreach (var file in inFolder)
                    {
                        if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                        {
                            var fileName = Path.GetFileNameWithoutExtension(file);
                            extraFiles.Add(fileName, file);
                            if (MGH.Extras.Length != 0 && MGH.Extras.Contains(fileName))
                            {
                                var eTran = new Dictionary<string, string>();
                                if (ParseFile(file, ref eTran))
                                    extraTranslations.Add(fileName.Split('_')[0], eTran);
                            }
                        }
                    }
                }
            }
        }
        static internal bool ParseFile(string file, ref Dictionary<string, string> translationDictionary)
        {
            try
            {
                string fileContent = File.ReadAllText(file);
                var translationPair = JsonSerializer.Deserialize<TranslationInfo[]>(fileContent, options);
                foreach (var info in translationPair)
                {
                    if (!translationDictionary.ContainsKey(info.Name + info.Category))
                        translationDictionary.Add(info.Name + info.Category, info.Translation);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        static internal void LangAnnoSwitch(object sender, EventArgs e)
        {
            var sel = sender.ToString();
            Dictionary<string, string> translationDictionary = new Dictionary<string, string>();
            if (sel == Menu.EN || ParseFile(files[sel], ref translationDictionary))
            {
                translations = translationDictionary;
                MGH.LangAnno = sel;
                ((ToolStripMenuItem)sender).Checked = true;
                Menu.language.Text = MGH.LangAnno;
                if (MGH.LangAnno != Menu.EN) Menu.showEng.Visible = true;
                else Menu.showEng.Visible = false;
                foreach (ToolStripMenuItem lang in Menu.language.DropDownItems)
                {
                    if (lang.Name != ((ToolStripMenuItem)sender).Name)
                        lang.Checked = false;
                }
                if (Menu.Canvas.Document == null) return;
                Menu.Canvas.Document.DefineConstant("MGH_LangAnno", new GH_Variant(MGH.LangAnno));
                if (MGH.Enabled && !MGH.TextDisplay && (Menu.Canvas.Document.ConstantServer["MGH_LangAnnoPrev"] != Menu.Canvas.Document.ConstantServer["MGH_LangAnno"]))
                {
                    Clear(Menu.Canvas.Document);
                    Menu.Canvas.Document.DefineConstant("MGH_LangAnnoPrev", new GH_Variant(MGH.LangAnno));
                }
                MGH.EventHandler(Menu.Canvas);
            }
            else
            {
                Menu.RemoveInvalid(sender, Menu.language);
            }
        }
        static internal void Extras(object sender, EventArgs e)
        {
            var plugin = sender.ToString().Split('_')[0];
            if (((ToolStripMenuItem)sender).Checked == false)
            {
                Dictionary<string, string> translationDictionary = new Dictionary<string, string>();
                if (ParseFile(extraFiles[sender.ToString()], ref translationDictionary))
                {
                    if (extraTranslations.ContainsKey(plugin))
                    {
                        extraTranslations.Remove(plugin);
                        Regex r = new Regex("\\*" + plugin + ".*?\\*");
                        Match m = r.Match(MGH.Extras);
                        MGH.Extras = MGH.Extras.Replace(m.Value, "");
                        var found = m.Value.Replace("*", "");
                        ((ToolStripMenuItem)Menu.MGHMenu.DropDownItems[found]).Checked = false;
                    }
                    extraTranslations.Add(plugin, translationDictionary);
                    ((ToolStripMenuItem)sender).Checked = true;
                    if (Menu.Canvas.Document != null) MGH.EventHandler(Menu.Canvas);
                    MGH.Extras += $"*{sender}*";
                }
                else
                {
                    Menu.RemoveInvalid(sender, Menu.MGHMenu);
                }
            }
            else
            {
                extraTranslations.Remove(plugin);
                MGH.Extras = MGH.Extras.Replace($"*{sender}*", "");
                ((ToolStripMenuItem)sender).Checked = false;
                if (Menu.Canvas.Document != null) MGH.EventHandler(Menu.Canvas);
            }
        }
        static internal void Reload(object sender, EventArgs e)
        {
            files.Clear();
            extraFiles.Clear();
            uiFiles.Clear();
            translations.Clear();
            extraTranslations.Clear();
            GetFiles();
            Menu.Extra2Menu();
            Menu.Lang2Menu();
            Menu.UI2Menu();
            MGH.SoundOK();
        }

        static internal void CompAdded(object sender, EventArgs e)
        {
            var ghDoc = sender as GH_Document;
            if (ghDoc == null || ghDoc.ObjectCount == 0) return;

            var exclusions = ExclusionSetup();

            var shouldRemove = new ConcurrentDictionary<IGH_DocumentObject, byte>();
            var newlyAdded = new ConcurrentDictionary<IGH_DocumentObject, byte>();
            var inGroup = new ConcurrentDictionary<IGH_DocumentObject, byte>();
            Parallel.ForEach(e is GH_DocObjectEventArgs newObj ? newObj.Objects : ghDoc.Objects, docObject =>
            {
                if (docObject is GH_Group ghGroup)
                {
                    if (ghGroup.Description == exclusionDefault[0])
                    {
                        var objectsInGroup = ghGroup.Objects();
                        if (IsExclusion(objectsInGroup[0], exclusions))
                        {
                            shouldRemove.TryAdd(docObject, temp);
                        }
                        inGroup.TryAdd(objectsInGroup[0], temp);
                        newlyAdded.TryRemove(objectsInGroup[0], out temp);
                        return;
                    }
                    else return;
                }
                else
                {
                    if (IsExclusion(docObject, exclusions) ||
                    inGroup.ContainsKey(docObject) ||
                    OnlyCustomNames(docObject))
                        return;
                    else
                        newlyAdded.TryAdd(docObject, temp);
                }
            });
            foreach (var comp in newlyAdded.Keys)
            {
                GH_Group annotation = new GH_Group
                {
                    Description = exclusionDefault[0],
                    Colour = Color.FromArgb(0, 255, 255, 255)
                };
                annotation.AddObject(comp.InstanceGuid);
                annotation.NickName = Alias(comp);
                ghDoc.AddObject(annotation, false);
                annotation.ExpireCaches();
            }
            ghDoc.RemoveObjects(shouldRemove.Keys, false);
        }
        static internal void Paint(GH_Canvas sender)
        {
            if (MGH.Enabled && MGH.TextDisplay && GH_Canvas.ZoomFadeLow > 0)
            {
                sender.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                var exclusions = ExclusionSetup();
                bool ZUI = GH_Canvas.ZoomFadeHigh == 255;
                float size = (float)(ZUI ? MGH.Size * 0.5 : MGH.Size);
                Font font = new Font("sans-serif", size);
                SolidBrush brush = new SolidBrush(Color.Black);
                StringFormat alignment = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
                foreach (var comp in sender.Document.Objects)
                {
                    RectangleF bnd = comp.Attributes.Bounds;
                    if (!sender.Viewport.IsVisible(ref bnd, 20) ||
                        IsExclusion(comp, exclusions) ||
                        comp is GH_Group ||
                        OnlyCustomNames(comp)
                       )
                        continue;
                    RectangleF anchor = comp.Attributes.Bounds;
                    float x = anchor.X + 0.5f * anchor.Width;
                    float y = anchor.Y - 0.1f * size;

                    sender.Graphics.DrawString(Alias(comp), font, brush, x, y, alignment);
                }
                font.Dispose();
                brush.Dispose();
                alignment.Dispose();
            }
        }
        static internal bool OnlyCustomNames(IGH_DocumentObject docObj)
        {
            if (MGH.DisplayName == MGH.DisplayType.custom &&
                           (docObj.NickName == CompServer.EmitObjectProxy(docObj.ComponentGuid).Desc.NickName ||
                           docObj.NickName == ""))
                return true;
            else return false;
        }
        static ConcurrentDictionary<string, byte> ExclusionSetup()
        {
            var exclusions = new ConcurrentDictionary<string, byte>();
            exclusions.TryAdd("Sketch", 0);
            string[] userExclusions = MGH.ExcludeUser.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (MGH.ExcludeDefault)
                foreach (var val in exclusionDefault)
                    exclusions.TryAdd(val, temp);
            if (userExclusions.Length != 0)
                foreach (var val in userExclusions)
                    exclusions.TryAdd(val, temp);
            return exclusions;
        }
        static bool IsExclusion(IGH_DocumentObject obj, ConcurrentDictionary<string, byte> exclusions)
        {
            if (exclusions.ContainsKey(obj.Name) || exclusions.ContainsKey(obj.Name + obj.Category))
                return true;
            return false;
        }
        static string Alias(IGH_DocumentObject comp)
        {
            string subfix = MGH.DisplayName == MGH.DisplayType.full ||
                (MGH.DisplayName == MGH.DisplayType.customFull && comp.NickName == CompServer.EmitObjectProxy(comp.ComponentGuid).Desc.NickName)
                ? comp.Name
                : MGH.DisplayName == MGH.DisplayType.nickname
                    ? CompServer.EmitObjectProxy(comp.ComponentGuid).Desc.NickName
                    : comp.NickName;
            if (MGH.LangAnno == Menu.EN) goto End;
            if (translations.TryGetValue(comp.Name + comp.Category, out string translated) || translations.TryGetValue(comp.Name, out translated))
                return translated + (MGH.ShowEng ? Environment.NewLine + subfix : "");
            if (extraTranslations.TryGetValue(comp.Category, out Dictionary<string, string> eIndex))
                if (eIndex.TryGetValue(comp.Name, out string eTranslated))
                    return eTranslated + (MGH.ShowEng ? Environment.NewLine + subfix : "");
                End:
            return subfix;
        }

        static internal void Clear(GH_Document ghDoc)
        {
            var shouldRemove = new ConcurrentDictionary<IGH_DocumentObject, byte>();
            Parallel.ForEach(ghDoc.Objects, docObject =>
            {
                if (docObject is GH_Group ghGroup)
                {
                    if (ghGroup.Description == exclusionDefault[0])
                    {
                        shouldRemove.TryAdd(docObject, temp);
                        return;
                    }
                    else return;
                }
            });
            ghDoc.RemoveObjects(shouldRemove.Keys, false);
        }
    }
}
