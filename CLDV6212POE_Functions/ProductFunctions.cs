using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CLDV6212_POE_ST10435542.Models;
using CLDV6212_POE_ST10435542.Models.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace CLDV6212POE_Functions;

// As explained by IIE Emeris School of Computer Science (2025), the ProductFunctions class is an Azure Functions implementation that provides CRUD operations for managing product data using Azure Table Storage and Blob Storage
// I implmented the logic from the videos and the CRUD operations from the ProductController in Part 1 of the project

public class ProductFunctions
{
    private readonly BlobService _blobService; // handles blob storage operations
    private readonly TableStorageService _tableStorageService; // handles table storage operations

    public ProductFunctions()
    {
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage"); // gets connection string from environment

        _blobService = new BlobService(connectionString); // initializes the BlobService with the connection string
        _tableStorageService = new TableStorageService(connectionString); // initializes the TableStorageService with the connection string
    }

    [Function("AddProduct")]
    public async Task<HttpResponseData> AddProduct([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        using var reader = new StreamReader(req.Body); // reads the request body
        var bodyString = await reader.ReadToEndAsync(); // reads the entire body as a string

        var product = JsonSerializer.Deserialize<Product>(bodyString); // deserializes json to the product object

        if (product == null)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid product data.");
            return badResponse;
        }

        product.ProductID = await _tableStorageService.IncrementProductID(); // increments and assigns a unique ProductID

        product.PartitionKey = "ProductPartition"; // sets the PartitionKey for the entity
        product.RowKey = Guid.NewGuid().ToString(); // generates a unique RowKey for the entity

        await _tableStorageService.AddProductAsync(product); // adds the product entity to the table

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("Product stored in Azure Table and image uploaded to Blob Storage using Function ;)");

        return response;
    }

    [Function("GetAllProducts")]
    public async Task<HttpResponseData> GetAllProducts([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        var products = await _tableStorageService.GetAllProductsAsync(); // fecthes all the products by calling services from mvc from Part 1

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(products);

        return response;
    }

    [Function("GetProduct")]
    public async Task<HttpResponseData> GetProduct([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query); // parses query parameters
        var partitionKey = query["partitionKey"];
        var rowKey = query["rowKey"];

        var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey); // fetches the specific product by calling services from mvc from Part 1

        if (product == null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Product not found.");

            return notFound;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(product); // return product as json

        return response;
    }

    [Function("UpdateProduct")]
    public async Task<HttpResponseData> UpdateProduct([HttpTrigger(AuthorizationLevel.Function, "put")] HttpRequestData req)
    {
        var body = await req.ReadAsStringAsync(); // reads request body
        var updatedProduct = JsonSerializer.Deserialize<Product>(body); // deserializes json to the product object

        if (updatedProduct == null || string.IsNullOrEmpty(updatedProduct.PartitionKey) || string.IsNullOrEmpty(updatedProduct.RowKey))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid product data :(");
            return badResponse;
        }

        var existingProduct = await _tableStorageService.GetProductAsync(updatedProduct.PartitionKey, updatedProduct.RowKey); // fetches the existing product from table storage

        if (existingProduct == null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Product not found :(");
            return notFound;
        }

        if (!string.IsNullOrEmpty(updatedProduct.ImageUrl) && updatedProduct.ImageUrl != existingProduct.ImageUrl)
        {
            if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
            {
                await _blobService.DeleteBlobAsync(existingProduct.ImageUrl); // deletes the old image from blob storage
            }
        }

        existingProduct.ProductName = updatedProduct.ProductName; // updates the existing product's properties
        existingProduct.Category = updatedProduct.Category;
        existingProduct.Description = updatedProduct.Description;
        existingProduct.AvailabilityStatus = updatedProduct.AvailabilityStatus;
        existingProduct.ImageUrl = updatedProduct.ImageUrl;

        var updatedSuccess = await _tableStorageService.UpdateProductAsync(existingProduct); // updates the product entity in the table

        var response = req.CreateResponse(updatedSuccess ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
        await response.WriteStringAsync(updatedSuccess ? "Product updated successfully using Function ;)" : "Error updating product :(");

        return response;
    }

    [Function("DeleteProduct")]
    public async Task<HttpResponseData> DeleteProduct([HttpTrigger(AuthorizationLevel.Function, "delete")] HttpRequestData req)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query); // parses query parameters
        var partitionKey = query["partitionKey"];
        var rowKey = query["rowKey"];

        var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey); // fetches the specific product by calling services from mvc from Part 1

        if (product == null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Product not found :(");

            return notFound;
        }

        if (!string.IsNullOrEmpty(product.ImageUrl))
        {
            await _blobService.DeleteBlobAsync(product.ImageUrl); // deletes the image from blob storage
        }

        await _tableStorageService.DeleteProductAsync(partitionKey, rowKey); // deletes the product entity from the table

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("Product deleted successfully using Function ;)");

        return response;
    }
}

/*

References

IIE Emeris School of Computer Science. (2025). CLDV6212 Azure functions part 3 Azure functions and MVC [video online]. 
Available at: <https://youtu.be/x7yTh85fQbw?si=JP3qYOitBIE8cmPj>
[Accessed 01 October 2025].

*/