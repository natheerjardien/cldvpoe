using Azure.Storage.Blobs;
using CLDV6212_POE_ST10435542.Models;
using CLDV6212_POE_ST10435542.Models.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc;
using System.Configuration;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CLDV6212_POE_ST10435542.Controllers
{
// As demonstarted by IIEVC School of Computer Science (2025), the ProductController is responsible for managing product-related actions such as adding, viewing, editing, and deleting products in the application
// Ive added the necessary methods to handle these actions, and the controller interacts with the BlobService and TableStorageService to perform operations like uploading images to Blob conatiner, and data to the Product table
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient; // used to send http requests to azure functions
        private readonly BlobService _blobService;
        private readonly TableStorageService _tableStorageService;
        private readonly string _functionBaseUrl; // stores the base url for azure functions api

        public ProductController(IHttpClientFactory httpClientFactory, BlobService blobService, TableStorageService tableStorageService, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _blobService = blobService;
            _tableStorageService = tableStorageService;
            _functionBaseUrl = configuration.GetValue<string>("AzureFunctionSettings:BaseUrl") ?? "http://localhost:7291/api";
        }

        private void PopulateDropdowns(Product? product = null)
        {
            ViewBag.Categories = new List<string> { "Electronics", "Clothing", "Home & Kitchen", "Outdoor" };

            ViewBag.AvailabilityStatuses = new List<string>
            {
                "In Stock",
                "Out of Stock",
                "Pre-Order"
            };
        }

        public async Task<IActionResult> Index()
        {
            List<Product> products = new List<Product>();
            try
            {
                var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetAllProducts");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    products = JsonSerializer.Deserialize<List<Product>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    TempData["Error"] = $"Error fetching products from Function: {response.ReasonPhrase}";
                }
            }
            catch (Exception ew)
            {
                TempData["Error"] = "Exception occurred while fetching products: " + ew.Message;
            }

            return View(products); // returns the view with the list of products
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            PopulateDropdowns();

            var product = new Product();

            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            ModelState.Remove(nameof(product.ImageUrl));
            ModelState.Remove(nameof(product.PartitionKey));
            ModelState.Remove(nameof(product.RowKey));

            if (!ModelState.IsValid || product.ImageFile == null || product.ImageFile.Length == 0)
            {
                PopulateDropdowns();
                TempData["Error"] = "Validation failed. Please select an image and check all required fields.";
                return View(product);
            }

            try
            {
                var rowKey = Guid.NewGuid().ToString();
                var nextId = await _tableStorageService.IncrementProductID();

                string containerName = "productimages";
                string fileName = $"{rowKey}_{Path.GetFileName(product.ImageFile.FileName)}";

                // This call uploads the file stream and returns the public URL
                string imageUrl = await _blobService.UploadAsync(product.ImageFile, containerName, fileName);

                if (string.IsNullOrEmpty(imageUrl))
                {
                    TempData["Error"] = "Image upload failed. Cannot proceed.";
                    return View(product);
                }

                var productDto = new ProductTableEntity
                {
                    ProductID = nextId,
                    ProductName = product.ProductName,
                    ProductDescription = product.Description,
                    Category = product.Category,
                    AvailabilityStatus = product.AvailabilityStatus,
                    ImageUrl = imageUrl, // sends the URL
                    PartitionKey = "ProductsPartition",
                    RowKey = rowKey
                };

                var json = JsonSerializer.Serialize(productDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_functionBaseUrl}/AddProduct", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Failed to add product via Function: {errorContent}";

                    return View(product);
                }

                TempData["Message"] = "Product added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                PopulateDropdowns();
                TempData["Error"] = $"An exception occurred: {ex.Message}";
                return View(product);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey)
        {
            try
            {
                // 1. Get the product from the Azure Function or Table Storage directly
                var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetProduct?partitionKey={partitionKey}&rowKey={rowKey}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"Product not found: {response.ReasonPhrase}";
                    return RedirectToAction("Index");
                }

                var json = await response.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                // 2. Delete the blob if it exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    await _blobService.DeleteBlobAsync(product.ImageUrl);
                }

                // 3. Delete the product from the Azure Function/Table Storage
                var deleteResponse = await _httpClient.DeleteAsync($"{_functionBaseUrl}/DeleteProduct?partitionKey={partitionKey}&rowKey={rowKey}");

                if (deleteResponse.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Product and its image deleted successfully!";
                }
                else
                {
                    var errorContent = await deleteResponse.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Error deleting product: {deleteResponse.ReasonPhrase}. Details: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Exception occurred while deleting product: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ViewProduct(string partitionKey, string rowKey) // method for viewing a specific customer (view their details)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetProduct?partitionKey={partitionKey}&rowKey={rowKey}"); // calls the function to get the specific product

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var json = await response.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View(product); // returns the view with the customer details
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving product details: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(string partitionKey, string rowKey)
        {
            var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetProduct?partitionKey={partitionKey}&rowKey={rowKey}"); // calls the function to get the specific product
            
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            PopulateDropdowns(product);

            return View(product); // returns the view with the product details for editing
        }
        [HttpPost]
        public async Task<IActionResult> EditProduct(IFormFile? imageFile, Product product)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(product);
                TempData["Error"] = "Please ensure all required fields are filled in.";
                return View(product);
            }

            try
            {
                // FIX: Bundle metadata and optional new file into MultipartFormDataContent for the Function
                using var content = new MultipartFormDataContent();

                // 1. Add new file (if present)
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileStreamContent = new StreamContent(imageFile.OpenReadStream());
                    content.Add(fileStreamContent, "imageFile", imageFile.FileName);
                }

                // 2. Add product metadata, including PK/RK needed for the update in the Function
                content.Add(new StringContent(product.PartitionKey ?? ""), "PartitionKey");
                content.Add(new StringContent(product.RowKey ?? ""), "RowKey");
                content.Add(new StringContent(product.ProductName ?? ""), "ProductName");
                content.Add(new StringContent(product.Category ?? ""), "Category");
                content.Add(new StringContent(product.Description ?? ""), "Description");
                content.Add(new StringContent(product.AvailabilityStatus.ToString()), "AvailabilityStatus");

                var response = await _httpClient.PutAsync($"{_functionBaseUrl}/UpdateProduct", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Product updated successfully using Azure Function ;)";
                }
                else
                {
                    PopulateDropdowns(product);
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Error updating product via Function: {response.ReasonPhrase}. Details: {errorContent}";
                    return View(product);
                }
            }

            catch (Exception ex)
            {
                PopulateDropdowns(product);
        TempData["Error"] = "Exception occurred calling Azure Function: " + ex.Message;
                return View(product);
    }

            return RedirectToAction("Index"); // redirects to the Index action to view the updated product list
        }
    }
}

/* References:

IIEVC School of Computer Science, 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 2: Adding Image Uploads with Blob Storage!. [video online] 
Available at: <https://youtu.be/CuszKqZvRuM?si=RZaHcDniR_ZWB-59>
[Accessed 16 August 2025].

*/
