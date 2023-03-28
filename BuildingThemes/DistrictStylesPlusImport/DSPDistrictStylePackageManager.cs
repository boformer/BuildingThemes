using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ColossalFramework;
using ColossalFramework.Packaging;
using HarmonyLib;

namespace BuildingThemes.DistrictStylesPlusImport
{
    public static class DSPDistrictStylePackageManager
    {
        
        public const string VanillaPrefix = "System";
        
        internal static void RemoveDistrictStylePackage(string dsFullName)
        {
            PackageManager.DisableEvents();
            var dsAsset = PackageManager.FindAssetByName(dsFullName);
            PackageManager.Remove(dsAsset.package);
            File.Delete(dsAsset.package.packagePath);
            PackageManager.EnabledEvents();
        }
        
        /**
         * Enabled empty styles needs to be loaded to game too as they can contain only vanilla buildings, which
         * are loaded to the style only in the game
         */
        internal static void AddEmptyEnabledStylesToGame()
        {
            foreach (var districtStyleAsset in PackageManager.FilterAssets(UserAssetType.DistrictStyleMetaData))
            {
                if (districtStyleAsset == null || !districtStyleAsset.isEnabled) continue;
                
                var districtStyleMetaData = districtStyleAsset.Instantiate<DistrictStyleMetaData>();

                var styleExists = Singleton<DistrictManager>.instance.m_Styles
                    .Any(style => style.Name.Equals(districtStyleMetaData.name));

                if (styleExists) continue;
                
                var newStyle = new DistrictStyle(districtStyleMetaData.name, false);
                var styles = Singleton<DistrictManager>.instance.m_Styles.AddItem(newStyle);
                Singleton<DistrictManager>.instance.m_Styles = styles.ToArray();
            }
        }
        
        /**
         * Vanilla buildings has to be loaded to styles after game load. If style contains only vanilla buildings,
         * it has to be created in district manager.
         */
        internal static void LoadVanillaBuildingsToStyles()
        {
            foreach (var districtStyleAsset in PackageManager.FilterAssets(UserAssetType.DistrictStyleMetaData))
            {
                if (districtStyleAsset == null || !districtStyleAsset.isEnabled) continue;
                
                var districtStyleMetaData = districtStyleAsset.Instantiate<DistrictStyleMetaData>();
                var vanillaAssetsArray = districtStyleMetaData.assets?
                    .Where(assetName => assetName.StartsWith(VanillaPrefix + "."))
                    .ToArray();
                
                if (vanillaAssetsArray == null || vanillaAssetsArray.Length <= 0) continue;
                
                var districtStyle = Singleton<DistrictManager>.instance.m_Styles
                    .FirstOrDefault(ds => ds.Name.Equals(districtStyleMetaData.name));
                
                if (districtStyle == null) continue;

                foreach (var vanillaAssetName in vanillaAssetsArray)
                {
                    string buildingName = vanillaAssetName.Substring(
                        vanillaAssetName.IndexOf(".", StringComparison.Ordinal) + 1);
                    UnityEngine.Debug.Log($"Try to find building {buildingName} for style {districtStyle.Name}");
                    var buildingInfo = PrefabCollection<BuildingInfo>.FindLoaded(buildingName);
                    if (buildingInfo != null && !districtStyle.GetBuildingInfos().Contains(buildingInfo))
                    {
                        districtStyle.Add(buildingInfo);
                    }
                }
            }
        }

        /// <summary>
        /// It refreshes district style assets metadata.
        /// </summary>
        /// <param name="districtStyle">District Style which metadata should be refreshed</param>
        internal static void RefreshDistrictStyleAssetsMetaData(DistrictStyle districtStyle)
        {
            PackageManager.DisableEvents();
            var dsMeta = GetDistrictStyleMetaDataByName(districtStyle.Name);
            
            if (dsMeta == null)
            {
                UnityEngine.Debug.LogError($"District style metadata not found! (DS: ${districtStyle.Name})");
                return; // TODO: should we throw exception?
            }

            if (dsMeta.assets == null)
            {
                dsMeta.assets = new string[0];
            }

            var styleBuildingInfos = districtStyle.GetBuildingInfos();
            var newAssetList = new string[styleBuildingInfos.Length];

            if (styleBuildingInfos.Length > 0)
            {
                for (int i = 0; i < styleBuildingInfos.Length; i++)
                {
                    var assetName = GetPackageAssetName(styleBuildingInfos[i].name);
                    newAssetList[i] = assetName;
                }
            }

            dsMeta.assets = newAssetList;
            StylesHelper.SaveStyle(dsMeta, dsMeta.name, false);
            PackageManager.EnabledEvents();
            PackageManager.ForcePackagesChanged();
        }
        
