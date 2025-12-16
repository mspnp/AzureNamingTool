# Dashboard Implementation Plan

## Overview
This document outlines the plan to implement a modern, informative dashboard for the Azure Naming Tool. The dashboard will serve as the landing page, providing users with quick insights into naming tool usage, key statistics, and quick actions.

## Current State
- **Current Home Page**: Likely basic/minimal or traditional layout
- **Design Reference**: `src/Components/Pages/DesignMockups/DesignLinear.razor`
- **Status**: Planning phase

## Dashboard Goals

### Primary Objectives
1. **At-a-Glance Insights**: Show key metrics and statistics
2. **Quick Actions**: Provide fast access to common tasks
3. **Recent Activity**: Display recent naming requests and changes
4. **User Guidance**: Help users understand what they can do

### User Benefits
- Understand tool usage and trends
- Quickly access common operations
- See what's been recently generated
- Monitor configuration changes

---

## Dashboard Layout Structure

### Layout Components (from DesignLinear)
```
┌─────────────────────────────────────────────────────┐
│ Page Header: "Dashboard"                           │
├─────────────────────────────────────────────────────┤
│ Stats Grid (4 cards)                                │
│  ┌───────┐ ┌───────┐ ┌───────┐ ┌───────┐          │
│  │ Stat  │ │ Stat  │ │ Stat  │ │ Stat  │          │
│  │   1   │ │   2   │ │   3   │ │   4   │          │
│  └───────┘ └───────┘ └───────┘ └───────┘          │
├─────────────────────────────────────────────────────┤
│ Quick Actions (3 cards)                             │
│  ┌────────────┐ ┌────────────┐ ┌────────────┐     │
│  │  Generate  │ │ Configure  │ │   View     │     │
│  │    Name    │ │ Components │ │ Reference  │     │
│  └────────────┘ └────────────┘ └────────────┘     │
├─────────────────────────────────────────────────────┤
│ Recent Activity (timeline/list)                     │
│  • Generated name for Storage Account (2m ago)     │
│  • Updated resource type configuration (1h ago)    │
│  • Generated name for Virtual Machine (3h ago)     │
│  • Configuration backup created (5h ago)           │
└─────────────────────────────────────────────────────┘
```

---

## Implementation Phases

### Phase 1: Data Model & Services (Est. 3-4 hours)

#### 1.1: Create Dashboard Model
**File**: `src/Models/DashboardData.cs`

**Model Structure**:
```csharp
public class DashboardData
{
    // Stats
    public int TotalResourceTypes { get; set; }
    public int TotalLocations { get; set; }
    public int TotalNamesGenerated { get; set; }
    public int CustomComponents { get; set; }
    
    // Calculated values
    public int NamesGeneratedToday { get; set; }
    public int NamesGeneratedThisWeek { get; set; }
    public int NamesGeneratedThisMonth { get; set; }
    
    // Recent activity
    public List<ActivityItem> RecentActivity { get; set; }
}

public class ActivityItem
{
    public string Id { get; set; }
    public string Type { get; set; } // "NameGenerated", "ConfigUpdated", "BackupCreated", etc.
    public string Title { get; set; }
    public string Detail { get; set; }
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } // Emoji or icon identifier
    
    // Computed property
    public string TimeAgo => CalculateTimeAgo(Timestamp);
    
    private string CalculateTimeAgo(DateTime timestamp)
    {
        var span = DateTime.Now - timestamp;
        if (span.TotalMinutes < 1) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
        return timestamp.ToString("MMM d");
    }
}
```

**Action Items**:
- [ ] Create `DashboardData.cs` model
- [ ] Create `ActivityItem.cs` model (or include in DashboardData)
- [ ] Add time calculation logic
- [ ] Add validation/null checks

#### 1.2: Create Dashboard Service
**File**: `src/Services/DashboardService.cs`

**Service Responsibilities**:
- Gather statistics from various sources
- Retrieve recent activity
- Calculate derived metrics
- Cache results for performance

**Service Methods**:
```csharp
public interface IDashboardService
{
    Task<DashboardData> GetDashboardDataAsync();
    Task<List<ActivityItem>> GetRecentActivityAsync(int count = 10);
    Task LogActivityAsync(ActivityItem activity);
    Task<int> GetNamesGeneratedCountAsync(DateTime? since = null);
}
```

**Data Sources**:
1. **Resource Types Count**: From ResourceTypesService
2. **Locations Count**: From ResourceLocationsService
3. **Names Generated**: From naming request logs/history
4. **Custom Components**: From CustomComponentsService
5. **Recent Activity**: From activity log/history

**Action Items**:
- [ ] Create `IDashboardService` interface
- [ ] Create `DashboardService` implementation
- [ ] Implement `GetDashboardDataAsync()` method
- [ ] Implement `GetRecentActivityAsync()` method
- [ ] Implement `LogActivityAsync()` method
- [ ] Register service in DI container
- [ ] Add caching for performance (optional)

