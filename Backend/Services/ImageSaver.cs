using Minio.DataModel.Args;
using Minio;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class ImageSaver(IOptions<MinioSettings> settings)
{
    private readonly MinioSettings _settings = settings.Value;

    public async Task<string> SaveImageToS3(byte[] imageBytes, string mimeType, string bucketName)
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