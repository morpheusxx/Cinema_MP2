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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Cinema.Settings;
using MediaPortal.Common;
using MediaPortal.Common.Settings;

namespace Cinema.GoogleMovies
{
  internal class GoogleMovies
  {
    #region All Data

    public static CinemaDataList DataList;
    private static Locations _lSettings = new Locations();

    #endregion

    #region Get Movies

    public static CinemaData GetCinemaData(Cinema cinema)
    {
      var modlist = new List<MoviesOnDay>();
      for (var x = 0; x <= 3; x++)
      {
        modlist.Add(GetMoviesOnDay(cinema, x));
      }

      var cd = new CinemaData { Current = cinema, MoviesOnDayList = modlist };
      return cd;
    }

    public static MoviesOnDay GetMoviesOnDay(Cinema cinema, int day)
    {
      var md = new MoviesOnDay();
      md.Day = md.Day = GetNewDay(day);
      md.Movielist = AddMoviesFromSite(GetWebsiteText("http://www.google.com/movies?near=" + cinema.Near + "&tid=" + cinema.Id + "&date=" + day));
      return md;
    }

    private static List<Movie> AddMoviesFromSite(string siteText)
    {
      var ml = new List<Movie>();

      string[] b = siteText.Split(new[] { "<div class=movie>" }, StringSplitOptions.None);

      for (int i = 1; i <= b.Length - 1; i++)
      {
        string t = GetSubstring(b[i], "a href=\"", ">", "<");
        string n = GetSubstring(b[i], "class=info>", "<");
        string[] g = n.Split(new[] { " - " }, StringSplitOptions.None);


        string v = GetSubstring(b[i], "class=times>‎‎", "</div>");

        string g0 = string.Empty;
        string g1 = string.Empty;

        if (g.Length > 0)
        {
          g0 = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(g[0]).Where(bn => bn < 128).ToArray()).Replace("hr ",":").Replace("min","");
        }

        if (g.Length > 1)
        {
          g1 = g[1];
        }

        var m = new Movie { Title = t, Runtime = g0, Info = g1, Showtimes = GetShowtimes(v) };
        ml.Add(m);
      }
      return ml;
    }

