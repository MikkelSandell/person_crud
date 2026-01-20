using Minio;
using Minio.DataModel.Args;

namespace CrudApp.Backend.Services
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<Stream> GetFileAsync(string fileName);
        Task<bool> DeleteFileAsync(string fileName);
        Task<string> GetFileUrlAsync(string fileName);
        Task<bool> FileExistsAsync(string fileName);
    }

    public class FileService : IFileService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;
        private readonly ILogger<FileService> _logger;

        public FileService(IMinioClient minioClient, IConfiguration configuration, ILogger<FileService> logger)
        {
            _minioClient = minioClient;
            _bucketName = configuration["MinIO:BucketName"] ?? "profile-pictures";
            _logger = logger;
            EnsureBucketExistsAsync().Wait();
        }

        private async Task EnsureBucketExistsAsync()
        {
            try
            {
                var bucketExistsArgs = new BucketExistsArgs()
                    .WithBucket(_bucketName);
                bool found = await _minioClient.BucketExistsAsync(bucketExistsArgs).ConfigureAwait(false);

                if (!found)
                {
                    var makeBucketArgs = new MakeBucketArgs()
                        .WithBucket(_bucketName);
                    await _minioClient.MakeBucketAsync(makeBucketArgs).ConfigureAwait(false);
                    _logger.LogInformation($"Bucket '{_bucketName}' created successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error ensuring bucket '{_bucketName}' exists.");
                throw;
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                _logger.LogInformation($"File '{fileName}' uploaded successfully.");
                
                return await GetFileUrlAsync(fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file '{fileName}'.");
                throw;
            }
        }

        public async Task<Stream> GetFileAsync(string fileName)
        {
            try
            {
                var memoryStream = new MemoryStream();
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName)
                    .WithCallbackStream((stream) =>
                    {
                        stream.CopyTo(memoryStream);
                    });

                await _minioClient.GetObjectAsync(getObjectArgs).ConfigureAwait(false);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting file '{fileName}'.");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName);

                await _minioClient.RemoveObjectAsync(removeObjectArgs).ConfigureAwait(false);
                _logger.LogInformation($"File '{fileName}' deleted successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file '{fileName}'.");
                return false;
            }
        }

        public async Task<string> GetFileUrlAsync(string fileName)
        {
            try
            {
                var presignedGetObjectArgs = new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName)
                    .WithExpiry(60 * 60 * 24 * 7); // 7 days

                string url = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs).ConfigureAwait(false);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting URL for file '{fileName}'.");
                throw;
            }
        }

        public async Task<bool> FileExistsAsync(string fileName)
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName);

                await _minioClient.StatObjectAsync(statObjectArgs).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
