using Azure.Storage.Queues;

namespace CLDV6212_POE_ST10435542.Models.Services
{
// As demonstrated by IIEVC School of Computer Science (2025), the QueueService is responsible for managing message queues in Azure Queue Storage
// Ive implemented a method to send messages to the queue, which can be used for various purposes such as order processing or notifications
    public class QueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(string connectionString, string queueName)
        {
            _queueClient = new QueueClient(connectionString, queueName);
        }

        public async Task SendMessage(string message)
        {
            await _queueClient.SendMessageAsync(message);
        }
    }
}

/* References:

IIEVC School of Computer Science, 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 3: Never Lose Data Again with Queue Storage! . [video online] 
Available at: <https://youtu.be/VbZ3Pi63yEc?si=ZyWocGlx2fbWzt7T>
[Accessed 20 August 2025].

*/
