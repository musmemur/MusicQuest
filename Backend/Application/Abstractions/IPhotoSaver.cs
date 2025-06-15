namespace Backend.Application.Abstractions;

public interface IPhotoSaver
{
    Task<string> SavePhotoToS3(byte[] imageBytes, string mimeType, string bucketName);
}