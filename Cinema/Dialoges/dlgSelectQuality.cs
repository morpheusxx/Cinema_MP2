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
using MediaPortal.Common;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Screens;
using MediaPortal.UI.Presentation.Workflow;

namespace Cinema.Dialoges
{
  public class DlgSelectQuality
  {
    #region Consts

    public const string MODEL_ID_STR = "6EF784F0-6B89-42D9-B958-85DE8E5D8940";
    public const string NAME = "name";

    #endregion

    public static ItemsList items = new ItemsList();

    public static void Show()
    {
      items.Clear();

      foreach (var s in MovieInfo.TrailerFormates(0))
      {
        var item = new ListItem();
        item.AdditionalProperties[NAME] = s;
        item.SetLabel("Name", s);
        items.Add(item);
      }
      ServiceRegistration.Get<IWorkflowManager>().NavigatePushAsync(new Guid("75691F40-B249-400D-AB62-F0F737CFE0C1"));
    }

    public static void Select(ListItem item)
    {
      ServiceRegistration.Get<IScreenManager>().CloseTopmostDialog();
      var s = MovieInfo.Trailer((string)item.AdditionalProperties[NAME],0);
      CinemaPlayerHelper.PlayStream(new Trailer(s, (string)item.AdditionalProperties[NAME]));
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
