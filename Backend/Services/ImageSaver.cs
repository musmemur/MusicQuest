using Minio.DataModel.Args;
using Minio;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Backend.Services;

public static class ImageSaver
{
    public static async Task<string> SaveImageToS3(byte[] imageBytes, string mimeType, string bucketName)
    {
        const string endpoint = "127.0.0.1:9000";
        const string accessKey = "admin";
        const string secretKey = "password";

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
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .Build();

        using var memoryStream = new MemoryStream(imageBytes);
        await minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(uniqueFileName)
            .WithStreamData(memoryStream)
            .WithObjectSize(memoryStream.Length)
            .WithContentType(mimeType));

        return $"http://{endpoint}/{bucketName}/{uniqueFileName}"; 
    }
}