#### 1.3: Activity Logging System
**File**: `src/Services/ActivityLogService.cs` (or extend existing logging)

**Purpose**: Track user actions for "Recent Activity" display

**Activity Types to Track**:
1. **NameGenerated**: When a resource name is generated
2. **ConfigurationUpdated**: When resource types/locations/etc. are modified
3. **BackupCreated**: When configuration backup is created
4. **RestoreCompleted**: When configuration restore is completed
5. **MigrationCompleted**: When SQLite migration is completed
6. **ComponentAdded**: When custom component is added
7. **ComponentDeleted**: When component is deleted

**Storage Options**:
- **Option A**: Store in SQLite database (new `ActivityLog` table)
- **Option B**: Store in JSON file (filesystem)
- **Option C**: In-memory with persistence (hybrid)

**Recommendation**: Use SQLite database for persistence and queryability

**Action Items**:
- [ ] Create `ActivityLog` model/table
- [ ] Create `ActivityLogService` or extend existing service
- [ ] Add logging calls throughout application:
  - [ ] ResourceNamingController (name generation)
  - [ ] Configuration page (imports/exports)
  - [ ] Admin page (migration)
  - [ ] All component controllers (add/edit/delete)
- [ ] Implement activity retrieval with filtering/paging
- [ ] Add cleanup logic (delete old activities after X days)

---

### Phase 2: Dashboard Page Implementation (Est. 4-5 hours)

#### 2.1: Create Dashboard Page Component
**File**: `src/Components/Pages/Dashboard.razor` or update `Home.razor`

**Page Structure**:
```razor
@page "/"
@page "/dashboard"
@using Services
@inject IDashboardService DashboardService
@inject NavigationManager Navigation

<PageTitle>Dashboard - Azure Naming Tool</PageTitle>

<div class="dashboard-container">
    <!-- Page Header -->
    <div class="page-header">
        <h1 class="page-title">Dashboard</h1>
        <p class="page-subtitle">Welcome to the Azure Naming Tool</p>
    </div>

    <!-- Stats Grid -->
    <div class="stats-grid">
        <!-- Stat cards will be here -->
    </div>

    <!-- Quick Actions -->
    <div class="action-cards-section">
        <h2 class="section-title">Quick Actions</h2>
        <div class="action-cards-grid">
            <!-- Action cards will be here -->
        </div>
    </div>

    <!-- Recent Activity -->
    <div class="recent-activity-section">
        <div class="section-header">
            <h2 class="section-title">Recent Activity</h2>
            <a href="/activity" class="section-link">View all →</a>
        </div>
        <div class="activity-list">
            <!-- Activity items will be here -->
        </div>
    </div>
</div>

@code {
    private DashboardData? dashboardData;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        isLoading = true;
        try
        {
            dashboardData = await DashboardService.GetDashboardDataAsync();
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error loading dashboard: {ex.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

**Action Items**:
- [ ] Create Dashboard.razor page component
- [ ] Add route (`/` and `/dashboard`)
- [ ] Inject required services
- [ ] Add loading state handling
- [ ] Add error handling

#### 2.2: Implement Stats Grid
**Stats to Display** (Initial Phase):
1. **Resource Types**: Total count of configured resource types
2. **Locations**: Total count of configured Azure locations
3. **Names Generated**: Total count (all-time or recent)
4. **Custom Components**: Total count of custom components

**Stat Card Component** (can be inline or separate):
```razor
<div class="stat-card">
    <div class="stat-label">@Label</div>
    <div class="stat-value">@Value</div>
    <div class="stat-change">@Change</div>
</div>
```

**Example Stats Grid Markup**:
```razor
<div class="stats-grid">
    @if (isLoading)
    {
        <div class="loading-stats">Loading statistics...</div>
    }
    else if (dashboardData != null)
    {
        <div class="stat-card">
            <div class="stat-label">Resource Types</div>
            <div class="stat-value">@dashboardData.TotalResourceTypes</div>
            <div class="stat-change">Configured</div>
        </div>

        <div class="stat-card">
            <div class="stat-label">Locations</div>
            <div class="stat-value">@dashboardData.TotalLocations</div>
            <div class="stat-change">Global coverage</div>
        </div>

        <div class="stat-card">
            <div class="stat-label">Names Generated</div>
            <div class="stat-value">@dashboardData.TotalNamesGenerated.ToString("N0")</div>
            <div class="stat-change">
                +@dashboardData.NamesGeneratedToday today
            </div>
        </div>

        <div class="stat-card">
            <div class="stat-label">Custom Components</div>
            <div class="stat-value">@dashboardData.CustomComponents</div>
            <div class="stat-change">Active</div>
        </div>
    }
