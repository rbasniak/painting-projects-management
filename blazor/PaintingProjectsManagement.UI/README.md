# Painting Projects Management - Blazor UI

This is a modular Blazor application built with MudBlazor for managing painting projects.

## Architecture

The application follows a modular architecture where each module is independent and self-contained. Modules do not communicate with each other directly, ensuring loose coupling and maintainability.

### Module Structure

Each module follows this structure:

```
Modules/
├── [ModuleName]/
│   ├── Models/           # Data transfer objects and enums
│   ├── Services/         # API communication and business logic
│   ├── Pages/           # Main page components
│   ├── Components/      # Reusable components for the module
│   └── [ModuleName]Module.cs  # Module metadata
```

### Current Modules

#### Materials Module
- **Route**: `/materials`
- **Features**: 
  - List materials with pagination and sorting
  - Create new materials
  - Edit existing materials
  - Delete materials
  - Search and filter functionality

### Adding a New Module

1. **Create the module folder structure**:
   ```
   Modules/
   └── [NewModule]/
       ├── Models/
       ├── Services/
       ├── Pages/
       ├── Components/
       └── [NewModule]Module.cs
   ```

2. **Create the module class**:
   ```csharp
   public class NewModule : IModule
   {
       public string Name => "New Module";
       public string Route => "new-module";
       public string Icon => Icons.Material.Filled.YourIcon;
       public int Order => 2; // Order in navigation
   }
   ```

3. **Register the module**:
   - Add the module service to `Program.cs`
   - Register the module in `ModuleInitializer.razor`

4. **Create the main page**:
   ```razor
   @page "/new-module"
   @inject INewModuleService NewModuleService
   
   <PageTitle>New Module</PageTitle>
   
   <!-- Your page content -->
   ```

### API Integration

The application communicates with the backend API through service classes. Each module has its own service that implements an interface:

```csharp
public interface INewModuleService
{
    Task<IReadOnlyCollection<NewModuleDetails>> GetItemsAsync();
    Task<NewModuleDetails> CreateItemAsync(CreateRequest request);
    // ... other methods
}
```

### MudBlazor Components

The application uses MudBlazor for the UI components. Key components used:

- `MudTable` - For data tables with pagination, sorting, and filtering
- `MudDialog` - For modal dialogs
- `MudForm` - For form validation
- `MudSnackbar` - For notifications
- `MudNavMenu` - For navigation

### Development

To run the application:

1. Ensure the backend API is running
2. Update the API base URL in `appsettings.json` if needed
3. Run the Blazor application

### Module Independence

Each module is designed to be independent:
- Modules have their own models, services, and components
- No direct dependencies between modules
- Shared functionality should be placed in a common/shared module
- Each module can be developed and tested independently 