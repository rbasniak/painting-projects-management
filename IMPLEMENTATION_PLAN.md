# Implementation Plan: Match Paints Feature

## Overview
Add a "Match Paints" feature to the Projects module that allows users to find the 10 closest color matches from their inventory for each zone color in a project's color groups.

## Architecture Overview

### Components Involved
1. **Frontend (Blazor)**
   - ProjectsList.razor - Add menu item
   - MatchPaintsDialog.razor - New dialog component
   - IProjectsService - Add new service methods

2. **Backend (Projects Module)**
   - New endpoint: `POST /api/projects/{projectId}/color-sections/match-paints`
   - New endpoint: `PUT /api/projects/color-sections/picked-color`
   - ColorSection model - Update SuggestedColorIds structure
   - Database migration - Update ColorSection table

3. **Integration**
   - Use IDispatcher to send command from Projects module to Inventory module
   - Create integration interface in new `PaintingProjectsManagement.Features.Inventory.Integration` project
   - Implement command handler in Inventory module
   - Use typed dispatcher (doesn't return object)

---

## Detailed Implementation Steps

### Phase 1: Data Model Changes

#### 1.1 Update ColorSection Model
**File:** `./back/PaintingProjectsManagement.Features.Projects/Models/ColorSection.cs`

**Changes:**
- Replace `SuggestedColorIds` (Guid[]) with `SuggestedColorsJson` (JSONB column)
- Use PostgreSQL JSONB column type for native JSON support
- Add method to update suggested colors
- Add method to set picked color

**Data Structure:**
```csharp
[Column(TypeName = "jsonb")]  // PostgreSQL JSONB column
public string SuggestedColorsJson { get; private set; } = "[]";
```

**Suggested Color Object Structure:**
```json
{
  "paintColorId": "guid",
  "name": "string",
  "hexColor": "#RRGGBB",
  "brandName": "string",
  "lineName": "string",
  "distance": 0.0
}
```

#### 1.2 Update ColorSectionDetails DTO
**File:** `./back/PaintingProjectsManagement.Features.Projects/UseCases/Web/DataTransfer/ColorSectionDetails.cs`

**Changes:**
- Replace `SuggestedColorIds` with `SuggestedColors` array
- Add `PickedColorId` property (nullable Guid)

**New Structure:**
```csharp
public class ColorMatchDetails
{
    public Guid PaintColorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string LineName { get; set; } = string.Empty;
    public double Distance { get; set; }
}

public class ColorSectionDetails
{
    // ... existing properties
    public ColorMatchDetails[] SuggestedColors { get; set; } = Array.Empty<ColorMatchDetails>();
    public Guid? PickedColorId { get; set; }
}
```

#### 1.3 Update Database Configuration
**File:** `./back/PaintingProjectsManagement.Features.Projects/Database/ColorSectionConfig.cs`

**Changes:**
- Update property mapping for `SuggestedColorsJson`
- Configure as JSONB column type for PostgreSQL
- Use EF Core 7+ JSON column support
- Ensure proper JSON serialization/deserialization

**Configuration:**
```csharp
builder.Property(x => x.SuggestedColorsJson)
    .HasColumnType("jsonb")
    .HasDefaultValue("[]");
```

#### 1.4 Create Database Migration
- Generate migration to:
  - Rename `SuggestedColorIds` column to `SuggestedColorsJson`
  - Change column type to `jsonb` (PostgreSQL)
  - Migrate existing data (if any) from Guid array to JSON format
  - Add migration script

---

### Phase 2: Inventory Integration Project

#### 2.1 Create Inventory Integration Project
**File:** `./back/PaintingProjectsManagement.Features.Inventory.Integration/PaintingProjectsManagement.Features.Inventory.Integration.csproj` (NEW)

**Project Structure:**
- Create new .csproj file similar to `PaintingProjectsManagement.Features.Materials.Integration.csproj`
- Reference `rbkApiModules.Commons.Core` package
- This project will contain the integration interface
- Create `Usings.cs` file if needed (similar to Materials.Abstractions pattern)

#### 2.2 Create Integration Interface
**File:** `./back/PaintingProjectsManagement.Features.Inventory.Integration/Commands/IFindColorMatchesCommand.cs` (NEW)

**Interface:**
```csharp
namespace PaintingProjectsManagement.Features.Inventory.Integration;

public interface IFindColorMatchesCommand
{
    string ReferenceColor { get; } // Hex color
    int MaxResults { get; }
    string Tenant { get; } // User tenant for filtering paints
}
```

**Response Type:**
```csharp
namespace PaintingProjectsManagement.Features.Inventory.Integration;

public class ColorMatchResult
{
    public Guid PaintColorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string LineName { get; set; } = string.Empty;
    public double Distance { get; set; }
}
```

### Phase 3: Inventory Module Implementation

#### 3.1 Create Find Color Matches Command
**File:** `./back/PaintingProjectsManagement.Features.Inventory/UseCases/MyPaints/Commands/FindColorMatches.cs` (NEW)

**Note:** This is a Command (not Query) because it's called via IDispatcher from another module

**Request Class:**
```csharp
public class Request : AuthenticatedRequest, ICommand, IFindColorMatchesCommand
{
    public string ReferenceColor { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 10;
    
    // Explicit interface implementation
    string IFindColorMatchesCommand.ReferenceColor => ReferenceColor;
    int IFindColorMatchesCommand.MaxResults => MaxResults;
    string IFindColorMatchesCommand.Tenant => Identity.Tenant ?? string.Empty;
}
```

**Handler:**
```csharp
public class Handler(DbContext _context) : ICommandHandler<Request, IReadOnlyCollection<ColorMatchResult>>
{
    public async Task<CommandResponse<IReadOnlyCollection<ColorMatchResult>>> HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var username = request.Identity.Tenant ?? string.Empty;

        var userPaints = await _context.Set<UserPaint>()
            .Where(up => up.Username == username)
            .Include(up => up.PaintColor)
            .ThenInclude(x => x.Line)
            .ThenInclude(x => x!.Brand)
            .ToListAsync(cancellationToken);

        var matches = userPaints
            .Select(up => new
            {
                Paint = up,
                Distance = ColorHelper.CalculateColorDistance(request.ReferenceColor, up.PaintColor.HexColor)
            })
            .OrderBy(x => x.Distance)
            .Take(request.MaxResults)
            .Select(x => new ColorMatchResult
            {
                PaintColorId = x.Paint.PaintColorId,
                Name = x.Paint.PaintColor.Name,
                HexColor = x.Paint.PaintColor.HexColor,
                BrandName = x.Paint.PaintColor.Line.Brand.Name,
                LineName = x.Paint.PaintColor.Line.Name,
                Distance = x.Distance
            })
            .ToList();

        return CommandResponse<IReadOnlyCollection<ColorMatchResult>>.Success(matches);
    }
}
```

**Note:** Use typed `ICommandHandler<Request, Response>` that returns `CommandResponse<Response>` instead of `CommandResponse` (object)

#### 3.2 Register Command in Inventory Builder
**File:** `./back/PaintingProjectsManagement.Features.Inventory/UseCases/MyPaints/Builder.cs`

**Changes:**
- Register the FindColorMatches command handler so it can be discovered by IDispatcher
- Add to the module's command/query registration

**Example:**
```csharp
// Ensure the command handler is registered in DI container
// This is typically done automatically by the framework if following conventions
// But verify the handler is discoverable by IDispatcher
```

---

### Phase 4: Projects Module Backend

#### 4.1 Add Inventory Integration Reference
**File:** `./back/PaintingProjectsManagement.Features.Projects/PaintingProjectsManagement.Features.Projects.csproj`

**Changes:**
- Add project reference to `PaintingProjectsManagement.Features.Inventory.Integration`

#### 4.2 Create Match Paints Command
**File:** `./back/PaintingProjectsManagement.Features.Projects/UseCases/Web/Commands/MatchPaints.cs` (NEW)

**Endpoint:** `POST /api/projects/{projectId}/color-sections/match-paints`

**Request:**
```csharp
public class MatchPaintsRequest : AuthenticatedRequest, ICommand
{
    public Guid ProjectId { get; set; }
}
```

**Handler:**
```csharp
public class Handler(DbContext _context, IDispatcher _dispatcher) : ICommandHandler<Request>
{
    public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
    {
        // Load project with all color groups and sections
        var project = await _context.Set<Project>()
            .Include(p => p.Groups)
                .ThenInclude(g => g.Sections)
            .FirstAsync(p => p.Id == request.ProjectId && 
                           p.TenantId == request.Identity.Tenant, 
                     cancellationToken);

        // For each color section, find matches
        foreach (var group in project.Groups)
        {
            foreach (var section in group.Sections)
            {
                // Create command request that implements IFindColorMatchesCommand
                // Note: The request must also be an AuthenticatedRequest to work with the Inventory handler
                var findMatchesCommand = new FindColorMatchesCommandRequest
                {
                    ReferenceColor = section.ReferenceColor,
                    MaxResults = 10
                };
                // Copy identity from parent request (framework may handle this automatically)
                // If AuthenticatedRequest has a constructor or method to set Identity, use that
                // Otherwise, ensure the dispatcher passes Identity context

                // Use typed dispatcher - doesn't return object, returns typed response
                var matchesResponse = await _dispatcher.SendAsync<FindColorMatchesCommandRequest, IReadOnlyCollection<ColorMatchResult>>(
                    findMatchesCommand, 
                    cancellationToken);

                if (matchesResponse.IsSuccess && matchesResponse.Data != null)
                {
                    // Serialize matches to JSON
                    var json = JsonSerializer.Serialize(matchesResponse.Data);
                    section.UpdateSuggestedColors(json);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return CommandResponse.Success();
    }
}

// Command request class that implements the integration interface
public class FindColorMatchesCommandRequest : AuthenticatedRequest, ICommand, IFindColorMatchesCommand
{
    public string ReferenceColor { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 10;
    
    // Explicit interface implementation
    string IFindColorMatchesCommand.ReferenceColor => ReferenceColor;
    int IFindColorMatchesCommand.MaxResults => MaxResults;
    string IFindColorMatchesCommand.Tenant => Identity.Tenant ?? string.Empty;
}
```

**Key Points:**
- Inject `IDispatcher` (not `IHttpClientFactory`)
- Use typed `SendAsync<TRequest, TResponse>` method
- Create request class that implements `IFindColorMatchesCommand` interface
- Import `ColorMatchResult` from Inventory.Integration project

#### 4.3 Create Update Picked Color Command
**File:** `./back/PaintingProjectsManagement.Features.Projects/UseCases/Web/Commands/UpdatePickedColor.cs` (NEW)

**Endpoint:** `PUT /api/projects/color-sections/picked-color`

**Request:**
```csharp
public class UpdatePickedColorRequest : AuthenticatedRequest, ICommand
{
    public Guid SectionId { get; set; }
    public Guid PaintColorId { get; set; }
}
```

**Handler Logic:**
1. Load ColorSection by ID
2. Validate section belongs to user's tenant
3. Validate PaintColorId exists in SuggestedColorsJson
4. Update ColorSection.UsedColorId
5. Save to database
6. Return updated ColorSectionDetails

#### 4.4 Update ColorSection Model Methods
**File:** `./back/PaintingProjectsManagement.Features.Projects/Models/ColorSection.cs`

**Add Methods:**
```csharp
public void UpdateSuggestedColors(string suggestedColorsJson)
{
    SuggestedColorsJson = suggestedColorsJson ?? "[]";
}

public void SetPickedColor(Guid paintColorId)
{
    UsedColorId = paintColorId;
}
```

#### 4.5 Update GetProjectDetails Query
**File:** `./back/PaintingProjectsManagement.Features.Projects/UseCases/Web/Queries/GetProjectDetails.cs`

**Changes:**
- Update `ColorSectionDetails.FromModel` to deserialize JSON and populate SuggestedColors array
- Include PickedColorId in response

---

### Phase 5: Frontend Implementation

#### 5.1 Add Menu Item to ProjectsList
**File:** `./back/PaintingProjectsManagement.Features.Projects.UI/UI/Pages/ProjectsList.razor`

**Changes:**
- Add new `MudMenuItem` in the dropdown menu (after "Color Zones")
- Text: "Match Paints"
- Icon: `Icons.Material.Filled.Colorize` or similar
- OnClick handler: `OpenMatchPaints(context)`

**New Method:**
```csharp
private async Task OpenMatchPaints(ProjectDetails project)
{
    var projectDetails = await ProjectsService.GetDetailsAsync(project.Id, default);
    var parameters = new DialogParameters { ["Model"] = projectDetails };
    var options = new DialogOptions { MaxWidth = MaxWidth.ExtraLarge, FullWidth = true };
    var dialog = DialogService.Show<MatchPaintsDialog>(title: null, parameters, options);
}
```

#### 5.2 Create MatchPaintsDialog Component
**File:** `./back/PaintingProjectsManagement.Features.Projects.UI/UI/Dialogs/MatchPaintsDialog.razor` (NEW)

**Component Structure:**
- Similar layout to ColorGroupsDialog
- Show all color groups in expansion panels
- For each color section:
  - Display reference color (larger square, ~60x60px)
  - Display suggested colors (smaller squares, ~40x40px) in a row
  - Add tooltip on hover showing: Brand Name, Line Name, Color Name
  - Make suggestions clickable
- "Match Colors" button at top to trigger matching
- Loading state during matching process

**Key Features:**
- Use MudBlazor components (MudPaper, MudStack, MudButton, MudTooltip)
- Show reference color prominently
- Display suggestions in a grid/row layout
- Highlight picked color (if one is selected)
- Show loading spinner during API calls

#### 5.3 Update IProjectsService Interface
**File:** `./back/PaintingProjectsManagement.Features.Projects.UI/Services/IProjectsService.cs`

**Add Methods:**
```csharp
Task MatchPaintsAsync(Guid projectId, CancellationToken cancellationToken);
Task UpdatePickedColorAsync(UpdatePickedColorRequest request, CancellationToken cancellationToken);
```

#### 5.4 Implement Service Methods
**File:** `./back/PaintingProjectsManagement.Features.Projects.UI/Services/IProjectsService.cs` (ProjectsService class)

**Implementation:**
```csharp
public async Task MatchPaintsAsync(Guid projectId, CancellationToken cancellationToken)
{
    var response = await _httpClient.PostAsync($"api/projects/{projectId}/color-sections/match-paints", null, cancellationToken);
    response.EnsureSuccessStatusCode();
}

public async Task UpdatePickedColorAsync(UpdatePickedColorRequest request, CancellationToken cancellationToken)
{
    var response = await _httpClient.PutAsJsonAsync("api/projects/color-sections/picked-color", request, cancellationToken);
    response.EnsureSuccessStatusCode();
}
```

#### 5.5 Create Request Models
**File:** `./back/PaintingProjectsManagement.Features.Projects.UI/Requests/ColorMatchingRequests.cs` (NEW or add to existing)

```csharp
public class UpdatePickedColorRequest
{
    public Guid SectionId { get; set; }
    public Guid PaintColorId { get; set; }
}
```

---

## Technical Considerations

### 1. Data Storage Strategy

**Using: PostgreSQL JSONB Column**
- **Pros:**
  - Native JSON support in PostgreSQL
  - Can query/index JSON fields if needed later
  - Type safety at database level
  - Better performance for JSON operations
  - Supports JSON operators and functions
- **Cons:**
  - Requires EF Core 7+ for JSON column support
  - More complex migration
  - PostgreSQL-specific (not portable to other databases)

**Implementation:**
- Use `[Column(TypeName = "jsonb")]` attribute
- EF Core 7+ handles JSONB serialization/deserialization automatically
- Store as JSON string in C# model, EF Core converts to/from JSONB

### 2. Service Communication via IDispatcher

**Using: IDispatcher for Inter-Module Communication**
- **Approach:** Use `IDispatcher.SendAsync<TRequest, TResponse>` for typed command dispatch
- **Benefits:**
  - Type-safe communication between modules
  - No HTTP overhead (same process)
  - Clear contract via integration interface
  - Easy to test and mock
  - Maintains separation of concerns

**Architecture:**
1. **Integration Project:** `PaintingProjectsManagement.Features.Inventory.Integration`
   - Contains `IFindColorMatchesCommand` interface
   - Contains shared response types (`ColorMatchResult`)
   - Referenced by both Projects and Inventory modules

2. **Inventory Module:**
   - Implements command handler with `Request` class implementing `IFindColorMatchesCommand`
   - Handler returns typed `CommandResponse<IReadOnlyCollection<ColorMatchResult>>`

3. **Projects Module:**
   - Creates request class implementing `IFindColorMatchesCommand`
   - Uses `IDispatcher.SendAsync<FindColorMatchesCommandRequest, IReadOnlyCollection<ColorMatchResult>>`
   - Receives typed response (not object)

**Implementation:**
- Inject `IDispatcher` in Projects command handler
- Use typed `SendAsync<TRequest, TResponse>` method
- No HTTP client configuration needed
- Works within same process (current architecture)
- Ensure `AuthenticatedRequest` identity is properly propagated to nested commands
  - May need to copy Identity from parent request to child request
  - Verify framework handles Identity context automatically or implement manual copy

### 3. Error Handling
- Handle Inventory service unavailability
- Handle invalid color formats
- Handle missing user paints
- Show user-friendly error messages in UI

### 4. Performance Considerations
- Batch color matching requests (if Inventory service supports it)
- Consider caching matches (optional future enhancement)
- Show progress indicator for large projects with many color sections

### 5. UI/UX Considerations
- Show loading state during matching
- Disable "Match Colors" button while processing
- Show success/error notifications
- Refresh dialog after matching to show results
- Visual feedback when clicking a suggestion

---

## File Structure Summary

### New Files
1. `./back/PaintingProjectsManagement.Features.Inventory.Integration/PaintingProjectsManagement.Features.Inventory.Integration.csproj`
2. `./back/PaintingProjectsManagement.Features.Inventory.Integration/Commands/IFindColorMatchesCommand.cs`
3. `./back/PaintingProjectsManagement.Features.Inventory.Integration/Commands/ColorMatchResult.cs`
4. `./back/PaintingProjectsManagement.Features.Inventory/UseCases/MyPaints/Commands/FindColorMatches.cs`
5. `./back/PaintingProjectsManagement.Features.Projects/UseCases/Web/Commands/MatchPaints.cs`
6. `./back/PaintingProjectsManagement.Features.Projects/UseCases/Web/Commands/UpdatePickedColor.cs`
7. `./back/PaintingProjectsManagement.Features.Projects.UI/UI/Dialogs/MatchPaintsDialog.razor`
8. `./back/PaintingProjectsManagement.Features.Projects.UI/Requests/ColorMatchingRequests.cs` (if new file)

### Modified Files
1. `./back/PaintingProjectsManagement.Features.Projects/PaintingProjectsManagement.Features.Projects.csproj` (add Inventory.Integration reference)
2. `./back/PaintingProjectsManagement.Features.Projects/Models/ColorSection.cs`
3. `./back/PaintingProjectsManagement.Features.Projects/UseCases/Web/DataTransfer/ColorSectionDetails.cs`
4. `./back/PaintingProjectsManagement.Features.Projects/Database/ColorSectionConfig.cs`
5. `./back/PaintingProjectsManagement.Features.Projects/UseCases/Web/Queries/GetProjectDetails.cs`
6. `./back/PaintingProjectsManagement.Features.Projects.UI/UI/Pages/ProjectsList.razor`
7. `./back/PaintingProjectsManagement.Features.Projects.UI/Services/IProjectsService.cs`
8. Database migration file (auto-generated)

---

## Testing Considerations

### Unit Tests
- ColorSection model methods
- Color matching logic (distance calculation)
- JSON serialization/deserialization

### Integration Tests
- MatchPaints command handler
- UpdatePickedColor command handler
- IDispatcher integration between Projects and Inventory modules
- FindColorMatches command handler in Inventory
- End-to-end flow: Match → Display → Pick

### UI Tests (Manual)
- Dialog opens correctly
- Color groups display properly
- Matching process works
- Suggestions display with tooltips
- Clicking suggestion updates picked color

---

## Migration Strategy

### Database Migration Steps
1. Add new column `SuggestedColorsJson` as `jsonb` type (nullable initially)
2. Migrate existing data (if any) from `SuggestedColorIds` (Guid array) to JSON format
3. Make `SuggestedColorsJson` non-nullable with default value `[]`
4. Drop old `SuggestedColorIds` column (in separate migration if needed)

**PostgreSQL JSONB Migration Example:**
```sql
ALTER TABLE project.project_color_sections 
ADD COLUMN SuggestedColorsJson jsonb DEFAULT '[]'::jsonb;

-- Migrate existing data (if any)
UPDATE project.project_color_sections 
SET SuggestedColorsJson = '[]'::jsonb 
WHERE SuggestedColorsJson IS NULL;

ALTER TABLE project.project_color_sections 
ALTER COLUMN SuggestedColorsJson SET NOT NULL;
```

### Backward Compatibility
- Handle cases where `SuggestedColorIds` might still exist
- Provide migration script for existing data
- Consider feature flag for gradual rollout

---

## Open Questions / Decisions Needed

1. **Error Handling:** How to handle Inventory command failures?
2. **Caching:** Should matches be cached, or always recalculated?
3. **UI Layout:** Exact layout preferences for displaying matches
4. **Tooltip Content:** Exact format for tooltip (e.g., "Brand: X, Line: Y, Color: Z")
5. **Visual Feedback:** How to indicate which color is currently picked?
6. **Dispatcher Registration:** Ensure IDispatcher is properly registered and can route commands between modules

---

## Estimated Effort

- **Phase 1 (Data Model):** 4-6 hours
- **Phase 2 (Inventory Integration Project):** 2-3 hours
- **Phase 3 (Inventory Module Implementation):** 3-4 hours
- **Phase 4 (Projects Backend):** 6-8 hours
- **Phase 5 (Frontend):** 8-10 hours
- **Testing & Bug Fixes:** 4-6 hours

**Total:** ~27-37 hours

---

## Dependencies

- MudBlazor components (already in use)
- System.Text.Json for JSON handling
- EF Core 7+ for JSONB column support
- IDispatcher for inter-module communication
- PostgreSQL with JSONB column type support
- rbkApiModules.Commons.Core (for IDispatcher and command/query infrastructure)
