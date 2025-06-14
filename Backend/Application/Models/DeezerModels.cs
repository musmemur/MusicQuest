namespace Backend.Application.Models;

public class DeezerChartResponse
{
    public DeezerTrackList Tracks { get; set; }
}

public class DeezerTrackList
{
    public List<DeezerTrack> Data { get; set; }
}

public class DeezerSearchResponse
{
    public List<DeezerTrack> Data { get; set; }
}

public class DeezerTrack
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Preview { get; set; }
    public DeezerArtist Artist { get; set; }
    public DeezerAlbum Album { get; set; }
}

public class DeezerArtist
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Picture { get; set; }
}

public class DeezerAlbum
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Cover { get; set; }
}