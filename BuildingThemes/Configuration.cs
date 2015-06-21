using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using UnityEngine;

namespace BuildingThemes
{
    public class Configuration
    {
        [XmlArray(ElementName = "Themes")]
        [XmlArrayItem(ElementName = "Theme")]
        public List<Theme> themes = new List<Theme>();

        public Theme getTheme(string name)
        {
            foreach (Theme theme in themes)
            {
                if (theme.name == name) return theme;
            }
            return null;
        }

        public class Theme
        {
            [XmlAttribute("name")]
            public string name;

            [XmlIgnoreAttribute]
            public bool isBuiltIn = false;

            [XmlArray(ElementName = "Buildings")]
            [XmlArrayItem(ElementName = "Building")]
            public List<Building> buildings = new List<Building>();

            public void addAll(string[] buildingNames, bool builtIn)
            {
                foreach (string b in buildingNames)
                {
                    if(!containsBuilding(b))
                    {
                        buildings.Add(new Building(b, builtIn));
                    }
                }
            }
            public bool containsBuilding(string name)
            {
                foreach (Building building in buildings)
                {
                    if (building.name == name) return true;
                }
                return false;
            }

            public Theme(string name)
            {
                this.name = name;
            }

            public Theme()
            {
            }
        }

        public class Building
        {
            [XmlAttribute("name")]
            public string name;

            [XmlIgnoreAttribute]
            public bool isBuiltIn = false;

            public Building(string name)
            {
                this.name = name;
            }

            public Building(string name, bool builtIn)
            {
                this.name = name;
                this.isBuiltIn = true;
            }

            public Building()
            {
            }
        }

        public static Configuration Deserialize(string filename)
        {
            if (!File.Exists(filename)) return null;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Configuration));
            try
            {
                using (System.IO.StreamReader streamReader = new System.IO.StreamReader(filename))
                {
                    return (Configuration)xmlSerializer.Deserialize(streamReader);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't load configuration (XML malformed?)");
                throw e;
            }
        }

