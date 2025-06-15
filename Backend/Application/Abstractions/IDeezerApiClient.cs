using Backend.Application.Models;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Abstractions;

public interface IDeezerApiClient
{
    Task<List<DeezerTrack>> GenerateTracksToPlaylistByGenreAsync(DeezerGenre genre);
    
    Task<QuizQuestion> GenerateQuizQuestionAsync(DeezerGenre genre, string questionType);

    string ToDisplayString(DeezerGenre genre);
}