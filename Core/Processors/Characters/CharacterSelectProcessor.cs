using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Processors;

namespace Core.Processors.Characters
{
    public class CharacterSelectProcessor : PooledProcessor
    {
        public class CharacterSelectStateData
        {
            public bool ForceCharacterCreate = false;
        }

        public override void ProcessorAttach(Connection con)
        {
            base.ProcessorAttach(con);

            var data = GetConStateData<CharacterSelectStateData>(con);


            data.ForceCharacterCreate = true;
            // send out the need new 
            SendUserFileMessage(con, "character/no_characters.data");
        }

        protected override void ProcessConnection(Connection con)
        {
            base.ProcessConnection(con);

            var data = GetConStateData<CharacterSelectStateData>(con);
            if (data.ForceCharacterCreate)
            {
                con.SetMessageProcessor(ProcessorPool.GetProcessor("CharacterCreate",con));
            }
            else
            {

            }
        }
    }
}
