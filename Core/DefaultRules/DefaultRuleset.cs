using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DefaultRules
{
    public static class DefaultRuleset
    {
        public static void Init()
        {
            Scripting.Register.SetCharacterCreator(new DefaultCharacterCreator());
        }
    }
}
