using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctionsDemo
{
    public static class OnRentalPaymentRecieved
    {
        [FunctionName("OnRentalPaymentRecieved")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,  // HTTP Trigger on post request - what triggers the function - Auth Level gives us a secret code once deployed 
            [Queue("payments")] IAsyncCollector<RentalPayment> orderQueue, // que binding to function. Param is name of que to write to - !exist it will create one.  We can also use AZ services bus or other providers https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?tabs=csharp
            [Table("payments")] IAsyncCollector<RentalPayment> paymentTable,
            ILogger log)
        {
            log.LogInformation("HTTP trigger function triggered");

            // get the request body 
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            // deserialize a rentalPayment Object from request body
            var payment = JsonConvert.DeserializeObject<RentalPayment>(requestBody);

            // add partition key and row key to add to azure table storage - 
            payment.PartitionKey = "payments";
            payment.RowKey = payment.paymentId;

            await orderQueue.AddAsync(payment);  // it will serialize the payment and place it on the que. Azure functions bindings does a lot of the work for us. 
            await paymentTable.AddAsync(payment); // add payment to tableStorage. 
            log.LogInformation($"Payment Info: \n Id: {payment.CustomerId} \n Email: {payment.Email} \n  Payment Amount: {payment.PaymentAmount}");

            // return a response 200ok with a message
            return new OkObjectResult("Payment is being processed");
        }
    }
}
