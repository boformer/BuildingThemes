using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using HarmonyLib;
using JetBrains.Annotations;

namespace BuildingThemes.Data.DistrictStylesPlusImport
{
    
    /// <summary>
    /// DSP District Style Manager (CRUD operations etc)
    /// </summary>
    public class DSPDistrictStyleManager : Singleton<DSPDistrictStyleManager>
    {
        
        /// <summary>
        /// Creates new in-game district style and also a asset package for it.
        /// </summary>
        /// <param name="districtStyleName">name of new district style</param>
        internal static void CreateDistrictStyle(string districtStyleName)
        {
            var districtStyleMetaData = new DistrictStyleMetaData
            {
                assets = new string[0],
                builtin = false,
                name = districtStyleName,
                timeStamp = DateTime.Now,
                steamTags = new string[1] {"District Style"}
            };
            StylesHelper.SaveStyle(districtStyleMetaData, districtStyleName, true);
            var newDistrictStyle = new DistrictStyle(districtStyleMetaData.name, false);
            var styles = Singleton<DistrictManager>.instance.m_Styles.AddItem(newDistrictStyle);
            Singleton<DistrictManager>.instance.m_Styles = styles.ToArray();
            
            // refresh style buildings in building manager
            Singleton<DSPBuildingManager>.instance.RefreshStylesInBuildingManager();
        }
        
        /// <summary>
        /// Removes District Style by mentioned name
        /// </summary>
        /// <param name="districtStyle">name of DS to remove</param>
        /// <param name="transientStyle">is it transient style</param>
        internal static void DeleteDistrictStyle(DistrictStyle districtStyle, bool transientStyle)
        {
            // we have to add +1 because 0 in the District means a Default style.
            // Yet styles in DistrictManager are counted from 0 without any Default style value!
            var districtStyleId = GetDistrictStyleIdByFullName(districtStyle.FullName) + 1;
            
            var districts = Singleton<DistrictManager>.instance.m_districts.m_buffer;
            for (var i = 0; i < districts.Length; i++)
            {
                // process vanilla district setting in districts
                // set style to default, if district uses given style
                if (districts[i].m_Style == districtStyleId)
                {
                    districts[i].m_Style = 0;
                }
                // shift style id -1, if district uses style with id > given style
                if (districts[i].m_Style > districtStyleId)
                {
                    districts[i].m_Style--;
                }
            }

            // create new district styles list for districtManager - for vanilla functionality
            Singleton<DistrictManager>.instance.m_Styles = Singleton<DistrictManager>.instance.m_Styles
                .Where(instanceStyle => !instanceStyle.FullName.Equals(districtStyle.FullName)).ToArray();

            // if deleted style is not an transient style
            if (!transientStyle)
            {
                // remove district style package
                DSPDistrictStylePackageManager.RemoveDistrictStylePackage(districtStyle.FullName);
                
                // remove style from transient styles
                DSPTransientStyleManager.RemoveDeletedDistrictStyleFromTransients(districtStyle.FullName);
            }

            // refresh style buildings in building manager
            Singleton<DSPBuildingManager>.instance.RefreshStylesInBuildingManager();
        }

        /// <summary>
        /// It adds list of BuildingInfos to Style and refresh the style package and styles in building manager.
        /// </summary>
        /// <param name="buildingInfos">List of buildingInfos to add to DistrictStyle</param>
        /// <param name="districtStyle">DistrictStyle which is going to be changed</param>
        internal static void AddBuildingInfoListToStyle(List<BuildingInfo> buildingInfos, DistrictStyle districtStyle)
        {
            if (buildingInfos == null || !buildingInfos.Any() || districtStyle == null) return;

            var refreshNeeded = false;

            var validatedBuildingInfos = buildingInfos
                .Where(buildingInfo => buildingInfo != null && !districtStyle.Contains(buildingInfo))
                .ToList();

            foreach (var buildingInfo in validatedBuildingInfos)
            {
                UnityEngine.Debug.Log($"Building Info: {buildingInfo} - districtStyle: {districtStyle} - dsBIs: {districtStyle.GetBuildingInfos()}");
                districtStyle.Add(buildingInfo);
                refreshNeeded = true;
            }

            if (!refreshNeeded) return;
            
            // update transient styles if they use updated district style
            DSPTransientStyleManager.ModifyTransientStylesByDSBuildingInfos(
                new List<BuildingInfo>(validatedBuildingInfos), districtStyle.FullName, true);
            
            DSPDistrictStylePackageManager.RefreshDistrictStyleAssetsMetaData(districtStyle);
            DSPBuildingManager.instance.RefreshStylesInBuildingManager();
        } 
        
        /// <summary>
        /// Adds building information to the district style.
        /// </summary>
        /// <param name="buildingInfo">BuildingInfo to add to the district style</param>
        /// <param name="districtStyle">Target district style</param>
        internal static void AddBuildingInfoToStyle(BuildingInfo buildingInfo, DistrictStyle districtStyle)
        {
            districtStyle.Add(buildingInfo);
            
            DSPDistrictStylePackageManager.AddAssetToDistrictStyleMetaData(districtStyle.Name, buildingInfo.name);
            
            UnityEngine.Debug.Log($"Adding {buildingInfo} from {districtStyle.Name}");
            
            // update transient styles if they use updated district style
            DSPTransientStyleManager.ModifyTransientStylesByDSBuildingInfos(
                new List<BuildingInfo> {buildingInfo}, districtStyle.FullName, true);
            
            DSPBuildingManager.instance.RefreshStylesInBuildingManager();
        }
        
