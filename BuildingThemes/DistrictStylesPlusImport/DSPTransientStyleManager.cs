using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using HarmonyLib;

namespace BuildingThemes.DistrictStylesPlusImport
{
    /// <summary>
    /// Manager of transient styles. Transient style is style which is created only in-game which means that it does not
    /// have a crp package file. Transient style is one per district and it is a merged style from selected styles for
    /// given district.
    /// </summary>
    public class DSPTransientStyleManager : Singleton<DSPTransientStyleManager>
    {
        public const string TransientStylePackage = "DSPTransient";
        public const string TransientStylePrefix = "DSPTransientDS-";
        public const byte MaxDistrictCount = 128; // Max number of districts in the city
        
        /// <summary>
        /// Selected styles for districts. It contains DistrictStyle.FullName !
        /// </summary>
        private static readonly Dictionary<byte, HashSet<string>> TransientDistrictStyleConfigs = 
            new Dictionary<byte, HashSet<string>>();

        internal static Dictionary<byte, HashSet<string>> GetTransientDistrictStyleConfigs()
        {
            return TransientDistrictStyleConfigs;
        }

        /// <summary>
        /// Get district style which were used for transient style for district
        /// </summary>
        /// <param name="districtId">chosen district id</param>
        /// <returns>list of styles fullNames</returns>
        internal static HashSet<string> GetSelectedStylesForDistrict(byte districtId)
        {
            return !TransientDistrictStyleConfigs.ContainsKey(districtId) ? 
                GetSelectedStylesForDistrictFromVanilla(districtId) : 
                new HashSet<string>(TransientDistrictStyleConfigs.GetValueSafe(districtId));
        }

        private static HashSet<string> GetSelectedStylesForDistrictFromVanilla(byte districtId)
        {
            
            // get style id (-1 because district attribute of style has 0 for default style)
            var styleId = DistrictManager.instance.m_districts.m_buffer[districtId].m_Style - 1;
            
            // it was a default style, nothing to pick
            if (styleId < 0) return new HashSet<string>();
            
            // get style from district manager
            var style = DistrictManager.instance.m_Styles[styleId];
            
            // if style exist and if it is not a transient style
            if (style != null && !style.PackageName.Equals(TransientStylePackage))
            {
                // use that style as selected default
                return new HashSet<string> {style.FullName};
            }

            // nothing found, return just empty set
            return new HashSet<string>();
        }

        /// <summary>
        /// Save selected styles used for a transient style for given district
        /// </summary>
        /// <param name="districtId">district Id</param>
        /// <param name="dsStyleFullNames">list of chosen styles fullNames</param>
        internal static void SetSelectedStylesForDistrict(byte districtId, HashSet<string> dsStyleFullNames)
        {
            Logging.DebugLog($"Selected styles {string.Join(", ", dsStyleFullNames.ToArray())} " +
                             $"for district {districtId}");

            var currentDistrictStyleSelection = GetSelectedStylesForDistrict(districtId);
            
            Logging.DebugLog($"Already in transient " +
                             $"{string.Join(",", currentDistrictStyleSelection.ToArray())}");
            
            Logging.DebugLog($"Compared result: {currentDistrictStyleSelection.SetEquals(dsStyleFullNames)}");
            
            if (currentDistrictStyleSelection.SetEquals(dsStyleFullNames)) 
                return; // do nothing if selection is same
            
            // remove old configuration of transient style
            TransientDistrictStyleConfigs.Remove(districtId);
            
            GetTransientStyleNames(districtId, out var transientStyleName, out var transientStyleFullName);
            var transientStyle = DSPDistrictStyleManager.GetDistrictStyleByFullName(transientStyleFullName);
            
            // no style selected
            if (dsStyleFullNames.Count == 0)
            {
                Logging.DebugLog($"No style selected for district {districtId}");
                
                // assign default style to district
                DistrictManager.instance.m_districts.m_buffer[districtId].m_Style = 0;
                
                
                Logging.DebugLog($"Try to delete transient DS if exists {transientStyleFullName}");
                // transient style can be removed
                if (transientStyle != null)
                    DSPDistrictStyleManager.DeleteDistrictStyle(transientStyle, true);
                return; // transient style has been removed
            }
            
            // some DS has been selected so add new configuration of transient style
            TransientDistrictStyleConfigs.Add(districtId, dsStyleFullNames);
            Logging.DebugLog($"New configuration added for district {districtId}");

            // if transient style does not exist, create new one
            if (transientStyle == null)
            {
                transientStyle = CreateTransientStyle(transientStyleName);
                Logging.DebugLog($"New transient style created {transientStyleName}");
            }
            
            // merge selected district styles to transient style
            MergeDistrictStylesToTransientStyle(dsStyleFullNames, transientStyle);

            // get transient style id
            var transientStyleId = Array.IndexOf(DistrictManager.instance.m_Styles, transientStyle);
            
            // assign transient style to district (+ 1 because 0 is default one)
            if (transientStyleId > -1) {
                DistrictManager.instance.m_districts.m_buffer[districtId].m_Style = (ushort) (transientStyleId + 1);
            }
            else
            {
                Logging.ErrorLog($"Transient style for district {districtId} does not exist!");
                DistrictManager.instance.m_districts.m_buffer[districtId].m_Style = 0;
            }
        }

