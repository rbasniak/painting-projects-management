# Painting Projects Management - Blazor Application

A modern Blazor WebAssembly application for managing painting projects, built with MudBlazor components.

## Features

- **Materials Management**: Track and manage painting materials with categories and quantities
- **Models Management**: Organize models and model categories
- **Projects Management**: Track painting projects with progress and status
- **Modern UI**: Built with MudBlazor for a beautiful and responsive interface
- **Search and Filter**: Advanced search and filtering capabilities
- **Responsive Design**: Works on desktop and mobile devices

## Prerequisites

- .NET 8.0 SDK or later
- A modern web browser

## Getting Started

1. Navigate to the blazor directory:
   ```bash
   cd blazor
   ```

2. Restore the NuGet packages:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. Open your browser and navigate to `https://localhost:5001` (or the URL shown in the console)

## Project Structure

```
blazor/
├── Pages/                 # Blazor pages
│   ├── Index.razor       # Home page
│   ├── Materials.razor   # Materials management
│   ├── Models.razor      # Models management
│   ├── ModelCategories.razor # Model categories
│   └── Projects.razor    # Projects management
├── Shared/               # Shared components
│   ├── MainLayout.razor  # Main layout
│   └── NavMenu.razor     # Navigation menu
├── Models/               # Data models
│   ├── Material.cs
│   ├── Model.cs
│   ├── ModelCategory.cs
│   └── Project.cs
├── wwwroot/              # Static files
│   └── index.html
├── App.razor             # Main app component
├── Program.cs            # Application entry point
└── PaintingProjectsManagement.Blazor.csproj
```

## Navigation

The application features a side navigation menu with the following sections:

- **Home**: Welcome page with quick access cards
- **Materials**: Manage painting materials (paints, brushes, etc.)
- **Models**: 
  - Models: List and manage individual models
  - Model Categories: Organize models by categories
- **Projects**: Track painting projects with progress and status

## Features

### Materials Module
- View all materials in a sortable table
- Search and filter materials
- Add, edit, and delete materials (TODO: Implement dialogs)

### Models Module
- **Models Page**: List all models with details like scale, brand, and category
- **Model Categories Page**: Manage model categories with description and model count

### Projects Module
- Track projects with status indicators
- Progress bars showing completion percentage
- Color-coded status chips
- Start dates and completion tracking

## Technology Stack

- **Blazor WebAssembly**: Modern web framework
- **MudBlazor**: Material Design component library
- **.NET 8**: Latest .NET framework
- **C#**: Primary programming language

## Development

The application is set up with:
- MudBlazor for UI components
- Responsive design patterns
- Clean architecture with separation of concerns
- Extensible model structure for future enhancements

## Future Enhancements

- CRUD operations with dialogs
- API integration with backend services
- User authentication and authorization
- Image upload and management
- Advanced filtering and sorting
- Export functionality
- Dark mode toggle implementation 