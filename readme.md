# Azure Functions Demo - 
## Aure Function Workflow: 
![](https://krevaas.com/flow.png)

### 1) Payment Web-hook -> OnRentalPaymentRecieved.cs
* This function has an HTTPTrigger which desterilizes the body of the request as a \<RentalPayment\> 
* This function is triggered (locally) when a Post request is made to **http://localhost:7071/api/OnRentalPaymentRecieved** with a body of:
```json
      {
        "PaymentId" : "5",
        "RentalUnitId" : "1",
        "CustomerId" : "1",
        "Email" : "emailAddress@gmail.com",
        "PaymentAmount" : 9800.00
      }
 ```
* I add a partition Key and row key to the payment and add to the order Queue using the Queue attribute binding [Queue("payments")] IAsyncCollector<RentalPayment> orderQueue
* Then saved to Azure Table Storage. This is accomplished through the table Attribute Binding [Table("payments")] IAsyncCollector<RentalPayment> paymentTable
      
## 2) Consume Queue Message and create receipt in blob storage - GenerateReceipt.cs
* This function is triggered when a new item enters the queue. Using the Que trigger attribute
     ```C#
     [QueueTrigger("payments", Connection = "AzureWebJobsStorage")]RentalPayment payment
    ```
* Using the Interface IBinder as an attribute we can customize the binding at runtime 
* Binding the BlobAttribute with a TextWriter, we can write a file to our Table Storage based on the Connection parameter **AzureWebJobsStorage** credential located in local.settings.json (this will be changed upon deployment) 
* Using the Queue data from the queue trigger we can create the receipt in blob storage:
``` C#
            outputBlob.WriteLine($"Thank you for your payment");
            outputBlob.WriteLine("Details \n");
            outputBlob.WriteLine($"Customer Id: {payment.CustomerId}");
            outputBlob.WriteLine($"Email: {payment.Email}");
            outputBlob.WriteLine($"Invoice Number: {payment.paymentId}");
            outputBlob.WriteLine($"Purchase Date: {DateTime.UtcNow}");
```
## 3) When a new item is added to BlobStorage (payments), Send Receipt to Customer - EmailReceipt.cs
* This function is triggered when a new item enter blob storage at payments location via the Blob trigger attribute
``` c#
[BlobTrigger("payments/{recId}.txt",
```
* Using the SendGrid attribute we can bind the interface ICollector to a <SendGridMessage> allowing us to control access through the sender object that we create.  
``` c#
[SendGrid(ApiKey="SendGridApiKey")] ICollector<SendGridMessage> sender
```
* Using the Table attribute we can access data from Table Storage providing the table name, partition name, and table key.  We are getting the table key from the filename.   We saved the filename as the table key name.  You can also use IBinder to bind the Table so we can find data at runtime if key wasn’t accessible. 
``` c#
       [Table("payments", "payments", "{recId}")] RentalPayment payment,
```
*  The email is formatted, an attachment was added and is converted to base64.  We then send the email if the email does not end in @test.  I did this so I was not constantly spamming myself with emails.  
``` C#
 if(!email.EndsWith("@test.com"))
                sender.Add(message);
```
<<<<<<< HEAD
      
=======
      
>>>>>>> 5a1376013e5d1063e86822ed95106fce3967b87e