    private static List<string> GetShowtimes(string value)
    {
      var l = new List<string>();

      var b = value.Split(new[] { "<!--  -" }, StringSplitOptions.None);

      for (var i = 1; i <= b.Length - 1; i++)
      {
        var t = GetSubstring(b[i], ">", "</");

        if (t.Substring(0, 1) == "<")
        {
          t = t.Replace("<", "");
        }

        t = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(t).Where(bn => bn < 128).ToArray());

        l.Add(t.Substring(t.IndexOf(">", StringComparison.Ordinal) + 1));
      }
      return l;
    }

    public static List<Movie> GetMoviesByCinema(Cinema cinema)
    {
      var l = new List<string>();
      var ml = new List<Movie>();

      foreach (var m in from cd in DataList.Datalist where cinema.Id == cd.Current.Id from m in cd.MoviesOnDayList.SelectMany(mod => mod.Movielist.Where(m => !l.Contains(m.Title))) select m)
      {
        l.Add(m.Title);
        ml.Add(m);
      }
      return ml;
    }

    public static List<string> GetShowTimesByCinemaAndMovieAndDay(Cinema cinema, Movie movie, int day)
    {
      var l = new List<string>();
      foreach (var t in from cd in DataList.Datalist where cd.Current.Id == cinema.Id from t in (from mod in cd.MoviesOnDayList where mod.Day == GetNewDay(day) from t in (from m in mod.Movielist where m.Title == movie.Title from t in m.Showtimes.Where(t => !l.Contains(t)) select t) select t) select t)
      {
        l.Add(t);
      }
      return l;
    }

    #endregion

    #region Get Cinemas

    public static List<Cinema> GetCinemas(string search)
    {
      var cl = new List<Cinema>();
      var sublinks = new List<string>();
      var x = 10;
      int i;
      var url = "http://www.google.com/movies?near=" + search.Replace(" ", "+");
      var siteText = GetWebsiteText(url);

      do
      {
        i = siteText.IndexOf("&start=" + Convert.ToString(x), StringComparison.Ordinal);
        if (!(i > 0 & !sublinks.Contains(Convert.ToString(x)))) continue;
        sublinks.Add(Convert.ToString(x));
        x += 10;
      } while (i > 0);

      var iDl = new List<string>();
      foreach (var c in AddCinemasFromSite(siteText).Where(c => !iDl.Contains(c.Id)))
      {
        iDl.Add(c.Id);
        cl.Add(c);
      }

      foreach (var c in sublinks.SelectMany(sl => AddCinemasFromSite(GetWebsiteText(url + "&start=" + sl)).Where(c => !iDl.Contains(c.Id))))
      {
        iDl.Add(c.Id);
        cl.Add(c);
      }
      return cl;
    }

    private static IEnumerable<Cinema> AddCinemasFromSite(string siteText)
    {
      var cl = new List<Cinema>();
      var b = siteText.Split(new[] { "id=theater_" }, StringSplitOptions.None);

      for (var x = 1; x <= b.Length - 1; x++)
      {
        // ID
        var i = GetSubstring(b[x], "&tid=", "\"");
        // Title
        var n = GetSubstring(b[x], "_theater_", ">", "</");
        // Address
        var a = GetSubstring(b[x], "info>", "<");
        // Ort
        var near = GetSubstring(b[x], "near=", "&tid=");
        // Neue Kinos eintragen
        if (i == "") continue;
        var cinema = new Cinema { Id = i, Name = n, Address = a, Near = near };
        cl.Add(cinema);
      }
      return cl;
    }

    public static Cinema GetCinemaById(string id)
    {
      foreach (CinemaData cd in DataList.Datalist)
      {
        if (cd.Current.Id == id)
        {
          return cd.Current;
        }
      }
      return new Cinema();
    }

    #endregion

    #region Helper

    private static string GetSubstring(string value, string start, string ende)
    {
      try
      {
        var a = value.IndexOf(start) + start.Length;
        if (a - start.Length <= 0) return "";
        var b = value.IndexOf(ende, a, StringComparison.Ordinal);
        return value.Substring(a, b - a);
      }
      catch (Exception)
      {
        return "";
      }
    }

    public static string GetSubstring(string value, string start, string start2, string ende)
    {
      try
      {
        var a = value.IndexOf(start) + start.Length;
        var a2 = value.IndexOf(start2, a, StringComparison.Ordinal) + start2.Length;
        var b = value.IndexOf(ende, a2, StringComparison.Ordinal);
        return value.Substring(a2, b - a2);
      }
      catch (Exception)
      {
        return "";
      }
    }

    public static string GetWebsiteText(string url)
    {
      WebRequest request = WebRequest.Create(url);
      request.Credentials = CredentialCache.DefaultCredentials;
      WebResponse response = null;
      StreamReader reader = null;

      try
      {
        response = request.GetResponse();
        reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
        return HttpUtility.HtmlDecode(reader.ReadToEnd());
      }
      catch (Exception)
      {
        return "";
      }
      finally
      {
        if (reader != null) reader.Close();
        if (response != null) response.Close();
      }
    }

    public static string GetNewDay(int day)
    {
      DateTime today = DateTime.Now;
      var duration = new TimeSpan(day, 0, 0, 0);
      return (today.Add(duration).ToString("dd.MM.yyyy"));
    }

    #endregion
  }

  #region Cinema Structs

  public struct CinemaDataList
  {
    public List<CinemaData> Datalist;
  }

  public struct CinemaData
  {
    public Cinema Current;
    public List<MoviesOnDay> MoviesOnDayList;
  }

  public struct MoviesOnDay
  {
    public string Day;
    public List<Movie> Movielist;
  }

  public struct Cinema
  {
    public string Address;
    public string Id;
    public string Name;
    public string Near;
  }

  public struct Movie
  {
    public string Info;
    public string Runtime;
    public List<string> Showtimes;
    public string Title;
    public string AgeLimit;
    public string Year;

  }

  #endregion
}