</div>
```

**Action Items**:
- [ ] Add stats grid markup
- [ ] Add stat card styling (from DesignLinear)
- [ ] Add loading state
- [ ] Add empty state (if no data)
- [ ] Add number formatting (commas for large numbers)
- [ ] Test with various data values

#### 2.3: Implement Quick Actions Section
**Action Cards to Include**:
1. **Generate Resource Name**: Navigate to name generation page
2. **Configure Components**: Navigate to configuration page
3. **View Reference**: Navigate to reference/documentation page

**Action Card Component**:
```razor
<div class="action-card" @onclick="() => NavigateToPage(link)">
    <div class="card-header">
        <h3 class="card-title">@Title</h3>
        <span class="card-icon">@Icon</span>
    </div>
    <p class="card-description">@Description</p>
    <button class="card-btn">@ButtonText →</button>
</div>
```

**Example Quick Actions Markup**:
```razor
<div class="action-cards-grid">
    <div class="action-card" @onclick="() => Navigation.NavigateTo('/resourcenaming')">
        <div class="card-header">
            <h3 class="card-title">Generate Resource Name</h3>
            <span class="card-icon">🎯</span>
        </div>
        <p class="card-description">
            Create standardized Azure resource names following your organization's naming conventions.
        </p>
        <button class="card-btn">Generate Name →</button>
    </div>

    <div class="action-card" @onclick="() => Navigation.NavigateTo('/configuration')">
        <div class="card-header">
            <h3 class="card-title">Configure Components</h3>
            <span class="card-icon">⚙️</span>
        </div>
        <p class="card-description">
            Manage resource types, locations, environments, and other naming components.
        </p>
        <button class="card-btn">Configure →</button>
    </div>

    <div class="action-card" @onclick="() => Navigation.NavigateTo('/reference')">
        <div class="card-header">
            <h3 class="card-title">View Reference</h3>
            <span class="card-icon">📚</span>
        </div>
        <p class="card-description">
            Browse Azure naming best practices and component reference documentation.
        </p>
        <button class="card-btn">View Reference →</button>
    </div>
</div>
```

**Action Items**:
- [ ] Add action cards markup
- [ ] Add navigation handlers
- [ ] Add card styling (from DesignLinear)
- [ ] Add hover effects
- [ ] Test navigation links
- [ ] Make cards keyboard accessible

#### 2.4: Implement Recent Activity Section
**Activity List Component**:
```razor
<div class="activity-list">
    @if (dashboardData?.RecentActivity == null || !dashboardData.RecentActivity.Any())
    {
        <div class="activity-empty">
            <p>No recent activity to display.</p>
        </div>
    }
    else
    {
        @foreach (var activity in dashboardData.RecentActivity)
        {
            <div class="activity-item">
                <div class="activity-icon">@activity.Icon</div>
                <div class="activity-content">
                    <div class="activity-title">@activity.Title</div>
                    @if (!string.IsNullOrEmpty(activity.Detail))
                    {
                        <div class="activity-detail">@activity.Detail</div>
                    }
                </div>
                <div class="activity-time">@activity.TimeAgo</div>
            </div>
        }
    }
</div>
```

**Activity Icons** (based on type):
- **NameGenerated**: ✓ (checkmark)
- **ConfigurationUpdated**: ⚙️ (gear)
- **BackupCreated**: 💾 (floppy disk)
- **RestoreCompleted**: ↻ (refresh)
- **MigrationCompleted**: 🔄 (cycle)
- **ComponentAdded**: ➕ (plus)
- **ComponentDeleted**: 🗑️ (trash)

**Action Items**:
- [ ] Add activity list markup
- [ ] Add activity item styling (from DesignLinear)
- [ ] Add empty state
- [ ] Add icon mapping logic
- [ ] Add time formatting
- [ ] Limit to 5-10 most recent items
- [ ] Add "View all" link (future: full activity page)

---

### Phase 3: Styling & Polish (Est. 2-3 hours)

#### 3.1: Dashboard CSS
**File**: `src/Components/Pages/Dashboard.razor.css` (scoped CSS) or add to global CSS

**Key Styles from DesignLinear**:
```css
/* Stats Grid */
.stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
    gap: 16px;
    margin-bottom: 24px;
}

.stat-card {
    background-color: #ffffff;
    border: 1px solid #e5e5e5;
    border-radius: 8px;
    padding: 20px;
    transition: all 0.15s ease;
}

.stat-card:hover {
    border-color: #00b7c3;
    background-color: #f0fafb;
}

.stat-label {
    font-size: 13px;
    color: #737373;
    margin-bottom: 8px;
}

.stat-value {
    font-size: 28px;
    font-weight: 600;
    color: #00b7c3;
    margin-bottom: 4px;
}

.stat-change {
    font-size: 13px;
    color: #059669;
    font-weight: 500;
}

/* Action Cards */
.action-cards-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: 16px;
    margin-bottom: 32px;
}

.action-card {
    background-color: #ffffff;
    border: 1px solid #e5e5e5;
    border-radius: 8px;
    padding: 24px;
    cursor: pointer;
    transition: all 0.15s ease;
}

.action-card:hover {
    border-color: #0078d4;
    box-shadow: 0 0 0 1px #0078d4;
}

