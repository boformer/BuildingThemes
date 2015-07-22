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
        
        private static RedirectCallsState _InitializePrefab_state;
        private static MethodInfo _InitializePrefab_original;
        private static MethodInfo _InitializePrefab_detour;

        public static void Deploy() 
        {
            if (!deployed)
            {
                _InitializePrefab_original = typeof(BuildingInfo).GetMethod("InitializePrefab", BindingFlags.Instance | BindingFlags.Public);
                _InitializePrefab_detour = typeof(BuildingInfoDetour).GetMethod("InitializePrefab", BindingFlags.Instance | BindingFlags.Public);
                _InitializePrefab_state = RedirectionHelper.RedirectCalls(_InitializePrefab_original, _InitializePrefab_detour);

                deployed = true;

                Debugger.Log("Building Themes: BuildingInfo Methods detoured!");
            }
        }

        public static void Revert() 
        {
            if (deployed)
            {
                RedirectionHelper.RevertRedirect(_InitializePrefab_original, _InitializePrefab_state);
                _InitializePrefab_original = null;
                _InitializePrefab_detour = null;

                deployed = false;

                Debugger.Log("Better Themes: BuildingInfo Methods restored!");
            }
        }

        public new virtual void InitializePrefab()
        {
            bool growable = this.m_class.GetZone() != ItemClass.Zone.None;

            if (growable)
            {
                //Debugger.Log("InitializePrefab called: " + this.name);
            }

            RedirectionHelper.RevertRedirect(_InitializePrefab_original, _InitializePrefab_state);
            base.InitializePrefab();
            RedirectionHelper.RedirectCalls(_InitializePrefab_original, _InitializePrefab_detour);

            if (growable) 
            {
                var prefabVariations = Singleton<BuildingVariationManager>.instance.CreateVariations(this).Values.ToArray<BuildingInfo>();

                if (prefabVariations.Length > 0) 
                { 
                    PrefabCollection<BuildingInfo>.InitializePrefabs("BetterUpgrade", prefabVariations, null);
                }
                //Debugger.Log("InitializePrefab done:   " + this.name);
            }
        }
    }
}
