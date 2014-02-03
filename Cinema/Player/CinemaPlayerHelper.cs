using System;
using System.Collections.Generic;
using Cinema.Helper;
using MediaPortal.Common;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.Services.ResourceAccess.RawUrlResourceProvider;
using MediaPortal.Common.SystemResolver;
using MediaPortal.UiComponents.Media.Models;

namespace Cinema.Player
{
  class CinemaPlayerHelper
  {
    public const string CINEMA_MIMETYPE = "video/stream";

    public static void PlayStream(Trailer trailer)
    {
      var mediaItem = CreateStreamMediaItem(trailer);
      PlayItemsModel.PlayItem(mediaItem);
    }

    /// <summary>
    /// Constructs a dynamic <see cref="MediaItem"/> that contains the URL for the given <paramref name="trailer"/>.
    /// </summary>
    /// <param name="trailer">Trailer.</param>
    private static MediaItem CreateStreamMediaItem(Trailer trailer)
    {
      IDictionary<Guid, MediaItemAspect> aspects = new Dictionary<Guid, MediaItemAspect>();

      MediaItemAspect providerResourceAspect;
      aspects[ProviderResourceAspect.ASPECT_ID] = providerResourceAspect = new MediaItemAspect(ProviderResourceAspect.Metadata);
      MediaItemAspect mediaAspect;
      aspects[MediaAspect.ASPECT_ID] = mediaAspect = new MediaItemAspect(MediaAspect.Metadata);
      aspects[AudioAspect.ASPECT_ID] = new MediaItemAspect(AudioAspect.Metadata); // AudioAspect needs to be contained for player mapping

      providerResourceAspect.SetAttribute(ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH, RawUrlResourceProvider.ToProviderResourcePath(trailer.Url).Serialize());
      providerResourceAspect.SetAttribute(ProviderResourceAspect.ATTR_SYSTEM_ID, ServiceRegistration.Get<ISystemResolver>().LocalSystemId);

      mediaAspect.SetAttribute(MediaAspect.ATTR_MIME_TYPE, CINEMA_MIMETYPE);
      mediaAspect.SetAttribute(MediaAspect.ATTR_TITLE, trailer.Title);

      var mediaItem = new MediaItem(Guid.Empty, aspects);
      return mediaItem;
    }
  }
}
