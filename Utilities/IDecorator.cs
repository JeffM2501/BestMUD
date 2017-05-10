using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public interface IDecorator
    {
        string LogFileHeader(string title);

        string LogSessionOpen();
        string LogSessionClose();
        string LogDecorate(string text);
    }
}
