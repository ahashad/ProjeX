# ProjeX - Modern Project Management

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Blazor](https://img.shields.io/badge/Blazor-Server-purple.svg)](https://blazor.net/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

**ProjeX** is a comprehensive, modern web application for managing consultancy projects, built with .NET 8, Blazor Server, and Clean Architecture principles. It provides a robust platform for managing clients, projects, employees, time entries, and financials within a secure and scalable environment.

## ✨ Features

- **🎨 Modern UI/UX**: Built with Blazor Server, Bootstrap 5, and Syncfusion components
- **🏗️ Clean Architecture**: Service-based architecture following SOLID principles
- **🔐 Secure Authentication**: ASP.NET Core Identity with role-based access control
- **👥 Client & Project Management**: Complete CRUD operations with detailed views
- **👨‍💼 Employee Management**: Role-based employee management with compensation tracking
- **⏱️ Time Tracking**: Comprehensive time entry system with approval workflows
- **💰 Financial Management**: Invoice generation and overhead tracking (ready for implementation)
- **📊 Interactive Grids**: Powerful data grids with filtering, sorting, and paging
- **📱 Responsive Design**: Works seamlessly on desktop, tablet, and mobile devices

## 🚀 Technology Stack

- **Backend**: .NET 8, ASP.NET Core, Entity Framework Core
- **Frontend**: Blazor Server, Syncfusion Blazor Components, Bootstrap 5
- **Architecture**: Clean Architecture, Service-Based Pattern
- **Authentication**: ASP.NET Core Identity
- **Database**: SQL Server (Entity Framework Core compatible)
- **UI Components**: Syncfusion Blazor, Font Awesome Icons

## 🏁 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or higher)
- [Git](https://git-scm.com/downloads)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/ahashad/ProjeX.git
   cd ProjeX
   ```

2. **Configure the database**
   
   Update the connection string in `src/ProjeX.Blazor/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=ProjeX;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     }
   }
   ```

3. **Apply database migrations**
   ```bash
   cd src/LastMinute.Consultancy.Infrastructure
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   cd ../ProjeX.Blazor
   dotnet run
   ```

5. **Access the application**
   
   Open your browser and navigate to `https://localhost:5001`

## 📖 Documentation

- **[Setup Guide](SETUP.md)**: Detailed setup and configuration instructions
- **[Migration Guide](MIGRATION_GUIDE.md)**: Complete migration documentation from ASP.NET Core MVC

## 🏗️ Architecture

ProjeX follows Clean Architecture principles with clear separation of concerns:

```
src/
├── LastMinute.Consultancy.Domain/          # Domain entities and business rules
├── LastMinute.Consultancy.Application/     # Application services and DTOs
├── LastMinute.Consultancy.Infrastructure/  # Data access and external services
├── LastMinute.Consultancy.Web/            # Original MVC application (legacy)
└── ProjeX.Blazor/                         # Modern Blazor Server application
```

## 🔐 User Roles

- **Admin**: Full access to all features and system settings
- **Manager**: Can manage projects, employees, and approve time entries
- **Employee**: Can view assigned projects and log time entries

## 🛠️ Development

### Building the Solution

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Code Structure

The application follows these key patterns:

- **Repository Pattern**: For data access abstraction
- **Service Layer**: Business logic encapsulation
- **DTO Pattern**: Data transfer between layers
- **Dependency Injection**: Loose coupling and testability

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Syncfusion](https://www.syncfusion.com/) for their excellent Blazor components
- [Bootstrap](https://getbootstrap.com/) for the responsive CSS framework
- [Font Awesome](https://fontawesome.com/) for the beautiful icons
- Microsoft for the amazing .NET and Blazor frameworks

## 📞 Support

If you have any questions or need help getting started, please open an issue or reach out to the maintainers.

---

**Built with ❤️ using .NET 8 and Blazor Server**