.card-btn {
    background: linear-gradient(135deg, #0078d4 0%, #005a9e 100%);
    border: none;
    padding: 10px 20px;
    border-radius: 6px;
    color: #ffffff;
    font-weight: 600;
    box-shadow: 0 2px 8px rgba(0, 120, 212, 0.2);
}

/* Activity List */
.activity-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.activity-item {
    display: flex;
    align-items: start;
    gap: 12px;
    padding: 12px;
    border-radius: 6px;
    transition: background-color 0.15s ease;
}

.activity-item:hover {
    background-color: #fafafa;
}

.activity-icon {
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, #8661c5 0%, #6b4ba1 100%);
    color: #ffffff;
    border-radius: 6px;
    font-size: 14px;
    flex-shrink: 0;
}
```

**Action Items**:
- [ ] Create scoped CSS file or add to global CSS
- [ ] Copy/adapt styles from DesignLinear
- [ ] Add responsive breakpoints
- [ ] Test on mobile (< 768px)
- [ ] Test on tablet (768px - 1024px)

#### 3.2: Loading & Empty States
**Loading State**:
```razor
@if (isLoading)
{
    <div class="dashboard-loading">
        <div class="loading-spinner"></div>
        <p>Loading dashboard...</p>
    </div>
}
```

**Empty State** (no data):
```razor
@if (dashboardData?.RecentActivity?.Count == 0)
{
    <div class="activity-empty">
        <span class="empty-icon">📊</span>
        <h3>No activity yet</h3>
        <p>Start generating names to see activity here.</p>
        <button class="btn-primary" @onclick="() => Navigation.NavigateTo('/resourcenaming')">
            Generate Your First Name
        </button>
    </div>
}
```

**Action Items**:
- [ ] Add loading spinner component/CSS
- [ ] Add empty state for stats
- [ ] Add empty state for activity
- [ ] Add error state
- [ ] Test all states

#### 3.3: Responsive Design
**Breakpoints**:
- **Mobile** (< 768px): 1 column for stats/cards
- **Tablet** (768px - 1024px): 2 columns
- **Desktop** (> 1024px): 4 columns for stats, 3 for actions

**Action Items**:
- [ ] Test dashboard on mobile devices
- [ ] Adjust grid layouts for small screens
- [ ] Test touch interactions on cards
- [ ] Verify readability of text sizes

---

### Phase 4: Data Integration (Est. 3-4 hours)

#### 4.1: Integrate Name Generation Tracking
**Goal**: Track every name generation request

**Integration Points**:
1. **ResourceNamingRequestsController**: Add activity logging after successful generation

**Code Example**:
```csharp
[HttpPost]
public async Task<IActionResult> GenerateName([FromBody] NameRequest request)
{
    // ... existing generation logic ...
    
    // Log activity
    await _activityLogService.LogActivityAsync(new ActivityItem
    {
        Type = "NameGenerated",
        Title = $"Generated name for {request.ResourceType}",
        Detail = generatedName,
        Timestamp = DateTime.Now,
        Icon = "✓"
    });
    
    return Ok(generatedName);
}
```

**Action Items**:
- [ ] Add activity logging to ResourceNamingRequestsController
- [ ] Test name generation creates activity log
- [ ] Verify activity appears on dashboard

#### 4.2: Integrate Configuration Change Tracking
**Goal**: Track configuration imports, exports, and changes

**Integration Points**:
1. **Configuration.razor**: Log backup/restore operations
2. **Component Controllers**: Log add/edit/delete operations

**Action Items**:
- [ ] Add logging to Configuration page (backup/restore)
- [ ] Add logging to ResourceTypes controller
- [ ] Add logging to ResourceLocations controller
- [ ] Add logging to other component controllers
- [ ] Test configuration changes appear on dashboard

#### 4.3: Integrate Migration Tracking
**Goal**: Track SQLite migration completion

**Integration Points**:
1. **Admin.razor**: Log successful migration

**Action Items**:
- [ ] Add logging to Admin migration workflow
- [ ] Test migration creates activity log
- [ ] Verify activity appears on dashboard

#### 4.4: Statistics Calculation
**Goal**: Accurately calculate all dashboard statistics

**Calculations Needed**:
1. **Total Names Generated**: Count from activity log or dedicated counter
2. **Names Generated Today**: Filter by date
3. **Names Generated This Week**: Filter by date range
4. **Names Generated This Month**: Filter by date range

**Performance Considerations**:
- Cache statistics for 5-10 minutes
- Use indexed database queries
- Consider pre-calculated counters for large datasets

**Action Items**:
- [ ] Implement total names calculation
- [ ] Implement date-filtered calculations
- [ ] Add caching layer
- [ ] Test with large datasets
- [ ] Optimize queries if needed

---

### Phase 5: Testing & Refinement (Est. 2-3 hours)

#### 5.1: Functional Testing
**Test Scenarios**:
1. Dashboard loads successfully on first visit
2. Statistics display correct values
3. Quick action cards navigate correctly
4. Recent activity shows latest items
5. Loading states display properly
6. Empty states display when no data
7. Error states handle failures gracefully

**Action Items**:
- [ ] Test dashboard load
- [ ] Test all navigation links
- [ ] Test with no data
- [ ] Test with lots of data (100+ activities)
- [ ] Test error scenarios (service failures)

#### 5.2: Visual Testing
**Test Items**:
- [ ] Stats cards align properly
- [ ] Colors match design system
- [ ] Icons display correctly
- [ ] Hover effects work smoothly
- [ ] Typography is consistent
- [ ] Spacing is consistent

#### 5.3: Performance Testing
**Test Items**:
- [ ] Dashboard loads quickly (< 1 second)
- [ ] No visible lag when navigating
- [ ] Activity list doesn't cause scroll jank
- [ ] Images/icons load efficiently

#### 5.4: Accessibility Testing
**Test Items**:
- [ ] Keyboard navigation works (Tab, Enter)
- [ ] Focus indicators visible
- [ ] Screen reader announces content properly
- [ ] Color contrast meets WCAG AA
- [ ] Action cards have proper ARIA labels

---

## Future Enhancements (Post-MVP)

---

## Admin-Only Features & Access Control

### Overview
The Azure Naming Tool already has an admin authentication system in place. The dashboard implementation must respect these existing access controls and potentially provide different experiences for admin vs. public users.

### Current Admin System
- **Admin Pages**: Some pages are restricted to authenticated administrators
- **Authentication Attribute**: `[ApiKey]` attribute used on controllers
- **Admin Pages Include**:
  - Configuration management
  - Component management (add/edit/delete)
  - Admin settings and migration
  - Import/Export functionality

### Dashboard Access Control Strategy

#### Option A: Public Dashboard with Limited Data
**Recommendation**: Make dashboard accessible to everyone, but show different statistics based on authentication state.

**Public Users See**:
- ✅ Total Resource Types (count only)
- ✅ Total Locations (count only)
- ✅ Names Generated (count only, no details)
- ✅ Quick Action: "Generate Resource Name"
- ❌ Recent Activity (hidden or limited)
- ❌ Configuration and Admin actions (hidden)

**Admin Users See**:
- ✅ All statistics (with more detail)
- ✅ Recent Activity (full list with configuration changes)
- ✅ Quick Actions: "Generate Name", "Configure Components", "Admin Settings"
- ✅ Configuration change logs
- ✅ Migration activity logs
- ✅ Additional stats (failed validations, etc.)

#### Option B: Dashboard Only for Admins
**Alternative**: Restrict entire dashboard to admin users only, show simple landing page for public.

**Trade-offs**:
- ✅ **Pros**: Protects all internal metrics, simpler access control
- ❌ **Cons**: Public users get no value from home page

### Implementation Approach (Option A - Recommended)

#### 1. Check Admin Status in Dashboard Component
**File**: `src/Components/Pages/Dashboard.razor`

```razor
@page "/"
@page "/dashboard"
@inject IIdentityHelper IdentityHelper

@code {
    private bool isAdmin = false;
    
    protected override async Task OnInitializedAsync()
    {
        // Check if user is authenticated as admin
        isAdmin = await IdentityHelper.IsAdminAsync();
        
        await LoadDashboardDataAsync();
    }
}
```

#### 2. Conditional Rendering Based on Admin Status

**Stats Grid** (Public: counts only, Admin: counts + trends):
```razor
<div class="stat-card">
    <div class="stat-label">Names Generated</div>
    <div class="stat-value">@dashboardData.TotalNamesGenerated.ToString("N0")</div>
    @if (isAdmin)
    {
        <div class="stat-change">
            +@dashboardData.NamesGeneratedToday today
        </div>
    }
    else
    {
        <div class="stat-change">All time</div>
    }
</div>
```

**Recent Activity** (Admin only):
```razor
@if (isAdmin)
{
    <div class="recent-activity-section">
        <div class="section-header">
            <h2 class="section-title">Recent Activity</h2>
            <a href="/activity" class="section-link">View all →</a>
        </div>
        <div class="activity-list">
            @foreach (var activity in dashboardData.RecentActivity)
            {
                <div class="activity-item">
                    <!-- Activity content -->
                </div>
            }
        </div>
    </div>
}
```

**Quick Actions** (Different for public vs admin):
```razor
<div class="action-cards-grid">
    <!-- Always show: Generate Name -->
    <div class="action-card" @onclick="() => Navigation.NavigateTo('/resourcenaming')">
        <div class="card-header">
            <h3 class="card-title">Generate Resource Name</h3>
            <span class="card-icon">🎯</span>
        </div>
        <p class="card-description">
            Create standardized Azure resource names following naming conventions.
        </p>
        <button class="card-btn">Generate Name →</button>
    </div>

    <!-- Admin only: Configure Components -->
    @if (isAdmin)
    {
        <div class="action-card" @onclick="() => Navigation.NavigateTo('/configuration')">
            <div class="card-header">
                <h3 class="card-title">Configure Components</h3>
                <span class="card-icon">⚙️</span>
            </div>
            <p class="card-description">
                Manage resource types, locations, environments, and other naming components.
            </p>
            <button class="card-btn">Configure →</button>
        </div>

        <div class="action-card" @onclick="() => Navigation.NavigateTo('/admin')">
            <div class="card-header">
                <h3 class="card-title">Admin Settings</h3>
                <span class="card-icon">🔧</span>
            </div>
            <p class="card-description">
                Manage application settings, backup/restore, and system configuration.
            </p>
            <button class="card-btn">Admin →</button>
        </div>
    }
    else
    {
        <!-- Public: Show Reference instead -->
        <div class="action-card" @onclick="() => Navigation.NavigateTo('/reference')">
            <div class="card-header">
                <h3 class="card-title">View Reference</h3>
                <span class="card-icon">📚</span>
            </div>
            <p class="card-description">
                Browse Azure naming best practices and component reference documentation.
            </p>
            <button class="card-btn">View Reference →</button>
        </div>
    }
</div>
```

#### 3. Filter Activity Logs by Sensitivity

**Service Layer Filtering**:
```csharp
public async Task<List<ActivityItem>> GetRecentActivityAsync(int count = 10, bool isAdmin = false)
{
    var activities = await _repository.GetRecentActivitiesAsync(count * 2); // Get more, then filter
    
    if (!isAdmin)
    {
        // Only show public-safe activities
        activities = activities
            .Where(a => a.Type == "NameGenerated")
            .ToList();
    }
    
    return activities.Take(count).ToList();
}
```

**Public-Safe Activity Types**:
- ✅ `NameGenerated` (if you want to show public users activity)

**Admin-Only Activity Types**:
- ❌ `ConfigurationUpdated`
- ❌ `BackupCreated`
- ❌ `RestoreCompleted`
- ❌ `MigrationCompleted`
- ❌ `ComponentAdded`
- ❌ `ComponentDeleted`

#### 4. Dashboard Service Access Control

**Update `IDashboardService` Interface**:
```csharp
public interface IDashboardService
{
    Task<DashboardData> GetDashboardDataAsync(bool isAdmin = false);
    Task<List<ActivityItem>> GetRecentActivityAsync(int count = 10, bool isAdmin = false);
    Task LogActivityAsync(ActivityItem activity);
    Task<int> GetNamesGeneratedCountAsync(DateTime? since = null);
}
```

**Update Service Implementation**:
```csharp
public async Task<DashboardData> GetDashboardDataAsync(bool isAdmin = false)
{
    var data = new DashboardData
    {
        TotalResourceTypes = await GetResourceTypesCountAsync(),
        TotalLocations = await GetLocationsCountAsync(),
        TotalNamesGenerated = await GetNamesGeneratedCountAsync(),
        CustomComponents = await GetCustomComponentsCountAsync()
    };
    
    if (isAdmin)
    {
        // Add admin-only metrics
        data.NamesGeneratedToday = await GetNamesGeneratedCountAsync(DateTime.Today);
        data.NamesGeneratedThisWeek = await GetNamesGeneratedCountAsync(DateTime.Today.AddDays(-7));
        data.NamesGeneratedThisMonth = await GetNamesGeneratedCountAsync(DateTime.Today.AddMonths(-1));
        data.RecentActivity = await GetRecentActivityAsync(10, isAdmin: true);
    }
    else
    {
        // Public users don't get recent activity or detailed trends
        data.RecentActivity = new List<ActivityItem>();
    }
    
    return data;
}
```

### Implementation Checklist

#### Phase 1: Foundation
- [ ] Add `isAdmin` check in Dashboard.razor
- [ ] Update `IDashboardService` to accept `isAdmin` parameter
- [ ] Update `DashboardService` implementation with access control logic
- [ ] Add activity filtering based on admin status

#### Phase 2: UI Conditional Rendering
- [ ] Add conditional rendering for statistics detail
- [ ] Add conditional rendering for recent activity section
- [ ] Add conditional rendering for admin quick actions
- [ ] Show appropriate actions for public users

#### Phase 3: Testing
- [ ] Test dashboard as public user (no authentication)
- [ ] Test dashboard as admin user (authenticated)
- [ ] Verify public users cannot see configuration activities
- [ ] Verify admin users see all features
- [ ] Test navigation links work correctly for both user types
- [ ] Verify no data leakage between user types

### Security Considerations

1. **Server-Side Validation**: Always check authentication on server/service layer, not just UI
2. **API Endpoints**: If dashboard calls API endpoints, those must also check `[ApiKey]` attribute
3. **Activity Logs**: Sensitive activities should only be retrievable by admins
4. **Navigation Guards**: Admin pages should already have auth checks (verify they exist)

### Future Enhancements

1. **User Roles**: If multi-user support is added, implement role-based access (Admin, Editor, Viewer)
2. **Audit Trail**: Track who viewed what on the dashboard (for compliance)
3. **Personalization**: Allow admins to configure what public users can see
4. **Public API Mode**: If tool has public API, dashboard could show API usage stats (admin only)

---

### Phase 6: Advanced Statistics (Future)
1. **Charts & Graphs**: 
   - Line chart: Names generated over time
   - Bar chart: Most used resource types
   - Pie chart: Names by environment (prod/dev/test)
2. **Filters**: Date range filters for statistics
3. **Comparisons**: Week-over-week, month-over-month comparisons
4. **Export**: Export statistics to CSV/Excel

### Phase 7: Enhanced Activity (Future)
1. **Full Activity Page**: Paginated list of all activities
2. **Activity Filters**: Filter by type, date range, user
3. **Activity Search**: Search activity logs
4. **Activity Export**: Export activity logs

### Phase 8: Personalization (Future)
1. **User Preferences**: Choose which stats to display
2. **Custom Widgets**: Add/remove dashboard widgets
3. **Widget Layout**: Drag-and-drop widget arrangement
4. **Favorites**: Quick access to favorite actions

### Phase 9: Additional Stats (Future)
1. **Most Used Resource Types**: Top 5 most generated types
2. **Most Used Locations**: Top 5 most used Azure regions
3. **Most Active Users**: If multi-user support added
4. **Naming Patterns**: Common naming patterns used
5. **Validation Failures**: Count of failed name generations

---

## Data Persistence

### SQLite Database Schema (Recommended)

#### ActivityLog Table
```sql
CREATE TABLE ActivityLog (
    Id TEXT PRIMARY KEY,
    Type TEXT NOT NULL,
    Title TEXT NOT NULL,
    Detail TEXT,
    Timestamp DATETIME NOT NULL,
    Icon TEXT,
    UserId TEXT,
    Metadata TEXT -- JSON for additional data
);

CREATE INDEX IX_ActivityLog_Timestamp ON ActivityLog(Timestamp DESC);
CREATE INDEX IX_ActivityLog_Type ON ActivityLog(Type);
```

#### NameGenerationCounter Table (Optional - for performance)
```sql
CREATE TABLE NameGenerationCounter (
    Id INTEGER PRIMARY KEY,
    Date DATE NOT NULL UNIQUE,
    Count INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IX_NameGenerationCounter_Date ON NameGenerationCounter(Date DESC);
```

**Migration Notes**:
- If using FileSystem storage, create JSON file: `repository/activitylog.json`
- If using SQLite, add migration to create tables
- Consider data retention policy (delete old activities after 90 days)

---

## Implementation Timeline

### Recommended Schedule (2-3 weeks)

**Week 1: Core Implementation**
- **Day 1-2**: Phase 1 (Data Models & Services)
- **Day 3-4**: Phase 2 (Dashboard Page)
- **Day 5**: Phase 3 (Styling & Polish)

**Week 2: Integration & Access Control**
- **Day 6-7**: Phase 4 (Data Integration)
- **Day 8**: Phase 4.5 (Admin Access Control Implementation)
- **Day 9**: Phase 5 (Testing & Refinement)
- **Day 10**: Buffer for issues, documentation

**Total Estimated Time**: 16-22 hours (increased from 14-19 hours to account for admin access control)

---

## Success Criteria

### Must Have (MVP)
- [ ] Dashboard page loads successfully
- [ ] All 4 statistics display correctly:
  - [ ] Resource Types count
  - [ ] Locations count
  - [ ] Names Generated count
  - [ ] Custom Components count
- [ ] Quick action cards navigate correctly
- [ ] Recent activity shows last 10 items
- [ ] Activity logging works for name generation
- [ ] Page is responsive (mobile/tablet/desktop)
- [ ] No console errors

### Nice to Have
- [ ] Activity logging for all configuration changes
- [ ] Loading animations
- [ ] Smooth transitions
- [ ] Cached statistics
- [ ] Empty states with helpful messaging

---

## File Inventory

### Files to Create
- `src/Models/DashboardData.cs`
- `src/Models/ActivityItem.cs`
- `src/Services/IDashboardService.cs`
- `src/Services/DashboardService.cs`
- `src/Services/ActivityLogService.cs` (or extend existing)
- `src/Components/Pages/Dashboard.razor`
- `src/Components/Pages/Dashboard.razor.css` (optional)
- Database migration file (if using SQLite)

### Files to Modify
- `src/Controllers/ResourceNamingRequestsController.cs` - Add activity logging
- `src/Components/Pages/Configuration.razor` - Add activity logging
- `src/Components/Pages/Admin.razor` - Add activity logging
- All component controllers - Add activity logging (ResourceTypes, Locations, etc.)
- `src/Program.cs` or `Startup.cs` - Register new services
- Navigation menu - Update home link (if needed)

### Files to Reference
- `src/Components/Pages/DesignMockups/DesignLinear.razor` - Copy styles and structure

---

## Testing Checklist

### Functional Tests
- [ ] Dashboard loads on app start
- [ ] Statistics show correct values
- [ ] "Generate Resource Name" card navigates correctly
- [ ] "Configure Components" card navigates correctly (admin only)
- [ ] "View Reference" card navigates correctly
- [ ] Recent activity shows latest items (sorted by time)
- [ ] Activity "time ago" updates appropriately
- [ ] Generating a name creates activity log
- [ ] Configuration changes create activity logs
- [ ] Statistics update when new data is added
- [ ] **Admin Access Control**: Public users see limited stats
- [ ] **Admin Access Control**: Public users don't see recent activity
- [ ] **Admin Access Control**: Public users don't see admin action cards
- [ ] **Admin Access Control**: Admin users see all features
- [ ] **Admin Access Control**: Admin users see full recent activity
- [ ] **Admin Access Control**: Admin users see admin action cards

### Visual Tests
- [ ] Matches DesignLinear mockup design
- [ ] Stats cards have teal values (#00b7c3)
- [ ] Action card buttons have Azure blue gradient
- [ ] Activity icons have purple gradient
- [ ] Hover effects work on all cards
- [ ] Typography is consistent with design system
- [ ] Spacing matches mockup

### Responsive Tests
- [ ] Mobile (< 768px): 1 column layout
- [ ] Tablet (768px - 1024px): 2 column layout
- [ ] Desktop (> 1024px): 4 column stats, 3 column actions
- [ ] All text is readable on mobile
- [ ] Touch targets are adequate (min 44px)
- [ ] No horizontal scrolling issues

### Performance Tests
- [ ] Dashboard loads in < 1 second with 100 activities
- [ ] No visible lag when clicking action cards
- [ ] Activity list renders smoothly
- [ ] Statistics calculation doesn't block UI

### Accessibility Tests
- [ ] Can tab through all interactive elements
- [ ] Focus indicators are visible
- [ ] Action cards are keyboard accessible (Enter to activate)
- [ ] Screen reader announces statistics correctly
- [ ] Color contrast passes WCAG AA
- [ ] All icons have text alternatives

---

## Risk Assessment

### High Risk
1. **Performance with Large Datasets**: Many activities could slow dashboard load
   - **Mitigation**: Implement caching, limit to recent 10-50 items, use pagination
2. **Activity Logging Integration**: Adding logging everywhere could break existing features
   - **Mitigation**: Add logging in try-catch blocks, test thoroughly

### Medium Risk
1. **Statistics Accuracy**: Calculations might not match reality
   - **Mitigation**: Validate calculations, add unit tests
2. **Database Migration**: Adding new tables could fail
   - **Mitigation**: Test migration thoroughly, provide rollback

### Low Risk
1. **UI Styling**: Dashboard might not match mockup exactly
   - **Mitigation**: Reference DesignLinear closely, iterate on feedback
2. **Empty States**: Dashboard might look empty initially
   - **Mitigation**: Provide helpful empty state messaging

---

## Rollback Plan

### If Critical Issues Found
1. Keep previous home page as `Home.razor.bak`
2. Git commits should be incremental
3. Can disable dashboard route and revert to previous home page
4. Activity logging is non-critical, can be disabled

### Rollback Triggers
- Dashboard crashes on load
- Statistics show incorrect data causing confusion
- Performance degrades significantly
- Critical bug in activity logging breaks name generation

---

## Status Tracking

### Phase 1: Data Model & Services
- [ ] 1.1: Create Dashboard Model
- [ ] 1.2: Create Dashboard Service
- [ ] 1.3: Activity Logging System

### Phase 2: Dashboard Page Implementation
- [ ] 2.1: Create Dashboard Page Component
- [ ] 2.2: Implement Stats Grid
- [ ] 2.3: Implement Quick Actions Section
- [ ] 2.4: Implement Recent Activity Section

### Phase 3: Styling & Polish
- [ ] 3.1: Dashboard CSS
- [ ] 3.2: Loading & Empty States
- [ ] 3.3: Responsive Design

### Phase 4: Data Integration
- [ ] 4.1: Integrate Name Generation Tracking
- [ ] 4.2: Integrate Configuration Change Tracking
- [ ] 4.3: Integrate Migration Tracking
- [ ] 4.4: Statistics Calculation

### Phase 4.5: Admin Access Control Implementation
- [ ] 4.5.1: Add isAdmin check in Dashboard component
- [ ] 4.5.2: Update IDashboardService with isAdmin parameter
- [ ] 4.5.3: Update DashboardService with access control logic
- [ ] 4.5.4: Add activity filtering by admin status
- [ ] 4.5.5: Add conditional rendering for admin features
- [ ] 4.5.6: Test public vs admin user experiences

### Phase 5: Testing & Refinement
- [ ] 5.1: Functional Testing
- [ ] 5.2: Visual Testing
- [ ] 5.3: Performance Testing
- [ ] 5.4: Accessibility Testing

---

**Last Updated**: January 16, 2025
**Document Version**: 1.0
**Status**: Planning Phase
