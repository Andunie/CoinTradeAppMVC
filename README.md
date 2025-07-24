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

<img width="1919" height="936" alt="screenshot-2025-07-25-021615" src="https://github.com/user-attachments/assets/225d9070-b01a-4566-80d1-05c1a38ac03e" />

<img width="1919" height="935" alt="screenshot-2025-07-25-021626" src="https://github.com/user-attachments/assets/aea7dc10-9f02-4645-a05f-0708b7096932" />

<img width="1916" height="1027" alt="screenshot-2025-07-25-022049" src="https://github.com/user-attachments/assets/73d2667f-4365-4a11-9a33-973352815d49" />

<img width="1916" height="1027" alt="screenshot-2025-07-25-021600" src="https://github.com/user-attachments/assets/bea9f283-116b-4af0-b2d7-1160dc8c6d58" />

![1748710190776](https://github.com/user-attachments/assets/ba8a8a56-8e9a-4c3e-a0d9-4679a975b17b)

![1748710190770](https://github.com/user-attachments/assets/61808535-0d95-40f8-a7bf-9a1e69a00968)

![1748710190753](https://github.com/user-attachments/assets/b84a79bd-d4e8-42b8-818b-920ceb028f1e)

![1748710190596](https://github.com/user-attachments/assets/0e6b614f-1438-4db9-aa9d-7a9d3ff11a96)

![1748710190578](https://github.com/user-attachments/assets/9899bd9a-5e95-4523-bfa4-b782db5bf28f)

![1748710190416](https://github.com/user-attachments/assets/3768417b-1242-4267-b089-d46cbc98900e)

![1748710190312](https://github.com/user-attachments/assets/34d56fe5-0192-45b7-9132-0c9e51de0c45)

![1748710190310](https://github.com/user-attachments/assets/219fa3e6-a1b5-4d22-993a-1393ce65fc1a)

![1748710190279](https://github.com/user-attachments/assets/cdd0cea7-e879-4fdd-aba1-011a5c3825d1)

![1748710190188](https://github.com/user-attachments/assets/44ede4f8-f666-4042-8b85-59f99ef28a40)

![1748710189630](https://github.com/user-attachments/assets/ff993921-269f-4aa1-ba41-436ff1ed4e04)

![1748710189580](https://github.com/user-attachments/assets/b1037d04-c68d-41d9-b7e3-973d9adeb803)
