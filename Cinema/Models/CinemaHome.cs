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

using System;
using System.Collections.Generic;
using System.Linq;
using Cinema.Dialoges;
using Cinema.GoogleMovies;
using Cinema.Helper;
using Cinema.Player;
using Cinema.Settings;
using MediaPortal.Common;
using MediaPortal.Common.General;
using MediaPortal.Common.Settings;
using MediaPortal.Extensions.UserServices.FanArtService.Client.Models;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Workflow;
using MediaPortal.UI.SkinEngine.Controls.ImageSources;

namespace Cinema.Models
{
  public class CinemaHome : IWorkflowModel
  {
    #region Consts

    public const string MODEL_ID_STR = "78E0D999-D87A-4340-B8D1-9CF97814D2FD";
    public const string NAME = "name";
    public const string TRAILER = "trailer";

    #endregion

    public ItemsList Cinemas = new ItemsList();
    public static ItemsList Movies = new ItemsList();

    #region Propertys

    public static readonly AbstractProperty _selectedCinema = new WProperty(typeof(string), string.Empty);

    public AbstractProperty SelectedCinemaProperty
    {
      get { return _selectedCinema; }
    }

    public static string SelectedCinema
    {
      get { return (string)_selectedCinema.GetValue(); }
      set { _selectedCinema.SetValue(value); }
    }

    #endregion

    private static readonly ISettingsManager SETTINGS_MANAGER = ServiceRegistration.Get<ISettingsManager>();

    #region public Methods

    public static void SelectCinema(string id)
    {
      foreach (var cd in GoogleMovies.GoogleMovies.DataList.Datalist.Where(cd => cd.Current.Id == id))
      {
        SelectedCinema = cd.Current.Name;
        AddMoviesByCinema(cd.Current);
      }
    }

    public static void SelectMovie(ListItem item)
    {
      var t = new Trailer { Title = (string)item.AdditionalProperties[NAME], Url = (string)item.AdditionalProperties[TRAILER] };
      if (t.Url != "")
      {
        CinemaPlayerHelper.PlayStream(t);
      }
    }

    public static void AddMoviesByCinema(GoogleMovies.Cinema cinema)
    {
      Movies.Clear();
      var oneItemSelected = false;

      var ml = GoogleMovies.GoogleMovies.GetMoviesByCinema(cinema);

      foreach (var m in ml)
      {
        var item = new ListItem();
        item.AdditionalProperties[NAME] = m.Title;
        item.SetLabel("Name", m.Title);

        for (var i = 0; i <= 3; i++)
        {
          item.SetLabel("Day" + Convert.ToString(i), ShowtimesByCinemaMovieDay(cinema, m, i));
        }

        var mm = SETTINGS_MANAGER.Load<Movies>().MovieList;

        foreach (var t in mm)
        {
          if (t.Title == m.Title)
          {
            item.SetLabel("Poster", t.Poster);
            item.SetLabel("Picture", t.Picture);
            item.SetLabel("Description", t.Description);
            item.SetLabel("Year", t.Year);
            item.SetLabel("AgeLimit", t.AgeLimit);
            item.SetLabel("Genre", t.Genre);
            item.AdditionalProperties[TRAILER] = t.Trailer;
          }
        }

        item.SetLabel("Duration", m.Runtime);

        Movies.Add(item);
        //if (oneItemSelected) continue;
        //SelectMovie(item);
        //oneItemSelected = true;
      }
      Movies.FireChange();
    }

    public void SetSelectedItem(ListItem selectedItem)
    {
      var fanArtBgModel = (FanArtBackgroundModel)ServiceRegistration.Get<IWorkflowManager>().GetModel(FanArtBackgroundModel.FANART_MODEL_ID);
      if (fanArtBgModel != null)
      {
        string uriSource = null;
        if (selectedItem != null)
          uriSource = selectedItem.Labels["Picture"].ToString();

        fanArtBgModel.ImageSource = uriSource != null ? new MultiImageSource { UriSource = uriSource } : null;
      }
    }

    public static void MakeUpdate()
    {
      ServiceRegistration.Get<IWorkflowManager>().NavigatePush(new Guid("48FE28A6-868D-4531-BF2F-1E746769B177"));
      DlgUpdate.MakeUpdate();
    }

    #endregion

    #region private Methods

    private static void Init()
    {
      CkeckUpdate();

      if (GoogleMovies.GoogleMovies.DataList.Datalist != null)
      {
        SelectCinema(GoogleMovies.GoogleMovies.DataList.Datalist[0].Current.Id);
      }
    }

    private static void CkeckUpdate()
    {
      var dt1 = Convert.ToDateTime(SETTINGS_MANAGER.Load<Settings.CinemaSettings>().LastUpdate);
      var dt = DateTime.Now - dt1;
      // Is it a New Day ?
      if (dt > new TimeSpan(1, 0, 0, 0))
      {
        MakeUpdate();
      }
      else if (SETTINGS_MANAGER.Load<Locations>().Changed)
      {
        MakeUpdate();
      }
      else
      {
        GoogleMovies.GoogleMovies.DataList = SETTINGS_MANAGER.Load<Datalist>().CinemaDataList;
      }
    }

    private static String ShowtimesByCinemaMovieDay(GoogleMovies.Cinema cinema, Movie movie, int day)
    {
      var st = GoogleMovies.GoogleMovies.GetShowTimesByCinemaAndMovieAndDay(cinema, movie, day).Aggregate("", (current, s) => current + (s + " | "));
      return GoogleMovies.GoogleMovies.GetNewDay(day) + ": " + st;
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