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
