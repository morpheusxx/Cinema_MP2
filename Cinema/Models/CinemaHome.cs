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
using Cinema.GoogleMovies;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Workflow;

namespace Cinema.Models
{
  public class CinemaHome : IWorkflowModel
  {
    #region Consts

    public const string MODEL_ID_STR = "78E0D999-D87A-4340-B8D1-9CF97814D2FD";
    public const string NAME = "name";

    #endregion

    private CinemaDataList _dataList;
    public ItemsList Cinemas = new ItemsList();
    public ItemsList Movies = new ItemsList();

    public void Init()
    {
      _dataList = new CinemaDataList();
      GoogleMovies.GoogleMovies.FillDataList();
      _dataList = GoogleMovies.GoogleMovies.DataList;

      AddAllCinemas();
    }

    private void AddAllCinemas()
    {
      Cinemas.Clear();
      var oneItemSelected = false;
      foreach (var cd in _dataList.Datalist)
      {
        var item = new ListItem();
        item.AdditionalProperties[NAME] = cd.Current.Id;
        item.SetLabel("Name", cd.Current.Name + " - " + cd.Current.Address);
        Cinemas.Add(item);
        if (oneItemSelected) continue;
        SelectCinema(item);
        oneItemSelected = true;
      }
    }

    public void SelectCinema(ListItem item)
    {
      var id = (string)item.AdditionalProperties[NAME];
      foreach (var cd in _dataList.Datalist.Where(cd => cd.Current.Id == id))
      {
        AddMoviesByCinema(cd.Current);
      }
    }

    private void AddMoviesByCinema(GoogleMovies.Cinema cinema)
    {
      Movies.Clear();
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
        Movies.Add(item);
      }
      Movies.FireChange();
    }

    private static String ShowtimesByCinemaMovieDay(GoogleMovies.Cinema cinema, Movie movie, int day)
    {
      var st = GoogleMovies.GoogleMovies.GetShowTimesByCinemaAndMovieAndDay(cinema, movie, day).Aggregate("", (current, s) => current + (s + " | "));
      return GoogleMovies.GoogleMovies.GetNewDay(day) + ": " + st;
    }

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