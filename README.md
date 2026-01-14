# 🛡️ WAAD: Enterprise Commitment Tracking System

**WAAD** (meaning *Promise* or *Commitment*) is a proactive enterprise platform designed to eliminate fragmented accountability and ensure organizational reliability.  
Unlike traditional task managers, WAAD features an automated **"Brain"** that monitors deadlines in real time and notifies users instantly via WebSockets.

---

## 🚀 Key Features

- **Proactive Monitoring**  
  A dedicated Background Service polls the database every 60 seconds to detect overdue commitments.

- **Real-Time Alerts**  
  Integrated **SignalR** and **Toastr** provide instant, non-intrusive notifications without page refreshes.

- **Executive Analytics**  
  A dynamic **Chart.js** dashboard provides leadership with a high-level visual **Source of Truth**.

- **Enterprise Security**  
  Built with **ASP.NET Identity**, featuring:
  - Two-Factor Authentication (2FA) via Microsoft Authenticator  
  - Role-Based Access Control (RBAC)

- **Quality Assurance Focused**  
  Implements **Server-Side Ownership Validation** to prevent IDOR attacks and ensure strict data isolation.

---

## 🏗️ Technical Architecture

The system follows a clean **Separation of Concerns** using the **ASP.NET Core MVC** pattern:

- **Backend:** .NET 8 / C#  
- **Frontend:** Razor Views, Bootstrap 5, Chart.js, Toastr  
- **Database:** SQL Server (Entity Framework Core)  
- **Real-Time Communication:** SignalR (WebSockets)

---

## 📂 Project Structure

| Folder       | Responsibility |
|--------------|----------------|
| Controllers  | Handles HTTP requests and business logic flow |
| Models       | Defines the data schema and relational mappings (1:N, M:N) |
| Views        | User Interface layer (Admin & Employee portals) |
| Services     | Contains `OverdueTaskService` (Background Worker) and `NotificationHub` |
| Data         | Contains `ApplicationDbContext` for SQL Server communication |

---

## 🛠️ Database Schema

The relational model ensures complete **Referential Integrity**:

- **User → Follow-Ups (1:N)**  
  Strict data isolation via `AssignedEmployeeId`.

- **Follow-Up → Alerts (1:N)**  
  Permanent audit trail for every notification sent.

- **Roles → Users (M:N)**  
  Flexible RBAC enabling users to hold both **Admin** and **Employee** roles.

---

## 🔮 Future Roadmap

- **ERP Integration**  
  RESTful API for SAP / Oracle integration.

- **AI Intent Detection**  
  NLP-based extraction of "Waads" from Microsoft Teams conversations.

- **Meeting Intelligence**  
  AI agents to transcribe meetings and extract verbal commitments automatically.

---

