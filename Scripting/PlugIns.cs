using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripting
{
    public interface IBestMudPlugin
    {
        void ActivatePlugIn();
        void DeactivatePlugIn();
    }

    internal class PlugInLoader
    {
    }
}
