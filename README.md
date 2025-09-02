# Camera Detection AI App

A real-time AI-powered camera detection system built with .NET Core, implementing advanced computer vision capabilities for object detection and monitoring through RTSP camera streams.

## 🚀 Features

- **Real-time Object Detection**: Advanced AI model integration for live object detection
- **RTSP Stream Support**: Connect to multiple IP cameras via RTSP protocol
- **Repository Pattern**: Clean architecture with repository and unit of work patterns
- **Background Processing**: Continuous frame processing with background services
- **RESTful API**: Comprehensive REST API for camera and detection management
- **Database Integration**: Entity Framework Core with SQL Server
- **Real-time Updates**: SignalR integration for live detection updates
- **Configurable Settings**: Camera-specific detection settings and zones
- **Alert System**: Email, SMS, and webhook notifications
- **Image Storage**: Automatic saving of detection images
- **Analytics Dashboard**: Detection statistics and reporting

## 🏗️ Architecture

The application follows a clean 4-layer architecture pattern:

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
│   ├── Services/             
│   ├── appsettings.json        
│   ├── PL.http                 
│   └── Program.cs              
└── Utilities/                 
```

### Layer Responsibilities:

**🎯 Presentation Layer (PL)**
- REST API controllers and endpoints
- HTTP request/response handling
- Background services for camera processing
- SignalR hubs for real-time updates
- Authentication & authorization

**💼 Business Logic Layer (BLL)**
- Service interfaces and business rules
- Repository pattern interfaces
- Domain specifications and validation
- Business workflows and orchestration

**🗃️ Data Access Layer (DAL)**
- Entity Framework DbContext
- Entity configurations and mappings
- Database migrations and seeding
- Repository pattern implementations
- Data models and relationships

**🔧 Utilities**
- Cross-cutting concerns
- Extension methods and helpers
- Constants and configurations
- Mapping profiles (AutoMapper)

## 🛠️ Technology Stack

- **.NET 8.0**: Backend framework
- **Entity Framework Core**: ORM for data access
- **SQL Server**: Database
- **ML.NET**: Machine learning framework
- **OpenCV**: Computer vision library
- **SignalR**: Real-time communication
- **RTSP**: Real-Time Streaming Protocol for cameras
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: IoC container
- **Background Services**: Continuous processing

## 📋 Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
- IP cameras with RTSP support (optional for testing)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/Amg04/drifters.git
```

### 2. Database Setup

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CameraDetectionDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Install Dependencies

```bash
# Restore NuGet packages
dotnet restore

# Install Entity Framework tools
dotnet tool install --global dotnet-ef
```


The API will be available at:
- HTTP: `http://localhost:7040`
- HTTPS: `https://localhost:7040`
- Swagger: `https://localhost:7040/swagger`

## 📚 API Endpoints

### Cameras
- `GET /api/cameras` - Get all cameras
- `GET /api/cameras/{id}` - Get camera by ID
- `POST /api/cameras` - Add new camera
- `PUT /api/cameras/{id}` - Update camera
- `DELETE /api/cameras/{id}` - Delete camera
- `POST /api/cameras/{id}/toggle` - Enable/disable camera

### Detections
- `GET /api/detections` - Get all detections
- `GET /api/detections/camera/{cameraId}` - Get detections by camera
- `GET /api/detections/{id}` - Get detection by ID
- `GET /api/detections/stats` - Get detection statistics
- `POST /api/detections/process-frame` - Process camera frame

### Real-time Updates
- SignalR Hub: `/detectionsHub`
- Events: `DetectionUpdate`, `CameraStatusChanged`

## 🎯 Usage Examples

### Add a Camera

```bash
curl -X POST "https://localhost:7040/api/cameras" \
  -H "Content-Type: application/json" \
  -d '{
  "name": "cam1",
  "host": "192.168.1.100",
  "port": 443,
  "username": "admin",
  "passwordEnc": "Pass@123",
  "rtspPath": "/rtsp",
  "enabled": true
}
  }'
```

### Get Recent Detections

```bash
curl -X GET "https://localhost:5001/api/detections?take=10"
```

### Process Camera Frame

```bash
curl -X POST "https://localhost:5001/api/detections/process-frame" \
  -H "Content-Type: multipart/form-data" \
  -F "cameraId=cam-001" \
  -F "imageData=@frame.jpg"
```


# Generic IP Camera
rtsp://username:password@192.168.1.100:554/stream1

# Hikvision
rtsp://admin:password@192.168.1.100:554/Streaming/Channels/101

# Dahua
rtsp://admin:password@192.168.1.100:554/cam/realmonitor?channel=1&subtype=0
```

## 📊 Database Schema

### Cameras Table
- `Id` (string, PK)
- `Name` (string)
- `StreamUrl` (string)
- `IsActive` (bool)


### Detections Table
- `Id` (int, PK, Identity)
- `CameraId` (string, FK)
- `Description` (string)


## 🔍 Monitoring & Logging

The application uses structured logging with Serilog:

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/app-.log" } }
    ]
  }
}
```

## 🚀 Deployment

### Docker Support

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CameraDetection.API.dll"]
```

### Azure Deployment

1. Create Azure SQL Database
2. Deploy to Azure App Service
3. Configure Application Settings
4. Set up Application Insights for monitoring

## 🧪 Testing

```bash
# Run unit tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📈 Performance Optimization

- **Frame Skipping**: Skip frames during high load
- **GPU Acceleration**: CUDA support for AI inference
- **Caching**: Redis for frequently accessed data
- **Load Balancing**: Multiple API instances
- **Database Indexing**: Optimized queries with proper indexes
