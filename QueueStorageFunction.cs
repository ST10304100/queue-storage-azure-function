using Azure.Data.Tables;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Azure.Storage.Queues;

namespace FileShareFunctionApp
{
    public class QueueStorageFunction
    {
        private readonly ILogger<QueueStorageFunction> _logger;

        public QueueStorageFunction(ILogger<QueueStorageFunction> logger)
        {
            _logger = logger;
        }

        [Function("ProcessOrders")]
        public async Task<IActionResult> Run(
   [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Processing order");


            string name = req.Query["name"];


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();


            Order order = JsonConvert.DeserializeObject<Order>(requestBody);


            string responseMessage = string.IsNullOrEmpty(name)
                ? "This Http trigged successfully. Pass a name in the query string or in the request body for a personalised response"
                : $"Hello, {name}. The Http trigged function executed successfully";


            QueueServiceClient processOrderClient = new QueueServiceClient("");


            QueueClient processOrderQueue = processOrderClient.GetQueueClient("processorders");


            await processOrderQueue.SendMessageAsync($"Processing Order: Order ID: {order.OrderId} created on Order Date: {order.OrderDate}");


            return new OkObjectResult(responseMessage);
        }


    }


    public class Order : ITableEntity
    {
        [Key]
        public int OrderId { get; set; }
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        //Introduce validation sample
        [Required(ErrorMessage = "Please select a customer.")]
        public int? CustomerId { get; set; } // FK to the Customer who made the order
        [Required(ErrorMessage = "Please select a product.")]
        public int ProductId { get; set; } // FK to the Product being ordered
                                           // [Required(ErrorMessage = "Please select the date.")]
        public DateTime? OrderDate { get; set; }

    }
}