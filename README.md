Here is the updated README with your connection string included, translated into English:

---

````markdown
# 🍔 MarcDonalds — Online Fast Food Ordering System

This is a course project called **MarcDonalds** — a fully functional online fast food ordering system built with **ASP.NET Core MVC** using the **Code First** approach and **SQL Server** running in **Docker**.

---

## 🚀 Main Features

- 🛒 **Online Ordering**  
  Add and remove products from the cart.  
  Place orders with address input, payment simulation, and delivery emulation.

- 🔐 **Authentication & Authorization**  
  Implemented with `User.Identity` and ASP.NET Core Identity.  
  Account confirmation, password reset, and email receipt sending.  
  Cookie-based authentication with a 5–10 minute session timeout (in development mode).

- 📦 **Address Management**  
  Add multiple addresses and edit their properties before placing an order.

- 💳 **Payment and Tipping Simulation**  
  Simulated card payment with the option to leave tips.

- 📍 **Geolocation & Maps**  
  - Location detection via **Nominatim** (reverse geocoding).  
  - Initially used **Leaflet.js**, but switched to **Mapbox** due to dark mode issues (paid, stable, supports bounds and zoom).

- 🌍 **Localization**  
  - Language switching using `.NET CultureInfo`.  
  - Selected language is saved in cookies for user convenience.

- 🧩 **Architecture & Code**  
  - Repository pattern used for data access.  
  - Partial Views used to reduce code duplication.  
  - Custom Error Modal View for clean error display.  
  - Caching implemented to reduce database load.

- 🧾 **Logging & Resilience**  
  - Kafka was considered, but due to project scope, logging is handled with `ILogger`.  
  - Extensive logging for stability and diagnostics.

- 🎨 **Modern Design**  
  A strong, modern UI design was created following current trends.  
  Intuitive interface adapted for mobile, tablet, and desktop.  
  Custom logo designed in **Figma**, suitable for various formats and backgrounds.

---

## 🛠️ Technologies

- ASP.NET Core MVC (.NET 7)  
- Entity Framework Core (Code First)  
- SQL Server (Docker)  
- Identity Framework  
- Mapbox + Nominatim  
- CultureInfo + Cookies  
- Figma (for logo and visual design)

---

## 📦 How to Run

1. **Clone the repository**  
   ```bash
   git clone https://github.com/your-username/MarcDonalds.git
   cd MarcDonalds
````

2. **Run SQL Server via Docker**
   Make sure Docker is installed, then run:

   ```bash
   "docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=StrongPassw0rd!" -p 1433:1433 --name sql-marc -d mcr.microsoft.com/mssql/server"
   ```

3. **Configure the connection string**
   In `appsettings.Development.json`, use the following connection string:

   ```json
   "DefaultConnection": "Server=localhost,1433;Database=MarkRestaurant;User Id=sa;Password=StrongPassw0rd!;TrustServerCertificate=True;"
   ```

4. **Apply database migrations**

   ```bash
   dotnet ef database update
   ```

5. **Run the project**

   ```bash
   dotnet run
   ```
