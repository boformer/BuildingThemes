using ColossalFramework;
using System.Reflection;
using UnityEngine;

namespace BuildingThemes.Detour
{
    // This detour catches the position of abandoned buildings which are replaced by new level 1 buildings
    public class ImmaterialResourceManagerDetour
    {
        private static bool deployed = false;

        private static RedirectCallsState _ImmaterialResourceManager_AddResource_state;
        private static MethodInfo _ImmaterialResourceManager_AddResource_original;
        private static MethodInfo _ImmaterialResourceManager_AddResource_detour;

        public static void Deploy()
        {
            if (deployed || Util.IsModActive(BuildingThemesMod.EIGHTY_ONE_MOD))
            {
                Debugger.Log("Building Themes: ImmaterialResourceManager Methods won't be detoured: 81 Tiles detected");
                return;
            }
            _ImmaterialResourceManager_AddResource_original = typeof(ImmaterialResourceManager).GetMethod("AddResource",
                new[] { typeof(ImmaterialResourceManager.Resource), typeof(int), typeof(Vector3), typeof(float) });
            _ImmaterialResourceManager_AddResource_detour = typeof(ImmaterialResourceManagerDetour).GetMethod("AddResource", BindingFlags.Instance | BindingFlags.Public);
            _ImmaterialResourceManager_AddResource_state = RedirectionHelper.RedirectCalls(_ImmaterialResourceManager_AddResource_original, _ImmaterialResourceManager_AddResource_detour);

            deployed = true;

            Debugger.Log("Building Themes: ImmaterialResourceManager Methods detoured!");
        }

        public static void Revert()
        {
            if (!deployed)
            {
                return;
            }
            RedirectionHelper.RevertRedirect(_ImmaterialResourceManager_AddResource_original, _ImmaterialResourceManager_AddResource_state);
            _ImmaterialResourceManager_AddResource_original = null;
            _ImmaterialResourceManager_AddResource_detour = null;

            deployed = false;

            Debugger.Log("Building Themes: ImmaterialResourceManager Methods restored!");
        }

        private static int debugCounter = 0;

        // Detours

        public int AddResource(ImmaterialResourceManager.Resource resource, int rate, Vector3 positionArg, float radius)
        {
            if (Debugger.Enabled && debugCounter < 10)
            {
                debugCounter++;
                Debugger.Log("Building Themes: Detoured ImmaterialResource.AddResource was called.");
            }

            // Catch the position of the abandoned building
            if (resource == ImmaterialResourceManager.Resource.Abandonment)
            {
                BuildingThemesMod.position = positionArg;
            }

            // Call the original method
            RedirectionHelper.RevertRedirect(_ImmaterialResourceManager_AddResource_original, _ImmaterialResourceManager_AddResource_state);
            var result = Singleton<ImmaterialResourceManager>.instance.AddResource(resource, rate, positionArg, radius);
            RedirectionHelper.RedirectCalls(_ImmaterialResourceManager_AddResource_original, _ImmaterialResourceManager_AddResource_detour);

            return result;
        }
    }
}
