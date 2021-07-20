using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace MultilingualGH
{
    static internal class MGH
    {
        internal enum DisplayType { full, customFull, nickname, custom, customNick };
        static internal GH_SettingsServer Settings = new GH_SettingsServer("MultilingualGH", true);

        static internal bool Enabled { get; set; } = Settings.GetValue("Enable", true);
        static internal bool TextDisplay { get; set; } = Settings.GetValue("Method", true);
        static internal bool ExcludeDefault { get; set; } = Settings.GetValue("UseDe", true);
        static internal string ExcludeUser { get; set; } = Settings.GetValue("UseUe", string.Empty);
        static internal bool ShowEng { get; set; } = Settings.GetValue("ShowEng", true);
        static internal DisplayType DisplayName { get; set; } = (DisplayType)Settings.GetValue("Name", (int)DisplayType.full);
        static internal string LangAnno { get; set; } = Settings.GetValue("LangAnno", "English");
        static internal string Extras { get; set; } = Settings.GetValue("Extras", string.Empty);
        static internal double Size { get; set; } = Settings.GetValue("TextSize", 8.0);
        static internal string LangUI { get; set; } = Settings.GetValue("LangUI", "English");

        // File > Special Folders > Settings Folder > MultilingualGH.xml
        static internal void SaveSettings()
        {
            Settings.SetValue("Enable", Enabled);
            Settings.SetValue("Method", TextDisplay);
            Settings.SetValue("UseDe", ExcludeDefault);
            Settings.SetValue("UseUe", ExcludeUser);
            Settings.SetValue("ShowEng", ShowEng);
            Settings.SetValue("Name", (int)DisplayName);
            Settings.SetValue("LangUI", LangUI);
            Settings.SetValue("LangAnno", LangAnno);
            Settings.SetValue("Extras", Extras);
            Settings.SetValue("TextSize", Size);
            Settings.WritePersistentSettings();
            SoundOK();
        }
        static internal void EventHandler(GH_Canvas sender)
        {
            sender.Document.ObjectsAdded -= Translation.CompAdded;
            if (Enabled)
            {
                if (TextDisplay)
                {
                    Translation.Clear(sender.Document);
                }
                else
                {
                    Translation.CompAdded(sender.Document, null);
                    sender.Document.ObjectsAdded += Translation.CompAdded;
                }
            }
            else
            {
                if (!TextDisplay)
                {
                    Translation.Clear(sender.Document);
                }
            }
            sender.Refresh();
        }
        static internal void SoundOK()
        {
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"AppEvents\Schemes\Apps\.Default\Notification.Proximity\.Default"))
                {
                    System.Media.SoundPlayer alert = new System.Media.SoundPlayer((String)key.GetValue(null));
                    alert.Play();
                }
            }
            catch { System.Windows.Forms.MessageBox.Show("OK!"); }
        }
    }
}
