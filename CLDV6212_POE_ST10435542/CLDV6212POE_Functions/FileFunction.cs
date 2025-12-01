using Azure.Storage.Files.Shares;
using CLDV6212_POE_ST10435542.Models.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Reflection.Metadata;
using System.Text;

namespace CLDV6212POE_Functions;

// As explained by IIE Emeris School of Computer Science (2025), the FileFunction class is an Azure Functions implementation that provides an endpoint for uploading files to Azure File Share
// I implemented the logic from the videos and created an endpoint that allows file uploads to Azure File Share

public class FileFunction
{
    private readonly AzureFileShareService _fileShareService; // initializes the AzureFileShareService to interact with the Azure File Share
    private readonly ILogger<FileFunction> _logger; // logger for logging information and errors

    public FileFunction(AzureFileShareService fileShareService, ILogger<FileFunction> logger)
    {
        _fileShareService = fileShareService;
        _logger = logger;
    }

    [Function("UploadFile")]
    public async Task<HttpResponseData> UploadFileAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "files/upload")] HttpRequestData req)
    {
        var response = req.CreateResponse(); // creates a response object

        try
        {
            string contentType = null; // holds the content type from headers

            if (!req.Headers.TryGetValues("Content-Type", out var values)) // check if header exists
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Missing Content-Type header.");
                return response;
            }

            contentType = values.FirstOrDefault(); // gts the Content-Type header value

            if (contentType == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Content-Type header is empty.");
                return response;
            }

            var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(contentType).Boundary).Value; // extracts the boundary from header

            var reader = new MultipartReader(boundary, req.Body); // reads multipart content
            var section = await reader.ReadNextSectionAsync(); // gets the first file section

            if (section == null) // handles case when no file is sent
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("No file provided.");
                return response;
            }

            var contentDisposition = ContentDispositionHeaderValue.Parse(section.ContentDisposition); // parses file info
            var fileName = contentDisposition.FileName.Value.Trim('"'); // gets filename

            using var stream = new MemoryStream(); // creates a memory stream
            await section.Body.CopyToAsync(stream); // copies the file data to stream
            stream.Position = 0; // reset position before upload

            await _fileShareService.UploadFileAsync("uploads", fileName, stream); // uploads to azure file share

            response.StatusCode = HttpStatusCode.OK;
            await response.WriteStringAsync($"File '{fileName}' uploaded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file.");
            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteStringAsync($"Error uploading file: {ex.Message}");
        }

        return response;
    }
}

/*

References

IIE Emeris School of Computer Science. (2025). CLDV6212 Azure functions part 3 Azure functions and MVC [video online]. 
Available at: <https://youtu.be/x7yTh85fQbw?si=JP3qYOitBIE8cmPj>
[Accessed 02 October 2025].

*/