using System.Runtime.CompilerServices;
using BuildingThemes.Redirection;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace BuildingThemes.Detour
{
    [TargetType(typeof(BuildingManager))]
    public class BuildingManagerDetour
    {
        // Detour

        // The original GetRandomBuildingInfo method. 
        // The only method that still points here is the "Downgrade" method which resets abandoned buildings to L1
        [RedirectMethod]
        public BuildingInfo GetRandomBuildingInfo(ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode, int style)
        {
            return RandomBuildings.GetRandomBuildingInfo_Spawn(BuildingThemesMod.position, ref r, service, subService, level, width, length, zoningMode, style);
        }
    }
}
