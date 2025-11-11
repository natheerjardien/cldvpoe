using Azure;
using Azure.Data.Tables;
using CLDV6212_POE_ST10435542.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace CLDV6212POE_Functions;

// According to IIE Emeris School of Computer Science (2025), the CustomerFunctions class is an Azure Functions implementation that provides CRUD operations for managing customer data using Azure Table Storage
// I implmented the logic from the videos and the CRUD operations from the CustomerController in Part 1 of the project
public class CustomerFunctions
{
    private readonly TableClient _tableClient; // azure table client for customer data
    public CustomerFunctions()
    {
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage"); // gets connection string from environment
        _tableClient = new TableClient(connectionString, "Customers"); // connects to the customers table
        _tableClient.CreateIfNotExists(); // createz the table if it doesnt exist
    }

    [Function("AddCustomer")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var body = await req.ReadAsStringAsync(); // reads request body
        var customer = JsonSerializer.Deserialize<Customer>(body); // deserializes json to the customer object

        if (customer == null)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest); // handles invalid input
            await badResponse.WriteStringAsync("Invalid customer data.");
            return badResponse;
        }

        customer.PartitionKey = "CustomersPartition"; // Set PartitionKey for the entity

        if (string.IsNullOrEmpty(customer.RowKey))
        {
            customer.RowKey = Guid.NewGuid().ToString(); // Generate a unique RowKey for the entity if not provided
        }

        await _tableClient.AddEntityAsync(customer); // add customer to table

        var response = req.CreateResponse(HttpStatusCode.OK); // returns success
        await response.WriteStringAsync("Customer stored in Azure Table.");

        return response;
    }

    [Function("GetAllCustomers")]
    public async Task<HttpResponseData> GetAllCustomers([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        var customers = _tableClient.Query<Customer>().ToList(); // fetch all customers
        var response = req.CreateResponse(HttpStatusCode.OK); 
        await response.WriteAsJsonAsync(customers); // return customers as json

        return response;
    }

    [Function("GetCustomer")]
    public async Task<HttpResponseData> GetCustomer([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query); // parses query parameters
        var partitionKey = query["partitionKey"];
        var rowKey = query["rowKey"];

        try
        {
            var customer = await _tableClient.GetEntityAsync<Customer>(partitionKey, rowKey); // fetches the specific customer
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(customer.Value);

            return response;
        }
        catch (RequestFailedException)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync("Customer not found.");

            return notFoundResponse;
        }
    }

    [Function("UpdateCustomer")]
    public async Task<HttpResponseData> UpdateCustomer([HttpTrigger(AuthorizationLevel.Function, "put")] HttpRequestData req)
    {
        var body = await req.ReadAsStringAsync(); // reads request body
        var customer = JsonSerializer.Deserialize<Customer>(body); // deserializes json to the customer object

        if (customer == null || string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest); // handles invalid input
            await badResponse.WriteStringAsync("Unable to update customer due to invalid data.");

            return badResponse;
        }

        try
        {
            await _tableClient.UpdateEntityAsync(customer, ETag.All, TableUpdateMode.Replace); // updates the customer entity in the table
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Customer updated succesfully ;)");

            return response;
        }
        catch (RequestFailedException)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync("Customer not found :(");

            return notFoundResponse;
        }
    }

    [Function("DeleteCustomer")]
    public async Task<HttpResponseData> DeleteCustomer([HttpTrigger(AuthorizationLevel.Function, "delete")] HttpRequestData req)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query); // reads keys from query
        var partitionKey = query["partitionKey"];
        var rowKey = query["rowKey"];

        try
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey); // deletes the customer entity from the table
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Customer deleted succesfully ;)");

            return response;
        }
        catch (RequestFailedException)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync("Customer not found :(");

            return notFoundResponse;
        }
    }
}

/*

References

IIE Emeris School of Computer Science. (2025). CLDV6212 Azure functions part 3 Azure functions and MVC [video online]. 
Available at: <https://youtu.be/x7yTh85fQbw?si=JP3qYOitBIE8cmPj>
[Accessed 30 September 2025].

*/