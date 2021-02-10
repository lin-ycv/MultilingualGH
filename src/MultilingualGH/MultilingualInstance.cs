using Grasshopper.GUI.Canvas;
using System;
using System.Collections.Generic;

namespace MultilingualGH
{
    class MultilingualInstance
    {
        static internal Dictionary<Guid, MultilingualInstance> documents = new Dictionary<Guid, MultilingualInstance>();

        internal bool enabled = false;
        internal bool textLabel = false;
        internal bool excludeDefault = true;
        internal string excludeUser = string.Empty;
        internal bool keep = false;
        internal string language = "English";
        internal string prevLang;
        internal Guid compGuid = Guid.Empty;

        internal MultilingualInstance()
        {
            enabled = Grasshopper.Instances.Settings.GetValue("MGHenable", false);
            textLabel = Grasshopper.Instances.Settings.GetValue("MGHLabelMethod", false);
            excludeDefault = Grasshopper.Instances.Settings.GetValue("MGHUseDe", true);
            excludeUser = Grasshopper.Instances.Settings.GetValue("MGHUseUe", string.Empty);
            keep = Grasshopper.Instances.Settings.GetValue("MGHKeepAnno", false);
            language = Grasshopper.Instances.Settings.GetValue("MGHLangSel", "English");
        }
        static internal void EventHandler(GH_Canvas sender, MultilingualInstance mgh)
        {
            sender.Document.ObjectsAdded -= Translation.CompAdded;
            if (mgh.enabled)
            {
                if (mgh.textLabel)
                {
                    Translation.Clear(sender.Document);
                }
                else
                {
                    Translation.CompAdded(sender.Document, 0);
                    sender.Document.ObjectsAdded += Translation.CompAdded;
                }
            }
            else
            {
                if (!mgh.textLabel && !mgh.keep)
                {
                    Translation.Clear(sender.Document);
                }
            }
            sender.Refresh();
        }
        static internal void SaveSettings(MultilingualInstance mgh)
        {
            Grasshopper.Instances.Settings.SetValue("MGHenable", mgh.enabled);
            Grasshopper.Instances.Settings.SetValue("MGHLabelMethod", mgh.textLabel);
            Grasshopper.Instances.Settings.SetValue("MGHUseDe", mgh.excludeDefault);
            Grasshopper.Instances.Settings.SetValue("MGHUseUe", mgh.excludeUser);
            Grasshopper.Instances.Settings.SetValue("MGHKeepAnno", mgh.keep);
            Grasshopper.Instances.Settings.SetValue("MGHLangSel", mgh.language);
        }
    }
}
