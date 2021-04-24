using System;
using System.Linq;
using ColossalFramework;

namespace BuildingThemes.HarmonyPatches.BuildingInfoPatch
{
    public class InitializePrefabPatch
    {
        private static bool deployed;

        public static void Deploy()
        {
            if (deployed)
            {
                return;
            }

            PatchUtil.Patch(
                new PatchUtil.MethodDefinition(typeof(BuildingInfo), nameof(BuildingInfo.InitializePrefab)),
                new PatchUtil.MethodDefinition(typeof(InitializePrefabPatch), nameof(PreInitializeHook)),
                new PatchUtil.MethodDefinition(typeof(InitializePrefabPatch), nameof(PostInitializeHook)) 
                );

            deployed = true;
        }

        public static void Revert()
        {
            if (!deployed)
            {
                return;
            }

            PatchUtil.Unpatch(
                new PatchUtil.MethodDefinition(typeof(BuildingInfo), nameof(BuildingInfo.InitializePrefab)));
            deployed = false;
        }

        public struct State
        {
            public bool isGrowable;
            public bool prefixSuccess;
        }

        public static void PreInitializeHook(BuildingInfo __instance, out State __state)
        {
            try
            {
                var growable = __instance.m_class.GetZone() != ItemClass.Zone.None;
                if (growable)
                {
                    //Debugger.Log("InitializePrefab called: " + this.name);
                }

                __state = new State
                {
                    prefixSuccess = true,
                    isGrowable = growable
                };
            }
            catch
            {
                __state = new State
                {
                    prefixSuccess = false,
                    isGrowable = false
                };
            }
        }

        public static void PostInitializeHook(BuildingInfo __instance, State __state)
        {
            try
            {
                if (!__state.isGrowable || !__state.prefixSuccess)
                {
                    return;
                }

                var prefabVariations = Singleton<BuildingVariationManager>.instance.CreateVariations(__instance).Values
                    .ToArray<BuildingInfo>();

                if (prefabVariations.Length > 0)
                {
                    PrefabCollection<BuildingInfo>.InitializePrefabs("BetterUpgrade", prefabVariations, null);
                }
                //Debugger.Log("InitializePrefab done:   " + this.name);

            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
    }
}
