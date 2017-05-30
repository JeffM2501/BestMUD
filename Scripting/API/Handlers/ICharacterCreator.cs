using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Data.Game.Characters;
using Core.Data.Game.Classes;
using Core.Data.Game.Races;

using Networking;

namespace Scripting.API.Handlers
{
    public interface ICharacterCreator
    {
        RaceInfo[] FilterRaces(Connection user, RaceInfo[] races);

        ClassInfo[] FilterClasses(Connection user, RaceInfo race, ClassInfo[] classes);

        PlayerCharacter CreateCharacter(Connection user, RaceInfo raceData, ClassInfo classData);
    }
}
