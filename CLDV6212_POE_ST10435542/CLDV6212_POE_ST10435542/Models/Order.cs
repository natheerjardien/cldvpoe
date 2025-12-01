using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace CLDV6212_POE_ST10435542.Models
{
// As demonstrated by IIEVC School of Computer Science (2025), the Order class is used to represent an order in the Azure Table Storage that contains foreign keys to the Customer and Product tables
// This class implements the ITableEntity interface, which is required for Azure Table Storage entities. The foreign keys link the order to a specific customer and product
    public class Order : ITableEntity
    {
        [Key]
        public int OrderID { get; set; }
        //[Required(ErrorMessage = "Order Status is Required")]
        public string OrderStatus { get; set; } // additonal property to show the status of the order
        public string? PartitionKey { get; set; } // Partition key for Table Storage
        public string? RowKey { get; set; } // Row key for Table Storage
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        // add orderStatus property

        // Introduce validation sample
        [Required(ErrorMessage = "Please select a Customer.")]
        public string CustomerID { get; set; } // Foreign key to Customer table who made the Order

        [Required(ErrorMessage = "Please select the Product.")]
        public string ProductID { get; set; } // Foreign key to Product table of which Product was ordered

        [Required(ErrorMessage = "Please select the Date of the Order.")]
        public DateTime OrderDate { get; set; } // Date of the order
    }
}

/* References:

IIEVC School of Computer Science, 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 3: Never Lose Data Again with Queue Storage! . [video online] 
Available at: <https://youtu.be/VbZ3Pi63yEc?si=ZyWocGlx2fbWzt7T>
[Accessed 20 August 2025].

*/
