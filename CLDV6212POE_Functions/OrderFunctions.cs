using Azure;
using Azure.Data.Tables;
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

// As explained by IIE Emeris School of Computer Science (2025), the OrderFunctions class is an Azure Functions implementation that provides CRUD operations for managing order data using Azure Table Storage and Queue Storage
// I implmented the logic from the videos and the CRUD operations from the OrderController in Part 1 of the project

public class OrderFunctions
{
    private readonly TableStorageService _tableStorageService; // handles table storage operations
    private readonly QueueService _queueService; // handles queue storage operations
    private readonly TableClient _tableClient; // azure table client for order data

    public OrderFunctions()
    {
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage"); // gets connection string from environment

        _tableStorageService = new TableStorageService(connectionString); // initializes the TableStorageService with the connection string
        _queueService = new QueueService(connectionString, "orders"); // initializes the QueueService with the connection string and queue name
        _tableClient = new TableClient (connectionString, "Orders"); // connects to the Orders table
        _tableClient.CreateIfNotExists();
    }

    [Function("AddOrder")]
    public async Task<HttpResponseData> AddOrder([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync(); // reads request body
        var order = JsonSerializer.Deserialize<Order>(body); // deserializes json to the order object

        if (order == null)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid order data :(");
            return badResponse;
        }

        order.OrderID = await _tableStorageService.IncrementOrderID(); // increments and assigns a unique OrderID
        order.PartitionKey = "OrderPartition"; // sets the PartitionKey for the entity
        order.RowKey = Guid.NewGuid().ToString(); // generates a unique RowKey for the entity
        order.OrderStatus = "Order Processed"; // sets the default status of the order
        order.OrderDate = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc);

        await _tableClient.AddEntityAsync(order); // adds the order entity to the table

        string message = $"New Order by Customer {order.CustomerID}" // message that gets sent to the queue when a new order is added
                             + $" || for product {order.ProductID}"
                             + $" || scheduled for {order.OrderDate}"
                             + $" || order status = {order.OrderStatus}";

        // sends a message to the queue for further processing
        await _queueService.SendMessage(message);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("Order stored in Azure Table and message sent to Queue ;)");

        return response;
    }

    [Function("GetAllOrders")]
    public async Task<HttpResponseData> GetAllOrders([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        var orders = _tableClient.Query<Order>().ToList(); // fetch all orders

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(orders);

        return response;
    }

    [Function("GetOrder")]
    public async Task<HttpResponseData> GetOrder([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query); // parses query parameters
        var partitionKey = query["partitionKey"];
        var rowKey = query["rowKey"];

        if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid query parameters :(");
            return badResponse;
        }

        try
        {
            var order = await _tableClient.GetEntityAsync<Order>(partitionKey, rowKey); // fetches the specific order
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(order);

            return response;
        }
        catch (RequestFailedException)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Order not found :(");

            return notFound;
        }
    }

    [Function("UpdateOrder")]
    public async Task<HttpResponseData> UpdateOrder([HttpTrigger(AuthorizationLevel.Function, "put")] HttpRequestData req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync(); // reads request body
        var updatedOrder = JsonSerializer.Deserialize<Order>(body); // deserializes json to the order object

        if (updatedOrder == null || string.IsNullOrEmpty(updatedOrder.PartitionKey) || string.IsNullOrEmpty(updatedOrder.RowKey))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid order data :(");
            return badResponse;
        }

        try
        {
            var existing = await _tableClient.GetEntityAsync<Order>(updatedOrder.PartitionKey, updatedOrder.RowKey); // fetches the specific order
            existing.Value.OrderStatus = updatedOrder.OrderStatus; // this will be the only field that gets updated on an order

            await _tableClient.UpdateEntityAsync(existing.Value, ETag.All, TableUpdateMode.Replace); // updates the order entity in the table

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Order updated successfully.");
            return response;
        }
        catch (RequestFailedException)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Order not found.");
            return notFound;
        }
    }

    [Function("DeleteOrder")]
    public async Task<HttpResponseData> DeleteOrder([HttpTrigger(AuthorizationLevel.Function, "delete")] HttpRequestData req)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query); // parses query parameters
        var partitionKey = query["partitionKey"];
        var rowKey = query["rowKey"];

        if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid query parameters :(");
            return badResponse;
        }

        try
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey); // deletes the specific order
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Order deleted successfully ;)");

            return response;
        }
        catch (RequestFailedException)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Order not found :(");
            return notFound;
        }
    }
}

/*

References

IIE Emeris School of Computer Science. (2025). CLDV6212 Azure functions part 3 Azure functions and MVC [video online]. 
Available at: <https://youtu.be/x7yTh85fQbw?si=JP3qYOitBIE8cmPj>
[Accessed 02 October 2025].

*/