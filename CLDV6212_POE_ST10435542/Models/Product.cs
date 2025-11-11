using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace CLDV6212_POE_ST10435542.Models
{
// As demonstarted by IIEVC School of Computer Science (2025), the Product class is used to represent a product in the Azure Table Storage and makes use of the ITableEntity interface
// Ive defined all the relevant properties for the Products and made use of data annotations to ensure that the properties are validated correctly (?)
    public class Product : ITableEntity
    {
        [Key]
        public int ProductID { get; set; }
        [Required(ErrorMessage = "Product Name is Required")] // made these properties required so that they are not null when creating a new product
        public string ProductName { get; set; }
        [Required(ErrorMessage = "Description is Required")]
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        [Required(ErrorMessage = "Category is Required")]
        public string Category { get; set; }
        [Required(ErrorMessage = "Availablity Status is Required")]
        public string AvailabilityStatus { get; set; }

        // ITableEntity Implementation
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}

/* References:

IIEVC School of Computer Science, 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 2: Adding Image Uploads with Blob Storage!. [video online] 
Available at: <https://youtu.be/CuszKqZvRuM?si=RZaHcDniR_ZWB-59>
[Accessed 16 August 2025].

*/
