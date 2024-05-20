using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Mime;

namespace FileProvider.Services;

public class FileService(DataContext context, ILogger<FileService> logger, BlobServiceClient blobClient)
{
    private readonly DataContext _context = context;
    private readonly ILogger<FileService> _logger = logger;
    private readonly BlobServiceClient _blobClient = blobClient;
  
   


    public async Task SetBlobContainerAsync(string containerName)
    {
        var container = _blobClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync();
    }



    public string SetFileName(IFormFile file)
    {
        return  $"{Guid.NewGuid()}_{file.FileName}";
      
    }

    public async Task<string> UploadFileAsync(IFormFile file, FileEntity fileEntity)
    {
        var container = _blobClient.GetBlobContainerClient(fileEntity.ContainerName);
        var blobClient = container.GetBlobClient(fileEntity.FileName);

        BlobHttpHeaders headers = new BlobHttpHeaders { ContentType = file.ContentType };

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, headers);

        return blobClient.Uri.ToString();
    }


    public async Task SaveToDatabaseAsync(FileEntity fileEntity)
    {
        _context.Files.Add(fileEntity);
        await _context.SaveChangesAsync();
    }

}
