using System;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace MultilingualGH
{
    static internal class MGH
    {
        internal enum DisplayType { full, customFull, nickname, custom, customNick };
        static internal GH_SettingsServer Settings = new GH_SettingsServer(nameof(MultilingualGH), true);
        // File > Special Folders > Settings Folder > MultilingualGH.xml

        static internal bool Enabled { get; set; } = Settings.GetValue(nameof(Enabled), true);
        static internal bool TextDisplay { get; set; } = Settings.GetValue(nameof(TextDisplay), true);
        static internal bool ExcludeDefault { get; set; } = Settings.GetValue(nameof(ExcludeDefault), true);
        static internal string ExcludeUser { get; set; } = Settings.GetValue(nameof(ExcludeUser), string.Empty);
        static internal bool ShowEng { get; set; } = Settings.GetValue(nameof(ShowEng), true);
        static internal DisplayType DisplayName { get; set; } = (DisplayType)Settings.GetValue(nameof(DisplayName), (int)DisplayType.full);
        static internal string LangAnno { get; set; } = Settings.GetValue(nameof(LangAnno), "English");
        static internal string Extras { get; set; } = Settings.GetValue(nameof(Extras), string.Empty);
        static internal double Size { get; set; } = Settings.GetValue(nameof(Size), 8.0);
        static internal string LangUI { get; set; } = Settings.GetValue(nameof(LangUI), "English");

        static internal void SaveSettings()
        {
            Settings.SetValue(nameof(Enabled), Enabled);
            Settings.SetValue(nameof(TextDisplay), TextDisplay);
            Settings.SetValue(nameof(ExcludeDefault), ExcludeDefault);
            Settings.SetValue(nameof(ExcludeUser), ExcludeUser);
            Settings.SetValue(nameof(ShowEng), ShowEng);
            Settings.SetValue(nameof(DisplayName), (int)DisplayName);
            Settings.SetValue(nameof(LangUI), LangUI);
            Settings.SetValue(nameof(LangAnno), LangAnno);
            Settings.SetValue(nameof(Extras), Extras);
            Settings.SetValue(nameof(Size), Size);
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
                    System.Media.SoundPlayer alert = new System.Media.SoundPlayer((string)key.GetValue(null));
                    alert.Play();
                }
            }
            catch {/*No sound on MacOS*/}
        }
    }
}
