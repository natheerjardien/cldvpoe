using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace CLDV6212_POE_ST10435542.Models
{
// According to IIEVC School of Computer Science (2025), the Customer class is used to represent a customer in the Azure Table Storage
// This class implements the ITableEntity interface, which is required for Azure Table Storage entities. Ive defined all the relevant properties for the Customer entity
    public class Customer : ITableEntity
    {
        [Key]
        public string CustomerID { get; set; }
        [Required(ErrorMessage="First Name is Required")] // made these properties required so that they are not null when creating a new customer
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is Required")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is Required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Contact is Required")]
        public string Contact { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        public string Password { get; set; }

        // ITable implementation
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}

/* References:

IIEVC School of Computer Science, 2025. CLDV6212 Building a Modern Web App with Azure Table Storage & ASP.NET Core MVC - Part 1. [video online] 
Available at: <https://youtu.be/Txp7VYUMBGQ?si=5sD7TV0vS90-pPHY>
[Accessed 19 August 2025].

*/