        public static void Serialize(string filename, Configuration config)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Configuration));
            try
            {
                using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(filename))
                {
                    Configuration configCopy = new Configuration();
                    foreach (var theme in config.themes)
                    {
                        if (!theme.isBuiltIn)
                        {
                            Theme newTheme = new Theme(theme.name);

                            foreach (var building in theme.buildings)
                            {
                                if (!building.isBuiltIn) 
                                {
                                    newTheme.buildings.Add(building);
                                }
                            }
                            
                            configCopy.themes.Add(newTheme);
                        }
                    }

                    xmlSerializer.Serialize(streamWriter, configCopy);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't create configuration file at \"" + Directory.GetCurrentDirectory() + "\"");
                throw e;
            }
        }

        public static void addBuiltInEuropeanTheme(Configuration config)
        {
            var theme = config.getTheme("European");

            if(theme == null) 
            {
                theme = new Theme("European");
                //theme.isBuiltIn = true;
                config.themes.Add(theme);
            }
            theme.addAll(sharedBuildings, true);
            theme.addAll(euroOnlyBuildings, true);
        }

        public static void addBuiltInInternationalTheme(Configuration config)
        {
            var theme = config.getTheme("International");

            if (theme == null)
            {
                theme = new Theme("International");
                //theme.isBuiltIn = true;
                config.themes.Add(theme);
            }
            //intTheme.isBuiltIn = true;
            theme.addAll(sharedBuildings, true);
            theme.addAll(intOnlyBuildings, true);
        }

        private static string[] sharedBuildings = {
                                         "Agricultural 1x1 processing 1",
                                         "Agricultural 2x2 processing 2",
                                         "Agricultural 3x2 processing 2",
                                         "Agricultural 3x3 Processing 03",
                                         "Agricultural 3x3 Processing 04",
                                         "Agricultural 4x4 Processing 01",
                                         "Agricultural 4x4 Processing 02",
                                         "agricultural_building_05",
                                         "cargoyard",
                                         "Farming 4x4 Farm",
                                         "Farming 4x4 Farm 2",
                                         "Farming 4x4 Farm 3",
                                         "Farming2x2",
                                         "Farming3x2",
                                         "Farming4x4",
                                         "Farming4x4_02",
                                         "Farming4x4_03",
                                         "Forestry 1x1",
                                         "Forestry 2x2",
                                         "Forestry 3x3 Extractor",
                                         "Forestry 3x3 Forest",
                                         "Forestry 3x3 Processing",
                                         "Forestry 3x3 Processing 2",
                                         "Forestry 4x3 Processing",
                                         "Forestry 4x4 Forest",
                                         "Forestry 4x4 Forest 1",
                                         "Forestry1x1 forest",
                                         "Forestry2x2 forest",
                                         "H1 1x1 Facility01",
                                         "H1 1x1 Facility02",
                                         "H1 1x1 FarmingFacility03",
                                         "H1 2x2 Sweatshop03",
                                         "H1 2x2 Sweatshop05",
                                         "H1 2x2 Sweatshop06",
                                         "H1 3x3 Sweatshop04",
                                         "H1 3x3 Sweatshop07",
                                         "H1 4x3 Bigfactory05",
                                         "H1 4x4 Bigfactory01",
                                         "H1 4x4 Mediumfactory02",
                                         "H1 4x4 Mediumfactory03",
                                         "H1 4x4 Sweatshop02",
                                         "H2 1x1 Facility03",
                                         "H2 1x1 Facility04",
                                         "H2 2x2 sweatshop01",
                                         "H2 3x3 Sweatshop04",
                                         "H2 4x3 Sweatshop01",
                                         "H2 4x4 Bigfactory02",
                                         "H2 4x4 Bigfactory07",
                                         "H2 4x4 Sweatshop04",
                                         "H3 1x1 Facility05",
                                         "H3 1x1 Facility06",
                                         "H3 2x2 Bigfactory06",
                                         "H3 3x3 Mediumfactory08",
                                         "H3 4x3 Bigfactory06",
                                         "H3 4x4 Bigfactory 04",
                                         "H3 4x4 Mediumfactory06",
                                         "L1 1x1 Detached",
                                         "L1 1x1 Shop",
                                         "L1 1x2 Shop04",
                                         "L1 2x2 Detached01",
                                         "L1 2x2 Detached03",
                                         "L1 2x2 Detached04",
                                         "L1 2x2 Detached06",
                                         "L1 2x2 Shop02",
                                         "L1 2x2 Shop03",
                                         "L1 2x2 Shop04",
                                         "L1 2x3 Detached03",
                                         "L1 2x3 Detached05",
                                         "L1 3x2 Detached04",
                                         "L1 3x2 Shop01",
                                         "L1 3x2 Shop03b",
                                         "L1 3x2 Shop05",
                                         "L1 3x3 Detached 1a",
                                         "L1 3x3 Detached02",
                                         "L1 3x3 Shop07",
                                         "L1 3x4 detached04",
                                         "L1 3x4 Detached04a",
                                         "L1 3x4 Detached07a",
                                         "L1 3x4 Shop05",
                                         "L1 4x3 detached04",
                                         "L1 4x3 Detached05",
                                         "L1 4x3 Shop02a",
                                         "L1 4x3 Shop03b 1a",
                                         "L1 4x3 Shop06",
                                         "L1 4x3 Shop06a",
                                         "L1 4x4 Detached02",
                                         "L1 4x4 Detached06a",
                                         "L1 4x4 Shop08a",
                                         "L2 1x1 detached01",
                                         "L2 1x1 Shop08",
                                         "L2 1x2 Shop07",
                                         "L2 2x2 Detached01",
                                         "L2 2x2 Detached05",
                                         "L2 2x2 Shop2",
                                         "L2 2x3 Detached01",
                                         "L2 2x3 Detached03",
                                         "L2 2x3 Semi-detachedhouse01",
                                         "L2 3x2 Detached01",
                                         "L2 3x2 Shop09",
                                         "L2 3x3 Detached02",
                                         "L2 3x3 Shop03",
                                         "L2 3x4 Detached02",
                                         "L2 3x4 Detached02a",
                                         "L2 3x4 Detached04a",
                                         "L2 3x4 Semi-detachedhouse02a",
                                         "L2 3x4 Shop04",
                                         "L2 4x3 Detached02",
                                         "L2 4x3 Detached02a",
                                         "L2 4x3 Shop05",
                                         "L2 4x3 Shop06",
                                         "L2 4x3 Shop10a",
                                         "L2 4x4 Detached04",
                                         "L2 4x4 Shop04",
                                         "L3 1x1 Detached",
                                         "L3 1x1 Shop",
                                         "L3 1x1 Shop07",
                                         "L3 1x2 Shop07",
                                         "L3 2x2 Detached04",
                                         "L3 2x2 Detached05",
                                         "L3 2x2 Shop03",
                                         "L3 2x2 Shop11",
                                         "L3 2x3 Detached02",
                                         "L3 2x3 Semi-detachedhouse02",
                                         "L3 3x2 Detached03",
                                         "L3 3x2 Detached06",
                                         "L3 3x2 Shop11",
                                         "L3 3x2 Shop12",
                                         "L3 3x3 Semi-detachedhouse02",
                                         "L3 3x3 Shop05",
                                         "L3 3x3 Shop06",
                                         "L3 3x4 detached01",
                                         "L3 3x4 Detached03",
                                         "L3 3x4 Semi-detachedhouse03a",
                                         "L3 3x4 Shop03",
                                         "L3 4x3 Detached01",
                                         "L3 4x3 Detached04a",
                                         "L3 4x3 Shop10",
                                         "L3 4x3 Shop13a",
                                         "L3 4x4 Detached07",
                                         "L3 4x4 Semi-detachedhouse03a",
                                         "L3 4x4 Shop07a",
                                         "L4 1x1 Villa04",
                                         "L4 2x2 Villa02",
                                         "L4 2x2 Villa07",
                                         "L4 2x3 Villa04",
                                         "L4 3x2 Villa02",
                                         "L4 3x2 Villa02b",
                                         "L4 3x2 Villa04",
                                         "L4 3x3 Villa05",
                                         "L4 3x4 Villa01",
                                         "L4 4x3 Villa01",
                                         "L4 4x4 Villa06a",
                                         "L4 4x4 Villa08a",
                                         "L5 1x1 Detached",
                                         "L5 1x1 DetachedEF",
                                         "L5 2x2 Villa07",
                                         "L5 2x2 Villa09",
                                         "L5 2x3 Villa07",
                                         "L5 3x2 Villa06",
                                         "L5 3x3 Villa01",
                                         "L5 3x3 Villa02",
                                         "L5 3x3 Villa08",
                                         "L5 3x4 Villa05",
                                         "L5 4x2 Villa04",
                                         "L5 4x3 Villa05",
                                         "L5 4x3 Villa07",
                                         "L5 4x4 Villa03",
                                         "L5 4x4 Villa08a",
                                         "Oil 1x1 processing",
                                         "Oil 2x2 Processing",
                                         "Oil 3x2 Processing",
                                         "Oil 3x3 Extractor",
                                         "Oil 3x3 Processing",
                                         "Oil 4x4 Processing",
                                         "Oil 4x4 Processing02",
                                         "Oil2x2",
                                         "Ore 1x1 processing",
                                         "Ore 2x2 Extractor",
                                         "Ore 2x2 Processing",
                                         "Ore 3x2 Processing",
                                         "Ore 3x3 Processing",
                                         "Ore 4x3 Processing",
                                         "Ore 4x3 Processing02",
                                         "Ore 4x4 Extractor",
                                         "Ore1x1",
                                         "OreCrusher" };
        private static string[] euroOnlyBuildings = {
                                         "1x1_Factory_EU01",
                                         "2X2_Factory_EU04",
                                         "3x3_Factory_EU05",
                                         "3x3_Factory_EU06",
                                         "3x3_Factory_EU07",
                                         "3x3_Factory_EU09",
                                         "4x3_Factory_EU08",
                                         "4x3_Factory_EU10",
                                         "4x4_Factory_EU02",
                                         "4x4_Factory_EU03",
                                         "H1_1x1_Blockhouse",
                                         "H1_1x1_CommercialBlockhouse",
                                         "H1_1x1_Officeblock",
                                         "H1_2x2_Blockhouse",
                                         "H1_2x2_Blockhouse_Corner",
                                         "H1_2x2_CommercialBlockhouse",
                                         "H1_2x2_CommercialBlockhouse_Corner",
                                         "H1_2x2_Officeblock",
                                         "H1_2x2_Officeblock_Corner",
                                         "H1_2x3_Blockhouse",
                                         "H1_2x3_Blockhouse_Corner",
                                         "H1_2x3_CommercialBlockhouse",
                                         "H1_2x3_CommercialBlockhouse_Corner",
                                         "H1_2x3_Officeblock_Corner",
                                         "H1_2x4_Blockhouse",
                                         "H1_2x4_CommercialBlockhouse",
                                         "H1_2x4_Officeblock",
                                         "H1_3x2_Blockhouse",
                                         "H1_3x2_Blockhouse_Corner",
                                         "H1_3x2_Blockhouse01",
                                         "H1_3x2_CommercialBlockhouse",
                                         "H1_3x2_CommercialBlockhouse_Corner",
                                         "H1_3x2_CommercialBlockhouse01",
                                         "H1_3x2_Officeblock_Corner",
                                         "H1_3x3_Blockhouse",
                                         "H1_3x3_Blockhouse_Corner",
                                         "H1_3x3_CommercialBlockhouse",
                                         "H1_3x3_CommercialBlockhouse_Corner",
                                         "H1_3x3_Officeblock",
                                         "H1_3x3_Officeblock_Corner",
                                         "H1_3x4_Blockhouse",
                                         "H1_3x4_CommercialBlockhouse",
                                         "H1_3x4_Officeblock",
                                         "H1_4x3_Blockhouse",
                                         "H1_4x3_CommercialBlockhouse",
                                         "H1_4x3_officeblock",
                                         "H1_4x4_Blockhouse",
                                         "H1_4x4_CommercialBlockhouse",
                                         "H1_officebuilding02EU_4x4",
                                         "H1_officebuilding02EU_4x4b",
                                         "H1_officebuilding04EU_4x4",
                                         "H2_1x1_Blockhouse",
                                         "H2_1x1_CommercialBlockhouse",
                                         "H2_1x1_Officeblock",
                                         "H2_2x2_Blockhouse",
                                         "H2_2x2_Blockhouse_Corner",
                                         "H2_2x2_CommercialBlockhouse",
                                         "H2_2x2_CommercialBlockhouse_Corner",
                                         "H2_2x2_Officeblock",
                                         "H2_2x2_Officeblock_Corner",
                                         "H2_2x3_Blockhouse",
                                         "H2_2x3_Blockhouse_Corner",
                                         "H2_2x3_CommercialBlockhouse",
                                         "H2_2x3_CommercialBlockhouse_Corner",
                                         "H2_2x3_Officeblock_Corner",
                                         "H2_2x4_Blockhouse",
                                         "H2_2x4_CommercialBlockhouse",
                                         "H2_2x4_Officeblock",
                                         "H2_3x2_Blockhouse",
                                         "H2_3x2_Blockhouse_Corner",
                                         "H2_3x2_Blockhouse01",
                                         "H2_3x2_CommercialBlockhouse",
                                         "H2_3x2_CommercialBlockhouse_Corner",
                                         "H2_3x2_CommercialBlockhouse01",
                                         "H2_3x2_Officeblock_Corner",
                                         "H2_3x3_Blockhouse",
                                         "H2_3x3_Blockhouse_Corner",
                                         "H2_3x3_CommercialBlockhouse",
                                         "H2_3x3_CommercialBlockhouse_Corner",
                                         "H2_3x3_Officeblock",
                                         "H2_3x3_Officeblock_Corner",
                                         "H2_3x4_Blockhouse",
                                         "H2_3x4_CommercialBlockhouse",
                                         "H2_3x4_Officeblock",
                                         "H2_4x3_Blockhouse",
                                         "H2_4x3_CommercialBlockhouse",
                                         "H2_4x3_officeblock",
                                         "H2_4x4_Blockhouse",
                                         "H2_4x4_CommercialBlockhouse",
                                         "H2_officebuilding02EU_4x4",
                                         "H2_officebuilding02EU_4x4b",
                                         "H2_officebuilding04EU_4x4",
                                         "H3_1x1_Blockhouse",
                                         "H3_1x1_CommercialBlockhouse",
                                         "H3_1x1_Officeblock",
                                         "H3_2x2_Blockhouse",
                                         "H3_2x2_Blockhouse_Corner",
                                         "H3_2x2_CommercialBlockhouse",
                                         "H3_2x2_CommercialBlockhouse_Corner",
                                         "H3_2x2_Officeblock",
                                         "H3_2x2_Officeblock_Corner",
                                         "H3_2x3_Blockhouse",
                                         "H3_2x3_Blockhouse_Corner",
                                         "H3_2x3_CommercialBlockhouse",
                                         "H3_2x3_CommercialBlockhouse_Corner",
                                         "H3_2x3_Officeblock_Corner",
                                         "H3_2x4_Blockhouse",
                                         "H3_2x4_CommercialBlockhouse",
                                         "H3_2x4_Officeblock",
                                         "H3_3x2_Blockhouse",
                                         "H3_3x2_Blockhouse_Corner",
                                         "H3_3x2_Blockhouse01",
                                         "H3_3x2_CommercialBlockhouse",
                                         "H3_3x2_CommercialBlockhouse_Corner",
                                         "H3_3x2_CommercialBlockhouse01",
                                         "H3_3x2_Officeblock_Corner",
                                         "H3_3x3_Blockhouse",
                                         "H3_3x3_Blockhouse_Corner",
                                         "H3_3x3_CommercialBlockhouse",
                                         "H3_3x3_CommercialBlockhouse_Corner",
                                         "H3_3x3_Officeblock",
                                         "H3_3x3_Officeblock_Corner",
                                         "H3_3x4_Blockhouse",
                                         "H3_3x4_CommercialBlockhouse",
                                         "H3_3x4_Officeblock",
                                         "H3_4x3_Blockhouse",
                                         "H3_4x3_CommercialBlockhouse",
                                         "H3_4x3_officeblock",
                                         "H3_4x4_Blockhouse",
                                         "H3_4x4_CommercialBlockhouse",
                                         "H3_officebuilding02EU_4x4",
                                         "H3_officebuilding02EU_4x4b",
                                         "H3_officebuilding04EU_4x4",
                                         "H4_1x1_Blockhouse",
                                         "H4_2x2_Blockhouse",
                                         "H4_2x2_Blockhouse_Corner",
                                         "H4_2x3_Blockhouse",
                                         "H4_2x3_Blockhouse_Corner",
                                         "H4_2x4_Blockhouse",
                                         "H4_3x2_Blockhouse",
                                         "H4_3x2_Blockhouse_Corner",
                                         "H4_3x2_Blockhouse01",
                                         "H4_3x3_Blockhouse",
                                         "H4_3x3_Blockhouse_Corner",
                                         "H4_3x4_Blockhouse",
                                         "H4_4x3_Blockhouse",
                                         "H4_4x4_Blockhouse",
                                         "H5_1x1_Blockhouse",
                                         "H5_2x2_Blockhouse",
                                         "H5_2x2_Blockhouse_Corner",
                                         "H5_2x3_Blockhouse",
                                         "H5_2x3_Blockhouse_Corner",
                                         "H5_2x4_Blockhouse",
                                         "H5_3x2_Blockhouse",
                                         "H5_3x2_Blockhouse_Corner",
                                         "H5_3x2_Blockhouse01",
                                         "H5_3x3_Blockhouse",
                                         "H5_3x3_Blockhouse_Corner",
                                         "H5_3x4_Blockhouse",
                                         "H5_4x3_Blockhouse",
                                         "H5_4x4_Blockhouse"
                                       };
        private static string[] intOnlyBuildings = {
                                         "H1 1x1 Shop01",
                                         "H1 1x1 Office",
                                         "H1 1x1 Shop07",
                                         "H1 1x1 Tenement",
                                         "H1 2x2 Office02",
                                         "H1 2x2 Office03",
                                         "H1 2x2 Office05",
                                         "H1 2x2 Shop01",
                                         "H1 2x2 Tenement05",
                                         "H1 2x3 Office01",
                                         "H1 2x3 Office07",
                                         "H1 2x3 Shop01",
                                         "H1 2x3 Tenement02",
                                         "H1 3x2 Office06",
                                         "H1 3x2 Shop03",
                                         "H1 3x2 Tenement01",
                                         "H1 3x2 Tenement04",
                                         "H1 3x3 Office04",
                                         "H1 3x3 Shop01",
                                         "H1 3x3 Shop04",
                                         "H1 3x3 Tenement03",
                                         "H1 3x3 Tenement08",
                                         "H1 3x4 Office01",
                                         "H1 3x4 Office02a",
                                         "H1 3x4 Office06",
                                         "H1 3x4 Office08a",
                                         "H1 3x4 Shop02a",
                                         "H1 3x4 Shop04",
                                         "H1 3x4 Tenement03a",
                                         "H1 3x4 Tenement07",
                                         "H1 4x3 Office06",
                                         "H1 4x3 Shop01",
                                         "H1 4x3 Shop02",
                                         "H1 4x3 Shop04a",
                                         "H1 4x3 Tenement02",
                                         "H1 4x3 Tenement05",
                                         "H1 4x3 Tenement07",
                                         "H1 4x4 Office05a",
                                         "H1 4x4 Office06",
                                         "H1 4x4 Shop03",
                                         "H1 4x4 Tenement03",
                                         "H1 4x4 Tenement04a",
                                         "H2 1x1 Office01",
                                         "H2 1x1 Shop",
                                         "H2 1x1 Tenement01",
                                         "H2 2x2 Office04",
                                         "H2 2x2 Office07",
                                         "H2 2x2 Office08",
                                         "H2 2x2 Shop01",
                                         "H2 2x2 tenement06",
                                         "H2 2x3 Office04",
                                         "H2 2x3 Office07",
                                         "H2 2x3 Shop03",
                                         "H2 2x3 Tenement01 ",
                                         "H2 3x2 Office08",
                                         "H2 3x2 Shop01",
                                         "H2 3x2 Tenement05",
                                         "H2 3x3 Office06",
                                         "H2 3x3 Office08",
                                         "H2 3x3 Shop02",
                                         "H2 3x3 Shop05",
                                         "H2 3x3 Tenement01",
                                         "H2 3x3 Tenement06",
                                         "H2 3x4 Office05a",
                                         "H2 3x4 Shop04a",
                                         "H2 3x4 Shop05",
                                         "H2 3x4 Tenement01 1a",
                                         "H2 3x4 Tenement06",
                                         "H2 4x3 Office06",
                                         "H2 4x3 Office09",
                                         "H2 4x3 Office09a",
                                         "H2 4x3 Shop02a",
                                         "H2 4x3 Shop06",
                                         "H2 4x3 Tenement06",
                                         "H2 4x3 Tenement06a",
                                         "H2 4x4 Office06",
                                         "H2 4x4 Shop06",
                                         "H2 4x4 Tenement02a",
                                         "H2 4x4 Tenement05b",
                                         "H2 4x4 Tenement07a",
                                         "H3 1x1 Office01",
                                         "H3 1x1 Shop10",
                                         "H3 1x1 Shop13",
                                         "H3 1x1 Tenement05",
                                         "H3 2x2 Office11",
                                         "H3 2x2 Shop01",
                                         "H3 2x2 Tenement04",
                                         "H3 2x3 Office10",
                                         "H3 2x3 Shop04",
                                         "H3 2x3 tenement02",
                                         "H3 3x2 Office01",
                                         "H3 3x2 Shop04",
                                         "H3 3x2 Tenement04a",
                                         "H3 3x3 Office10",
                                         "H3 3x3 Shop06",
                                         "H3 3x3 Tenement04",
                                         "H3 3x4 Hotel",
                                         "H3 3x4 Office08",
                                         "H3 3x4 Office11a",
                                         "H3 3x4 tenement03a",
                                         "H3 3x4 Tenement08",
                                         "H3 4x3 Office02a",
                                         "H3 4x3 Office03",
                                         "H3 4x3 Office04",
                                         "H3 4x3 Office06",
                                         "H3 4x3 Shop03",
                                         "H3 4x3 Shop05a",
                                         "H3 4x3 Tenement04a",
                                         "H3 4x3 Tenement08",
                                         "H3 4x4 Office01",
                                         "H3 4x4 Office02",
                                         "H3 4x4 Office05",
                                         "H3 4x4 Office07",
                                         "H3 4x4 Office12",
                                         "H3 4x4 Shop04",
                                         "H3 4x4 Shop07a",
                                         "H3 4x4 Shop11",
                                         "H3 4x4 Tenement05a",
                                         "H3 4x4 Tenement08b",
                                         "H4 1x1 Tenement06",
                                         "H4 2x2 Tenement09",
                                         "H4 2x3 tenement04",
                                         "H4 3x2 tenement07",
                                         "H4 3x3 Tenement08",
                                         "H4 3x4 tenement05a",
                                         "H4 3x4 Tenement07b",
                                         "H4 4x3 Tenement07",
                                         "H4 4x3 tenement08a",
                                         "H4 4x4 Tenement07b",
                                         "H4 4x4 Tenement09a",
                                         "H5 1x1 highrise_hiden_hightech01",
                                         "H5 2x2 tenement03",
                                         "H5 2x3 Tenement06",
                                         "H5 3x2 Highrise06",
                                         "H5 3x2 Tenement02",
                                         "H5 3x2 Tenement06",
                                         "H5 3x3 Tenement03",
                                         "H5 3x3 Tenement05",
                                         "H5 3x4 Highrise05",
                                         "H5 3x4 Tenement09",
                                         "H5 4x3 Highrise01",
                                         "H5 4x3 Highrise03",
                                         "H5 4x3 Highrise04",
                                         "H5 4x3 Highrise07a",
                                         "H5 4x3 Tenement04",
                                         "H5 4x4 Highrise02",
                                         "H5 4x4 Highrise07",
                                         "H5 4x4 Highrise08",
                                         "H5 4x4 Tenement01"
                                        };
    }
}
