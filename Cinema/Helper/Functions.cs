#region Copyright (C) 2007-2013 Team MediaPortal

/*
    Copyright (C) 2007-2013 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using MediaPortal.Common;
using MediaPortal.Common.Localization;
using Previewnetworks_v31;

namespace Cinema.Helper
{
  public class Functions
  {
    public static Search.Country DefaultCountry()
    {
      var localization = ServiceRegistration.Get<ILocalization>();
      var re = localization.CurrentCulture.Name;

      if (re.Contains("de-CH"))
        return Search.Country.Switzerland_German;

      if (re.Contains("fr-CH"))
        return Search.Country.Switzerland_French;

      if (re.Contains("de-"))
        return Search.Country.Germany;

      if (re.Contains("en-"))
        return Search.Country.United_Kingdom;

      if (re.Contains("fr-"))
        return Search.Country.France;

      if (re.Contains("es-"))
        return Search.Country.Spain;

      if (re.Contains("it-"))
        return Search.Country.Italy;

      if (re.Contains("nl-"))
        return Search.Country.Netherlands;

      if (re.Contains("da-"))
        return Search.Country.Denmark;

      if (re.Contains("sv-"))
        return Search.Country.Sweden;

      if (re.Contains("fi-"))
        return Search.Country.Finland;

      return Search.Country.Germany;
    }
  }
}
