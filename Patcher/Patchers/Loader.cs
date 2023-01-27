using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSLocalization
{
    public class Loader
    {
        public Loader(PatcherBase basePatcher)
        {
            BasePatcher = basePatcher;
        }

        public PatcherBase BasePatcher { get; }


        public void Run()
        { 
            if (!BasePatcher.Validation()) 
                BasePatcher.Patch();
            ProcessStart();
        }


        private void ProcessStart()
        {
            BasePatcher.Run();
        }
    }
}
