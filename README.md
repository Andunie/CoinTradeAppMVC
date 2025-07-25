A crypto trading **simulation platform** built with **ASP.NET Core MVC (.NET 8.0)** that enables users to test cryptocurrency trading strategies using real-time market data without financial risk.

---

## 📌 Overview

CoinTradeAppMVC allows users to:

- 🪙 Simulate buy/sell spot trades using real-time data
- 📈 Monitor their portfolio with live price updates via **SignalR**
- 🔐 Sign up, log in, and manage their account securely with **ASP.NET Identity**
- 🧠 Detect candlestick patterns (Bullish Engulfing, Doji, Hammer, etc.) using a **FastAPI** Python service
- 📊 Visualize portfolio distribution via **dynamic Chart.js doughnut graphs**
- 💬 Contact support via email integration

---

## ⚙️ Technologies

| Layer        | Technologies & Tools                                   |
|--------------|--------------------------------------------------------|
| Backend      | ASP.NET Core MVC (.NET 8.0), Entity Framework Core     |
| Frontend     | CSS, Bootstrap 5, Chart.js, JavaScript, Chart.JS       |
| Realtime     | SignalR   (JS)                                         |
| Authentication | ASP.NET Core Identity + Email confirmation           |
| Pattern Detection | Python (FastAPI) + TA-Lib (exclude code)          |
| Database     | MS SQL Server                                          |
| Others       | Redis (for caching), SMTP (Gmail)                      |
| Binance Spot | For real time cryptocurrency data                      |
---

## 🗂️ Project Structure

```plaintext
CoinTradeAppMVC/
├── ApiModels/
├── Areas/
├── Controllers/
├── CustomValidations/
├── Entities/
├── Models/
├── OptionsModels/
├── Resources/
├── ViewModels/
├── Views/
├── Hubs/                    # SignalR Hub for live updates
├── Services/                # Email service, price service
├── wwwroot/
├── Data/                    # EF Core DbContext
├── Migrations/
└── appsettings.json

## Another Project for Background Services **(not included on this repository)**

OrderWorkerService/
├── Worker.cs

## Screenshots :

[[![Screenshot 1](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/screenshot-2025-07-25-021600.png)](https://github.com/Andunie/CoinTradeAppMVC/blob/6199c51f97c36b470a0f5244d07bdf7d8bdb212d/images/1748710189580.jpg)](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/master/images/1748710189580.jpg)

![Screenshot 2](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/screenshot-2025-07-25-021615.png)
![Screenshot 3](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/screenshot-2025-07-25-021626.png)
![Screenshot 4](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/screenshot-2025-07-25-022049.png)
![Screenshot 5](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/1748710189580.jpg)
![Screenshot 6](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/1748710189630.jpg)
![Screenshot 7](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/1748710190188.jpg)
![Screenshot 8](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/1748710190279.jpg)
![Screenshot 9](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/1748710190310.jpg)
![Screenshot 10](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/1748710190312.jpg)
![Screenshot 11](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/1748710190416.jpg)
![Screenshot 12](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/1748710190578.jpg)
![Screenshot 13](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/1748710190596.jpg)
![Screenshot 14](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/1748710190753.jpg)
![Screenshot 15](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/1748710190770.jpg)
![Screenshot 16](https://raw.githubusercontent.com/Andunie/CoinTradeAppMVC/main/images/1748710190776.jpg)

