using System.ComponentModel.DataAnnotations;

namespace CLDV6212_POE_ST10435542.Models
{
// According to Rout (2025), a ViewModel contains more than one models data required a particular view, which in this case is suitable because extra properties are needed for the Index View, which cannot be achieved with the Order model alone
    // created a new model to include properties from the customer and product tables
    public class OrderViewModel
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        [Required] public string CustomerID { get; set; }
        [Required] public string ProductID { get; set; }
        public int OrderID { get; set; }
        public string OrderStatus { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        [Required] public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } // from Customer table but combines both the first and last name
        public string ProductName { get; set; } // from the product table
        public string AvailabilityStatus { get; set; } // from the product table
    }
}

/* References:

Pranaya Rout, 2025. Dot Net Tutorials. ViewModel in ASP.NET MVC. [online] 
Available at: <https://dotnettutorials.net/lesson/view-model-asp-net-mvc/>
[Accessed 21 August 2025].

*/