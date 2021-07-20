using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultilingualGH
{
    static internal class UI
    {
        private static readonly Dictionary<string, string> defaults = new Dictionary<string, string>() {
            { "Version", "Version " } ,
            { "TipEnable", "Click to Enable/Disable" } ,
            { "LanguageUI", "UI Language" } ,
            { "Methods", "Display Method" } ,
            { "MBubble", "Bubble Annotation" } ,
            { "MText", "Text Annotation" } ,
            { "TextSize", "Text Size" } ,
            { "Save", "Save As Default" } ,
            { "Reload", "Reload Files" },
            { "ExcludeDefault", "Default Exclusions" } ,
            { "ExcludeUser", "Custom Exclusions" } ,
            { "ShowEng", "Show English Below" } ,
            { "DisplayName", "Name Display Type" } ,
            { "DFull", "Full Name" } ,
            { "DNick", "Nickname" } ,
            { "DCustom", "Custom names only" } ,
            { "DCustomFull", "Custom w/ Full Name" } ,
            { "DCustomNick", "Custom w/ Nickame" } ,
        };
        static internal Dictionary<string, string> uiTran = new Dictionary<string, string>();

        static public string Version { get; set; } = defaults["Version"];
        static public string TipEnable { get; set; } = defaults["TipEnable"];
        static public string LanguageUI { get; set; } = defaults["LanguageUI"];
        static public string Methods { get; set; } = defaults["Methods"];
        static public string MBubble { get; set; } = defaults["MBubble"];
        static public string MText { get; set; } = defaults["MText"];
        static public string TextSize { get; set; } = defaults["TextSize"];
        static public string Save { get; set; } = defaults["Save"];
        static public string Reload { get; set; } = defaults["Reload"];
        static public string ExcludeDefault { get; set; } = defaults["ExcludeDefault"];
        static public string ExcludeUser { get; set; } = defaults["ExcludeUser"];
        static public string ShowEng { get; set; } = defaults["ShowEng"];
        static public string DisplayName { get; set; } = defaults["DisplayName"];
        static public string DFull { get; set; } = defaults["DFull"];
        static public string DNick { get; set; } = defaults["DNick"];
        static public string DCustom { get; set; } = defaults["DCustom"];
        static public string DCustomFull { get; set; } = defaults["DCustomFull"];
        static public string DCustomNick { get; set; } = defaults["DCustomNick"];

        static internal void Update(object sender, System.EventArgs e)
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
                Menu.MGHMenu.DropDownItems["Version"].ToolTipText = TipEnable;
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

