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
    }
}
