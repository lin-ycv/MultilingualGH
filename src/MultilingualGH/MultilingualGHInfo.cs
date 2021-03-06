using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace MultilingualGH
{
    public class MultilingualGHInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "MultilingualGH";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Properties.Resources.MultilingualGH;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Annotates components with desired language";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("b1114eca-d766-4400-b6f5-12a703165898");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Victor (Yu Chieh) Lin";
            }
        }
        public override string AuthorContact
        {
            get
            {
                return "https://github.com/v-xup6/MultilingualGH";
            }
        }

        static readonly Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        static internal readonly string Ver = v.Major + "." + v.Minor + "." + v.Build;
        public override string Version
        {
            get
            {
                return Ver;
            }
        }
    }
}
