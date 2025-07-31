# Painting Projects Management - Blazor Application

A modern Blazor WebAssembly application for managing painting projects, built with MudBlazor components using a modular feature-based architecture.

## 🏗️ **Modular Architecture**

The application follows a **feature-based modular architecture** where each feature is self-contained with its own:

- **Models** - Data transfer objects
- **Services** - API communication layer
- **Pages** - UI components
- **Components** - Reusable UI elements (future)

### **Project Structure**

```
blazor/
├── Features/                    # Feature modules
│   ├── Materials/              # Materials management
│   │   ├── Models/            # Material data models
│   │   ├── Services/          # API services
│   │   └── Pages/             # UI pages
│   ├── Models/                # Models management
│   │   ├── Models/            # Model data models
│   │   ├── Services/          # API services
│   │   └── Pages/             # UI pages
│   └── Projects/              # Projects management
│       ├── Models/            # Project data models
│       ├── Services/          # API services
│       └── Pages/             # UI pages
├── Shared/                    # Shared components
│   ├── Configuration/         # Shared configuration
│   ├── MainLayout.razor      # Main layout
│   └── NavMenu.razor         # Navigation menu
├── Pages/                     # Core pages
│   └── Index.razor           # Home page
└── wwwroot/                  # Static files
```

## 🎯 **Features**

### **Materials Module**
- **Models**: `Material`, `MaterialUnit`
- **Services**: `IMaterialsApiService`, `MaterialsApiService`
- **Pages**: `Materials.razor`
- **Endpoints**: `/api/materials`

### **Models Module**
- **Models**: `Model`, `ModelCategory`, `EntityReference`
- **Services**: `IModelsApiService`, `IModelCategoriesApiService`
- **Pages**: `Models.razor`, `ModelCategories.razor`
- **Endpoints**: `/api/models`, `/api/models/categories`

### **Projects Module**
- **Models**: `Project`
- **Services**: `IProjectsApiService`, `ProjectsApiService`
- **Pages**: `Projects.razor`
- **Endpoints**: `/api/projects`

## 🔧 **Best Practices Implemented**

### **1. Feature Isolation**
- ✅ Each feature is completely self-contained
- ✅ No cross-feature dependencies
- ✅ Clear separation of concerns

### **2. Service Layer Pattern**
- ✅ Interface-based services (`I*ApiService`)
- ✅ Dependency injection ready
- ✅ Easy to mock for testing

### **3. Shared Configuration**
- ✅ Centralized API configuration
- ✅ Consistent endpoint management
- ✅ Easy to change base URLs

### **4. Scalable Architecture**
- ✅ Easy to add new features
- ✅ Consistent patterns across modules
- ✅ Maintainable codebase

## 🚀 **Adding New Features**

To add a new feature (e.g., "Paints"):

1. **Create Feature Structure**:
   ```
   Features/Paints/
   ├── Models/
   │   └── Paint.cs
   ├── Services/
   │   ├── IPaintsApiService.cs
   │   └── PaintsApiService.cs
   └── Pages/
       └── Paints.razor
   ```

2. **Register Services** in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<IPaintsApiService, PaintsApiService>();
   ```

3. **Add Navigation** in `Shared/NavMenu.razor`

## 🛠️ **Development**

### **Prerequisites**
- .NET 8.0 SDK or later
- Backend API running at `https://localhost:7236`

### **Running the Application**
```bash
cd blazor
dotnet restore
dotnet run
```

### **Building**
```bash
dotnet build
```

## 📋 **API Integration**

The application connects to your backend API with the following endpoints:

- **Materials**: `GET/POST/PUT/DELETE /api/materials`
- **Models**: `GET/POST/PUT/DELETE /api/models`
- **Model Categories**: `GET/POST/PUT/DELETE /api/models/categories`
- **Projects**: `GET/POST/PUT/DELETE /api/projects`

## 🎨 **UI Features**

- **Modern Design**: MudBlazor Material Design components
- **Responsive Layout**: Works on desktop and mobile
- **Search & Filter**: Real-time filtering on all tables
- **Loading States**: Professional loading indicators
- **Error Handling**: Graceful error management

## 🔮 **Future Enhancements**

- **Authentication**: User login and authorization
- **Dialogs**: CRUD operations with modal dialogs
- **Validation**: Form validation and error messages
- **Notifications**: Snackbar notifications for user feedback
- **File Upload**: Image upload for models and projects
- **Advanced Filtering**: Multi-column filtering and sorting
- **Export**: Data export functionality
- **Dark Mode**: Theme switching capability 