using Data.Entities;
using FileProvider.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FileProvider.Funtions
{
    public class Upload(ILogger<Upload> logger, FileService service)
    {
        private readonly ILogger<Upload> _logger = logger;
        private readonly FileService _service = service;

        [Function("Upload")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {


                if(req.Form.Files["file"] is IFormFile file)
                {
                    var containerName = !string.IsNullOrEmpty(req.Query["containerName"]) ? req.Query["containerName"].ToString() : "files";

                    var fileEntity = new FileEntity
                    {
                        FileName = _service.SetFileName(file),
                        ContentType = file.ContentType,
                        ContainerName = containerName
                    };

                    await _service.SetBlobContainerAsync(fileEntity.ContainerName);

                    var filePath = await _service.UploadFileAsync(file, fileEntity);
                    fileEntity.FilePath = filePath; 

                    await _service.SaveToDatabaseAsync(fileEntity);                  
                    return new OkObjectResult(fileEntity);
                } 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return new BadRequestResult();
        }
    }
}
