using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CLDV6212_POE_ST10435542.Models;
using CLDV6212_POE_ST10435542.Models.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Text.Json;

namespace CLDV6212POE_Functions;

// As explained by IIE Emeris School of Computer Science (2025), the ProductFunctions class is an Azure Functions implementation that provides CRUD operations for managing product data using Azure Table Storage and Blob Storage
// I implmented the logic from the videos and the CRUD operations from the ProductController in Part 1 of the project

public class ProductFunctions
{
    private readonly ILogger<ProductFunctions> _logger; // logger for logging information
    private readonly TableClient _tableClient;

    public ProductFunctions(ILogger<ProductFunctions> logger)
    {
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        _logger = logger;
        _tableClient = new TableClient(connectionString, "Products");
        _tableClient.CreateIfNotExists();
    }

    [Function("AddProduct")]
    public async Task<HttpResponseData> AddProduct([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("AddProduct function processed a request.");

        var body = await req.ReadAsStringAsync();

        var productDto = JsonSerializer.Deserialize<ProductTableEntity>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (productDto == null || string.IsNullOrEmpty(productDto.ProductName))
        {
            _logger.LogError("Invalid product data received.");
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid product data structure in request body.");
            return badResponse;
        }

        try
        {
            var productEntity = new Product
            {
                ProductID = productDto.ProductID,
                ProductName = productDto.ProductName,
                Description = productDto.ProductDescription,
                Category = productDto.Category,
                AvailabilityStatus = productDto.AvailabilityStatus,
                ImageUrl = productDto.ImageUrl, // URL saved from the MVC
                PartitionKey = "ProductsPartition",
                RowKey = productDto.ProductID.ToString(),
            };

            await _tableClient.AddEntityAsync(productEntity);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync($"Product {productDto.ProductName} stored successfully.");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing product in Azure Table.");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Internal server error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("GetAllProducts")]
    public async Task<HttpResponseData> GetAllProducts([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("GetAllProducts function processed a request.");

        try
        {
            var products = new List<Product>();
            await foreach (var product in _tableClient.QueryAsync<Product>())
            {
                products.Add(product);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(products);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products.");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [Function("GetProduct")]
    public async Task<HttpResponseData> GetProduct([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("GetProduct function processed a request.");

        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var partitionKey = query["partitionKey"];
        var rowKey = query["rowKey"];

        if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
        {
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Missing partitionKey or rowKey.");
        }

        try
        {
            var productResponse = await _tableClient.GetEntityAsync<Product>(partitionKey, rowKey);

            if (productResponse?.Value == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Product not found.");
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(productResponse.Value);
            return response;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Product not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product.");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [Function("UpdateProduct")]
    public async Task<HttpResponseData> UpdateProduct([HttpTrigger(AuthorizationLevel.Function, "put")] HttpRequestData req)
    {
        _logger.LogInformation("UpdateProduct function processed a request.");

        try
        {
            var boundary = GetBoundary(req.Headers.GetValues("Content-Type").First());
            var reader = new MultipartReader(boundary, req.Body);

            var tempProduct = new Product();
            string? newImageUrl = null;
            bool fileUploaded = false;

            MultipartSection? section;
            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                var contentDisposition = ContentDispositionHeaderValue.Parse(section.ContentDisposition);

                if (!string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    var fileName = contentDisposition.FileName.Value.Trim('"');
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        fileUploaded = true;
                        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                        //var blobClient = _container.GetBlobClient(uniqueFileName);

                        //await blobClient.UploadAsync(section.Body, new BlobHttpHeaders { ContentType = section.ContentType });
                        //newImageUrl = blobClient.Uri.ToString();
                    }
                }
                else // Form field data
                {
                    var fieldName = contentDisposition.Name.Value;
                    using var streamReader = new StreamReader(section.Body);
                    var fieldValue = await streamReader.ReadToEndAsync();

                    var property = typeof(Product).GetProperty(fieldName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (property != null)
                    {
                        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        var convertedValue = Convert.ChangeType(fieldValue, targetType);
                        property.SetValue(tempProduct, convertedValue);
                    }
                }
            }

            if (string.IsNullOrEmpty(tempProduct.PartitionKey) || string.IsNullOrEmpty(tempProduct.RowKey))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid product data. PartitionKey and RowKey are required.");
            }

            var existingProductResponse = await _tableClient.GetEntityAsync<Product>(tempProduct.PartitionKey, tempProduct.RowKey);
            var existingProduct = existingProductResponse.Value;

            if (fileUploaded)
            {
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    var oldBlobName = new Uri(existingProduct.ImageUrl).Segments.Last();
                    //var oldBlobClient = _containerClient.GetBlobClient(oldBlobName);
                    //await oldBlobClient.DeleteIfExistsAsync();
                }
                existingProduct.ImageUrl = newImageUrl;
            }

            existingProduct.ProductName = tempProduct.ProductName;
            existingProduct.Category = tempProduct.Category;
            existingProduct.Description = tempProduct.Description;
            existingProduct.AvailabilityStatus = tempProduct.AvailabilityStatus;

            await _tableClient.UpdateEntityAsync(existingProduct, ETag.All, TableUpdateMode.Replace);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Product updated successfully.");
            return response;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Product not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product.");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [Function("DeleteProduct")]
    public async Task<HttpResponseData> DeleteProduct([HttpTrigger(AuthorizationLevel.Function, "delete")] HttpRequestData req)
    {
        _logger.LogInformation("DeleteProduct function processed a request.");

        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var partitionKey = query["partitionKey"];
        var rowKey = query["rowKey"];

        if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
        {
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Missing partitionKey or rowKey.");
        }

        try
        {
            var product = await _tableClient.GetEntityAsync<Product>(partitionKey, rowKey);

            if (product?.Value != null && !string.IsNullOrEmpty(product.Value.ImageUrl))
            {
                var blobName = new Uri(product.Value.ImageUrl).Segments.Last();
                //var blobClient = _containerClient.GetBlobClient(blobName);
                //await blobClient.DeleteIfExistsAsync();
            }

            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);

            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Product not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product.");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    // helper methods
    private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode code, string message)
    {
        var response = req.CreateResponse(code);
        await response.WriteStringAsync(message);
        return response;
    }

    private static string GetBoundary(string contentType)
    {
        var elements = contentType.Split(';');
        var boundary = elements.FirstOrDefault(e => e.Trim().StartsWith("boundary="));
        if (boundary == null) throw new InvalidOperationException("Missing boundary in Content-Type header.");
        return boundary.Substring("boundary=".Length).Trim('"');
    }
}

/*

References

IIE Emeris School of Computer Science. (2025). CLDV6212 Azure functions part 3 Azure functions and MVC [video online]. 
Available at: <https://youtu.be/x7yTh85fQbw?si=JP3qYOitBIE8cmPj>
[Accessed 01 October 2025].

*/