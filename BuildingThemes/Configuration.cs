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
        // TODO the xml configuration for the themes

        [XmlArray(ElementName = "Themes")]
        [XmlArrayItem(ElementName = "Theme")]
        public List<Theme> themes;

        public class Theme
        {
            public string name;

            [XmlArray(ElementName = "Buildings")]
            [XmlArrayItem(ElementName = "Building")]
            public List<Building> buildings;
        }

        public class Building
        {
            [XmlAttribute("name")]
            public string name;
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
                    xmlSerializer.Serialize(streamWriter, config);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't create configuration file at \"" + Directory.GetCurrentDirectory() + "\"");
                throw e;
            }
        }
    }
}
