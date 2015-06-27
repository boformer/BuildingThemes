using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;

namespace BuildingThemes
{
    public interface IBuildingThemeUserMod : IUserMod
    {
        IEnumerable<Configuration.Theme> Themes { get; }
    }
}
