using CLDV6212_POE_ST10435542.Models;
using CLDV6212_POE_ST10435542.Models.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CLDV6212_POE_ST10435542.Controllers
{
// As demonstrated by IIEVC School of Computer Science (2025), the CustomerController is responsible for managing customer-related actions such as adding, viewing, editing, and deleting customers in the application
// Ive added the necessary methods to handle these actions, and the controller interacts with the TableStorageService to perform operations on the Azure Table Storage
    public class CustomerController : Controller
    {
        //private readonly TableStorageService _tableStorageService;
        private readonly HttpClient _httpClient;
        private readonly string _functionBaseUrl = "http://localhost:7291/api";

        public CustomerController(/*TableStorageService tableStorageService*/ HttpClient httpClient)
        {
            //_tableStorageService = tableStorageService;
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            //var customers = await _tableStorageService.GetAllCustomersAsync(); // fetches all customers from the table storage
            List<Customer> customers = new List<Customer>();

            try
            {
                var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetAllCustomers"); // calls the function to get all customers
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    customers = JsonSerializer.Deserialize<List<Customer>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }); // deserializes the JSON response to a list of customers
                }
                else
                {
                    TempData["Error"] = "Error fetching customers using Azure Function :(";
                }
            }
            catch (Exception ew)
            {
                TempData["Error"] = "Exception occurred while fetching customers: " + ew.Message;
            }

            return View(customers); // returns the view with the list of customers
        }

        [HttpGet]
        public IActionResult AddCustomer()
        {
            return View();
        }    
        [HttpPost]
        public async Task<IActionResult> AddCustomer(Customer customer)
        {
            customer.PartitionKey = "CustomersPartition"; // Set PartitionKey for the entity
            customer.RowKey = Guid.NewGuid().ToString(); // Generate a unique RowKey for the entity
            customer.CustomerID = customer.RowKey; // assigned the rowkey to the CustomerId so that it does not appear as 0 each time a customer is added. This way the CustomerID will always be unique

            var jsonContent = JsonSerializer.Serialize(customer); // serializes the customer object to JSON format
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"); // creates a StringContent object with the JSON content

            var response = await _httpClient.PostAsync($"{_functionBaseUrl}/AddCustomer", content); // calls the function

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Customer added successfully using Azure Function ;)";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Error adding customer using Azure Function :(";
                return View(customer);
            }

            //await _tableStorageService.AddCustomerAsync(customer); // adds the customer to the table storage
            //return RedirectToAction("Index");
        }

        public async Task<IActionResult> ViewCustomer(string partitionKey, string rowKey) // method for viewing a specific customer (view their details)
        {
            //var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey); // gets individual customer for ViewCustomer View using their partitionKey and rowKey

            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetCustomer?partitionKey={partitionKey}&rowKey={rowKey}"); // calls the function to get the specific customer

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var customer = JsonSerializer.Deserialize<Customer>(json);

            return View(customer); // returns the view with the customer details
        }

        public async Task<IActionResult> DeleteCustomer(string partitionKey, string rowKey)
        {
            // Delete Table Entity
            //await _tableStorageService.DeleteCustomerAsync(partitionKey, rowKey); // deletes the customer by partitionKey and rowKey

            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var response = await _httpClient.DeleteAsync($"{_functionBaseUrl}/DeleteCustomer?partitionKey={partitionKey}&rowKey={rowKey}"); // calls the function to delete the specific customer

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            TempData["Message"] = "Customer deleted successfully using Azure Function ;)";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditCustomer(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) == null || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetCustomer?partitionKey={partitionKey}&rowKey={rowKey}"); // calls the function to get the specific customer

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var customer = JsonSerializer.Deserialize<Customer>(json);

            //var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey); // calls the GetCustomerAsync method to get the customer by partitionKey and rowKey

            return View(customer); // returns the view with the customers details for editing
        }

        [HttpPost]
        public async Task<IActionResult> EditCustomer(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please ensure all fields are filled in.";
                return View(customer);
            }

            var jsonContent = JsonSerializer.Serialize(customer); // serializes the customer object to JSON format
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"); // creates a StringContent object with the JSON content

            var response = await _httpClient.PutAsync($"{_functionBaseUrl}/UpdateCustomer", content); // calls the function to update the specific customer

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error updating customer using Azure Function :(";
                return View(customer);
            }

            //var updated = await _tableStorageService.UpdateCustomerAsync(customer); // updates the customers details in the table storage

            //if (!updated)
            //{
            //    ModelState.AddModelError("", "Unable to update customer");
            //    return View(customer); // returns the view with the customers details if the update was not successful
            //}

            TempData["Message"] = "Customer updated successfully using Azure Function ;)";

            return RedirectToAction("Index"); // redirects to the Index action if the update was successful
        }
    }
}

/* References:

IIEVC School of Computer Science, 2025. CLDV6212 Building a Modern Web App with Azure Table Storage & ASP.NET Core MVC - Part 1. [video online] 
Available at: <https://youtu.be/Txp7VYUMBGQ?si=5sD7TV0vS90-pPHY>
[Accessed 15 August 2025].

*/
