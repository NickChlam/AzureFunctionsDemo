using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Data.SqlClient;
using AzureFunctionsDemo.Data;

namespace AzureFunctionsDemo
{
    public static class EmailReceipt
    {
        [FunctionName("EmailReceipt")]
        public static void Run([BlobTrigger("payments/{recId}.txt",
            Connection = "AzureWebJobsStorage")]string receiptContents,
            string recId, 
            [SendGrid(ApiKey="SendGridApiKey")] ICollector<SendGridMessage> sender, 
            ILogger log)

        {
            var sql = $@"Select Email, PaymentAmount, CustomerName FROM Payments WHERE paymentId = '{Int32.Parse(recId)}'   ";
            
            var data = DataAccess.LoadData<RentalPayment>(sql);
            var email = data[0].Email;
            
            
            var message = new SendGridMessage();
            // get sender email from env variable Email sender - config'd with SendGrid API key @ sendgrid.com
            message.From = new EmailAddress(Environment.GetEnvironmentVariable("EmailSender"));
            email = email.Trim();
            message.AddTo(email);  // trim in case there are extra spaces.  SendGrid will invalidate and not send
            
            // encode reciept files to utf8
            var TextBytes = System.Text.Encoding.UTF8.GetBytes(receiptContents);
            // convert to base64 string ( req by sendGrid for attachments ) 
            var base64 = Convert.ToBase64String(TextBytes);
            // add attachment give it a filename, the content and content type 
            message.AddAttachment(filename: $"{recId}.txt", base64, type: "text/plain");
            // use custom created template made on sendgrid.com for sending HTML emails. You can also just use subject and content properties on message object
            message.SetTemplateId("d-a190fa8ce2864bb19866e36895a751ec");

            // sendGrid template data for email
           var messageData =  new {
                Sender_Name = "Nicholas Chlam",
                Sender_Address = "1560 Boulder Ave",
                Sender_City = "Denver", 
                Sender_State = "Co",  
                Sender_Zip = "80211",
                confirmation_number = Guid.NewGuid(),
                amount = String.Format("{0:C}", data[0].PaymentAmount),
                customerName = data[0].CustomerName
                };
            
            // set template data to message 
            message.SetTemplateData(messageData);
            
        
            // send email - only send if email isnt @test.com  - for testing purpses. 
             if(!email.EndsWith("@test.com"))
                sender.Add(message);

            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{recId} \n Size: {receiptContents.Length} Bytes");
        }
    }
}
