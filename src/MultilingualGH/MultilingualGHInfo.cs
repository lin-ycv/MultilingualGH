using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace MultilingualGH
{
    public class MultilingualGHInfo : GH_AssemblyInfo
    {
        public override string Name => "MultilingualGH";
        public override Bitmap Icon => Properties.Resources.MultilingualGH;
        public override string Description => "Annotates components with desired language";
        public override Guid Id => new Guid("b1114eca-d766-4400-b6f5-12a703165898");
        public override string AuthorName => "Victor (Yu Chieh) Lin";
        public override string AuthorContact => "https://github.com/v-xup6/MultilingualGH";

        static readonly Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        static internal readonly string Ver = v.Major + "." + v.Minor + "." + v.Build;
        public override string Version => Ver;
    }
}
