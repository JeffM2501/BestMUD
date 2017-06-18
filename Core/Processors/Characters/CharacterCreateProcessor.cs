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
using Utilities;

namespace Core.Processors.Characters
{
    public class CharacterCreateProcessor : PooledProcessor
    {
        public event EventHandler<Connection> CharacterCreateComplete = null;

        public class CharacterCreateStateData
        {
            public string Name = string.Empty;
            public int RaceChoice = -1;
            public int ClassChoice = -1;

            public int[] RaceIndexes = null;
            public int[] ClassIndexes = null;
        }

        public override void ProcessorAttach(Connection user)
        {
            base.ProcessorAttach(user);

            var data = GetConStateData<CharacterCreateStateData>(user);

            // send out the create name message
            SendUserFileMessage(user, "character/create/character_name.data");
        }

        protected void ShowRaceList(Connection user)
        {
            SendUserFileMessage(user, "character/create/race_list_header.data");

            var races = RaceDB.Instance.GetRaceList();
            races = Scripting.Register.CharacterHandler?.FilterRaces(user, races);

            for (int i = 1; i <= races.Length; i++)
                user.SendOutboundMessage(string.Format("{0}. {1}\n", i, races[i-1].Name));
            SendUserFileMessage(user, "character/create/race_list_footer.data");
        }

        protected void ShowClassList(Connection user, CharacterCreateStateData data)
        {
            SendUserFileMessage(user, "character/create/class_list_header.data");

            var classes = ClassDB.Instance.GetClassList();
            classes = Scripting.Register.CharacterHandler?.FilterClasses(user, RaceDB.Instance.FindRace(data.RaceChoice), classes);

            List<int> classIndexes = new List<int>();

            for (int i = 1; i <= classes.Length; i++)
            {
                user.SendOutboundMessage(string.Format("{0}. {1}\n", i, classes[i-1].Name));
                classIndexes.Add(classes[i-1].ClassID);
            }
            data.ClassIndexes = classIndexes.ToArray();
               
            SendUserFileMessage(user, "character/create/class_list_footer.data");
        }

        protected override bool ProcessUserMessage(Connection user, string msg)
        {
            if (base.ProcessUserMessage(user, msg))
                return true;

            var data = GetConStateData<CharacterCreateStateData>(user);

            if (data.Name == string.Empty)
            {
                if (PlayerCharacterDB.Instance.PCNameExists(msg))
                {
                    SendUserFileMessage(user, "character/create/name_invalid_entry.data");
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
                    SendUserFileMessage(user, "character/create/race_list_invalid_entry.data");
                    ShowRaceList(user);
                }
                else
                {
                    data.RaceChoice = data.RaceIndexes[index-1];
                    SendUserFileMessage(user, "character/create/character_name.data");
                    ShowClassList(user,data);
                }
            }
            else if (data.ClassChoice == -1)
            {
                int index = -1;
                if (!int.TryParse(msg, out index) || index < 1 || index > data.RaceIndexes.Length)
                {
                    SendUserFileMessage(user, "character/create/class_list_invalid_entry.data");
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
                        pc.Name = data.Name;
                        pc.UserID = user.UserID;

                        pc = PlayerCharacterDB.Instance.CreatePlayerCharacter(pc);

                        if (pc != null)
                        {
                            LogCache.Log(LogCache.BasicLog, "Character Created:(" + user.UserID.ToString() + ")" + pc.UID.ToString() + ":" + pc.Name);
                            CharacterCreateComplete?.Invoke(this, user);
                        }
                          
                        else
                        {
                            data.Name = string.Empty;
                            data.RaceChoice = -1;
                            data.ClassChoice = -1;
                            SendUserFileMessage(user, "character/create/name_invalid_entry.data");
                        }
                    }
                }
            }
            else
            {
                SendUserFileMessage(user, "character/unknown_data_state.data");
            }

            return true;
        }
    }
}
