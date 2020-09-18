using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace AzureFunctionsDemo
{
    public static class EmailReceipt
    {
        [FunctionName("EmailReceipt")]
        public static void Run([BlobTrigger("payments/{recId}.txt",
            Connection = "AzureWebJobsStorage")]string receiptContents,
            string recId, 
            [SendGrid(ApiKey="SendGridApiKey")] ICollector<SendGridMessage> sender, 
            [Table("payments", "payments", "{recId}")] RentalPayment payment,
            ILogger log)

        {
            var email = payment.Email;
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
            message.AddAttachment(filename: $"{recId}.txt", content: base64, type: "text/plain");
            // use custom created template made on sendgrid.com for sending HTML emails. You can also just use subject and content properties on message object
            message.SetTemplateId("d-a190fa8ce2864bb19866e36895a751ec");
            message.AddCustomArg("Sender_name", "THis is a test Name");

            

            

            // only send if email isnt @test.com  - for testing purpses. 
             if(!email.EndsWith("@test.com"))
                sender.Add(message);

            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{recId} \n Size: {receiptContents.Length} Bytes");
        }
    }
}
