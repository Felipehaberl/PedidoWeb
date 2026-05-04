# 📦 PedidoWeb - Sales Order Management System

Web application developed with ASP.NET Core MVC for sales order management, focused on external sales teams and real-time ERP integration.

---

## 🚀 Overview

**PedidoWeb** is a business-oriented system that allows sales representatives to:

- Create and manage sales orders  
- Access real-time product and stock data  
- Manage customers and payment conditions  
- Automatically synchronize orders with ERP systems  

---

## 🧠 Key Features

- 🔐 Authentication and role-based access (Admin / Sales)  
- 🛒 Order management (Draft → Approved → Integrated)  
- 🔍 Product search with AJAX (code, description, barcode)  
- 📦 Stock validation before order confirmation  
- 🔄 ERP integration (SOAP / WCF)  
- 📄 Order preview and printing  
- ⚙️ Business rules and validation layer  

---

## 🔄 ERP Integration

Bidirectional integration with ERP system:

**Import:**
- Customers  
- Products (stock and pricing)  
- Payment conditions  

**Export:**
- Automatic order submission  

**Error Handling:**
- XML sanitization  
- Logging and failure handling  

---

## 🛠 Tech Stack

- **Backend:** .NET 6 / ASP.NET Core MVC  
- **Database:** SQL Server (Entity Framework Core)  
- **Frontend:** Razor Views, Bootstrap 5, jQuery  
- **Authentication:** ASP.NET Core Identity  
- **Integration:** WCF (SOAP)  

---

## 🏗 Architecture

- MVC pattern (separation of concerns)  
- Service layer for business logic and integrations  
- Modular structure for scalability  
- Focus on maintainability and performance  

---

## ⚙️ Setup

```bash
git clone https://github.com/seu-usuario/PedidoWeb.git
cd PedidoWeb
dotnet ef database update
dotnet run