        /// <summary>
        /// When district style is deleted, remove it from all transient styles too.
        /// </summary>
        /// <param name="dsFullName"></param>
        internal static void RemoveDeletedDistrictStyleFromTransients(string dsFullName)
        {
            Logging.DebugLog($"Remove {dsFullName} from transient styles");

            var districtIds = GetDistrictIdsByDSFullName(dsFullName);
            
            // no transient style found, nothing to do
            if (districtIds.Count == 0) return;

            foreach (var districtId in districtIds)
            {
                var styleNames = GetSelectedStylesForDistrict(districtId);
                styleNames.Remove(dsFullName);
                SetSelectedStylesForDistrict(districtId, styleNames);
            }
        }
        
        private static DistrictStyle CreateTransientStyle(string transientStyleName)
        {
            var transientStyle = new DistrictStyle(transientStyleName, false)
            {
                PackageName = TransientStylePackage
            };
            var styles = Singleton<DistrictManager>.instance.m_Styles.AddItem(transientStyle);
            Singleton<DistrictManager>.instance.m_Styles = styles.ToArray();
            Singleton<DSPBuildingManager>.instance.RefreshStylesInBuildingManager();
            return transientStyle;
        }

        private static void MergeDistrictStylesToTransientStyle
            (HashSet<string> dsStyleFullNames, DistrictStyle transientStyle)
        {
            Logging.DebugLog($"Merge styles {string.Join(", ", dsStyleFullNames.ToArray())} " +
                             $"for transient style {transientStyle.Name}");
            
            // buildingInfos for transient style
            var buildingInfos = new HashSet<BuildingInfo>();
            Logging.DebugLog($"Building Hash set up. Size {buildingInfos.Count}");

            if (dsStyleFullNames != null && dsStyleFullNames.Count > 0)
            {
                foreach (var dsStyleFullName in dsStyleFullNames)
                {
                    var districtStyle = DSPDistrictStyleManager.GetDistrictStyleByFullName(dsStyleFullName);
                    
                    if (districtStyle?.GetBuildingInfos() == null 
                        || districtStyle.GetBuildingInfos().Length == 0) continue;
                    
                    buildingInfos.UnionWith(districtStyle.GetBuildingInfos()); 
                    Logging.DebugLog($"Building Hash modified by {districtStyle.Name}. " +
                                     $"Size {buildingInfos.Count}");
                }
            }
            
            // Update building infos in transient style
            UpdateBuildingInfosInTransientStyle(buildingInfos, transientStyle, true);
        }

        internal static void GetTransientStyleNames(
            byte districtId, out string transientStyleName, out string transientStyleFullName)
        {
            transientStyleName = TransientStylePrefix + districtId;
            transientStyleFullName = TransientStylePackage + "." + transientStyleName;
        }

