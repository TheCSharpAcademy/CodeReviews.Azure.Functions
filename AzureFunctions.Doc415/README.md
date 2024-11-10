# Product Orders Management With Azure Functions

Welcome to the Order Processing Automation project! 
This application is designed to streamline and automate the order processing system for an online retail company, enhancing efficiency and improving customer satisfaction.

In this project, we leverage the power of Azure Functions to manage the entire order lifecycle, from receiving new orders to updating inventory and sending notifications to customers. By utilizing various types of Azure Functions, this system effectively handles tasks such as:

- Receiving Orders: Automatically process incoming orders via HTTP triggers, ensuring that all customer requests are efficiently captured.
- Updating Inventory: Maintain accurate inventory levels by integrating with our database to reflect stock changes in real-time.
- Sending Notifications: Notify customers about their order status through email , enhancing the customer experience with timely updates.
- Scheduled Tasks: Utilize Timer triggers to run periodic tasks, such as generating reports, ensuring smooth operations.
- This project not only showcases the versatility of Azure Functions but also demonstrates how cloud technologies can transform traditional order processing into a seamless, automated workflow.

Feel free to explore the code, contribute to the project, and adapt the solution for your own applications!

## Design scetch:

![image](https://github.com/user-attachments/assets/a7fc3554-05c7-4080-a4fa-e53b62714723)




## Setup:

- The application is using Azurite for local development and testing. Azurite is used as a lightweight alternative to Azure Blob Storage. This allows developers to run the application locally without incurring Azure costs.
- The application is configured to use Azure Cosmos DB for data storage, providing scalable and high-performance database services.
- For testing email functionalities, Papercut SMTP is utilized. It captures all outgoing emails in a local SMTP server environment, allowing you to view and verify emails without sending them to real email addresses.
  
### Database:
- Create a Cosmos Db in Azure named "OrdersDemo" (case sensitive)
- Create a container named "Orders" (case sensitive)
- Create a container named "Products" (case sensitive)

### Azurite:
- Install azurite:  npm install -g azurite
- Run azurite:  azurite --silent --location c:\azurite --debug c:\azurite\debug.log
- Download and install Azure Storage Explorer for testing: https://azure.microsoft.com/en-us/products/storage/storage-explorer

### Papercut:
- Download and Install papercut : https://github.com/ChangemakerStudios/Papercut-SMTP/releases
- Run papercut and open options.
- Select 127.0.0.1 for Ip address.
  
![image](https://github.com/user-attachments/assets/63ad1333-66a3-459c-825c-c06252234dd1)


### Configuration files:
- Http trigger and CosmosDb trigger needs connection strings to connect your Azure CosmosDb
- Open a browser and login to your Azure account.
- Select CosmosDb from Azure Services
- Select your Azure subscription
- Select Data Explorer
- Select Connect
  
![image](https://github.com/user-attachments/assets/dc1f28ae-3382-4925-8a8b-bba3aa9ab53a)
- You'll see your connection information. You ll need URI and primary key for setup.
- Open your solution in VS.
- In solution explorer open HttpTrigger project and open local.settings.json
  
![image](https://github.com/user-attachments/assets/01f19485-81e7-4dbe-8d06-9180a5b9bdc5)

- Change connection setting with your own information.
 - "CosmosDb:AccountEndpoint": "URI",
 - "CosmosDb:AccountKey": "PRIMARY KEY",
-   In solution explorer open InventoryUpdate project and open local.settings.json
  
![image](https://github.com/user-attachments/assets/a0ea7d3d-ab7b-4185-adfb-8eb46127d0de)

- Change connection setting with your own information.
 - "CosmoLink:AccountEndpoint": "URI",
 - "CosmoLink:AccountKey": "PRIMARY KEY",

##Testing application

- I recommend using Postman to test the HTTP trigger of this application. Postman offers an intuitive interface and powerful features that simplify API testing and integration.
- Application seeds database with 7 different products and a single order when started.
- After running application , you can see connection URL in Http Trigger console.

![image](https://github.com/user-attachments/assets/9c7e180c-a72b-47dd-ae4e-f1a44b0a51d7)

- You can use this order model to test Post method and create a new order.
```
   {
        "id": "2",
        "customerEmail": "serdar415@gmail.com",
        "createDate": "2024-10-20T18:43:18.3011444+03:00",
        "products": [
            {
                "id": "1",
                "name": "Turkish Delight",
                "price": 10,
                "count": 0
            },
            {
                "id": "3",
                "name": "Brazilian Coffee",
                "price": 25,
                "count": 2
            },
            {
                "id": "5",
                "name": "Ethiopian Coffee",
                "price": 24,
                "count": 2
            }
        ],
        "status": 0,
        "previousStatus": 0,
        "totalFee": 98
    }
```
- After creating an order, the triggers will run and you ll get notification e mail to Papercut.
- Select Put and update models status  1 to receive payment confirmation
- Select Put and update models status  2 to receive payment confirmation
- Select Put and update models status  3 to receive payment confirmation

-  For testing purposes, the Timer trigger function is currently configured to run every minute. This allows for rapid testing and verification of functionality during development.
-  For a production environment, where the function is intended to run on a daily schedule, modify the cron expression as follows:
```
public async Task Run([TimerTrigger("0 0 * * *")] TimerInfo myTimer)
```

-Daily sales reports are available Azurite blob storage.

## Conclusion

This project provides a robust, scalable solution for automating order processing, product updates, and customer notifications using Azure Functions. With the integration of various triggers like HTTP, Timer, Queue, Blob, and Event Grid, it efficiently handles critical tasks such as sales report generation, product management, and communication with stakeholders. Additionally, leveraging Azurite for local storage and Entity Framework for data management ensures flexibility and consistency during both testing and production phases. The architecture is designed to be easily extendable, offering a future-proof platform for handling growing business operations.

A special thanks to [**The C# Academy**](https://thecsharpacademy.com/) for their guidance and resources throughout this project, which have been instrumental in shaping this solution.









