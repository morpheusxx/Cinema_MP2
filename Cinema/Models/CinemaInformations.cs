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
using Cinema.Helper;
using Cinema.Player;
using Cinema.Previewnetworks;
using MediaPortal.Common.General;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Workflow;
using Previewnetworks_v31;

namespace Cinema.Models
{
  public class CinemaInformations :IWorkflowModel
  {
    #region Consts

    public const string MODEL_ID_STR = "D4DC262C-0437-4832-8524-6847FE0AA8F3";
    public const string NAME = "name";
    public const string TRAILER = "trailer";

    #endregion

    #region Propertys

    private static AbstractProperty _infoProperty = new WProperty(typeof(string), string.Empty);

    public AbstractProperty InfoProperty
    {
      get { return _infoProperty; }
    }

    public static string Info
    {
      get { return (string)_infoProperty.GetValue(); }
      set { _infoProperty.SetValue(value); }
    }

    #endregion

    public static ItemsList Items = new ItemsList();

    #region public Methods

    public static void ComingSoonCinema()
    {
      Info = "Cinema";
      MovieInfo.Movies = new List<Movie>();
      MovieInfo.Movies = Search.Info(Functions.DefaultCountry(), Search.Feed.Cinema, Search.Typ.coming, 20);
      FillItems();
    }

    public static void ComingSoonBlyRay()
    {
      Info = "BluRay";
      MovieInfo.Movies = new List<Movie>();
      MovieInfo.Movies = Search.Info(Functions.DefaultCountry(), Search.Feed.BluRay, Search.Typ.coming, 20);
      FillItems();
    }

    public static void SelectMovie(ListItem item)
    {
      var t = new Trailer { Title = (string)item.AdditionalProperties[NAME], Url = (string)item.AdditionalProperties[TRAILER] };
      if (t.Url != "")
      {
        CinemaPlayerHelper.PlayStream(t);
      }
    }

    #endregion

    #region private Methods

    private static void FillItems()
    {
      for (int x = 0; x <= MovieInfo.Movies.Count - 1; x++)
      {
        var item = new ListItem();
        item.AdditionalProperties[NAME] = MovieInfo.Movies[x].Regions[0].products[0].product_title;
        item.SetLabel("Name", MovieInfo.Movies[x].Regions[0].products[0].product_title);
        item.SetLabel("Poster", MovieInfo.Poster(x));
        item.SetLabel("Picture", MovieInfo.Picture(x));
        item.SetLabel("Description", MovieInfo.Description(x));
        item.SetLabel("Year", MovieInfo.Year(x));
        item.SetLabel("AgeLimit", MovieInfo.AgeLimit(x));
        item.SetLabel("Genre", MovieInfo.Genres(x));
        item.AdditionalProperties[TRAILER] = MovieInfo.Trailer("mp4 / xxlarge", x);
        item.SetLabel("Duration", MovieInfo.Duration(x));
        item.SetLabel("Premiere", MovieInfo.Premiere(x));
        Items.Add(item);
      }
      Items.FireChange();
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
      ComingSoonCinema();
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
