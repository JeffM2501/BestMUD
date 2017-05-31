using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Processors;
using Core.Databases.GameData;
using Core.Databases.PlayerData;
using Core.Data.Game.Characters;

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
            public int[] ClassIndexes = null;
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
            races = Scripting.Register.CharacterHandler?.FilterRaces(user, races);

            for (int i = 1; i <= races.Length; i++)
                user.SendOutboundMessage(string.Format("{0}. {1}", i, races[i].Name));
            SendUserFileMessage(user, "character/race_list_end.data");
        }

        protected void ShowClassList(Connection user, CharacterCreateStateData data)
        {
            SendUserFileMessage(user, "character/class_list_start.data");

            var classes = ClassDB.Instance.GetClassList();
            classes = Scripting.Register.CharacterHandler?.FilterClasses(user, RaceDB.Instance.FindRace(data.RaceChoice), classes);

            List<int> classIndexes = new List<int>();

            for (int i = 1; i <= classes.Length; i++)
            {
                user.SendOutboundMessage(string.Format("{0}. {1}", i, classes[i].Name));
                classIndexes.Add(classes[i].ClassID);
            }
            data.ClassIndexes = classIndexes.ToArray();
               
            SendUserFileMessage(user, "character/class_list_end.data");
        }

        protected override bool ProcessUserMessage(Connection user, string msg)
        {
            if (!base.ProcessUserMessage(user, msg))
                return false;
            var data = GetConStateData<CharacterCreateStateData>(user);

            if (data.Name == string.Empty)
            {
                if (PlayerCharacterDB.Instance.PCNameExists(msg))
                {
                    SendUserFileMessage(user, "character/name_invalid_entry.data");
                }
                else
                {
                    data.Name = msg;
                    data.RaceIndexes = RaceDB.Instance.GetRaceIndexList();
                    ShowRaceList(user);
                }
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
                    data.RaceChoice = data.RaceIndexes[index-1];
                    SendUserFileMessage(user, "character/character_name.data");
                    ShowClassList(user,data);
                }
            }
            else if (data.ClassChoice == -1)
            {
                int index = -1;
                if (!int.TryParse(msg, out index) || index < 1 || index > data.RaceIndexes.Length)
                {
                    SendUserFileMessage(user, "character/class_list_invalid_entry.data");
                    ShowClassList(user, data);
                }
                else
                {
                    data.ClassChoice = data.ClassIndexes[index - 1];

                    // create the character
                    PlayerCharacter pc = new PlayerCharacter(true);
                    pc = Scripting.Register.CharacterHandler?.CreateCharacter(user, RaceDB.Instance.FindRace(data.RaceChoice), ClassDB.Instance.FindClass(data.ClassChoice));
                    if (pc != null)
                    {
                        pc = PlayerCharacterDB.Instance.CreatePlayerCharacter(pc);
                    }
                }
            }
            else
            {

            }

            return true;
        }
    }
}
