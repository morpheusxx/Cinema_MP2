using System;
using System.Collections.Generic;
using Cinema.Helper;
using Previewnetworks;

namespace Cinema.Previewnetworks
{
    public class MovieInfo
    {
        public static List<SortedMovie> Movies = new List<SortedMovie>();

        public string ImdbId = string.Empty;
        public string AgeLimit = string.Empty;
        public string Description = string.Empty;
        public string Genre = string.Empty;
        public string Picture = string.Empty;
        public string Poster = string.Empty;
        public string Trailer = string.Empty;
        public string Year = string.Empty;

        public MovieInfo(string imdbId)
        {
            GrappByImdb(imdbId);
        }

        private void GrappByImdb(string id)
        {
            FillInfos(new SortedMovie(Functions.DefaultCountry(), id));

            if (Filled()) return;
            FillInfos(new SortedMovie(Search.Country.UnitedKingdom, id));

            if (Filled()) return;
            FillInfos(new SortedMovie(Search.Country.SwitzerlandGerman, id));

            if (Filled()) return;
            FillInfos(new SortedMovie(Search.Country.SwitzerlandFrench, id));

            if (Filled()) return;
            FillInfos(new SortedMovie(Search.Country.France, id));

            if (Filled()) return;
            FillInfos(new SortedMovie(Search.Country.Spain, id));

            if (Filled()) return;;
            FillInfos(new SortedMovie(Search.Country.Italy, id));

            if (Filled()) return;
            FillInfos(new SortedMovie(Search.Country.Netherlands, id));

            if (Filled()) return;
            FillInfos(new SortedMovie(Search.Country.Denmark, id));

            if (Filled()) return;
            FillInfos(new SortedMovie(Search.Country.Sweden, id));

            if (Filled()) return;
            FillInfos(new SortedMovie(Search.Country.Finland, id));
        }

        private void FillInfos(SortedMovie movie)
        {
            Poster = movie.Cover;
            Picture = movie.Picture;
            Description = movie.Description;
            Year = movie.Year;
            Genre = movie.Genre;
            Trailer = movie.Trailer;
            ImdbId = movie.ImdbId;
        }

        private bool Filled()
        {
            if (Poster == "") return false;
            if (Picture == "") return false;
            if (Description == "") return false;
            if (Year == "") return false;
            if (Genre == "") return false;
            if (Trailer == "") return false;
            return true;
        }

        //private static void Grabb(string title)
        //{
        //    title = CleanTitle(title);

        //    if (!GrappByCountry(title, Functions.DefaultCountry()))
        //    {
        //        GrappAllContry(title);
        //    }

        //    if (Movies.Count == 0)
        //    {
        //        if (_research <= 4)
        //        {
        //            _research += 1;
        //            Grabb(title);
        //        }
        //    }

        //    _research = 0;
        //}

        //private static bool GrappByCountry(string title, Search.Country country)
        //{
        //    Movies = Search.Title(country, title);
        //    return Movies.Count > 0;
        //}

        //private static bool GrappAllContry(string title)
        //{
        //    foreach (var c in (Search.Country[])Enum.GetValues(typeof(Search.Country)))
        //    {
        //        Movies = Search.Title(c, title);
        //        if (Movies.Count > 0)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        private static string CleanTitle(string title)
        {
            // Remove (*)
            var a = title.IndexOf("(", StringComparison.Ordinal);
            if (a >= 0)
            {
                var b = title.IndexOf(")", a, StringComparison.Ordinal) + 1;
                if (b > 0)
                {
                    title = title.Replace(title.Substring(a, b - a), "");
                }
            }

            title = title.Replace("in 3D", "").Trim();
            title = title.Replace("3D", "").Trim();
            title = title.Replace("&", "+").Trim();

            return title;
        }

        public static string GetImdbID(string title)
        {
            var t = GoogleMovies.GoogleMovies.GetWebsiteText("http://www.imdb.com/find?q=" + CleanTitle(title));
            return GoogleMovies.GoogleMovies.GetSubstring(t, "<a name=\"tt\">", "/title/", "/");
        }
    }
}