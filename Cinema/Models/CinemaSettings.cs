﻿#region Copyright (C) 2007-2014 Team MediaPortal

/*
    Copyright (C) 2007-2014 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Linq;
using Cinema.Settings;
using MediaPortal.Common;
using MediaPortal.Common.General;
using MediaPortal.Common.Settings;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Workflow;

namespace Cinema.Models
{
  public class CinemaSettings : IWorkflowModel
  {
    #region Consts

    public const string MODEL_ID_STR = "23A02262-A337-4836-AAF5-E70DAD6AFDCC";
    public const string NAME = "name";
    public const string TEXT = "text";

    #endregion

    #region Vars

    public ItemsList Cinemas = new ItemsList();

    private static List<GoogleMovies.Cinema> _selectedCinemas = new List<GoogleMovies.Cinema>();
    private static List<GoogleMovies.Cinema> _allCinemas = new List<GoogleMovies.Cinema>();
    private Locations _lSettings = new Locations();

    #endregion

    #region Propertys

    private static readonly AbstractProperty _locationProperty = new WProperty(typeof(string), string.Empty);

    public AbstractProperty LocationProperty
    {
      get { return _locationProperty; }
    }

    public string Location
    {
      get { return (string)_locationProperty.GetValue(); }
      set { _locationProperty.SetValue(value); }
    }

    #endregion

    #region public Methods

    public void ReadCinemas()
    {
      Cinemas.Clear();
      AddSelectedCinemasToAllCinemas();

      foreach (var c in GoogleMovies.GoogleMovies.GetCinemas(Location).Where(IsCinemaNew))
      {
        _allCinemas.Add(c);
      }

      AddAllCinemas();
      Cinemas.FireChange();
    }

    public void Select(ListItem item)
    {
      item.Selected = !item.Selected;
      SetSelectedCinemas();
      item.FireChange();
    }

    #endregion

    #region private Methods

    private void Init()
    {
      var settingsManager = ServiceRegistration.Get<ISettingsManager>();
      _lSettings = settingsManager.Load<Locations>();
      if (_lSettings.LocationSetupList != null)
      {
        _selectedCinemas = _lSettings.LocationSetupList;
      }
      Cinemas.Clear();
      AddSelectedCinemasToAllCinemas();
      AddAllCinemas();
      Cinemas.FireChange();
    }

    private void AddSelectedCinemasToAllCinemas()
    {
      _allCinemas = new List<GoogleMovies.Cinema>();
      foreach (var c in _selectedCinemas)
      {
        _allCinemas.Add(c);
      }
    }

    private void AddAllCinemas()
    {
      foreach (var c in _allCinemas)
      {
        var item = new ListItem();
        item.AdditionalProperties[NAME] = c.Id;
        item.SetLabel("Text", c.Name + " - " + c.Address);
        if (IsCinemaSelected(c))
        {
          item.Selected = true;
        }
        Cinemas.Add(item);
      }
    }

    private static bool IsCinemaNew(GoogleMovies.Cinema cinema)
    {
      return _allCinemas.All(c => c.Id != cinema.Id);
    }

    private static bool IsCinemaSelected(GoogleMovies.Cinema cinema)
    {
      return _selectedCinemas.Any(ci => cinema.Id == ci.Id);
    }

    private void SetSelectedCinemas()
    {
      _selectedCinemas = new List<GoogleMovies.Cinema>();
      foreach (var c in from item in Cinemas where item.Selected from c in _allCinemas.Where(c => c.Id == (string)item.AdditionalProperties[NAME]) select c)
      {
        _selectedCinemas.Add(c);
      }
    }

    #endregion

    #region IWorkflowModel implementation

    public Guid ModelId
    {
      get { return new Guid(MODEL_ID_STR); }
    }

    public bool CanEnterState(NavigationContext oldContext, NavigationContext newContext)
    {
      return true;
    }

    public void EnterModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      Init();
    }

    public void ExitModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      // copy all CinemaIds in a List to compare with the old Cinemas
      var l = _selectedCinemas.Select(c => c.Id).ToList();
      var b = false;

      if (_lSettings.LocationSetupList != null)
      {
        // compare with old Cinemas
        b |= _lSettings.LocationSetupList.Any(c => !l.Contains(c.Id));

        // Cinema Added or Removed
        b |= _selectedCinemas.Count != _lSettings.LocationSetupList.Count;
      }
      else
        // If there was no old list
        b = true;

      // Save the Settings
      _lSettings.LocationSetupList = _selectedCinemas;
      _lSettings.Changed = b;
      ServiceRegistration.Get<ISettingsManager>().Save(_lSettings);
    }

    public void ChangeModelContext(NavigationContext oldContext, NavigationContext newContext, bool push)
    {
      // We could initialize some data here when changing the media navigation state
    }

    public void Deactivate(NavigationContext oldContext, NavigationContext newContext)
    {
    }

    public void Reactivate(NavigationContext oldContext, NavigationContext newContext)
    {
      // Todo: select any or the Last ListItem
    }

    public void UpdateMenuActions(NavigationContext context, IDictionary<Guid, WorkflowAction> actions)
    {
    }

    public ScreenUpdateMode UpdateScreen(NavigationContext context, ref string screen)
    {
      return ScreenUpdateMode.AutoWorkflowManager;
    }

    #endregion
  }
}