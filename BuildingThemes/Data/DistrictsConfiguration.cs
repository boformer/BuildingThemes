using System.Collections.Generic;

namespace BuildingThemes.Data
{
    public class DistrictsConfiguration
    {

        public class District
        {
            public byte id;
            public bool blacklistMode = false;
            public string[] themes;
        }

        public List<District> Districts = new List<District>();
    }
}