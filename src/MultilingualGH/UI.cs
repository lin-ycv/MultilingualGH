using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultilingualGH
{
    static internal class UI
    {
        static public string Version { get; set; } = "Version ";
        static public string Disabled { get; set; } = "Disabled";
        static public string TranslationBy { get; set; } = "Translation(s) by ";
        
        static public string CompDes { get; set; } = "Annotates components with desired language.";
        static public string CompIn { get; set; } = "Exclude";
        static public string CompInDes { get; set; } = "List of components to exclude from annotations";
        
        static public string UseTextLabel { get; set; } = "Use Text Label";
        static public string UseDefaultExclusions { get; set; } = "Use Default Exclusions";
        static public string CustomExclusions { get; set; } = "Custom Exclusions";
        static public string KeepAnnotations { get; set; } = "Keep Annotations";
        static public string NoNickname { get; set; } = "Show Full Names";
        static public string NicknamePreferred { get; set; } = "Prefer Nicknames";
        static public string NicknameOnly { get; set; } = "Only Custom Nicknames";
        static public string ShowEnglish { get; set; } = "Show English with Translation";
        static public string BubbleLabel { get; set; } = "Bubble Label";
        static public string TextLabel { get; set; } = "Text Label";
        static public string SaveAsDefault { get; set; } = "Save As Default";
        static public string Help { get; set; } = "Help...";

        static public string TooMany { get; set; } = "There is already an instance of MultilingualGH on the canvas. No need for another one.";
        static public string MenuDisabled { get; set; } = "Menu disabled when MGH component is in use";
        static public string NoDoc { get; set; } = "No GH document opened";
        static public string ClickEnable { get; set; } = "Click to enable/disable";
        static public string SaveDeTooltip { get; set; } = "Save current settings as the default values for new documents";


        static internal void Update (Dictionary<string, string> uiTran) 
        {
            System.Reflection.PropertyInfo[] properties = typeof(UI).GetProperties();
            foreach(var property in properties)
            {
                if(uiTran.TryGetValue(property.Name, out string value))
                {
                    property.SetValue(null, value);
                }
            }
        }
    }
}
