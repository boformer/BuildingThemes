using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using UnityEngine;

namespace BuildingThemes
{
    public class Configuration
    {
        public bool UnlockPolicyPanel = true;

        public bool CreateBuildingDuplicates = true;
        
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

            [XmlIgnoreAttribute]
            public Dictionary<string, string> upgrades = new Dictionary<string, string>();

            [XmlArray(ElementName = "Buildings")]
            [XmlArrayItem(ElementName = "Building")]
            public List<Building> buildings = new List<Building>();

            public void addAll(string[] buildingNames, bool builtIn)
            {
                foreach (string b in buildingNames)
                {
                    if (!containsBuilding(b))
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

            public Building getBuilding(string name)
            {
                foreach (Building building in buildings)
                {
                    if (building.name == name) return building;
                }
                return null;
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

            [XmlIgnoreAttribute]
            public bool notInLevelRange = false;

            [XmlAttribute("min-level"), DefaultValue(-1)]
            public int minLevel = -1;

            [XmlAttribute("max-level"), DefaultValue(-1)]
            public int maxLevel = -1;

            [XmlAttribute("include"), DefaultValue(true)]
            public bool include = true;

            public Building(string name)
            {
                this.name = name;
            }

            public Building(string name, bool isBuiltIn)
            {
                this.name = name;
                this.isBuiltIn = isBuiltIn;
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
                Debugger.Log("Couldn't load configuration (XML malformed?)");
                throw e;
            }
        }

        public static void Serialize(string filename, Configuration config)
        {
            var xmlSerializer = new XmlSerializer(typeof(Configuration));
            try
            {
                using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(filename))
                {
                    var configCopy = new Configuration();

                    configCopy.UnlockPolicyPanel = config.UnlockPolicyPanel;
                    configCopy.CreateBuildingDuplicates = config.CreateBuildingDuplicates;

                    foreach (var theme in config.themes)
                    {
                        var newTheme = new Theme(theme.name);

                        foreach (var building in theme.buildings.Where(building => !theme.isBuiltIn || !building.isBuiltIn || !building.include))
                        {
                            newTheme.buildings.Add(building);
                        }

                        if (!theme.isBuiltIn || newTheme.buildings.Count > 0)
                        {
                            configCopy.themes.Add(newTheme);
                        }
                    }

                    xmlSerializer.Serialize(streamWriter, configCopy);
                }
            }
            catch (Exception e)
            {
                Debugger.Log("Couldn't create configuration file at \"" + Directory.GetCurrentDirectory() + "\"");
                throw e;
            }
        }
    }
}
