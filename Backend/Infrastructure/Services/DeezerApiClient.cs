using Backend.Application.Models;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Infrastructure.Services;

public class DeezerApiClient(HttpClient httpClient)
{
    private const string BaseUrl = "https://api.deezer.com";
    private readonly Random _random = new();
    
    public async Task<List<DeezerTrack>> GenerateTracksToPlaylistByGenreAsync(DeezerGenre genre)
    {
        try
        {
            var trackCount = _random.Next(5, 16);
            var tracks = await GetTracksFromChart(genre, trackCount);
            
            if (tracks.Count >= trackCount)
            {
                return FilterAndTakeTracks(tracks, trackCount);
            }
            
            tracks = await GetTracksFromSearch(genre, trackCount);
            return FilterAndTakeTracks(tracks, trackCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting tracks for genre {genre}: {ex.Message}");
            return [];
        }
    }
    
    private async Task<List<DeezerTrack>> GetTracksFromChart(DeezerGenre genre, int limit)
    {
        var response = await httpClient.GetFromJsonAsync<DeezerChartResponse>(
            $"{BaseUrl}/editorial/{(int)genre}/charts?limit={limit}");
        return response?.Tracks.Data ?? [];
    }
    
    private async Task<List<DeezerTrack>> GetTracksFromSearch(DeezerGenre genre, int limit)
    {
        var genreName = genre.ToString().ToLower();
        var response = await httpClient.GetFromJsonAsync<DeezerSearchResponse>(
            $"{BaseUrl}/search?q=genre:\"{Uri.EscapeDataString(genreName)}\"&limit={limit}&order=RANKING");
        return response?.Data ?? [];
    }
    
    private static List<DeezerTrack> FilterAndTakeTracks(List<DeezerTrack> tracks, int count) =>
        FilterValidTracks(tracks).Take(count).ToList();
    
    private static IEnumerable<DeezerTrack> FilterValidTracks(IEnumerable<DeezerTrack> tracks) =>
        tracks.Where(t => !string.IsNullOrEmpty(t.Artist.Name) && 
                          !string.IsNullOrEmpty(t.Title) &&
                          !string.IsNullOrEmpty(t.Preview) &&
                          t.Album?.Cover != null);
    
    public async Task<QuizQuestion> GenerateQuizQuestionAsync(DeezerGenre genre, string questionType)
    {
        var tracks = await GenerateTracksForQuizQuestionAsync(genre);
        
        if (tracks.Count == 0 )
        {
            return new QuizQuestion(); 
        }

        var correctTrack = tracks[_random.Next(0, tracks.Count)];
        tracks.Remove(correctTrack);

        var question = new QuizQuestion
        {
            QuestionType = questionType,
            PreviewUrl = correctTrack.Preview,
            CoverUrl = correctTrack.Album.Cover
        };

        if (questionType == "artist")
        {
            SetupArtistQuestion(question, correctTrack, tracks);
        }
        else
        {
            SetupTitleQuestion(question, correctTrack, tracks);
        }

        return question;
    }
    
    private void SetupArtistQuestion(QuizQuestion question, DeezerTrack correctTrack, List<DeezerTrack> otherTracks)
    {
        question.QuestionText = "Кто исполнитель этой песни?";
        question.CorrectAnswer = correctTrack.Artist.Name;
        question.Options = [correctTrack.Artist.Name];
        
        AddDistinctOptions(otherTracks, question.Options, t => t.Artist.Name, 3);
        ShuffleOptions(question);
    }
    
    private void SetupTitleQuestion(QuizQuestion question, DeezerTrack correctTrack, List<DeezerTrack> otherTracks)
    {
        question.QuestionText = "Какое название этой песни?";
        question.CorrectAnswer = correctTrack.Title;
        question.Options = [correctTrack.Title];
        
        AddDistinctOptions(otherTracks, question.Options, t => t.Title, 3);
        ShuffleOptions(question);
    }
    
    private void AddDistinctOptions(
        List<DeezerTrack> tracks,
        List<string> options,
        Func<DeezerTrack, string> selector,
        int maxAdditionalOptions)
    {
        while (options.Count < maxAdditionalOptions + 1 && tracks.Count > 0)
        {
            var option = selector(tracks[_random.Next(0, tracks.Count)]);
            if (!options.Contains(option))
            {
                options.Add(option);
            }
            tracks.RemoveAll(t => selector(t) == option);
        }
    }
    
    private void ShuffleOptions(QuizQuestion question)
    {
        question.Options = question.Options.OrderBy(_ => _random.Next()).ToList();
        question.CorrectIndex = question.Options.IndexOf(question.CorrectAnswer);
    }
    
    private async Task<List<DeezerTrack>> GenerateTracksForQuizQuestionAsync(DeezerGenre genre)
    {
        try
        {
            var tracks = await GetTracksFromChart(genre, 10);
            
            if (tracks.Count < 10)
            {
                var additionalTracks = await GetTracksFromSearch(genre, 10 - tracks.Count);
                tracks.AddRange(additionalTracks);
            }
            
            return FilterValidTracks(tracks).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting tracks for genre {genre}: {ex.Message}");
            return [];
        }
    }
    
    public string ToDisplayString(DeezerGenre genre)
    {
        return genre switch
        {
            DeezerGenre.Pop => "Pop",
            DeezerGenre.Alternative => "Alternative",
            DeezerGenre.Rock => "Rock",
            DeezerGenre.HipHop => "Hip-Hop",
            DeezerGenre.Dance => "Dance",
            DeezerGenre.Electronic => "Electronic",
            DeezerGenre.Country => "Country",
            DeezerGenre.Metal => "Metal",
            _ => genre.ToString()
        };
    }
}

