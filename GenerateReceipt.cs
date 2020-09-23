using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsDemo
{
    public static class GenerateReceipt
    {
        [FunctionName("GenerateReceipt")]
        public static async Task Run([QueueTrigger("payments", Connection = "AzureWebJobsStorage")]RentalPayment payment, // que trigger on payments que connection to key AzureWwebJobsStorage in local.settings.json. When a new item is in the que this function is triggered.
        IBinder binder, // bind attribute so we can customize the binding at runtime.  Here this allows us to get the paymentId from the que or if we wanted to switch out connection parameters dynamically. 
        ILogger log)
        {
            var randomId =  Guid.NewGuid();
            // bind the blob atrribute to a textWriter - this allows us to write a file to storage. 
            var outputBlob = await binder.BindAsync<TextWriter>(
                new BlobAttribute(blobPath: $"payments/{payment.paymentId}.txt")
                {
                    Connection = "AzureWebJobsStorage"
                });
            // write the contents of the file
            outputBlob.WriteLine($"Thank you for your payment");
            outputBlob.WriteLine("Details \n");
            outputBlob.WriteLine($"Customer Id: {payment.CustomerId}");
            outputBlob.WriteLine($"Email: {payment.Email}");
            outputBlob.WriteLine($"Invoice Number: {randomId}");
            outputBlob.WriteLine($"Purchase Date: {DateTime.UtcNow}");
            outputBlob.WriteLine($"Purchase Amount: {payment.PaymentAmount}");
            
            log.LogInformation($"Queue trigger function processed and created invoice for invoice Id: {payment.paymentId}");
        }
    }
}