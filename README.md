# Simple humidity and temperature tracking

This project allows you to track humidity and temperature through a Raspberry Pi Pico. The backend stores the data in an Azure Cosmos DB database that can be hosted for free. The API is based on Azure Functions, which means it can also be hosted for free or very cheaply (less than €1 per month!).

## Python

The Python code interacts with a DHT22 sensor (easy to change to DHT11) and an SSD1306 screen to record temperature and humidity. The data is both shown on the screen and sent to the API configured on the .NET side. The code automatically restarts the board on failure.

## .NET 

The .NET side is based on .NET 8 and uses Azure Functions and Azure Cosmos DB to have a very cheap solution for data storage. The functions are configured to use function-level authorization to give a basic level of security across the solution.

## Frontend

There is also a very simple (unstyled) frontend that uses Plotly.js to draw the timeseries charts. This is served by the .NET backend, through the Index function.