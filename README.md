# Painting Projects Management

A comprehensive system for managing painting projects, materials, and resources.

## 📊 Code Coverage

[![Code Coverage](https://img.shields.io/badge/coverage-0%25-red.svg)](https://github.com/actions)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/actions)
[![.NET Version](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

## 🏗️ Project Structure

- **`back/`** - .NET 9.0 Backend API
- **`front/`** - Frontend application

## 🧪 Testing

The project includes comprehensive unit and integration tests with code coverage reporting.

### Running Tests

```bash
# Navigate to backend
cd back

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --coverage --coverage-output-format cobertura --coverage-settings coverage.config
```

### Coverage Reports

Coverage reports are automatically generated during CI/CD builds and are available as artifacts in GitHub Actions. The reports include:

- HTML reports for detailed coverage analysis
- Cobertura XML format for CI integration
- Coverage badges for repository display

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- Node.js (for frontend)

### Backend Setup

```bash
cd back
dotnet restore
dotnet build
dotnet run --project PaintingProjectsManagement.Api
```

### Frontend Setup

```bash
cd front
npm install
npm start
```

## 📝 License

This project is licensed under the MIT License.
