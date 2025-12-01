using Azure.Data.Tables;
using Azure;

namespace CLDV6212_POE_ST10435542.Models.Services
{
    public class TableStorageService
    {
// As demonstarted by IIEVC School of Computer Science (2025), TableClients are defined to access the Azure Table Storage
// I created three TableClients for Customer, Product, and Order tables
        private readonly TableClient _customerTableClient;
        private readonly TableClient _productTableClient;
        private readonly TableClient _orderTableClient; // used for orders

        public TableStorageService(string connectionString) // constructor that initializes the TableClients with the connection string and table names
        {
            _customerTableClient = new TableClient(connectionString, "Customers");
            //_orderTableClient.CreateIfNotExists(); // creates the Customers table if it does not exist
            _productTableClient = new TableClient(connectionString, "Products");
            //_productTableClient.CreateIfNotExists(); // creates the Products table if it does not exist
            _orderTableClient = new TableClient(connectionString, "Orders"); // used for orders
            //_orderTableClient.CreateIfNotExists(); // creates the Orders table if it does not exist
        }

// As demonstarted by IIEVC School of Computer Science (2025), the methods below are used to interact with the Azure Table Storage, to both retrive and add data to the predefined tables and blob container
// These methods were used to retrive data from the tables and to add data to the tables, as well as to delete and update data in the tables
        public async Task<Customer> GetCustomerAsync(string partitionKey, string rowKey) // gets individual customer for ViewCustomer View
        {
            try
            {
                var response = await _customerTableClient.GetEntityAsync<Customer>(partitionKey, rowKey); // gets the customer by partitionKey and rowKey in the customer table
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<List<Customer>> GetAllCustomersAsync() // returns a list of Customers, needed for the model class
        {
            var customers = new List<Customer>();
            await foreach (var cust in _customerTableClient.QueryAsync<Customer>())
            {
                customers.Add(cust); // adds each customer to the list
            }
            return customers; // retuns the list of customers
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set for the Customer entity."); // throws an exception if the PartitionKey and RowKey are not set
            }

            try
            {
                await _customerTableClient.AddEntityAsync(customer); // adds the customer
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to Table Storage", ex);
            }
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            await _customerTableClient.DeleteEntityAsync(partitionKey, rowKey); // deletes the customer by partitionKey and rowKey
        }

        // Ive made this method a bool so that it can return true or false if the update was successful or not
        public async Task<bool> UpdateCustomerAsync(Customer customer) // updates the customers details in the table storage
        {
            try
            {
// According to SoundCode (2023), ETag is used for multipart control, so in terms of updating enitities, it ensures that the correct version is passed and updated
// In this case, ETag.All is used to bypass the check and update the entity regardless of its current version
                await _customerTableClient.UpdateEntityAsync(customer, ETag.All, TableUpdateMode.Replace); // replaces the existing customer entity with the updated one
                return true; // returns true if the update was successful
            }
            catch (RequestFailedException ex)
            {
                return false;
            }
        }

        public async Task<Product> GetProductAsync(string partitionKey, string rowKey) // gets individual customer for ViewCustomer View
        {
            try
            {
                var prod = await _productTableClient.GetEntityAsync<Product>(partitionKey, rowKey); // gets the product by partitionKey and rowKey in the product table
                return prod.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();

            await foreach (var product in _productTableClient.QueryAsync<Product>())
            {
                products.Add(product); // adds each product to the list
            }
            return products; // returns the list of products
        }

        public async Task AddProductAsync(Product product)
        {
            if (string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set");
            }

            try
            {
                await _productTableClient.AddEntityAsync(product); // adds the product to the table storage
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to Table Storage", ex);
            }
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            await _productTableClient.DeleteEntityAsync(partitionKey, rowKey); // deletes the product by partitionKey and rowKey
        }

        public async Task AddOrderAsync(Order order)
        {
            if (string.IsNullOrEmpty(order.PartitionKey) || string.IsNullOrEmpty(order.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set");
            }

            try
            {
                await _orderTableClient.AddEntityAsync(order); // adds the order to the table storage
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding order" + "table storage", ex);
            }
        }

        public async Task<bool> UpdateOrderAsync(Order order) // updates the customers details in the table storage
        {
            try
            {
                await _orderTableClient.UpdateEntityAsync(order, ETag.All, TableUpdateMode.Replace); // replaces the existing customer entity with the updated one
                return true; // returns true if the update was successful
            }
            catch (RequestFailedException ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product) // updates the customers details in the table storage
        {
            try
            {
                await _productTableClient.UpdateEntityAsync(product, ETag.All, TableUpdateMode.Replace); // replaces the existing customer entity with the updated one
                return true; // returns true if the update was successful
            }
            catch (RequestFailedException ex)
            {
                return false;
            }
        }

        public async Task<Order> GetOrderAsync(string partitionKey, string rowKey) // gets individual customer for ViewCustomer View
        {
            try
            {
                var order = await _orderTableClient.GetEntityAsync<Order>(partitionKey, rowKey); // gets the order by partitionKey and rowKey in the order table
                return order.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();

            await foreach (var order in _orderTableClient.QueryAsync<Order>())
            {
                orders.Add(order); // adds each order to the list
            }

            return orders; // returns the list of orders
        }

        public async Task DeleteOrderAsync(string partitionKey, string rowKey)
        {
            await _orderTableClient.DeleteEntityAsync(partitionKey, rowKey); // deletes order by partitionKey and rowKey
        }

        // ive implemented a similar concept from previous projects on automating the incrementing of certain properties
        public async Task<int> IncrementProductID() // since the IDs are not automatically populated, i made these methods to increment the Ids so that they do not show up as 0 when added to tables
        {
            var products = await GetAllProductsAsync(); // gets all products from the table storage

            if (products.Count == 0)
            {
                return 1; // if there are not products loaded, then the default ID will be 1
            }

            return products.Max(p => p.ProductID) + 1; // gets the total number of products in the list and adds 1 (increment)
        }

        public async Task<int> IncrementOrderID()
        {
            var orders = await GetAllOrdersAsync();

            if (orders.Count == 0)
            {
                return 1; // if there are no orders, then the default ID will be 1
            }

            return orders.Max(p => p.OrderID) + 1;
        }

        public async Task EnsureTablesExistAsync()
        {
            await _orderTableClient.CreateIfNotExistsAsync();
            await _customerTableClient.CreateIfNotExistsAsync();
            await _productTableClient.CreateIfNotExistsAsync();
        }
    }
}

/* References:

IIEVC School of Computer Science, 2025. CLDV6212 Building a Modern Web App with Azure Table Storage & ASP.NET Core MVC - Part 1. [video online] 
Available at: <https://youtu.be/Txp7VYUMBGQ?si=5sD7TV0vS90-pPHY>
[Accessed 15 August 2025].

SoundCode, 2023. Using ETags and Patching Rows in Azure Table Storage. [online] 
Available at: <https://markheath.net/post/etags-patching-azure-table-storage>
[Accessed 22 August 2025].

*/
