using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Processors;
using Core.Databases.GameData;

namespace Core.Processors.Characters
{
    public class CharacterCreateProcessor : PooledProcessor
    {
        public class CharacterCreateStateData
        {
            public string Name = string.Empty;
            public int RaceChoice = -1;
            public int ClassChoice = -1;

            public int[] RaceIndexes = null;
        }

        public override void ProcessorAttach(Connection con)
        {
            base.ProcessorAttach(con);

            var data = GetConStateData<CharacterCreateStateData>(con);

            // send out the create name message
            SendUserFileMessage(con, "character/character_name.data");
        }

        protected void ShowRaceList(Connection user)
        {
            SendUserFileMessage(user, "character/race_list_start.data");

            var races = RaceDB.Instance.GetRaceList();
            for (int i = 1; i <= races.Length; i++)
                user.SendOutboundMessage(string.Format("{0}. {1}", i, races[i].Name));
            SendUserFileMessage(user, "character/race_list_end.data");
        }

        protected void ShowClassList(Connection user)
        {
            SendUserFileMessage(user, "character/class_list_start.data");

            var races = RaceDB.Instance.GetRaceList();
            for (int i = 1; i <= races.Length; i++)
                user.SendOutboundMessage(string.Format("{0}. {1}", i, races[i].Name));
            SendUserFileMessage(user, "character/class_list_end.data");
        }

        protected override bool ProcessUserMessage(Connection user, string msg)
        {
            if (!base.ProcessUserMessage(user, msg))
                return false;
            var data = GetConStateData<CharacterCreateStateData>(user);

            if (data.Name == string.Empty)
            {
                data.Name = msg;
                data.RaceIndexes = RaceDB.Instance.GetRaceIndexList();
                ShowRaceList(user);
            }
            else if (data.RaceChoice == -1)
            {
                int index = -1;
                if (!int.TryParse(msg,out index) || index < 1 || index > data.RaceIndexes.Length)
                {
                    SendUserFileMessage(user, "character/race_list_invalid_entry.data");
                    ShowRaceList(user);
                }
                else
                {
                    data.RaceChoice = data.RaceIndexes[index];
                    SendUserFileMessage(user, "character/character_name.data");
                }
            }
            else if (data.ClassChoice == -1)
            {

            }
            else
            {

            }

            return true;
        }
    }
}
