


# ProjeX Blazor - Setup Guide

This guide provides detailed instructions for setting up and running the ProjeX Blazor application on your local machine.

## 1. Prerequisites

Ensure you have the following software installed on your system:

- **.NET 8 SDK**: [Download and install the .NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server**: [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or any other version of SQL Server.
- **Git**: [Download and install Git](https://git-scm.com/downloads)

## 2. Clone the Repository

Clone the `Impact.Consultancy` repository to your local machine:

```bash
git clone https://github.com/ahashad/Impact.Consultancy.git
cd Impact.Consultancy
```

## 3. Database Configuration

The application uses Entity Framework Core to manage the database. You need to configure the connection string and apply migrations to create the database schema.

### 3.1. Configure Connection String

Open the `appsettings.json` file located in the `src/ProjeX.Blazor` directory. Update the `DefaultConnection` connection string with your SQL Server details:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=ProjeX_Blazor;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  // ... other settings
}
```

Replace `YOUR_SERVER_NAME` with your SQL Server instance name (e.g., `localhost`, `.\SQLEXPRESS`).

### 3.2. Apply Migrations

Open a terminal or command prompt and navigate to the `src/LastMinute.Consultancy.Infrastructure` directory. Run the following command to apply the database migrations:

```bash
dotnet ef database update
```

This will create the `ProjeX_Blazor` database and all the necessary tables.

## 4. Run the Application

Navigate to the `src/ProjeX.Blazor` directory and run the application:

```bash
dotnet run
```

The application will be available at `https://localhost:5001` (or a similar port). Open your web browser and navigate to this URL.

## 5. User Registration and Roles

### 5.1. Register a New User

Click on the "Register" link in the top navigation bar to create a new user account. The first registered user will be automatically assigned the "Admin" role.

### 5.2. User Roles

The application uses the following roles:

- **Admin**: Full access to all features and settings.
- **Manager**: Can manage projects, employees, and time entries.
- **Employee**: Can view assigned projects and log time entries.

## 6. Syncfusion License

The application uses Syncfusion Blazor components. To use the application without the trial watermark, you need to add a valid Syncfusion license key.

1.  Register for a free community license or purchase a license from [Syncfusion](https://www.syncfusion.com/).
2.  Open the `Program.cs` file in the `src/ProjeX.Blazor` directory.
3.  Add your license key at the beginning of the `Main` method:

```csharp
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_SYNCFUSION_LICENSE_KEY");
```

Replace `YOUR_SYNCFUSION_LICENSE_KEY` with your actual license key.


