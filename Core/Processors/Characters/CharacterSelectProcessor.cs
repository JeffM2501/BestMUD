using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Processors;
using Core.Databases.PlayerData;

namespace Core.Processors.Characters
{
    public class CharacterSelectProcessor : PooledProcessor
    {
        public event EventHandler<Connection> CharacterSelectionComplete = null;

        public class CharacterSelectStateData
        {
            public bool ForceCharacterCreate = false;

            public List<int> CharacterIndexes = new List<int>();
        }

        public override void ProcessorAttach(Connection user)
        {
            base.ProcessorAttach(user);

            user.Disconnected += User_Disconnected;

            var chars = PlayerCharacterDB.Instance.GetUserCharacters(user.UserID);

            if (chars.Count == 0)
            {
                // send out the need new 
                SendUserFileMessage(user, "character/no_characters.data");
                user.SetMessageProcessor(ProcessorPool.GetProcessor("CharacterCreate", user));
            }
            else
                ShowCharacterList(user);
        }

        /// <summary>
        /// Callback to be sure that we check in any outstanding players on a disconnect
        /// </summary>
        private void User_Disconnected(object sender, Connection e)
        {
            if (e.ActiveCharacter != null)
                PlayerCharacterDB.Instance.CheckInCharacter(e.ActiveCharacter);

            e.ActiveCharacter = null;
        }

        protected void ShowCharacterList(Connection user)
        {
            var data = GetConStateData<CharacterSelectStateData>(user);
            var chars = PlayerCharacterDB.Instance.GetUserCharacters(user.UserID);

            SendUserFileMessage(user, "character/select_character_header.data");
            SendUserFileMessage(user, "character/create_character_list_index.data");
            user.SendOutboundMessage("0. Create Character");

            data.CharacterIndexes.Clear();
            foreach (var c in chars)
            {
                data.CharacterIndexes.Add(c.UID);
                user.SendOutboundMessage(string.Format("{0}. {1}", data.CharacterIndexes.Count, c.Name));
            }
            SendUserFileMessage(user, "character/select_character_footer.data");
        }

        protected override bool ProcessUserMessage(Connection user, string msg)
        {
            base.ProcessUserMessage(user, msg);

            var data = GetConStateData<CharacterSelectStateData>(user);

            if (data.CharacterIndexes.Count == 0)
            {
                SendUserFileMessage(user, "character/no_characters.data");
                user.SetMessageProcessor(ProcessorPool.GetProcessor("CharacterCreate", user));
            }
            else
            {
                int selection = -1;
                int.TryParse(msg, out selection);

                if (selection == 0)
                    user.SetMessageProcessor(ProcessorPool.GetProcessor("CharacterCreate", user));
                else
                {
                    if (selection < 0 || selection >= data.CharacterIndexes.Count)
                    {
                        SendUserFileMessage(user, "character/invalid_character_selection.data");
                        ShowCharacterList(user);
                    }
                    else
                    {
                        if (user.ActiveCharacter != null)
                            PlayerCharacterDB.Instance.CheckInCharacter(user.ActiveCharacter);

                        user.ActiveCharacter = PlayerCharacterDB.Instance.CheckOutChracter(user.UserID, data.CharacterIndexes[selection]);
                        if (user.ActiveCharacter == null)
                        {
                            SendUserFileMessage(user, "character/invalid_character_selection.data");
                            ShowCharacterList(user);
                        }
                        else
                        {
                            SendUserFileMessage(user, "character/select_character_join_as.data", "<!CHAR_NAME>", user.ActiveCharacter.Name);
                            CharacterSelectionComplete?.Invoke(this, user);
                        }
                    }
                }
            }

            return true;
        }

        protected override void ProcessConnection(Connection con)
        {
            base.ProcessConnection(con);
        }
    }
}
