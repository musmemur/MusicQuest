namespace Backend;

public static class DeezerGenreExtensions
{
    public static string ToDisplayString(this DeezerGenre genre)
    {
        return genre switch
        {
            DeezerGenre.Pop => "Pop",
            DeezerGenre.Alternative => "Alternative",
            DeezerGenre.Rock => "Rock",
            DeezerGenre.HipHop => "Hip-Hop",
            DeezerGenre.Dance => "Dance",
            DeezerGenre.Electronic => "Electronic",
            DeezerGenre.Jazz => "Jazz",
            DeezerGenre.Metal => "Metal",
            _ => genre.ToString()
        };
    }
}