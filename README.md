# Drifters - Camera Management System

A .NET Core Web API for managing IP cameras and processing detection results. The system supports RTSP camera streams, user authentication, and real-time monitoring capabilities.

## Features

- **User Management**: Registration, login, email confirmation, and password reset
- **Camera Management**: Add, configure, and monitor IP cameras with RTSP support
- **Stream Processing**: Support for both RTSP and HLS streaming protocols
- **Role-based Access**: Manager and Observer roles with different permissions
- **Site Management**: Onboarding system for managing monitored entities
- **Detection Processing**: API endpoints for submitting and processing camera detection results
- **Dashboard Analytics**: Basic statistics and camera status monitoring

## Technology Stack

- **.NET 8.0**: Web API framework
- **Entity Framework Core**: ORM for data access
- **ASP.NET Core Identity**: Authentication and authorization
- **SQL Server**: Database
- **JWT Authentication**: Token-based authentication
- **Data Protection API**: Secure credential storage
- **Email Services**: SMTP integration for notifications

## Project Structure

```
drifters/ (Solution Root)
├── BLL/                           
│   ├── Dependencies/              
│   ├── Interfaces/                
│   ├── Repositories/             
│   └── Specifications/            
├── DAL/                          
│   ├── Dependencies/              
│   ├── Configurations/            
│   ├── Data/                    
│   ├── DbInitializer/           
│   ├── Migrations/               
│   └── Models/                   
├── PL/                           
│   ├── Connected Services/        
│   ├── Dependencies/              
│   ├── Properties/               
│   ├── Controllers/              
│   ├── DTOs/                     
│   ├── Email/                    
│   ├── Hubs/                     
│   ├── Services/                
│   ├── wwwroot/                  
│   ├── appsettings.json         
│   ├── ImageHelper.cs           
│   ├── PL.http                  
│   └── Program.cs                
└── Utilities/                    
```

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/Amg04/drifters.git
cd drifters
```

### 2. Database Setup

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DriftersDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```


### 3. Install Dependencies and Run

```bash
# Restore NuGet packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run --project PL
```

The API will be available at:
- HTTPS: `https://localhost:7040`
- Swagger: `https://localhost:7040/swagger`

## API Endpoints

### Account Management

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| POST | `/api/Account/Register` | Register a new user | None |
| GET | `/api/Account/ConfirmEmail` | Confirm user email address | None |
| POST | `/api/Account/Login` | User login | None |
| POST | `/api/Account/Logout` | User logout | Required |
| POST | `/api/Account/ResetPassword` | Request password reset | None |
| GET | `/api/Account/ShowResetPasswordPage` | Display password reset form | None |
| POST | `/api/Account/ResetPasswordConfirm` | Confirm password reset | None |

### Camera Management

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| POST | `/api/Cameras` | Create a new camera | Manager Role |
| GET | `/api/Cameras/{id}` | Get camera details by ID | Required |
| POST | `/api/Cameras/QuickAction` | Perform quick action on camera | None |

### Dashboard

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| GET | `/api/Dashboard` | Get dashboard statistics and camera status | None |

### Home/Streaming

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| GET | `/api/Home/hls/{id}` | Get HLS stream URL for camera | Required |
| GET | `/api/Home/LiveCamera` | Get all active HLS streams | Required |
| POST | `/api/Home/CameraDetection` | Submit camera detection results | None |
| GET | `/api/Home/rtsp/{id}` | Get RTSP stream URL for camera | None |
| GET | `/api/Home/rtsp` | Get all active RTSP streams | None |

### Onboarding

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| GET | `/api/Onboarding/SiteSelection` | Get site selection data and user's existing sites | Required |
| POST | `/api/Onboarding/SiteSelection` | Create or update monitored entity/site | Required |

### User Management

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| GET | `/api/User/Profile` | Get user profile information | Required |
| PUT | `/api/User/profile` | Update user profile | Required |
| POST | `/api/User/ChangePassword` | Change user password | Required |

## Usage Examples

### User Registration

```bash
curl -X POST "https://localhost:7040/api/Account/Register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "userName": "testuser",
    "password": "Password123!"
  }'
```

### User Login

```bash
curl -X POST "https://localhost:7040/api/Account/Login" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser",
    "password": "Password123!"
  }'
```

### Add a Camera

```bash
curl -X POST "https://localhost:7040/api/Cameras" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "host": "192.168.1.100",
    "port": 554,
    "username": "admin",
    "passwordEnc": "camera_password",
    "rtspPath": "/stream1",
    "enabled": true,
    "cameraLocation": "Front Entrance"
  }'
```

### Submit Detection Results

```bash
curl -X POST "https://localhost:7040/api/Home/CameraDetection" \
  -H "Content-Type: application/json" \
  -d '[{
    "cameraId": 1,
    "status": "abnormal",
    "crowd_density": 0.3,
    "detectionTime": "2024-01-01T12:00:00Z"
  }]'
```

## Authentication

The API uses JWT (JSON Web Tokens) for authentication. Include the token in the Authorization header:

```
Authorization: Bearer YOUR_JWT_TOKEN
```


## User Roles
- **Admin**: Can create cameras and manage observers ,add users and control 
- **Manager**: Can create cameras and manage observers
- **Observer**: Can view cameras and streams assigned by their manager

## RTSP Camera Configuration

The system supports various IP camera brands. Common RTSP URL formats:

```
# Generic format
rtsp://username:password@camera_ip:554/path

# Hikvision
rtsp://admin:password@192.168.1.100:554/Streaming/Channels/101

# Dahua
rtsp://admin:password@192.168.1.100:554/cam/realmonitor?channel=1&subtype=0
```

## Database Models

### Key Entities

- **AppUser**: Extended Identity user with manager relationships
- **MonitoredEntity**: Sites or locations being monitored
- **Camera**: IP camera configurations and settings
- **CameraDetection**: Detection results and alerts

## Configuration

### Email Service

Configure SMTP settings for email notifications:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password"
  }
}
```

### Data Protection

The system uses ASP.NET Core Data Protection to encrypt camera passwords. Keys are automatically managed but can be configured for production environments.

## Development

### Running Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```
