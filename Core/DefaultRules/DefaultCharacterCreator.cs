using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data.Common;
using Core.Data.Game.Characters;
using Core.Data.Game.Classes;
using Core.Data.Game.Races;
using Networking;
using Scripting.API.Handlers;

namespace Core.DefaultRules
{
    public class DefaultCharacterCreator : ICharacterCreator
    {
        public PlayerCharacter CreateCharacter(Connection user, RaceInfo raceData, ClassInfo classData)
        {
            PlayerCharacter pc = new PlayerCharacter(true);
            pc.UserID = user.UserID;
            pc.ClassID = classData.ClassID;
            pc.RaceID = raceData.RaceID;

            foreach (var raceItem in raceData.DefaultInventory)
                pc.Inventory.Add(raceItem);

            foreach (var classItem in classData.DefaultInventory)
                pc.Inventory.Add(classItem);

            // attributes
            pc.Attributes.Add(BasicAttributes.Strenght, 8);
            pc.Attributes.Add(BasicAttributes.Dexterity, 8);
            pc.Attributes.Add(BasicAttributes.Constitution, 8);
            pc.Attributes.Add(BasicAttributes.Intelligence, 8);
            pc.Attributes.Add(BasicAttributes.Wisdom, 8);
            pc.Attributes.Add(BasicAttributes.Charisma, 8);

            pc.Attributes.Merge(raceData.DefaultAttributeBonuses);
            pc.Attributes.Merge(classData.DefaultAttributeBonuses);

            foreach (var raceItem in raceData.DefaultInventory)
                pc.Inventory.Add(raceItem);

            foreach (var classItem in classData.DefaultInventory)
                pc.Inventory.Add(classItem);

            return pc;
        }

        public ClassInfo[] FilterClasses(Connection user, RaceInfo race, ClassInfo[] classes)
        {
            List<ClassInfo> endClasses = new List<ClassInfo>();

            foreach (var c in classes)
            {
                if (c.AllowedRaces.Count > 0)
                {
                    if (c.AllowedRaces.Contains(race.RaceID))
                        endClasses.Add(c);
                }
                else
                {
                    if (!c.RestrictedRaces.Contains(race.RaceID))
                        endClasses.Add(c);
                }
            }

            return endClasses.ToArray();
        }

        public RaceInfo[] FilterRaces(Connection user, RaceInfo[] races)
        {
            return races;
        }
    }
}
