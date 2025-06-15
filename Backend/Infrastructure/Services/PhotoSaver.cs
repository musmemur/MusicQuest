using Backend.Application.Abstractions;
using Backend.Configurations.Options;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Backend.Infrastructure.Services;

public class PhotoSaver(IOptions<MinioSettings> settings) : IPhotoSaver
{
    private readonly MinioSettings _settings = settings.Value;

    public async Task<string> SavePhotoToS3(byte[] imageBytes, string mimeType, string bucketName)
    {
        var fileExtension = mimeType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            _ => throw new Exception("Unsupported image format")
        };

        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

        // Создаем клиент MinIO
        var minioClient = new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithSSL(_settings.UseSsl)
            .Build();

        using var memoryStream = new MemoryStream(imageBytes);
        await minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(uniqueFileName)
            .WithStreamData(memoryStream)
            .WithObjectSize(memoryStream.Length)
            .WithContentType(mimeType));

        return $"http://{_settings.Endpoint}/{bucketName}/{uniqueFileName}"; 
    }
}