        internal static void RemoveBuildingInfoListFromStyle(List<BuildingInfo> buildingInfos, DistrictStyle districtStyle)
        {
            if (buildingInfos == null 
                || !buildingInfos.Any() 
                || districtStyle?.GetBuildingInfos() == null 
                || !districtStyle.GetBuildingInfos().Any()) return;


            if (!districtStyle.GetBuildingInfos().Intersect(buildingInfos).Any()) return;
            
            var reducedAssetList = districtStyle.GetBuildingInfos().Except(buildingInfos);
            Traverse.Create(districtStyle).Field("m_Infos").SetValue(new HashSet<BuildingInfo>(reducedAssetList));
            DSPDistrictStylePackageManager.RefreshDistrictStyleAssetsMetaData(districtStyle);
            
            // update transient styles if they use updated district style
            DSPTransientStyleManager.ModifyTransientStylesByDSBuildingInfos(
                new List<BuildingInfo>(buildingInfos), districtStyle.FullName, false);
            
            DSPBuildingManager.instance.RefreshStylesInBuildingManager();
        } 
        
        /// <summary>
        /// Removes given buildingInfo from the district style - and updates district style affected services
        /// accordingly.
        /// </summary>
        /// <param name="buildingInfo">BuildingInfo to remove</param>
        /// <param name="districtStyle">Target district style</param>
        internal static void RemoveBuildingInfoFromStyle(BuildingInfo buildingInfo, DistrictStyle districtStyle)
        {
            if (!districtStyle.Contains(buildingInfo)) return;
            
            BuildingInfo[] newBuildingInfoArray = districtStyle.GetBuildingInfos()
                .Where(bi => !bi.Equals(buildingInfo))
                .ToArray();

            var buildingInfosFieldInfo = GetDistrictStyleClassField("m_Infos", districtStyle);
            if (buildingInfosFieldInfo == null)
            {
                UnityEngine.Debug.LogError("District style does not have field m_Infos!");
                return; 
            }
            buildingInfosFieldInfo.SetValue(districtStyle, new HashSet<BuildingInfo>(newBuildingInfoArray));

            RefreshDistrictStyleAffectedService(districtStyle);
            DSPDistrictStylePackageManager.RemoveAssetFromDistrictStyleMetaData(districtStyle.Name, buildingInfo.name);
            
            UnityEngine.Debug.Log($"Removing {buildingInfo} from {districtStyle.Name}");
            
            // update transient styles if they use updated district style
            DSPTransientStyleManager.ModifyTransientStylesByDSBuildingInfos(
                new List<BuildingInfo> {buildingInfo}, districtStyle.FullName, false);
            
            DSPBuildingManager.instance.RefreshStylesInBuildingManager();
        }

        /// <summary>
        /// Returns district style id by district style full name. It is counted from 0. There is no value for "default".
        /// </summary>
        /// <returns>district style id in district manager</returns>
        internal static ushort GetDistrictStyleIdByFullName(string fullName)
        {
            var districtStyles = Singleton<DistrictManager>.instance.m_Styles;
            
            if (districtStyles == null) return 0; // TODO: this is wrong - 0 is not default but first style!!!
            // TODO: above works only because style at 0 is builtIn style!!! so it can't be deleted!
            
            for (ushort i = 0; i < districtStyles.Length; i++)
            {
                if (districtStyles[i].FullName.Equals(fullName))
                {
                    return i;
                }
                
            }
            return 0;
        }

        /// <summary>
        /// Returns District Style (or Transient Style) or it returns null.
        /// </summary>
        /// <param name="styleFullName"></param>
        /// <returns></returns>
        [CanBeNull]
        internal static DistrictStyle GetDistrictStyleByFullName(string styleFullName)
        {
            var districtStyles = DistrictManager.instance.m_Styles;

            if (districtStyles == null || districtStyles.Length == 0)
            {
                return null;
            }

            try
            {
                return districtStyles.First(ds => ds.FullName.Equals(styleFullName));
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
        
        /// <summary>
        /// Refresh affected services (zones) by this district style according its buildings
        /// </summary>
        /// <param name="districtStyle">target district style</param>
        internal static void RefreshDistrictStyleAffectedService(DistrictStyle districtStyle)
        {
            // get private field DistrictStyle.m_AffectedServices
            var affectedServicesFiledInfo = GetDistrictStyleClassField("m_AffectedServices", districtStyle);

            if (affectedServicesFiledInfo == null)
            {
                UnityEngine.Debug.LogError("District style does not have field m_AffectedServices!");
                return; // TODO: this should rather be an exception
            }
            
            // if there are some buildings in style, set affected services accordingly
            if (districtStyle.GetBuildingInfos() != null && districtStyle.GetBuildingInfos().Length > 0)
            {
                HashSet<int> affectedServices = new HashSet<int>();
                foreach (var buildingInfo in districtStyle.GetBuildingInfos())
                {
                    var level = buildingInfo.m_class != null ? buildingInfo.m_class.m_level : ItemClass.Level.Level1;
                    affectedServices.Add(
                        DistrictStyle.GetServiceLevelIndex(
                            buildingInfo.GetService(), buildingInfo.GetSubService(), level));
                }

                if (affectedServices.Count > 0)
                {
                    affectedServicesFiledInfo.SetValue(districtStyle, affectedServices);
                }
            }
            // otherwise set just empty Set
            else
            {
                affectedServicesFiledInfo.SetValue(districtStyle, new HashSet<int>());
            }
        }
        
        internal static FieldInfo GetDistrictStyleClassField(string fieldName, DistrictStyle districtStyle)
        {
            // TODO: whole this method can be replaced by HarmonyLib accessTools
            var type = districtStyle.GetType();
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            return type.GetField(fieldName, bindingFlags);
        }

    }
}