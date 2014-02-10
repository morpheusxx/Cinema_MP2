using System;
using System.Collections.Generic;
using System.Linq;
using Cinema.Helper;
using MediaPortal.Common;
using MediaPortal.Common.Localization;
using MediaPortal.Common.Settings;
using Previewnetworks_v31;

namespace Cinema.Previewnetworks
{
  public class MovieInfo
  {
    public static List<Movie> Movies = new List<Movie>();
    private static int _research;

    public static string Poster(int movie)
    {
      try
      {
        foreach (var p in Movies[movie].Regions[0].pictures.Where(p => p.type_name == "poster_large"))
        {
          return p.url;
        }

        foreach (var p in Movies[movie].Regions[0].pictures.Where(p => p.type_name == "poster"))
        {
          return p.url;
        }
      }
      catch (Exception)
      {
        return "";
      }
      return "";
    }

    public static string Picture(int movie)
    {
      try
      {
        foreach (var p in Movies[movie].Regions[0].pictures.Where(p => p.type_name == "Lobby Still Large"))
        {
          return p.url;
        }

        foreach (var p in Movies[movie].Regions[0].pictures.Where(p => p.type_name == "Lobby Still 1"))
        {
          return p.url;
        }
      }
      catch (Exception)
      {
        return "";
      }
      return "";
    }

    public static string Description(int movie)
    {
      try
      {
        return Movies[movie].Regions[0].products[0].description;
      }
      catch (Exception)
      {
        return "";
      }
    }

    public static List<string> TrailerFormates(int movie)
    {
      var l = new List<string>();
      try
      {
        foreach (var f in Movies[movie].Regions[0].products[0].clips[0].Files.Where(f => !l.Contains(f.format + " / " + f.size_type)))
        {
          l.Add(f.format + " / " + f.size_type);
        }
      }
      catch (Exception)
      {
        return l;
      }
      return l;
    }

    public static string Trailer(string format, int movie)
    {
      try
      {
        var f = format.Substring(0, format.IndexOf(" /", StringComparison.Ordinal));
        var t = format.Substring(format.IndexOf("/ ", StringComparison.Ordinal) + 2);

        foreach (var file in Movies[movie].Regions[0].products[0].clips[0].Files.Where(file => file.format == f).Where(file => file.size_type == t))
        {
          return file.url;
        }
      }
      catch (Exception)
      {
        return "";
      }
      return "";
    }

    public static string Duration(int movie)
    {
      try
      {
        return Movies[movie].movie_duration != "" ? Movies[0].movie_duration : "0";
      }
      catch (Exception)
      {
        return "0";
      }
    }

    public static string AgeLimit(int movie)
    {
      try
      {
        return Movies[movie].Regions[0].age_limit;
      }
      catch (Exception)
      {
        return "0";
      }
    }

    public static string Year(int movie)
    {
      try
      {
        return Movies[movie].Regions[0].pub_date.value.Substring(0, 4);
      }
      catch (Exception)
      {
        return "0";
      }
    }

    public static string Genres(int movie)
    {
      var g = "";
      try
      {
        g = Movies[movie].Regions[0].categories.Aggregate(g, (current, s) => current + (s.Value + ", "));
        g = g.Substring(0, g.Length - 2);
      }
      catch (Exception)
      {
        return g;
      }
      return g;
    }

    public static string Premiere(int movie)
    {
      try
      {
        var split = Movies[movie].Regions[0].products[0].premiere.value.Split(new[] {'-'});
        return split[2] + "." + split[1] + "." + split[0];
      }
      catch (Exception)
      {
        return "";
      }
    }

    public static void GrappByImdb(string id)
    {
      Movies = new List<Movie>();
      Movies = Search.ImdbId(Functions.DefaultCountry(), id);
    }

    public static void Grabb(string title)
    {
      title = CleanTitle(title);

      if (!GrappByCountry(title, Functions.DefaultCountry()))
      {
        GrappAllContry(title);
      }

      if (Movies.Count == 0)
      {
        if (_research <= 4)
        {
          _research += 1;
          Grabb(title);
        }
      }

      _research = 0;
    }

    private static bool GrappByCountry(string title, Search.Country country)
    {
      Movies = Search.Title(country, title);
      return Movies.Count > 0;
    }

    private static bool GrappAllContry(string title)
    {
      foreach (var c in (Search.Country[])Enum.GetValues(typeof(Search.Country)))
      {
        Movies = Search.Title(c, title);
        if (Movies.Count > 0)
        {
          return true;
        }
      }
      return false;
    }

    private static string CleanTitle(string title)
    {
      // Remove (*)
      var a = title.IndexOf("(", System.StringComparison.Ordinal);
      if (a >= 0)
      { 
        var b = title.IndexOf(")", a, StringComparison.Ordinal) + 1;
        if (b > 0)
        {
          title = title.Replace(title.Substring(a, b - a),"");
        }
      }

      title = title.Replace("in 3D", "").Trim();
      title = title.Replace("3D", "").Trim();
      title = title.Replace("&", "+").Trim();

      return title;
    }

    public static string ImdbID(string title)
    {
      var t = GoogleMovies.GoogleMovies.GetWebsiteText("http://www.imdb.com/find?q=" + CleanTitle(title));
      return GoogleMovies.GoogleMovies.GetSubstring(t, "<a name=\"tt\">", "/title/", "/");
    }
  }
}