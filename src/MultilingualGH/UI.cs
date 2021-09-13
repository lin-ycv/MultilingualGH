using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace MultilingualGH
{
    static internal class UI
    {
        internal static readonly Dictionary<string, string> defaults = new Dictionary<string, string>() {
            { nameof(Version), "Version " } ,
            { nameof(LanguageUI), "UI Language" } ,
            { nameof(Methods), "Display Method" } ,
            { nameof(MBubble), "Bubble Annotation" } ,
            { nameof(MText), "Text Annotation" } ,
            { nameof(TextSize), "Text Size" } ,
            { nameof(ReAnnotate), "Refresh Annotations" } ,
            { nameof(Save), "Save As Default" } ,
            { nameof(Reload), "Reload Files" },
            { nameof(ExcludeDefault), "Default Exclusions" } ,
            { nameof(ExcludeUser), "Custom Exclusions" } ,
            { nameof(ShowEng), "Show English Below" } ,
            { nameof(DisplayName), "Name Display Type" } ,
            { nameof(DFull), "Full Name" } ,
            { nameof(DNick), "Nickname" } ,
            { nameof(DCustom), "Custom Name" } ,
            { nameof(DCustomFull), "Custom w/ Full Name" } ,
            { nameof(DCustomNick), "Custom w/ Nickame" } ,
            { nameof(Missing), "is missing or is invalid" } ,
        };
        static internal Dictionary<string, string> uiTran = new Dictionary<string, string>();

        static public string Version { get; set; } = defaults[nameof(Version)];
        static public string LanguageUI { get; set; } = defaults[nameof(LanguageUI)];
        static public string Methods { get; set; } = defaults[nameof(Methods)];
        static public string MBubble { get; set; } = defaults[nameof(MBubble)];
        static public string MText { get; set; } = defaults[nameof(MText)];
        static public string TextSize { get; set; } = defaults[nameof(TextSize)];
        static public string ReAnnotate { get; set; } = defaults[nameof(ReAnnotate)];
        static public string Save { get; set; } = defaults[nameof(Save)];
        static public string Reload { get; set; } = defaults[nameof(Reload)];
        static public string ExcludeDefault { get; set; } = defaults[nameof(ExcludeDefault)];
        static public string ExcludeUser { get; set; } = defaults[nameof(ExcludeUser)];
        static public string ShowEng { get; set; } = defaults[nameof(ShowEng)];
        static public string DisplayName { get; set; } = defaults[nameof(DisplayName)];
        static public string DFull { get; set; } = defaults[nameof(DFull)];
        static public string DNick { get; set; } = defaults[nameof(DNick)];
        static public string DCustom { get; set; } = defaults[nameof(DCustom)];
        static public string DCustomFull { get; set; } = defaults[nameof(DCustomFull)];
        static public string DCustomNick { get; set; } = defaults[nameof(DCustomNick)];
        static public string Missing { get; set; } = defaults[nameof(Missing)];

        static internal void Update(object sender, EventArgs e)
        {
            bool valid = sender.ToString() != Menu.EN && Translation.ParseFile(Translation.uiFiles[sender.ToString()], ref uiTran);
            PropertyInfo[] properties = typeof(UI).GetProperties();
            foreach (var property in properties)
            {
                if (valid && uiTran.TryGetValue(property.Name, out string value))
                    property.SetValue(null, value);
                else
                    property.SetValue(null, defaults[property.Name]);
                if (Menu.MGHMenu.DropDownItems.ContainsKey(property.Name))
                    Menu.MGHMenu.DropDownItems[property.Name].Text = property.GetValue(null).ToString();
                else if (Menu.displayName.DropDownItems.ContainsKey(property.Name))
                    Menu.displayName.DropDownItems[property.Name].Text = property.GetValue(null).ToString();
                else if (Menu.method.DropDownItems.ContainsKey(property.Name))
                    Menu.method.DropDownItems[property.Name].Text = property.GetValue(null).ToString();
            }
            if (!valid && sender.ToString() != Menu.EN)
            {
                MGH.LangUI = Menu.EN;
                Menu.langUIEng.Checked = true;
                Menu.RemoveInvalid(sender, Menu.languageUI);
            }
            else
            {
                ((ToolStripMenuItem)sender).Checked = true;
                MGH.LangUI = sender.ToString();
                foreach (ToolStripMenuItem option in Menu.languageUI.DropDownItems)
                {
                    if (option.Name != sender.ToString())
                        option.Checked = false;
                }
            }
        }
    }
}

