# CRUD App - Person Management System

A full-stack application for managing persons with profile pictures, built with .NET backend, React frontend, MongoDB for data storage, and MinIO for file storage.

## Features

- ✅ Create, Read, Update, Delete (CRUD) operations for persons
- 📸 Profile picture upload and management with MinIO
- 🔍 Sorting and pagination
- 👥 Friend management system
- ⭐ Automatic star sign calculation from CPR
- 🎂 Age calculation from CPR
- 🐳 Docker containerization for MongoDB, Mongo Express, MinIO, and Seeder

## Tech Stack

### Backend
- **.NET 10.0** - Web API
- **MongoDB** - Database for person data
- **MinIO** - Object storage for images/files
- **C#** - Programming language

### Frontend
- **React** - UI framework
- **JavaScript** - Programming language

### DevOps
- **Docker & Docker Compose** - Containerization
- **Mongo Express** - MongoDB web interface
- **MinIO Console** - File storage web interface

## Prerequisites

Make sure you have the following installed:

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v16 or higher)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Docker Compose](https://docs.docker.com/compose/install/)

## Project Structure

```
crud_app_2/
├── backend/                # .NET Web API
│   ├── Models/            # Data models
│   ├── routs/             # API controllers
│   ├── Services/          # Business logic (FileService)
│   ├── Util/              # Helper utilities
│   ├── Program.cs         # Application entry point
│   └── appsettings.json   # Configuration
├── frontend/              # React application
│   ├── src/
│   │   ├── elements/      # React components
│   │   └── UtilElements/  # Utility components
│   └── public/
├── seeder/                # MongoDB seeder
├── docker-compose.yml     # Docker services configuration
├── .env                   # Environment variables
└── comands.txt           # Quick reference commands
```

## Setup Instructions

### 1. Configure Environment Variables

The `.env` file is already configured with default values:

```env
# MongoDB
MONGO_INITDB_ROOT_USERNAME=EXAMPEL
MONGO_INITDB_ROOT_PASSWORD=EXAMPEL

# Mongo Express
MONGO_EXPRESS_BASICAUTH_USERNAME=EXAMPEL
MONGO_EXPRESS_BASICAUTH_PASSWORD=EXAMPEL

# MinIO
MINIO_ROOT_USER=EXAMPEL
MINIO_ROOT_PASSWORD=EXAMPEL
```

You can modify these values if needed.

### 2. Start Docker Services

Start all Docker containers (MongoDB, Mongo Express, MinIO, and Seeder):

```bash
docker-compose up -d
```

This will start:
- **MongoDB** on `localhost:27017`
- **Mongo Express** on `http://localhost:8081`
- **MinIO API** on `http://localhost:9000`
- **MinIO Console** on `http://localhost:9001`
- **Seeder** (runs once to populate database)

### 3. Install Backend Dependencies

```bash
cd backend
dotnet restore
```

### 4. Install Frontend Dependencies

```bash
cd frontend
npm install
```

## Running the Application

### Option 1: Run Everything Separately

#### Terminal 1 - Backend
```bash
cd backend
dotnet run
```

Or with hot reload:
```bash
dotnet watch run
```

Backend will run on `http://localhost:5000` (or check console output for port)

#### Terminal 2 - Frontend
```bash
cd frontend
npm start
```

Frontend will run on `http://localhost:3000`

### Option 2: Using Multiple Terminals in PowerShell

Open 3 PowerShell terminals:

**Terminal 1 - Docker:**
```powershell
docker-compose up
```

**Terminal 2 - Backend:**
```powershell
cd backend
dotnet watch run
```

**Terminal 3 - Frontend:**
```powershell
cd frontend
npm start
```

## Accessing the Application

| Service | URL | Credentials |
|---------|-----|-------------|
| **Frontend** | http://localhost:3000 | - |
| **Backend API** | http://localhost:5000 | - |
| **Mongo Express** | http://localhost:8081 | EXAMPEL / EXAMPEL |
| **MinIO Console** | http://localhost:9001 | EXAMPEL / EXAMPEL |
| **MongoDB** | localhost:27017 | EXAMPEL / EXAMPEL |
| **MinIO API** | localhost:9000 | EXAMPEL / EXAMPEL |

## API Endpoints

### Person Endpoints

- **GET** `/api/person` - Get all persons (with pagination and sorting)
  - Query params: `skip`, `pageSize`, `sortBy`, `sortOrder`
- **GET** `/api/person/{id}` - Get person by ID
- **POST** `/api/person` - Create new person
- **PUT** `/api/person/{id}` - Update person
- **DELETE** `/api/person/{id}` - Delete person

### Example Request Body (Create/Update Person)

```json
{
  "username": "johndoe",
  "cpr": "0101901234",
  "profilePicture": "data:image/png;base64,iVBORw0KG..." // Base64 or URL
}
```

## How Profile Pictures Work

1. **Upload**: Send base64-encoded image in `profilePicture` field
2. **Backend**: Automatically uploads to MinIO and stores URL in MongoDB
3. **Retrieve**: Get person data with MinIO presigned URL (valid for 7 days)
4. **Update**: Old image is deleted from MinIO, new one is uploaded
5. **Delete**: Image is removed from MinIO when person is deleted

## File Storage (MinIO)

MinIO stores all uploaded files (images, PDFs, documents, etc.):
- Bucket name: `profile-pictures`
- Files are accessed via presigned URLs
- URLs expire after 7 days (configurable)
- Direct access via MinIO Console at http://localhost:9001

## Database Management

### View Data in Mongo Express
1. Go to http://localhost:8081
2. Login with `EXAMPEL` / `EXAMPEL`
3. Select `persons` database
4. View `persons` collection

### Reset Database and Reseed Data

```bash
docker-compose down -v
docker-compose up -d
```

This removes all data and runs the seeder again.

## Development Commands

### Backend

```bash
# Run backend
dotnet run

# Run with hot reload
dotnet watch run

# Restore packages
dotnet restore

# Build
dotnet build
```

### Frontend

```bash
# Start development server
npm start

# Install dependencies
npm install

# Build for production
npm run build
```

### Docker

```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f

# Restart specific service
docker-compose restart mongo

# Remove all containers and volumes
docker-compose down -v
```

## Troubleshooting

### Backend won't start
- Make sure MongoDB is running: `docker-compose ps`
- Check if MinIO is running: `docker-compose ps`
- Verify port 5000 is not in use

### Frontend won't start
- Run `npm install` in the frontend folder
- Check if port 3000 is available

### Can't connect to MongoDB
- Ensure Docker containers are running: `docker-compose ps`
- Check MongoDB logs: `docker-compose logs mongo`

### MinIO connection issues
- Verify MinIO is running: `docker-compose ps`
- Check MinIO logs: `docker-compose logs minio`
- Access MinIO Console at http://localhost:9001

### Images not uploading
- Check MinIO is running and accessible
- Verify MinIO credentials in `appsettings.json`
- Check backend logs for error messages
- Ensure bucket `profile-pictures` exists (created automatically)

## Additional Notes

- CPR format: `DDMMYYXXXX` (Danish personal number)
- Star signs are automatically calculated from birth date
- Age is calculated from CPR number
- Friend relationships are stored as arrays of person IDs
- All timestamps in MongoDB are in UTC

## License

This project is for educational/practice purposes.