        internal static void AddAssetToDistrictStyleMetaData(string districtStyleName, string buildingInfoName)
        {
            PackageManager.DisableEvents();
            var dsMeta = GetDistrictStyleMetaDataByName(districtStyleName);

            if (dsMeta == null)
            {
                UnityEngine.Debug.Log("district style metadata not found!");
                return; // TODO: should it be exception?
            }

            if (dsMeta.assets == null)
            {
                dsMeta.assets = new string[0];
            }
            
            var newAssetList = new string[dsMeta.assets.Length + 1];
            dsMeta.assets.CopyTo(newAssetList, 0);
            var assetName = GetPackageAssetName(buildingInfoName);
            
            if (!dsMeta.assets.Contains(assetName))
            {
                UnityEngine.Debug.Log($"Add asset {assetName}");
                newAssetList[newAssetList.Length - 1] = assetName;
                dsMeta.assets = newAssetList;
                StylesHelper.SaveStyle(dsMeta, dsMeta.name, false);
            }
            
            PackageManager.EnabledEvents();
            PackageManager.ForcePackagesChanged();
        }
        
        internal static void RemoveAssetFromDistrictStyleMetaData(string districtStyleName, string buildingInfoName)
        {
            PackageManager.DisableEvents();
            
            var dsMeta = GetDistrictStyleMetaDataByName(districtStyleName);
            if (dsMeta == null)
            {
                UnityEngine.Debug.Log("district style metadata not found!");
                return; // TODO: should it be exception?
            }

            if (dsMeta.assets == null)
            {
                dsMeta.assets = new string[0];
            }
            else
            {
                var assetName = GetPackageAssetName(buildingInfoName);
                for (int k = 0; k < dsMeta.assets.Length; k++)
                {
                    if (dsMeta.assets[k].Equals(assetName))
                    {
                        string[] array2 = new string[dsMeta.assets.Length - 1];
                        Array.Copy(dsMeta.assets, 0, array2, 0, k);
                        Array.Copy(dsMeta.assets, k + 1, array2, k, dsMeta.assets.Length - k - 1);
                        dsMeta.assets = array2;
                        StylesHelper.SaveStyle(dsMeta, dsMeta.name, false);
                        break;
                    }
                }
            }
            PackageManager.EnabledEvents();
            PackageManager.ForcePackagesChanged();
        }
        
        internal static string GetPackageAssetName(string buildingInfoName)
        {
            var assetName = buildingInfoName.Replace("_Data", "");
            // For vanilla buildings
            if (!BuildingInfoHelper.IsCustomAsset(buildingInfoName)) assetName = VanillaPrefix + "." + assetName;
            return assetName;
        }
        
        private static DistrictStyleMetaData GetDistrictStyleMetaDataByName(string dsName)
        {
            DistrictStyleMetaData dsMeta = null;
            var dsMetaList = GetDistrictStyleMetaDataList();
            foreach (var dsMetaInfo in dsMetaList)
            {
                UnityEngine.Debug.Log($"Searching - expected name {dsName} " +
                                 $"... offered name {dsMetaInfo.name}");
                if (!dsMetaInfo.name.Equals(dsName)) continue;
                dsMeta = dsMetaInfo;
                break;
            }
            return dsMeta;
        }
        
        private static List<DistrictStyleMetaData> GetDistrictStyleMetaDataList()
        {
            List<DistrictStyleMetaData> list = new List<DistrictStyleMetaData>();
            foreach (Package.Asset item in PackageManager.FilterAssets(UserAssetType.DistrictStyleMetaData))
            {
                if (item != null)
                {
                    list.Add(item.Instantiate<DistrictStyleMetaData>());
                }
            }
            return list;
        }
        
    }

    //just a stub
    internal class BuildingInfoHelper
    {
        public static bool IsCustomAsset(string buildingInfoName)
        {
            throw new NotImplementedException();
        }
    }
}