        /// <summary>
        /// Update building infos in transient district style
        /// </summary>
        /// <param name="buildingInfos"></param>
        /// <param name="transientStyle"></param>
        /// <param name="refreshStylesInBuildingManager"></param>
        private static void UpdateBuildingInfosInTransientStyle(
            HashSet<BuildingInfo> buildingInfos, DistrictStyle transientStyle, bool refreshStylesInBuildingManager)
        {
            Logging.DebugLog($"Update buildings transient style {transientStyle.Name}");
            
            // get buildingInfo field from transient district style
            var buildingInfosFieldInfo = 
                DSPDistrictStyleManager.GetDistrictStyleClassField("m_Infos", transientStyle);
            if (buildingInfosFieldInfo == null)
            {
                Logging.ErrorLog("District style does not have field m_Infos!");
                return;
            }
            
            // set new building info hash set in district style object
            buildingInfosFieldInfo.SetValue(transientStyle, buildingInfos);
            
            // refresh affected services of transient style
            DSPDistrictStyleManager.RefreshDistrictStyleAffectedService(transientStyle);
            
            // refresh styles in BuildingManager
            if (refreshStylesInBuildingManager) DSPBuildingManager.instance.RefreshStylesInBuildingManager();
        }

        /// <summary>
        /// Called when district style buildingInfos changed. Find if any transient style is using that DS and update it
        /// accordingly.
        /// </summary>
        /// <param name="buildingInfos">New building to be added to transient style</param>
        /// <param name="styleFullName">Changed district style</param>
        /// <param name="adding">true if adding buildings, false if removing buildings</param>
        internal static void ModifyTransientStylesByDSBuildingInfos(
            List<BuildingInfo> buildingInfos, string styleFullName, bool adding)
        {
            Logging.DebugLog($"Modify transient styles by {styleFullName} action add {adding}");
            
            // get district ids of ones which use provided district style in transient style
            var districtIds = GetDistrictIdsByDSFullName(styleFullName);

            // no transient style found, nothing to do
            if (districtIds.Count == 0) return;

            foreach (var districtId in districtIds)
            {
                GetTransientStyleNames(districtId, out var transientStyleName, out var transientStyleFullName);
                var transientStyle = DSPDistrictStyleManager.GetDistrictStyleByFullName(transientStyleFullName);
                var newBuildingInfos = new HashSet<BuildingInfo>();
                if (transientStyle?.GetBuildingInfos() != null && transientStyle.GetBuildingInfos().Length > 0)
                    newBuildingInfos.UnionWith(transientStyle.GetBuildingInfos());
                if (adding)
                {
                    Logging.DebugLog($"Add new buildings to transient style {transientStyleName}");
                    newBuildingInfos.UnionWith(buildingInfos);
                }
                else
                {
                    Logging.DebugLog($"Remove buildings from transient style {transientStyleName}");
                    newBuildingInfos.ExceptWith(buildingInfos);
                }
                UpdateBuildingInfosInTransientStyle(newBuildingInfos, transientStyle, false);
            }
        }

        /// <summary>
        /// Returns districtIds for districts which are using given district style
        /// </summary>
        /// <param name="styleFullName"></param>
        /// <returns></returns>
        private static List<byte> GetDistrictIdsByDSFullName(string styleFullName)
        {
            return TransientDistrictStyleConfigs
                .Where(pair => pair.Value != null && pair.Value.Contains(styleFullName))
                .Select(pair => pair.Key)
                .ToList();
        }

        /// <summary>
        /// Get data about transient styles for a district when saving a game.
        /// </summary>
        /// <param name="districtId">given district id</param>
        /// <returns></returns>
        internal static HashSet<string> GetStylesToSave(byte districtId)
        {
            return !TransientDistrictStyleConfigs.ContainsKey(districtId) ? 
                null : new HashSet<string>(TransientDistrictStyleConfigs.GetValueSafe(districtId));
        }

        /// <summary>
        /// Apply data from save game to create transient styles
        /// </summary>
        internal static void LoadDataFromSave()
        {
            var data = Serializer.GetSavedData();
            
            Logging.DebugLog("Apply saved DSP data.");
            
            for (var i = 0; i < data.Length; i++)
            {
                var districtId = (byte) i;

                if (DistrictManager.instance.m_districts.m_buffer[i].m_flags == District.Flags.None) continue;

                var transientDistrictStyle = data[i];
                if (transientDistrictStyle != null && transientDistrictStyle.StyleFullNames.Count > 0) {
                    SetSelectedStylesForDistrict(districtId, transientDistrictStyle.StyleFullNames);
                }
            }
            
            Logging.DebugLog("DSP data loaded!");
            
        }

    }
}