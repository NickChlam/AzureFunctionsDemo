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
      
## 2) Consume Que Message - Create Reciept 
* This function is triggered when a new item enters the que.  via the Que trigger attribute
     ```c#
     [QueueTrigger("payments", Connection = "AzureWebJobsStorage")]RentalPayment payment
    ```
* Using the Interface IBinder as an attribute we can customize the binding at runtime 
* Binding the BlobAttribute with a TextWriter, we can write a file to our Table Storage based on the Connection parameter **AzurWebJobsStorage** credential located in local.settings.json (this will be changed upon deployment) 
