using Backend.ApiModals;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Infrastructure.Services;

public class DeezerApiClient(HttpClient httpClient)
{
    private const string BaseUrl = "https://api.deezer.com";
    private readonly Random _random = new();
    
    public async Task<List<DeezerTrack>> GetTracksByGenreAsync(DeezerGenre genre)
    {
        try
        {
            var random = new Random();
            var trackCount = random.Next(5, 16);
            
            var chartResponse = await httpClient.GetFromJsonAsync<DeezerChartResponse>(
                $"{BaseUrl}/editorial/{(int)genre}/charts?limit={trackCount}");

            var tracks = chartResponse?.Tracks.Data;
        
            if (tracks != null && tracks.Count >= trackCount)
                return tracks
                    .Where(t => !string.IsNullOrEmpty(t.Artist.Name) && !string.IsNullOrEmpty(t.Title))
                    .Take(trackCount)
                    .ToList();
            
            var genreName = genre.ToString().ToLower();
            var searchResponse = await httpClient.GetFromJsonAsync<DeezerSearchResponse>(
                $"{BaseUrl}/search?q=genre:\"{Uri.EscapeDataString(genreName)}\"&limit={trackCount}&order=RANKING");
            
            tracks = searchResponse?.Data;

            return tracks?
                .Where(t => !string.IsNullOrEmpty(t.Artist.Name) && !string.IsNullOrEmpty(t.Title))
                .Take(trackCount)
                .ToList() ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting tracks for genre {genre}: {ex.Message}");
            return [];
        }
    }

    public async Task<QuizQuestion> GenerateQuizQuestionAsync(DeezerGenre genre, string questionType)
    {
        var question = new QuizQuestion();
        var tracks = await GetRandomTrackListAsync(genre);
        
        tracks = tracks.Where(t => 
            !string.IsNullOrEmpty(t.Artist.Name) &&
            !string.IsNullOrEmpty(t.Title))
            .ToList();

        var correctTrack = tracks[_random.Next(0, tracks.Count)];
        tracks.Remove(correctTrack);

        if (questionType == "artist")
        {
            question.QuestionText = "Кто исполнитель этой песни?";
            question.CorrectAnswer = correctTrack.Artist.Name;
            question.Options = [correctTrack.Artist.Name];
            
            while (question.Options.Count < 4 && tracks.Count > 0)
            {
                var wrongArtist = tracks[_random.Next(0, tracks.Count)].Artist.Name;
                if (question.Options.Contains(wrongArtist)) continue;
                question.Options.Add(wrongArtist);
                tracks.RemoveAll(t => t.Artist.Name == wrongArtist);
            }

            question.Options = question.Options.OrderBy(_ => _random.Next()).ToList();
            question.CorrectIndex = question.Options.IndexOf(correctTrack.Artist.Name);
        }
        else
        {
            question.QuestionText = "Какое название этой песни?";
            question.CorrectAnswer = correctTrack.Title;
            question.Options = [correctTrack.Title];
            
            while (question.Options.Count < 4 && tracks.Count > 0)
            {
                var wrongTitle = tracks[_random.Next(0, tracks.Count)].Title;
                if (question.Options.Contains(wrongTitle)) continue;
                question.Options.Add(wrongTitle);
                tracks.RemoveAll(t => t.Title == wrongTitle);
            }

            question.Options = question.Options.OrderBy(_ => _random.Next()).ToList();
            question.CorrectIndex = question.Options.IndexOf(correctTrack.Title);
        }

        question.QuestionType = questionType;
        question.PreviewUrl = correctTrack.Preview;
        question.CoverUrl = correctTrack.Album.Cover;
        
        return question;
    }
    
    private async Task<List<DeezerTrack>> GetRandomTrackListAsync(DeezerGenre genre)
    {
        try
        {
            var chartResponse = await httpClient.GetFromJsonAsync<DeezerChartResponse>(
                $"{BaseUrl}/editorial/{(int)genre}/charts?limit={4}");
        
            var chartTracks = chartResponse?.Tracks.Data;
            if (chartTracks?.Count > 0)
            {
                return chartTracks.Take(4).ToList();
            }

            var searchResponse = await httpClient.GetFromJsonAsync<DeezerSearchResponse>(
                $"{BaseUrl}/search?q=genre:\"{genre}\"&limit={4}&order=RANKING&strict=on");

            return searchResponse?.Data.Where(t => 
                    !string.IsNullOrEmpty(t.Artist.Name) &&
                    !string.IsNullOrEmpty(t.Title))
                .ToList() ?? [];
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
            DeezerGenre.Jazz => "Jazz",
            DeezerGenre.Metal => "Metal",
            _ => genre.ToString()
        };
    }
}

