using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Data.Game.Characters;
using Core.Data.Game.Classes;
using Core.Data.Game.Races;


namespace Scripting.API.Handlers
{
    public interface ICharacterCreator
    {
        PlayerCharacter CreateCharacter(int userID, RaceInfo raceData, ClassInfo classData);
    }
}
