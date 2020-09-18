# Azure Functions Demo - 
## Aure Function Workflow: 
![](https://krevaas.com/flow.png)

### 1) Payment Web-hook -> OnRentalPaymentRecieved.cs
* This function has an HTTPTrigger which deserilaizes the body of the request as a \<RentalPayment\> 
* This function is trigered (locally) when a Post request is made to **http://localhost:7071/api/OnRentalPaymentRecieved** with a body of:
```json
      {
        "paymentId" : "5",
        "RentalUnitId" : "1",
        "CustomerId" : "1",
        "Email" : "emailAddress@gmail.com",
        "PaymentAmount" : 9800.00
      }
 ```
* I add a partitionKey and rowkey to the payment and add to the orderQue using the Que atrribute binding [Queue("payments")] IAsyncCollector<RentalPayment> orderQueue
* Then to Azure Table Storage. This is accomplished through the table Attribute Binding [Table("payments")] IAsyncCollector<RentalPayment> paymentTable
      
## 2) Consume Queue Message - Create Receipt 
* This function is triggered when a new item enters the queue.  via the Que trigger attribute
     ```C#
     [QueueTrigger("payments", Connection = "AzureWebJobsStorage")]RentalPayment payment
    ```
* Using the Interface IBinder as an attribute we can customize the binding at runtime 
* Binding the BlobAttribute with a TextWriter, we can write a file to our Table Storage based on the Connection parameter **AzureWebJobsStorage** credential located in local.settings.json (this will be changed upon deployment) 
* Using the Queue data from the queue trigger we can create the receipt inlob storage:
``` C#
 outputBlob.WriteLine($"Thank you for your payment");
            outputBlob.WriteLine("Details \n");
            outputBlob.WriteLine($"Customer Id: {payment.CustomerId}");
            outputBlob.WriteLine($"Email: {payment.Email}");
            outputBlob.WriteLine($"Invoice Number: {payment.paymentId}");
            outputBlob.WriteLine($"Purchase Date: {DateTime.UtcNow}");
```
## 3) When a new item is added to BlobStorage (payments), Send Reciept to Customer 
* This function is triggered when a new item enter blob storage at payments location via the Blob trigger attribute
``` c#
[BlobTrigger("payments/{recId}.txt",
```
* Using the SendGrid attribute we can bind the interface ICollector to a <SendGridMessage> alowwing us to control access through the sender object that we create 
``` c#
[SendGrid(ApiKey="SendGridApiKey")] ICollector<SendGridMessage> sender
```
* using the Table attribute we can acccess data from Table Storage providng the table name, partition name, and table key.  We are getting the table key form the filename as we saved the filename as the tablekey name.  You can also using IBinder to bind the Table so we can find data at runtime if key wasnt accessible. 
``` c#
       [Table("payments", "payments", "{recId}")] RentalPayment payment,
```

