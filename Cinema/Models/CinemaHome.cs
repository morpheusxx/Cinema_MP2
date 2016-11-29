#region Copyright (C) 2007-2014 Team MediaPortal

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
using System.Timers;
using Cinema.Dialoges;
using Cinema.Player;
using Cinema.Settings;
using GoogleMovies;
using MediaPortal.Common;
using MediaPortal.Common.General;
using MediaPortal.Common.Logging;
using MediaPortal.Common.PluginManager;
using MediaPortal.Common.Settings;
using MediaPortal.Extensions.UserServices.FanArtService.Client.Models;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Screens;
using MediaPortal.UI.Presentation.Workflow;
using MediaPortal.UI.SkinEngine.Controls.ImageSources;
using MediaPortal.UI.SkinEngine.MpfElements;

namespace Cinema.Models
{
    public class CinemaHome : IWorkflowModel, IPluginStateTracker
    {
        #region Consts

        public const string MODEL_ID_STR = "78E0D999-D87A-4340-B8D1-9CF97814D2FD";
        public const string NAME = "name";
        public const string TRAILER = "trailer";

        #endregion

        public static Timer ATimer = new Timer();
        public ItemsList Cinemas = new ItemsList();
        public static ItemsList Movies = new ItemsList();

        private static readonly ISettingsManager SETTINGS_MANAGER = ServiceRegistration.Get<ISettingsManager>();

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

        #region public Methods

        public static void SelectCinema(string id)
        {
            foreach (var cd in GoogleMovies.GoogleMovies.Data.List.Where(cd => cd.Current.Id == id))
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

            var ml = GoogleMovies.GoogleMovies.GetMoviesByCinema(cinema);

            foreach (var m in ml)
            {
                var item = new ListItem { AdditionalProperties = { [NAME] = m.Title } };
                item.SetLabel("Name", m.Title);

                for (var i = 0; i <= 3; i++)
                {
                    item.SetLabel("Day" + Convert.ToString(i), ShowtimesByCinemaMovieDay(cinema, m, i).Substring(0, 10));
                    item.SetLabel("Day" + Convert.ToString(i) + "_Time", ShowtimesByCinemaMovieDay(cinema, m, i).Substring(12));
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
            }
            Movies.FireChange();
        }

        public static void SetSelectedItem(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.FirstAddedItem as ListItem;
            if (selectedItem != null)
            {
                var fanArtBgModel = (FanArtBackgroundModel)ServiceRegistration.Get<IWorkflowManager>().GetModel(FanArtBackgroundModel.FANART_MODEL_ID);
                if (fanArtBgModel != null)
                {
                    string uriSource = selectedItem.Labels["Picture"].ToString();

                    if (uriSource != "") fanArtBgModel.ImageSource = new MultiImageSource { UriSource = uriSource };
                    else fanArtBgModel.ImageSource = new MultiImageSource { UriSource = null };
                }
            }
        }

        public static void MakeUpdate(bool dialog)
        {
            if (dialog)
                ServiceRegistration.Get<IWorkflowManager>().NavigatePushAsync(new Guid("48FE28A6-868D-4531-BF2F-1E746769B177"));

            DlgUpdate.MakeUpdate(dialog);

            if (dialog)
                ServiceRegistration.Get<IScreenManager>().CloseTopmostDialog();
        }

        #endregion

        #region private Methods

        private static void Init()
        {
            CkeckUpdate(true);

            if (GoogleMovies.GoogleMovies.Data.List != null && GoogleMovies.GoogleMovies.Data.List.Count > 0)
            {
                SelectCinema(GoogleMovies.GoogleMovies.Data.List[0].Current.Id);
            }
        }

        private static void CkeckUpdate(bool dialog)
        {
            var dt1 = SETTINGS_MANAGER.Load<Settings.CinemaSettings>().LastUpdate ?? DateTime.MinValue;
            var dt = DateTime.Now - dt1;
            // Is it a New Day ?
            if (dt > new TimeSpan(1, 0, 0, 0))
            {
                MakeUpdate(dialog);
            }
            else if (SETTINGS_MANAGER.Load<Locations>().Changed)
            {
                MakeUpdate(dialog);
            }
            else
            {
                GoogleMovies.GoogleMovies.Data = SETTINGS_MANAGER.Load<Datalist>().CinemaDataList;
            }
        }

        private static string ShowtimesByCinemaMovieDay(GoogleMovies.Cinema cinema, Movie movie, int day)
        {
            var st = GoogleMovies.GoogleMovies.GetShowTimesByCinemaAndMovieAndDay(cinema, movie, day).Aggregate("", (current, s) => current + (s + " | "));
            return GoogleMovies.GoogleMovies.GetNewDay(day) + ": " + st;
        }

        private void ClearFanart()
        {
            var fanArtBgModel = (FanArtBackgroundModel)ServiceRegistration.Get<IWorkflowManager>().GetModel(FanArtBackgroundModel.FANART_MODEL_ID);
            if (fanArtBgModel != null)
            {
                fanArtBgModel.ImageSource = new MultiImageSource { UriSource = null };
            }
        }

        private static void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            ServiceRegistration.Get<ILogger>().Info("Cinema Timer Thread Check Update");
            CkeckUpdate(false);
        }

        #endregion

        #region IWorkflowModel implementation

        public Guid ModelId => new Guid(MODEL_ID_STR);

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
            ClearFanart();
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

        # region IPluginStateTracker implementation

        public void Activated(PluginRuntime pluginRuntime)
        {
            // Add Timer with event
            ATimer.Elapsed += OnTimedEvent;
            // Diff in sec To next Day 01.00.00
            ATimer.Interval = DateTime.Today.AddHours(25).Subtract(DateTime.Now).TotalMilliseconds;
            // Timer start
            ATimer.Start();

            // Make Update
            CkeckUpdate(false);
        }

        public bool RequestEnd()
        {
            return true;
        }

        public void Stop()
        {
        }

        public void Continue()
        {
        }

        public void Shutdown()
        {
        }

        #endregion
    }
}