# ProjeX Blazor - Deployment Guide

## Prerequisites

### Development Environment
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or higher)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/downloads)

### Production Environment
- Windows Server 2019+ or Linux (Ubuntu 20.04+)
- IIS 10+ (Windows) or Nginx/Apache (Linux)
- SQL Server 2019+ or Azure SQL Database
- .NET 8 Runtime (ASP.NET Core)

## Local Development Setup

### 1. Clone and Build
```bash
git clone <repository-url>
cd ProjeX
dotnet restore
dotnet build
```

### 2. Database Configuration
Update the connection string in `src/ProjeX.Blazor/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ProjeX;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### 3. Database Migration
```bash
cd src/ProjeX.Infrastructure
dotnet ef database update --startup-project ../ProjeX.Blazor
```

### 4. Run the Application
```bash
cd src/ProjeX.Blazor
dotnet run
```

The application will be available at `https://localhost:5001`

## Production Deployment

### Option 1: Windows Server with IIS

#### 1. Prepare the Server
- Install .NET 8 ASP.NET Core Runtime
- Install IIS with ASP.NET Core Module
- Install SQL Server or configure connection to Azure SQL

#### 2. Publish the Application
```bash
dotnet publish src/ProjeX.Blazor/ProjeX.Blazor.csproj -c Release -o ./publish
```

#### 3. Configure IIS
- Create a new website in IIS Manager
- Point to the published folder
- Set the Application Pool to "No Managed Code"
- Configure SSL certificate

#### 4. Update Configuration
Update `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=ProjeX;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

### Option 2: Linux with Nginx

#### 1. Install Dependencies
```bash
# Ubuntu/Debian
sudo apt update
sudo apt install nginx
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install aspnetcore-runtime-8.0
```

#### 2. Deploy Application
```bash
# Copy published files to server
sudo mkdir /var/www/projex
sudo cp -r ./publish/* /var/www/projex/
sudo chown -R www-data:www-data /var/www/projex
```

#### 3. Configure Systemd Service
Create `/etc/systemd/system/projex.service`:
```ini
[Unit]
Description=ProjeX Blazor Application
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/projex/ProjeX.Blazor.dll
Restart=always
RestartSec=5
KillSignal=SIGINT
SyslogIdentifier=projex
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

#### 4. Configure Nginx
Create `/etc/nginx/sites-available/projex`:
```nginx
server {
    listen 80;
    server_name your-domain.com;
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

#### 5. Enable and Start Services
```bash
sudo systemctl enable projex
sudo systemctl start projex
sudo ln -s /etc/nginx/sites-available/projex /etc/nginx/sites-enabled/
sudo systemctl restart nginx
```

### Option 3: Docker Deployment

#### 1. Create Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ProjeX.Blazor/ProjeX.Blazor.csproj", "src/ProjeX.Blazor/"]
COPY ["src/ProjeX.Application/ProjeX.Application.csproj", "src/ProjeX.Application/"]
COPY ["src/ProjeX.Domain/ProjeX.Domain.csproj", "src/ProjeX.Domain/"]
COPY ["src/ProjeX.Infrastructure/ProjeX.Infrastructure.csproj", "src/ProjeX.Infrastructure/"]
RUN dotnet restore "src/ProjeX.Blazor/ProjeX.Blazor.csproj"
COPY . .
WORKDIR "/src/src/ProjeX.Blazor"
RUN dotnet build "ProjeX.Blazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProjeX.Blazor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjeX.Blazor.dll"]
```

#### 2. Build and Run
```bash
docker build -t projex-blazor .
docker run -d -p 8080:80 --name projex-app projex-blazor
```

## Database Setup

### Initial Migration
```bash
cd src/ProjeX.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../ProjeX.Blazor
dotnet ef database update --startup-project ../ProjeX.Blazor
```

### Seed Data
The application includes automatic data seeding for:
- Default admin user (admin@projex.com / Admin123!)
- Sample roles and permissions
- Basic configuration data

## Configuration

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Development, Staging, Production
- `ConnectionStrings__DefaultConnection`: Database connection string
- `HTTPS_PORT`: HTTPS port for development

### Application Settings
Key configuration options in `appsettings.json`:
- Database connection strings
- Logging levels
- Authentication settings
- Syncfusion license key (if using licensed version)

## Security Considerations

### 1. Database Security
- Use strong passwords for database connections
- Enable SQL Server authentication
- Configure firewall rules
- Regular security updates

### 2. Application Security
- Enable HTTPS in production
- Configure secure cookie settings
- Implement proper CORS policies
- Regular dependency updates

### 3. Server Security
- Keep OS and runtime updated
- Configure proper firewall rules
- Use SSL certificates
- Monitor application logs

## Monitoring and Maintenance

### 1. Logging
The application uses Serilog for structured logging:
- Application logs are written to console and files
- Error tracking and performance monitoring
- Audit trail for user actions

### 2. Health Checks
Configure health check endpoints:
- Database connectivity
- External service dependencies
- Application performance metrics

### 3. Backup Strategy
- Regular database backups
- Application file backups
- Configuration backups
- Disaster recovery procedures

## Troubleshooting

### Common Issues

#### 1. Database Connection Issues
- Verify connection string format
- Check SQL Server service status
- Validate user permissions
- Test network connectivity

#### 2. Authentication Problems
- Check Identity configuration
- Verify cookie settings
- Review user roles and permissions
- Check HTTPS configuration

#### 3. Performance Issues
- Monitor database query performance
- Check memory usage
- Review application logs
- Optimize Blazor component rendering

### Support Resources
- Application logs: Check Serilog output
- Database logs: SQL Server error logs
- IIS logs: Windows Event Viewer
- System logs: Event Viewer (Windows) or journalctl (Linux)

## Scaling Considerations

### 1. Database Scaling
- Connection pooling optimization
- Read replicas for reporting
- Database indexing strategy
- Query optimization

### 2. Application Scaling
- Load balancer configuration
- Session state management
- Caching strategies
- CDN for static assets

### 3. Infrastructure Scaling
- Auto-scaling groups
- Container orchestration
- Monitoring and alerting
- Performance testing

## Maintenance Schedule

### Daily
- Monitor application health
- Check error logs
- Verify backup completion

### Weekly
- Review performance metrics
- Update security patches
- Database maintenance

### Monthly
- Dependency updates
- Security audit
- Performance optimization
- Backup testing

