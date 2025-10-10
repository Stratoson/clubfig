![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)
![Vue.js Version](https://img.shields.io/badge/Vue.js-3.x-green)
![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/bradenjt/clubfig?label=release&sort=semver)

**Clubfig** is a modern, scalable member management tool designed to streamline club and organization operations. 
It features a robust **.NET API** backend paired with a dynamic **Vue.js** frontend, offering a seamless experience for managing memberships, events, and communications.

---
## Project Status

Clubfig is actively maintained and open to contributions! We welcome pull requests, issues, and feedback to enhance the platform. Join our community to help shape the future of club management.

---

## Table of Contents

 1. [Overview](#overviwe)
 2. [Features](#features)
 3. [Architecture](#architecture)
 4. [Getting Started](#getting-started)
 5. [Installation](#installation)
 6. [Configuration](#configuration)
 7. [Contributing](#contributing)
 8. [API Documentation](#api-documentation)
 9. [Frontend Guide](#frontend-guide)
10. [License](#license)

---

## Overview

Clubfig is built to simplify the complexities of managing memberships for clubs, associations, or community groups. Whether you're handling registrations, tracking events, or managing communications, Clubfig provides an intuitive interface and powerful backend to get the job done efficiently.

- **Backend**: A .NET Core API for secure, scalable data management.
- **Frontend**: A Vue.js single-page application for a responsive and user-friendly experience.
- **Focus**: Developer-friendly setup, modular design, and extensibility.

---

## Features

- **Member Management**: Create, update, and manage member profiles with ease.
- **Event Scheduling**: Organize events, track attendance, and send reminders.
- **Role-Based Access**: Fine-grained permissions for admins, members, and guests.
- **RESTful API**: Well-documented endpoints for seamless integration.
- **Responsive UI**: Vue.js-powered frontend that works across devices.
- **Scalable Architecture**: Built with modern cloud-native principles in mind.

---

## Architecture

Clubfig is structured as a **client-server application** with a clear separation of concerns:

- **Backend (.NET Core)**:

  - Built with ASP.NET Core for high performance and scalability.
  - Uses Entity Framework Core for database operations.
  - Supports JWT-based authentication and role-based authorization.
  - Deployable to cloud platforms like Azure, AWS, or on-premises servers.

- **Frontend (Vue.js)**:

  - Powered by Vue 3 with Composition API for modular components.
  - Uses Vue Router for navigation and Vuex/Pinia for state management.
  - Styled with Tailwind CSS for a modern, customizable look.
  - Communicates with the backend via RESTful API calls.

- **Database**:

  - Supports SQL databases (e.g., SQL Server, PostgreSQL) via Entity Framework Core.
  - Configurable for other database providers as needed.

---

## Getting Started

To get Clubfig up and running locally, follow these steps:

### Prerequisites

- **.NET SDK**: Version 8.0 or higher
- **Node.js**: Version 16.x or higher
- **Database**: SQL Server, PostgreSQL, or any EF Core-compatible database
- **Git**: For cloning the repository
- **IDE**: Visual Studio, VS Code, or Rider recommended

---

## Installation

### Clone the Repository

```bash
git clone https://github.com/your-username/clubfig.git
cd clubfig
```

### Backend Setup

1. Navigate to the backend directory:

   ```bash
   cd clubfig-api
   ```

2. Restore dependencies:

   ```bash
   dotnet restore
   ```

3. Configure the database connection in `appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=Clubfig;Trusted_Connection=True;"
     }
   }
   ```

4. Run database migrations:

   ```bash
   dotnet ef database update
   ```

5. Start the API:

   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001`.

### Frontend Setup

1. Navigate to the frontend directory:

   ```bash
   cd clubfig-frontend
   ```

2. Install dependencies:

   ```bash
   npm install
   ```

3. Configure the API endpoint in `src/config.js`:

   ```javascript
   export const API_BASE_URL = 'https://localhost:5001/api';
   ```

4. Start the development server:

   ```bash
   npm run serve
   ```

The frontend will be available at `http://localhost:8080`.

---

## Configuration

### Backend Configuration

- **appsettings.json**: Configure database connections, JWT settings, and logging.
- **Environment Variables**: Override sensitive settings (e.g., API keys) using environment variables.
- **CORS**: Enable CORS for the frontend in `Program.cs` if needed.

### Frontend Configuration

- **Vue Config**: Adjust `vue.config.js` for custom build settings or proxy configurations.
- **Tailwind CSS**: Customize styles in `tailwind.config.js`.
- **API Calls**: Update `src/services/api.js` to handle API endpoints.

---

## Contributing

We love contributions! To get started:

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/awesome-feature`).
3. Commit your changes (`git commit -m 'Add awesome feature'`).
4. Push to the branch (`git push origin feature/awesome-feature`).
5. Open a pull request.

Please read our Contributing Guidelines for more details.

---

## API Documentation

The Clubfig API is fully documented using Swagger. Once the backend is running, access the interactive API docs at:

```
https://localhost:5001/swagger
```

Key endpoints include:

- `GET /api/members`: Retrieve all members.
- `POST /api/members`: Create a new member.
- `PUT /api/members/{id}`: Update a member.
- `DELETE /api/members/{id}`: Delete a member.

---

## Frontend Guide

The Vue.js frontend is organized as follows:

- **Components**: Reusable UI components in `src/components`.
- **Views**: Page-level components in `src/views`.
- **Services**: API interaction logic in `src/services`.
- **Store**: State management using Pinia in `src/store`.

To add a new feature:

1. Create a new component in `src/components`.
2. Add a route in `src/router/index.js`.
3. Connect to the API via `src/services/api.js`.

---

## License
