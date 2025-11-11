using CLDV6212_POE_ST10435542.Models.Services;
using CLDV6212_POE_ST10435542.Models;
using Microsoft.AspNetCore.Mvc;

namespace CLDV6212_POE_ST10435542.Controllers
{
// As demonstrated by IIEVC School of Computer Science (2025), the FilesController is responsible for managing file-related actions such as uploading, downloading, and listing files in the Azure File Share
    public class FilesController : Controller
    {
        private readonly AzureFileShareService _fileShareService;
        private readonly HttpClient _httpClient;

        public FilesController(AzureFileShareService fileShareService, HttpClient httpClient) // initializes the AzureFileShareService to interact with the Azure File Share
        {
            _fileShareService = fileShareService;
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index(string dateFilter = null)
        {
            List<FileModel> files;

            try
            {
                files = await _fileShareService.ListFilesAsync("uploads"); // retrieves the list of files from the Azure File Share in the "uploads" directory
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Failed to load files :{ex.Message}";
                files = new List<FileModel>();
            }

            // implementing the same filtering logic from Semester 3 POE (altered to filter by date)
            if (!string.IsNullOrEmpty(dateFilter))
            {
                var now = DateTimeOffset.UtcNow; // a reference point for calculating the specified date ranges

                files = dateFilter.ToLower() switch // used a switch case to differentiate the date ranges
                {
// According to StackOverflow (2016), ViewBag can be used to populate a select list without using HTML helpers. This structure defines the criteria for each switch case (filtering by days, weeks, months)
                    "today" => files.Where(f => f.LastModified?.Date == now.Date).ToList(), // filters files modified today
                    "week" => files.Where(f => f.LastModified >= now.AddDays(-7)).ToList(), // filters files modified in the last week
                    "month" => files.Where(f => f.LastModified >= now.AddMonths(-1)).ToList(), // filters files modified in the last month
                    _ => files // if no valid date filter is given the it will return all the files
                };
            }

            ViewBag.SelectedDateFilter = dateFilter; // loads the select list for selecting a date range for filtering files

            return View(files); // returns the view with the list of files
        }
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file) // method for uploading files to the Azure File Share
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please select a file to upload");

                return await Index();
            }

            try
            {
                using var content = new MultipartFormDataContent(); // creates a multipart form data
                using var stream = file.OpenReadStream(); // opens file stream
                content.Add(new StreamContent(stream), "file", file.FileName); // adds the file to form content

                string functionUrl = "http://localhost:7291/api/files/upload"; // url of azure function

                var response = await _httpClient.PostAsync(functionUrl, content); // calls the function to upload the file

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = $"File '{file.FileName}' uploaded succesfully!"; // sets a success message in TempData to be displayed after the redirect
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"File upload failed : {error}"; // sets an error message in TempData to be displayed after the redirect
                }

            }
            catch (Exception e)
            {
                TempData["Message"] = $"File upload failed :{e.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(string fileName) // method for downloading files from the Azure File Share
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name cannot be null or empty");
            }

            try
            {
                var fileStream = await _fileShareService.DownLoadFileAsync("uploads", fileName); // retrieves the file stream from the Azure File Share using the DownLoadFileAsync method in the AzureFileShareService

                if (fileStream == null)
                {
                    return NotFound($"File '{fileName}' not found");
                }

                return File(fileStream, "application/octet-stream", fileName); // returns the file stream as a downloadable file with the specified content type and file name
            }
            catch (Exception e)
            {
                return BadRequest($"Error downloading file :{e.Message}");
            }
        }
    }
}

/* References:

IIEVC School of Computer Science, 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 4: Mastering Azure File Share!. [video online] 
Available at: <https://youtu.be/A-mVVL88oEg?si=eIL4gyih_S6aWw2a>
[Accessed 21 August 2025].

StackOverflow, 2016. Populate Select using ViewBag without using html Helper. [online]
Available at: <https://stackoverflow.com/questions/39755802/populate-select-using-viewbag-without-using-html-helper> [Accessed 23 August 2025].

*/
