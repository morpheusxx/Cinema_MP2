using System;
using DirectShow;
using MediaPortal.UI.Players.Video;
using MediaPortal.UI.Players.Video.Tools;
using MediaPortal.UI.Presentation.Players;


namespace Cinema.Player
{
  public class CinemaPlayer : VideoPlayer , IUIContributorPlayer 
  {
    public CinemaPlayer()
    {
    }

    public Type UIContributorType
    {
      get { return typeof(CinemaUiContributor); }
    }
 
  }
}
