using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BuildingThemes.Detour
{
    public class BuildingInfoDetour : BuildingInfo
    {
        private static bool deployed = false;
        private static bool isGrowable = false;
        private static BuildingInfo previousPrefab = null;

        private static RedirectCallsState _InitializePrefab_state;
        private static MethodInfo _InitializePrefab_original;
        private static MethodInfo _InitializePrefab_detour;

        public static void Deploy()
        {
            if (!deployed)
            {
                if (Util.IsModActive("Prefab Hook"))
                {
                    var type = Util.FindType("BuildingInfoHookReflective");
                    if (type == null)
                    {
                        UnityEngine.Debug.LogError("Building Themes - type 'BuildingInfoHookReflective' not found. Update Prefab Hook!");
                    }
                    else
                    {
                        type.GetMethod("RegisterPreInitializationHook", BindingFlags.Public | BindingFlags.Static)
                            .Invoke(null, new object[]
                            {
                                new Action<BuildingInfo>(PreInitializeHook),
                            });
                        type.GetMethod("RegisterPostInitializationHook", BindingFlags.Public | BindingFlags.Static)
                            .Invoke(null, new object[]
                            {
                                new Action<BuildingInfo>(PostInitializeHook),
                            });
                        type.GetMethod("Deploy", BindingFlags.Public | BindingFlags.Static)
                            .Invoke(null, new object[]
                            {
                            });
                    }

                }
                else
                {
                    _InitializePrefab_original = typeof(BuildingInfo).GetMethod("InitializePrefab", BindingFlags.Instance | BindingFlags.Public);
                    _InitializePrefab_detour = typeof(BuildingInfoDetour).GetMethod("InitializePrefab", BindingFlags.Instance | BindingFlags.Public);
                    _InitializePrefab_state = RedirectionHelper.RedirectCalls(_InitializePrefab_original, _InitializePrefab_detour);
                }
                deployed = true;

                Debugger.Log("Building Themes: BuildingInfo Methods detoured!");
            }
        }

        public static void Revert()
        {
            if (deployed)
            {
                if (Util.IsModActive("Prefab Hook"))
                {
                    var type = Util.FindType("BuildingInfoHookReflective");
                    if (type == null)
                    {
                        UnityEngine.Debug.LogError(
                            "Building Themes - type 'BuildingInfoHookReflective' not found. Update Prefab Hook!");
                    }
                    else
                    {
                        type.GetMethod("Revert", BindingFlags.Public | BindingFlags.Static)
                            .Invoke(null, new object[]
                            {
                            });
                    }
                }
                else
                {
                    RedirectionHelper.RevertRedirect(_InitializePrefab_original, _InitializePrefab_state);
                    _InitializePrefab_original = null;
                    _InitializePrefab_detour = null;
                }
                deployed = false;

                Debugger.Log("Better Themes: BuildingInfo Methods restored!");
            }
        }

        public new virtual void InitializePrefab()
        {
            try
            {
                PreInitializeHook(this);
            }
            catch
            {
                isGrowable = false;
                previousPrefab = null;
            }
            RedirectionHelper.RevertRedirect(_InitializePrefab_original, _InitializePrefab_state);
            try
            {
                base.InitializePrefab();
            }
            finally
            {
                RedirectionHelper.RedirectCalls(_InitializePrefab_original, _InitializePrefab_detour);
            }
            try
            {
                PostInitializeHook(this);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

        }

        private static void PreInitializeHook(BuildingInfo prefab)
        {

            var growable = prefab.m_class.GetZone() != ItemClass.Zone.None;
            if (growable)
            {
                //Debugger.Log("InitializePrefab called: " + this.name);
            }
            isGrowable = growable;
            previousPrefab = prefab;


        }

        private static void PostInitializeHook(BuildingInfo prefab)
        {

            if (!isGrowable || prefab != previousPrefab)
            {
                return;
            }
            var prefabVariations = Singleton<BuildingVariationManager>.instance.CreateVariations(prefab).Values.ToArray<BuildingInfo>();

            if (prefabVariations.Length > 0)
            {
                PrefabCollection<BuildingInfo>.InitializePrefabs("BetterUpgrade", prefabVariations, null);
            }
            //Debugger.Log("InitializePrefab done:   " + this.name);
        }
    }
}
