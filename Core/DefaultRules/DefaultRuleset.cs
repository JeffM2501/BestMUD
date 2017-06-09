using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Databases.GameData;

namespace Core.DefaultRules
{
    public static class DefaultRuleset
    {
        private static List<int> StartingRooms = new List<int>();

        public static void Init()
        {
            Scripting.Register.SetCharacterCreator(new DefaultCharacterCreator());
            StartingRooms = ZoneDB.Instance.RoomIndexesWithAttribute("starting_room");

            Scripting.Register.OnCharacterJoin += Register_OnCharacterJoin;
        }

        private static void Register_OnCharacterJoin(object sender, Networking.Connection e)
        {
            if (e.ActiveCharacter == null || e.ActiveCharacter.CurrentRoom > 0 || StartingRooms.Count == 0)
                return;

            e.ActiveCharacter.CurrentRoom = StartingRooms[0];
        }
    }
}
