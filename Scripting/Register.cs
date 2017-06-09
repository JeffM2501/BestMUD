using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Networking;
using Scripting.API.Handlers;

namespace Scripting
{
    public static class Register
    {
        internal static Dictionary<string, string> RegisteredFunctionNames = new Dictionary<string, string>();

        public static event EventHandler<Connection> OnCharacterJoin;

        public static void CallOnCharacterJoin(Connection user)
        {
            OnCharacterJoin?.Invoke(user, user);
        }

        internal static void Clear()
        {
            RegisteredFunctionNames.Clear();
        }

        public static void HandlerFunction(string handler, string name)
        {
            if (RegisteredFunctionNames.ContainsKey(handler.ToLower()))
                RegisteredFunctionNames[handler.ToLower()] = name;
            else
                RegisteredFunctionNames.Add(handler.ToLower(), name);
        }

        public static ICharacterCreator CharacterHandler { get; private set; } = null;

        public static void SetCharacterCreator(ICharacterCreator handler)
        {
            CharacterHandler = handler;
        }
    }
}
