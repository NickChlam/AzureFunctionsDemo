using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureFunctionsDemo.Data;
using AzureFunctionsDemo.helper;
using System.Linq;

namespace AzureFunctionsDemo
{
    public static class OnRentalPaymentRecieved
    {
        [FunctionName("OnRentalPaymentRecieved")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,  // HTTP Trigger on post request - what triggers the function - Auth Level gives us a secret code once deployed 
            [Queue("payments")] IAsyncCollector<RentalPayment> orderQueue, // que binding to function. Param is name of que to write to - !exist it will create one.  We can also use AZ services bus or other providers https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?tabs=csharp
            ILogger log)
        {
            log.LogInformation("HTTP trigger function triggered");
              // get the request body and valdiate it
            var requestBody = await req.GetBodyAsync<RentalPayment>();
            if(!requestBody.IsValid)
            {
                return  new BadRequestObjectResult($"Model is invalid: {string.Join(", ", requestBody.ValidationResults.Select(s => s.ErrorMessage).ToArray())}");

            }
            
            var payment = requestBody.Value;
            // SQL - if table doesnt exist create it and insert the data else insert the payment data
            var payId = DataAccess.SaveData(OnRentalPaymentRecieved.sql, payment);
            // add payment id from data we just saved in DB
            payment.paymentId = Int32.Parse(payId.ToString());

            // add payment info to queue. This will serialize the payment and place it on the queue. Azure functions bindings does a lot of the work for us. 
            await orderQueue.AddAsync(payment);  
            
            log.LogInformation($"Payment Info: \n Id: {payment.CustomerId} \n Email: {payment.Email} \n  Payment Amount: {payment.PaymentAmount} PaymentId: {payment.paymentId}");
            
            // return a response 200ok with a message
            return new OkObjectResult("Payment is being processed");
        }
        // static string sql statement
        private static string sql = $@" 
                        IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
                            WHERE TABLE_SCHEMA = 'dbo'
                            AND TABLE_NAME = 'Payments'))
                            BEGIN
                                INSERT INTO Payments 
                                    (CustomerId,Email,PaymentAmount,RentalUnitId, CustomerName)
                                VALUES
                                    ( @CustomerId, @Email, @PaymentAmount, @RentalUnitId, @CustomerName);
                                SELECT SCOPE_IDENTITY()
                            END;
                        ELSE
                        BEGIN
                            CREATE TABLE Payments (
                                paymentId int NOT NULL IDENTITY(1,1) PRIMARY KEY,
                                RentalUnitId varchar(255),
                                CustomerId varchar(255),
                                Email varchar(255),
                                PaymentAmount money,
                                CustomerName varchar(255)
                            ); 
                            INSERT INTO Payments 
                                (CustomerId,Email,PaymentAmount,RentalUnitId, CustomerName)
                            VALUES
                                ( @CustomerId, @Email, @PaymentAmount, @RentalUnitId, @CustomerName);
                                SELECT SCOPE_IDENTITY()
                        END;
                        ";
    }
}
