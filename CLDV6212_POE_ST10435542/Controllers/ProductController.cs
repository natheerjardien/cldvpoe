using Azure.Storage.Blobs;
using CLDV6212_POE_ST10435542.Models;
using CLDV6212_POE_ST10435542.Models.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc;
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
        private readonly BlobService _blobService; // used to handle image uploads and deletions in azure blob storage
        private readonly string _functionBaseUrl; // stores the base url for azure functions api

        public ProductController(BlobService blobService, IHttpClientFactory httpClientFactory)
        {
            _blobService = blobService;
            _httpClient = httpClientFactory.CreateClient();
            _functionBaseUrl = Environment.GetEnvironmentVariable("ProductFunctionBaseUrl") ?? "http://localhost:7291/api"; // gets the base url from environment variable or uses local fallback
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetAllProducts"); // calls the function to get all products
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(); // reads the response content as a string
            var products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); // deserializes the JSON response to a list of products

            return View(products); // returns the view with the list of products
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            ViewData["Categories"] = new List<string> { "ELectronics", "Clothing", "Home & Kitchen", "Outdoor" }; // populated the dropdown lists with the categories so that it can be used in the AddProduct View
            ViewData["Availability"] = new List<string> { "In Stock", "Out of Stock", "Pre-Order" }; // populated the dropdown lists with the avaialbility statuses so that it can be used in the AddProduct View

            var product = new Product();

            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // uploads the file to blob directly
                using var stream = file.OpenReadStream();
                product.ImageUrl = await _blobService.UploadAsync(stream, file.FileName);
            }

            var productJson = JsonSerializer.Serialize(product); // serializes the product object to JSON
            var content = new StringContent(productJson, Encoding.UTF8, "application/json"); // creates a StringContent object with the JSON content

            var response = await _httpClient.PostAsync($"{_functionBaseUrl}/AddProduct", content); // calls the function to add a new product
            response.EnsureSuccessStatusCode(); // throws an exception if the response shows an error

            ViewData["Categories"] = new List<string> { "ELectronics", "Clothing", "Home & Kitchen", "Outdoor" }; // repopulated the dropdown lists with the categories in the case of validation erros
            ViewData["Availability"] = new List<string> { "In Stock", "Out of Stock", "Pre-Order" }; // repopulated the dropdown lists with the avaialbility statuses

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey, Product product)
        {
            var response = await _httpClient.DeleteAsync($"{_functionBaseUrl}/DeleteProduct?partitionKey={partitionKey}&rowKey={rowKey}"); // calls the function to delete the specific product
            response.EnsureSuccessStatusCode();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ViewProduct(string partitionKey, string rowKey) // method for viewing a specific customer (view their details)
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

            ViewData["Categories"] = new List<string> { "Electronics", "Clothing", "Home & Kitchen", "Outdoor" }; // populated the dropdown lists with the categories so that it can be used in the EditProuct View
            ViewBag.AvailabilityStatuses = new List<SelectListItem> // used ViewBag instead of ViewData because it was not bidning AvailabilityStatuses correctly when it cam to editing products
            { // predefined the availability statuses for the dropdown list so that i can reference it in the EditProduct View
                new SelectListItem { Value = "Available", Text = "Available"},
                new SelectListItem { Value = "Out of Stock", Text = "Out of Stock"},
                new SelectListItem { Value = "Pre-Order", Text = "Pre-Order"},
            };

            return View(product); // returns the view with the product details for editing
        }
        [HttpPost]
        public async Task<IActionResult> EditProduct(string partitionKey, string rowKey, IFormFile? imageFile, Product updatedProduct)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetProduct?partitionKey={partitionKey}&rowKey={rowKey}"); // calls the function to get the specific product
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var existingProduct = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (existingProduct == null)
                {
                    return NotFound();
                }

                if (imageFile != null && imageFile.Length > 0)
                {
                    using var stream = imageFile.OpenReadStream();
                    updatedProduct.ImageUrl = await _blobService.UploadAsync(stream, imageFile.FileName);

                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        await _blobService.DeleteBlobAsync(existingProduct.ImageUrl); // deletes the old image if a new one is uploaded
                    }
                }
                else
                {
                    updatedProduct.ImageUrl = existingProduct.ImageUrl; // keeps the old image if no new one is uploaded
                }

                updatedProduct.PartitionKey = existingProduct.PartitionKey; // ensures the PartitionKey remains unchanged
                updatedProduct.RowKey = existingProduct.RowKey; // ensures the RowKey remains unchanged

                var productJson = JsonSerializer.Serialize(updatedProduct);
                var content = new StringContent(productJson, Encoding.UTF8, "application/json");

                var updatedResponse = await _httpClient.PutAsync($"{_functionBaseUrl}/UpdateProduct?partitionKey={partitionKey}&rowKey={rowKey}", content); // calls the function to update the specific product
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error updating product:ex.Message");
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
