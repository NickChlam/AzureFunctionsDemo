# Azure Functions Demo - 
## Aure Function Workflow: 
![](https://krevaas.com/flow.png)

### 1) Payment Web-hook -> OnRentalPaymentRecieved.cs
* This function has an HTTPTrigger which desterilizes the body of the request as a \<RentalPayment\> 
* This function is triggered (locally) when a Post request is made to **http://localhost:7071/api/OnRentalPaymentRecieved** with a body of:
```json
      {
        "RentalUnitId" : "1",
        "CustomerId" : "1",
        "Email" : "emailAddress@gmail.com",
        "PaymentAmount" : 9818.76,
        "CustomerName" : "Keith Urban"
      }
 ```
* Add to the order Queue using the Queue attribute binding [Queue("payments")] IAsyncCollector<RentalPayment> orderQueue
* Then saved to SQL Server using a very basic data layer in DataAccess.cs to manage connections to the db. 
      
### 2) Consume Queue Message and create receipt in blob storage - GenerateReceipt.cs
* This function is triggered when a new item enters the queue. Using the Que trigger attribute
     ```C#
     [QueueTrigger("payments", Connection = "AzureWebJobsStorage")]RentalPayment payment
    ```
* Using the Interface IBinder as an attribute we can customize the binding at runtime 
* Binding the BlobAttribute with a TextWriter, we can write a file to Storage based on the Connection parameter **AzureWebJobsStorage** credential located in local.settings.json (this will be changed upon deployment) 
* Using the Queue data from the queue trigger we can create the receipt in blob storage:
``` C#
            outputBlob.WriteLine($"Thank you for your payment");
            outputBlob.WriteLine("Details \n");
            outputBlob.WriteLine($"Customer Id: {payment.CustomerId}");
            outputBlob.WriteLine($"Email: {payment.Email}");
            outputBlob.WriteLine($"Invoice Number: {payment.paymentId}");
            outputBlob.WriteLine($"Purchase Date: {DateTime.UtcNow}");
```
### 3) When a new item is added to BlobStorage (payments), Send Receipt to Customer - EmailReceipt.cs
* This function is triggered when a new item enter blob storage at payments location via the Blob trigger attribute
``` c#
[BlobTrigger("payments/{recId}.txt",
```
* Using the SendGrid attribute we can bind the interface ICollector to a <SendGridMessage> allowing us to control access through the sender object that we create.  
``` c#
[SendGrid(ApiKey="SendGridApiKey")] ICollector<SendGridMessage> sender
```
* Using SQL Server we can access data.  We are getting the paymentId from the filename.  
``` c#
        var data = DataAccess.LoadData<RentalPayment>(sql);
        var email = data[0].Email;
```
*  The email is formatted, an attachment was added and is converted to base64.  We then send the email if the email does not end in @test.  I did this so I was not constantly spamming myself with emails.  
``` C#
 if(!email.EndsWith("@test.com"))
                sender.Add(message);
```

## Running in Docker:

* This project can be run and tested all locally or in the cloud using Docker.  
1) download docker desktop - https://www.docker.com/products/docker-desktop
2) ensure you have docker installed - **docker --version**
3) ensure you have docker-compose - **docker-compose --version** 
4) move to the base directory of the project containing the docker-compose.yml file
5) Add your ENV vars to Dockerfile in root directory
```
ENV AzureWebJobsStorage="UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azureite"
ENV SendGridApiKey="YourSendGridAPIKey"
ENV EmailSender="EMail@SetUpInSendGrid"
ENV  ConnectionString="Server=sql-server-db,1433; Database=Master;User Id=SA;Password=!passw0rd1985"
```
6) run command - **docker-compose build** ( this may take a few minutes)
7) this will build the image:  
      **functions with a TAG of 1.0**
8) run **docker-compose up** 
  - this will create three containers and run them networked together. 
    - Creating sql-server-db ... done
    - Creating azurefunctionsdemo_azureite_1 ... done
    - Creating azurefunctionsdemo_azure_function_1 ... done
  
9) run **docker ps** in a new window to see what containers are running 

## Test everything works:
### 1) open postman or make a post request to **http://localhost:8080/api/OnRentalPaymentRecieved** with the following body using your email address to see the reciept and email
  ```
{
       
        "RentalUnitId" : "3",
        "CustomerId" : "22",
        "Email" : "YourEmail@gmail.com",
        "PaymentAmount" : 2368.11,
        "CustomerName" : "Keith Urban"
}
  ```
  
### 2) Look at docker-compose output to see each function get triggered - check your email for the message 
```
azure_function_1  | info: Function.OnRentalPaymentRecieved[1]
azure_function_1  |       Executing 'OnRentalPaymentRecieved' (Reason='This function was programmatically called via the 
host APIs.', Id=9c9383af-a644-488e-968d-80dae2e81a9f)
azure_function_1  | info: Function.OnRentalPaymentRecieved.User[0]
azure_function_1  |       HTTP trigger function triggered
...

azure_function_1  |       Executed 'OnRentalPaymentRecieved' (Succeeded, Id=9c9383af-a644-488e-968d-80dae2e81a9f, Duration=1662ms)
```

```
azure_function_1  | info: Function.GenerateReceipt[1]
azure_function_1  |       Executing 'GenerateReceipt' (Reason='New queue message detected on 'payments'.', Id=8ff36d1d-a968-4355-808a-d3a9fe105950)
azure_function_1  | info: Function.GenerateReceipt[0]
azure_function_1  |       Trigger Details: MessageId: 51a896c4-c058-432b-a3bf-f2478fd23a63, DequeueCount: 1, InsertionTime: 09/23/2020 15:50:09 +00:00

zure_function_1  | info: Function.GenerateReceipt.User[0]
azure_function_1  |       Queue trigger function processed and created invoice for invoice Id: 1

azure_function_1  | info: Function.GenerateReceipt[2]
azure_function_1  |       Executed 'GenerateReceipt' (Succeeded, Id=8ff36d1d-a968-4355-808a-d3a9fe105950, Duration=524ms)
```

```
azure_function_1  | info: Function.EmailReceipt[1]
azure_function_1  |       Executing 'EmailReceipt' (Reason='New blob detected: payments/1.txt', Id=ab68cc4d-5814-4daa-9c82-a03992368e0c)

azure_function_1  | info: Function.EmailReceipt[2]
azure_function_1  |       Executed 'EmailReceipt' (Succeeded, Id=ab68cc4d-5814-4daa-9c82-a03992368e0c, Duration=1179ms) 

```
### 3) View Data In SQL Server
* While docker-compose is still running log into the dockers instance of SQL Server with SSMS or CLI ( ** never use default settings for productions envs **) 
```
      Server name: localhost, 1433
      Authentication: SQL Server Authentication
            login: SA
            password: !passw0rd1985
```
<img src = "https://krevaas.com/SQLConnection.PNG" width="50%">

* See the data that was saved in the **master** db under the Payments table

<img src = "https://krevaas.com/Data.PNG" width="50%">



