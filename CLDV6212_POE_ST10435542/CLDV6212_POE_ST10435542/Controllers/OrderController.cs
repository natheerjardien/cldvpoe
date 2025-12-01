using CLDV6212_POE_ST10435542.Models;
using CLDV6212_POE_ST10435542.Models.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace CLDV6212_POE_ST10435542.Controllers
{
    public class OrderController : Controller
    {
        private readonly HttpClient _httpClient; // used to call azure functions
        private readonly string _functionBaseUrl = "http://localhost:7291/api"; // base url for azure functions

        public OrderController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index() // loads all orders
        {
            List<OrderViewModel> orders = new();

            try
            {
                var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetAllOrders"); // calls the function to get all orders

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync(); // reads json data
                    orders = JsonSerializer.Deserialize<List<OrderViewModel>>(jsonResponse, new JsonSerializerOptions{PropertyNameCaseInsensitive = true}); // deserializes the JSON response to a list of orders
                }
                else
                {
                    TempData["Error"] = "Error fetching orders using Azure Function :(";
                }
            }
            catch (Exception ew)
            {
                TempData["Error"] = "Exception occurred while fetching orders: " + ew.Message;
            }

            return View(orders); // returns view with orders
        }

        [HttpGet]
        public async Task<IActionResult> AddOrder() // loads add order form
        {
            await PopulateCustomersAndProducts(); // loads dropdown data

            return View(new OrderViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrder(OrderViewModel orderViewModel) // saves new order
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToList();

                await PopulateCustomersAndProducts();
                ModelState.AddModelError("", "Please fill in all fields and try again.");
                return View(orderViewModel);
            }

            var order = new Order // maps viewmodel to model
            {
                CustomerID = orderViewModel.CustomerID,
                ProductID = orderViewModel.ProductID,
                OrderDate = orderViewModel.OrderDate
            };

            var json = JsonSerializer.Serialize(order); // serializes the order object to JSON format
            var content = new StringContent(json, Encoding.UTF8, "application/json"); // creates a StringContent object with the JSON content
            var response = await _httpClient.PostAsync($"{_functionBaseUrl}/AddOrder", content); // calls the function to add the order

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error adding order using Azure Function :(";
                await PopulateCustomersAndProducts();

                return View(orderViewModel);
            }

            TempData["Message"] = "Order added successfully using Azure Function ;)";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteOrder(string partitionKey, string rowKey) // deletes an order
        {
            var response = await _httpClient.DeleteAsync($"{_functionBaseUrl}/DeleteOrder?partitionKey={partitionKey}&rowKey={rowKey}"); // calls the function to delete the order

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error deleting order using Azure Function :(";
            }
            else
            {
                TempData["Message"] = "Order deleted successfully using Azure Function ;)";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ViewOrder(string partitionKey, string rowKey) // method for viewing a specific order (view their details)
        {
            var order = await GetOrder(partitionKey, rowKey); // calls the GetOrder method to get the order by partitionKey and rowKey

            if (order == null)
            {
                TempData["Error"] = "Error fetching order using Azure Function :(";
                return RedirectToAction("Index");
            }

            var orderViewModel = await MapOrderToViewModel(order); // maps data to viewmodel

            return View(orderViewModel); // returns the view with the order details
        }

        public async Task<IActionResult> EditOrder(string partitionKey, string rowKey) // loads edit form
        {
            var order = await GetOrder(partitionKey, rowKey); // calls the GetOrder method to get the order by partitionKey and rowKey
            
            if (order == null)
            {
                TempData["Error"] = "Error fetching order using Azure Function :(";

                return RedirectToAction("Index");
            }

            var orderViewModel = await MapOrderToViewModel(order);

            await PopulateCustomersAndProducts();
            PopulateOrderStatuses(); // loads statuses

            return View(orderViewModel); // returns the view with the orders details for editing
        }
        [HttpPost]
        public async Task<IActionResult> EditOrder(OrderViewModel orderViewModel) // saves updated order
        {
            if (!ModelState.IsValid)
            {
                await PopulateCustomersAndProducts();
                PopulateOrderStatuses();
                return View(orderViewModel);
            }

            var order = new Order // maps updated data
            {
                PartitionKey = orderViewModel.PartitionKey,
                RowKey = orderViewModel.RowKey,
                CustomerID = orderViewModel.CustomerID,
                ProductID = orderViewModel.ProductID,
                OrderID = orderViewModel.OrderID,
                OrderStatus = orderViewModel.OrderStatus,
                Timestamp = orderViewModel.Timestamp,
                OrderDate = orderViewModel.OrderDate
            };

            var json = JsonSerializer.Serialize(order); // serializes the order object to JSON format
            var content = new StringContent(json, Encoding.UTF8, "application/json"); // creates a StringContent object with the JSON content
            var response = await _httpClient.PutAsync($"{_functionBaseUrl}/UpdateOrder", content); // calls the function to update the order

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error updating order using Azure Function :(";
                await PopulateCustomersAndProducts();
                PopulateOrderStatuses();

                return View(orderViewModel);
            }

            return RedirectToAction("Index"); // redirects to the Index action if the update was successful
        }

        private async Task PopulateCustomersAndProducts() // loads customers and products
        {
            var customers = new List<Customer>();
            var products = new List<Product>();

            // Fetch customers
            var customerResponse = await _httpClient.GetAsync($"{_functionBaseUrl}/GetAllCustomers");
            if (customerResponse.IsSuccessStatusCode)
            {
                var customerJson = await customerResponse.Content.ReadAsStringAsync();
                customers = JsonSerializer.Deserialize<List<Customer>>(customerJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            // Fetch products
            var productResponse = await _httpClient.GetAsync($"{_functionBaseUrl}/GetAllProducts");
            if (productResponse.IsSuccessStatusCode)
            {
                var productJson = await productResponse.Content.ReadAsStringAsync();
                products = JsonSerializer.Deserialize<List<Product>>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            ViewData["Customer"] = customers; // sets customer data
            ViewData["Product"] = products; // sets product data
        }

        private void PopulateOrderStatuses() // loads order statuses
        {
            ViewBag.OrderStatuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "Order Processed", Text = "Order Processed"},
                new SelectListItem { Value = "Backlogged", Text = "Backlogged"},
                new SelectListItem { Value = "Cancelled", Text = "Cancelled"},
                new SelectListItem { Value = "Delivered", Text = "Delivered"},
                new SelectListItem { Value = "Out for Delivery", Text = "Out for Delivery"},
            };
        }

        private async Task<Order> GetOrder(string partitionKey, string rowKey) // gets single order
        {
            var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetOrder?partitionKey={partitionKey}&rowKey={rowKey}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Order>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private async Task<OrderViewModel> MapOrderToViewModel(Order order) // maps order to viewmodel
        {
            return new OrderViewModel
            {
                PartitionKey = order.PartitionKey,
                RowKey = order.RowKey,
                CustomerID = order.CustomerID,
                ProductID = order.ProductID,
                OrderID = order.OrderID,
                OrderStatus = order.OrderStatus,
                Timestamp = order.Timestamp,
                OrderDate = order.OrderDate,
                CustomerName = await GetCustomerName(order.CustomerID),
                ProductName = await GetProductName(order.ProductID),
                AvailabilityStatus = await GetProductAvailability(order.ProductID)
            };
        }

        private async Task<string> GetCustomerName(string customerId) // gets customer name
        {
            var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetCustomer?customerId={customerId}");
            if (!response.IsSuccessStatusCode) return "Unknown Customer";

            var json = await response.Content.ReadAsStringAsync();
            var customer = JsonSerializer.Deserialize<Customer>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown Customer";
        }

        private async Task<string> GetProductName(string productId) // gets product name
        {
            if (!int.TryParse(productId, out var id))
            {
                return "Unknown Product";
            }

            var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetProduct?productId={id}");
            if (!response.IsSuccessStatusCode)
            {
                return "Unknown Product";
            }

            var json = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return product != null ? product.ProductName : "Unknown Product";
        }

        private async Task<string> GetProductAvailability(string productId) // gets availability status

        {
            if (!int.TryParse(productId, out var id))
            {
                return "Unknown";
            }

            var response = await _httpClient.GetAsync($"{_functionBaseUrl}/GetProduct?productId={id}");
            if (!response.IsSuccessStatusCode)
            {
                return "Unknown";
            }

            var json = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return product != null ? product.AvailabilityStatus : "Unknown";
        }
    }
}

/* References:

IIEVC School of Computer Science, 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 3: Never Lose Data Again with Queue Storage! . [video online] 
Available at: <https://youtu.be/VbZ3Pi63yEc?si=ZyWocGlx2fbWzt7T>
[Accessed 20 August 2025].

*/