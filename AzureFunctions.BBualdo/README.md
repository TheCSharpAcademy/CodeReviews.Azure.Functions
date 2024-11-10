# Azure Functions Order Processing System

This project is an implementation of an order processing system using Azure Functions. It demonstrates the use of various Azure Function triggers to automate the order processing workflow from receiving orders to sending notifications and updating the inventory.

## Table of Contents

- [Project Overview](#project-overview)
- [Function Descriptions](#function-descriptions)
    - [HTTP Trigger](#http-trigger)
    - [Queue Trigger](#queue-trigger)
    - [Blob Trigger](#blob-trigger)
    - [Cosmos DB Trigger](#cosmos-db-trigger)
    - [Event Grid Trigger](#event-grid-trigger)
    - [Timer Trigger](#timer-trigger)
- [Setup Instructions](#setup-instructions)
- [Running the Project Locally](#running-the-project-locally)
- [System Requirements](#system-requirements)

## Project Overview

This system automates the entire process of handling orders for an online retail company. It leverages Azure Functions to manage the process from order placement to customer notification and inventory updates. The main components include:

1. **Order receipt and storage** in Cosmos DB.
2. **Order processing** through Azure Queue Storage and Blob Storage.
3. **Automatic inventory updates** using Cosmos DB Triggers.
4. **Daily sales reporting** using Timer Triggers.
5. **Customer notifications** using Event Grid and Blob Triggers.

## Function Descriptions

### HTTP Trigger

- **Function**: Receives an order via a POST request, stores it in Cosmos DB, and adds a message to an Azure Storage Queue (`orders-queue`).
- **Interactions**:
    - Saves the order to Cosmos DB.
    - Sends a message to the Azure Queue (`orders-queue`).

### Queue Trigger

- **Function**: Processes messages from the `orders-queue`, updates the order status in Cosmos DB, generates an invoice in a `.txt` file, and saves it to Blob Storage.
- **Interactions**:
    - Updates the order status in Cosmos DB.
    - Saves the invoice in Blob Storage.

### Blob Trigger

- **Function**: Monitors new invoices added to Blob Storage and sends an email notification to the customer with the invoice attached.
- **Interactions**:
    - Sends an email notification to the customer with the attached invoice.

### Cosmos DB Trigger

- **Function**: Monitors changes in the `Orders` container in Cosmos DB. When the order status changes to `OrderShipped`, it updates the inventory.
- **Interactions**:
    - Updates inventory based on the change in order status.

### Event Grid Trigger

- **Function**: Subscribes to order status change events from Cosmos DB. For example, when an order is shipped, the function sends a notification to the customer.
- **Interactions**:
    - Sends a notification to the customer regarding the order status change.

### Timer Trigger

- **Function**: Generates a daily sales report by querying data from Cosmos DB.
- **Interactions**:
    - Creates a sales report from order data and can save it to Blob Storage or send it using another mechanism.

## Setup Instructions


**Configure local settings**:

    - Create file local.settings.json`.
    - Ask me for environment variables 😅

## Running the Project Locally

1. **Test the HTTP Trigger**:
    - Use Postman or a similar tool to send a POST request to `http://localhost:7071/api/HttpTrigger` with the order details in JSON format:
```json
{
  "ProductName": "Shoes",
  "Quantity": 1,
  "Price": 119.99
}
```

2. **Monitor the logs**:
    - Logs will display the processing steps for each function trigger.

## System Requirements

- **.NET 6.0 SDK**
- **Azure Functions Core Tools**
- **Azure Storage Emulator** (or use the connection string for an Azure Storage account)

## Additional Notes

- Ensure that all necessary Azure resources (Cosmos DB, Blob Storage, etc.) are either properly configured locally or mocked during development.
- This system is designed to be fully functional within a local development environment using Azure Storage Emulator and Cosmos DB hosted on Azure.
