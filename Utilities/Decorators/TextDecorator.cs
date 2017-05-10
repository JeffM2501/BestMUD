using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Decorators
{
    public class TextDecorator : IDecorator
    {
        string IDecorator.LogDecorate(string text)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(text);
            return sb.ToString();
        }

        string IDecorator.LogFileHeader(string title)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("==================================================");
            sb.AppendLine(title);
            sb.AppendLine("==================================================");
            sb.AppendLine();
            return sb.ToString();
        }

        string IDecorator.LogSessionClose()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            return sb.ToString();
        }

        string IDecorator.LogSessionOpen()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
