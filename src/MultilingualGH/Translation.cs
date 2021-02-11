﻿using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace MultilingualGH
{
    public class TranslationInfo
    {
        public string name { get; set; }
        public string translation { get; set; }
        public string category { get; set; }
    }
    class Translation
    {
        // Path to translations, Win: Documents/GHLanguage, Mac: User/GHLanguage
        static internal readonly string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "GHLanguage");
        static internal bool noRoot = false;
        static internal readonly string[] exclusionDefault = new string[] { "MultilingualGH", "Scribble", "Panel", "Value List", "Button", "Boolean Toggle", "Number Slider" };
        static internal readonly List<string> files = new List<string>();
        static internal readonly List<string> extraFiles = new List<string>();
        static internal readonly Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>();
        static internal Dictionary<string, Dictionary<string, string>> extraTranslations = new Dictionary<string, Dictionary<string, string>>();
        static byte temp;

        static internal void GetFiles()
        {
            if (Directory.Exists(folder))
            {
                string[] inFolder = Directory.GetFiles(folder);
                foreach (var file in inFolder)
                {
                    var translationDictionary = new Dictionary<string, string>();
                    string nameOnly = Path.GetFileNameWithoutExtension(file);
                    files.Add(nameOnly);
                    ParseFile(file, ref translationDictionary);
                    translations.Add(nameOnly, translationDictionary);
                }
            }
            else
                noRoot = true;
            if (Directory.Exists(Path.Combine(folder, "Extras")))
            {
                string[] inFolder = Directory.GetFiles(Path.Combine(folder, "Extras"));
                foreach (var file in inFolder)
                {
                    var translationDictionary = new Dictionary<string, string>();
                    string nameOnly = Path.GetFileNameWithoutExtension(file);
                    extraFiles.Add(nameOnly);
                    ParseFile(file, ref translationDictionary);
                    extraTranslations.Add(nameOnly.Split('_')[0], translationDictionary);
                }
            }
        }
        static private void ParseFile(string file, ref Dictionary<string, string> translationDictionary)
        {
            string fileContent = File.ReadAllText(file);
            if (fileContent[0] == '[')
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                try
                {
                    var translationPair = serializer.Deserialize<TranslationInfo[]>(fileContent);
                    foreach (var info in translationPair)
                    {
                        if (!translationDictionary.ContainsKey(info.name + info.category))
                            translationDictionary.Add(info.name + info.category, info.translation);
                    }
                }
                catch
                {
#if (DEBUG)
                    MessageBox.Show($"{file} is not a valid JSON");
#endif
                }
            }
            else
            {
                string[] translationPairs = fileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string info in translationPairs)
                {
                    string[] pair = info.Split('=');
                    if (!translationDictionary.ContainsKey(pair[0]) && pair.Length == 2 && pair[1] != "")
                        translationDictionary.Add(pair[0], pair[1]);
                }
            }
        }
        static internal void CompAdded(object sender, object e)
        {
            var ghDoc = sender as GH_Document;
            if (ghDoc == null || ghDoc.ObjectCount == 0) return;

            MultilingualInstance mgh = MultilingualInstance.documents[ghDoc.DocumentID];

            var exclusions = ExclusionSetup(mgh);

            var shouldRemove = new ConcurrentDictionary<IGH_DocumentObject, byte>();
            var newlyAdded = new ConcurrentDictionary<IGH_DocumentObject, byte>();
            var inGroup = new ConcurrentDictionary<IGH_DocumentObject, byte>();
            Parallel.ForEach(ghDoc.Objects, docObject =>
            {
                if (docObject is GH_Group ghGroup)
                {
                    var objectsInGroup = ghGroup.Objects();
                    if (objectsInGroup.Count == 1 && ghGroup.Description == objectsInGroup[0].Name)
                    {
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
                    if (IsExclusion(docObject, exclusions) || inGroup.ContainsKey(docObject))
                        return;
                    else
                        newlyAdded.TryAdd(docObject, temp);
                }
            });
            foreach (var comp in newlyAdded.Keys)
            {
                GH_Group annotation = new GH_Group
                {
                    Description = comp.Name,
                    Colour = Color.FromArgb(0, 255, 255, 255)
                };
                annotation.AddObject(comp.InstanceGuid);
                annotation.NickName = Alias(mgh, comp);
                ghDoc.AddObject(annotation, false);
                annotation.ExpireCaches();
            }
            ghDoc.RemoveObjects(shouldRemove.Keys, false);
        }
        static private Guid galapagosID = new Guid("E6DD2904-14BC-455b-8376-948BF2E3A7BC");
        static internal void Paint(GH_Canvas sender)
        {
            MultilingualInstance.documents.TryGetValue(sender.Document.DocumentID, out MultilingualInstance mgh);
            if (mgh == null) return;
            sender.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            if (mgh.enabled && mgh.textLabel && GH_Canvas.ZoomFadeLow > 0)
            {
                var exclusions = ExclusionSetup(mgh);
                bool ZUI = GH_Canvas.ZoomFadeHigh == 255;
                float size = ZUI ? 6 : 8;
                foreach (var comp in sender.Document.Objects)
                {
                    if (IsExclusion(comp, exclusions) || comp is GH_Group) continue;
                    RectangleF anchor = comp.Attributes.Bounds;
                    float x = anchor.X + 0.5f * anchor.Width;
                    float y;
                    if (comp.ComponentGuid == galapagosID || ((IGH_ActiveObject)comp).RuntimeMessageLevel == GH_RuntimeMessageLevel.Blank || ZUI)
                        y = anchor.Y - 0.25f * size;
                    else
                        y = anchor.Y - 1.25f * size;

                    Font font = new Font("Arial", size);
                    SolidBrush brush = new SolidBrush(Color.Black);
                    StringFormat alignment = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
                    sender.Graphics.DrawString(Alias(mgh, comp), font, brush, x, y, alignment);
                    font.Dispose();
                    brush.Dispose();
                    alignment.Dispose();
                }
            }
        }
        static ConcurrentDictionary<string, byte> ExclusionSetup(MultilingualInstance mgh)
        {
            var exclusions = new ConcurrentDictionary<string, byte>();
            exclusions.TryAdd("Sketch", 0);
            string[] userExclusions = mgh.excludeUser.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (mgh.excludeDefault)
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
        static string Alias(MultilingualInstance mgh, IGH_DocumentObject comp)
        {
            if (mgh.language == "English") return comp.Name;
            if (translations.TryGetValue(mgh.language, out Dictionary<string, string> index))
                if (index.TryGetValue(comp.Name + comp.Category, out string translated) || index.TryGetValue(comp.Name, out translated))
                    return translated;
            if (mgh.extras.Contains(comp.Category) && extraTranslations.TryGetValue(comp.Category, out Dictionary<string, string> eIndex))
                if (eIndex.TryGetValue(comp.Name, out string eTranslated))
                    return eTranslated;
            return comp.Name;
        }

        static internal void Clear(GH_Document ghDoc)
        {
            var shouldRemove = new ConcurrentDictionary<IGH_DocumentObject, byte>();
            Parallel.ForEach(ghDoc.Objects, docObject =>
            {
                if (docObject is GH_Group ghGroup)
                {
                    var objectsInGroup = ghGroup.Objects();
                    if (objectsInGroup.Count == 1 && ghGroup.Description == objectsInGroup[0].Name)
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