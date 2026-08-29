# Work Order Management System

A professional Windows desktop application for managing maintenance work orders and technicians. Built with **WPF + MVVM**, **Entity Framework Core 8** (Code-First), and **Clean Architecture** principles across four layered projects.

---

## Technology Stack

| Technology | Version | Purpose |
|---|---|---|
| C# 12 / .NET 8 | 8.0 | Primary language and runtime |
| WPF (.NET 8) | 8.0 | Desktop UI framework (Windows-native) |
| MVVM (CommunityToolkit.Mvvm) | 8.3.2 | UI design pattern (source generators) |
| Entity Framework Core | 8.0.8 | ORM — Code-First with migrations |
| SQL Server | 17.x+ | Primary relational database (localhost) |
| SQLite | 3.x | Fallback embedded database |
| Repository Pattern | — | Data access abstraction |
| Microsoft.Extensions.DependencyInjection | 8.0.1 | IoC container |
| Microsoft.Extensions.Hosting | 8.0.1 | Application host lifecycle |
| Microsoft.Extensions.Logging | 8.0 | Structured logging (Debug + Console) |
| xUnit + Moq | — | Unit testing framework |

---

## Architecture

### Layer Diagram

```
┌──────────────────────────────────────────────────────────────┐
│  WorkOrderManagement.App  (Presentation — WPF / MVVM)       │
│  ┌─────────────┐  ┌──────────────────┐  ┌───────────────┐   │
│  │ Views (XAML) │→ │ ViewModels (C#)  │→ │ Converters    │   │
│  └─────────────┘  └──────────────────┘  └───────────────┘   │
│            ↓ depends on                                      │
├──────────────────────────────────────────────────────────────┤
│  WorkOrderManagement.Application  (Business Logic)           │
│  ┌────────────────┐  ┌────────────────┐  ┌──────────────┐   │
│  │ Services        │  │ Interfaces     │  │ DTOs         │   │
│  │ WorkOrderService│  │ IWorkOrderRepo │  │ WorkOrderFil.│   │
│  │ TechnicianSvc   │  │ ITechnicianRepo│  │ WorkOrderSum.│   │
│  └────────────────┘  └────────────────┘  └──────────────┘   │
│            ↓ depends on                                      │
├──────────────────────────────────────────────────────────────┤
│  WorkOrderManagement.Domain  (Entities & Enums — no deps)    │
│  ┌─────────────────┐  ┌──────────────────────┐              │
│  │ Technician.cs    │  │ Priority enum        │              │
│  │ WorkOrder.cs     │  │ WorkOrderStatus enum │              │
│  └─────────────────┘  └──────────────────────┘              │
├──────────────────────────────────────────────────────────────┤
│  WorkOrderManagement.Infrastructure  (Data Access)           │
│  ┌──────────────────┐  ┌─────────────────────┐              │
│  │ Repositories      │  │ WorkOrderDbContext   │              │
│  │ EF Configurations │  │ Migrations (Code-1st)│              │
│  │ DatabaseSeeder    │  │ DbContextFactory     │              │
│  └──────────────────┘  └─────────────────────┘              │
└──────────────────────────────────────────────────────────────┘
         ↓
   SQL Server (localhost) or SQLite (workorders.db)
```

### Dependency Flow

```
App → Application → Domain ← Infrastructure
```

- **Domain** has zero external dependencies (pure C# entities and enums).
- **Application** depends only on Domain; defines interfaces and business logic.
- **Infrastructure** depends on Domain + Application; implements repositories and EF Core.
- **App** depends on all layers; wires DI, hosts the WPF shell, and runs the application.

---

## Architectural Decisions

### Why WPF + MVVM?

WPF is the most mature and feature-rich desktop UI framework on .NET for Windows. The **MVVM** (Model-View-ViewModel) pattern enforces strict separation between the UI (XAML Views) and logic (C# ViewModels), enabling:

- **Independent testability** — ViewModels can be unit-tested without launching the UI.
- **Data binding** — WPF's powerful binding engine eliminates manual UI synchronisation code.
- **CommunityToolkit.Mvvm** source generators (`[ObservableProperty]`, `[RelayCommand]`) minimise boilerplate.

### Why Clean Architecture (4-Project Solution)?

Separating Domain, Application, Infrastructure, and Presentation into distinct projects enforces the **Dependency Inversion Principle**:

- Business rules in `Application` never reference EF Core or SQL Server directly.
- Swapping the database provider (e.g., SQLite → SQL Server) requires changes only in `Infrastructure` and configuration — no service or ViewModel changes.
- The `Domain` layer is completely framework-agnostic.

### Why Repository Pattern?

Repositories abstract all data access behind interfaces (`IWorkOrderRepository`, `ITechnicianRepository`). This:

- Makes the service layer and unit tests independent of EF Core.
- Allows database filtering to happen at the `IQueryable` level (server-side SQL), not in-memory.
- Enables mocking with **Moq** in 28 unit tests without an actual database.

### Why EF Core Code-First with Migrations?

Code-First migrations keep the database schema version-controlled alongside application code:

- The `Migrations/` folder contains the full schema history.
- `EnsureCreatedAsync()` (SQL Server) or `MigrateAsync()` (SQLite) runs automatically on startup — **zero manual database setup required**.
- Entity configurations (`TechnicianConfiguration.cs`, `WorkOrderConfiguration.cs`) use the Fluent API for explicit column constraints, relationships, and delete behaviour.

### Why SQL Server (Primary) + SQLite (Fallback)?

- **SQL Server** is the industry-standard relational database for .NET enterprise applications. The app connects to `localhost` using Windows Authentication (no password needed).
- **SQLite** is retained as a fallback for portability — if the connection string points to a `.db` file, the app automatically switches to the SQLite provider.
- The provider selection is automatic based on connection string analysis in `App.xaml.cs`.

### Why `IDateTimeProvider` Abstraction?

`DateTime.Now` is never called directly in service or domain code. All date access goes through `IDateTimeProvider`, making time-dependent business rules (high-priority date enforcement, overdue detection) fully **deterministic in unit tests**.

---

## Project Structure

```
WorkOrderManagement/
├── WorkOrderManagement.slnx            # Solution file
├── .gitignore
├── README.md
│
├── src/
│   ├── WorkOrderManagement.Domain/          # Layer 1: Entities & Enums (no dependencies)
│   │   ├── Entities/
│   │   │   ├── Technician.cs                # Id, FullName, Specialty, WorkOrders nav
│   │   │   └── WorkOrder.cs                 # Id, Title, Description, Priority, Status, Dates, FK
│   │   └── Enums/
│   │       ├── Priority.cs                  # Low=0, Medium=1, High=2
│   │       └── WorkOrderStatus.cs           # Open=0, InProgress=1, Completed=2
│   │
│   ├── WorkOrderManagement.Application/     # Layer 2: Business Logic & Contracts
│   │   ├── Interfaces/
│   │   │   ├── IDateTimeProvider.cs          # Testable clock abstraction
│   │   │   ├── ITechnicianRepository.cs      # CRUD contract for technicians
│   │   │   ├── IWorkOrderRepository.cs       # CRUD + filtering contract
│   │   │   ├── ITechnicianService.cs         # Service contract
│   │   │   └── IWorkOrderService.cs          # Service contract + business rules
│   │   ├── Services/
│   │   │   ├── TechnicianService.cs          # Technician CRUD + deletion guard
│   │   │   └── WorkOrderService.cs           # Business rules + validation
│   │   ├── DTOs/
│   │   │   ├── WorkOrderFilter.cs            # Status/Priority filter criteria
│   │   │   └── WorkOrderSummary.cs           # Dashboard statistics
│   │   └── ValidationException.cs            # Multi-field validation errors
│   │
│   ├── WorkOrderManagement.Infrastructure/  # Layer 3: Data Access & EF Core
│   │   ├── Data/
│   │   │   ├── WorkOrderDbContext.cs          # DbContext with DbSet<Technician>, DbSet<WorkOrder>
│   │   │   ├── SystemDateTimeProvider.cs      # Production IDateTimeProvider
│   │   │   └── DatabaseSeeder.cs              # Auto-seeds demo data on first run
│   │   ├── Configurations/
│   │   │   ├── TechnicianConfiguration.cs     # Fluent API: PK, MaxLength, FK (Restrict)
│   │   │   └── WorkOrderConfiguration.cs      # Fluent API: PK, MaxLength, Required fields
│   │   ├── Repositories/
│   │   │   ├── TechnicianRepository.cs         # EF Core implementation
│   │   │   └── WorkOrderRepository.cs          # EF Core impl + IQueryable filtering
│   │   ├── Migrations/                         # EF Core Code-First migrations
│   │   │   ├── 20260825060731_InitialCreate.cs
│   │   │   ├── 20260825060731_InitialCreate.Designer.cs
│   │   │   └── WorkOrderDbContextModelSnapshot.cs
│   │   └── WorkOrderDbContextFactory.cs        # IDesignTimeDbContextFactory
│   │
│   └── WorkOrderManagement.App/              # Layer 4: WPF Presentation
│       ├── Views/
│       │   ├── MainWindow.xaml                  # Shell — sidebar navigation + content area
│       │   ├── WorkOrdersView.xaml              # Dashboard cards + DataGrid + filters
│       │   ├── TechniciansView.xaml              # Technician list
│       │   ├── AddEditWorkOrderDialog.xaml       # Work order form dialog
│       │   └── AddEditTechnicianDialog.xaml      # Technician form dialog
│       ├── ViewModels/
│       │   ├── BaseViewModel.cs                  # ObservableObject + IsBusy
│       │   ├── MainWindowViewModel.cs            # Navigation controller
│       │   ├── WorkOrdersViewModel.cs            # Work orders CRUD + filtering + dashboard
│       │   ├── TechniciansViewModel.cs            # Technicians CRUD
│       │   ├── AddEditWorkOrderViewModel.cs       # Form logic + overdue check
│       │   └── AddEditTechnicianViewModel.cs      # Form logic
│       ├── Converters/                            # IValueConverter implementations
│       ├── Resources/
│       │   └── Styles.xaml                        # Global dark theme styles
│       ├── App.xaml / App.xaml.cs                  # DI container, DB init, startup
│       └── appsettings.json                       # Connection strings + logging config
│
└── tests/
    └── WorkOrderManagement.Tests/               # 28 xUnit tests
        ├── HighPriorityRuleTests.cs               # Business Rule #1 (4 tests)
        ├── OverdueCompletionTests.cs              # Business Rule #2 (5 tests)
        ├── WorkOrderFilteringTests.cs             # Filtering logic (5 tests)
        ├── TechnicianAssignmentTests.cs           # Assignment + deletion (5 tests)
        └── ValidationTests.cs                    # Validation rules (9 tests)
```

---

## Database Setup

### Schema (Code-First — EF Core Migrations)

The database schema is managed entirely through **EF Core Code-First Migrations** located in:

```
src/WorkOrderManagement.Infrastructure/Migrations/
├── 20260825060731_InitialCreate.cs           # Creates Technicians + WorkOrders tables
├── 20260825060731_InitialCreate.Designer.cs  # Migration metadata
└── WorkOrderDbContextModelSnapshot.cs        # Current model snapshot
```

### Tables

| Table | Column | Type | Constraints |
|---|---|---|---|
| **Technicians** | `Id` | INT | PK, Identity/Auto-increment |
| | `FullName` | NVARCHAR(100) | NOT NULL |
| | `Specialty` | NVARCHAR(100) | Nullable |
| **WorkOrders** | `Id` | INT | PK, Identity/Auto-increment |
| | `Title` | NVARCHAR(200) | NOT NULL |
| | `Description` | NVARCHAR(2000) | Nullable |
| | `Priority` | INT | NOT NULL — 0=Low, 1=Medium, 2=High |
| | `Status` | INT | NOT NULL — 0=Open, 1=InProgress, 2=Completed |
| | `DateLogged` | DATETIME2 | NOT NULL |
| | `TargetCompletionDate` | DATETIME2 | NOT NULL |
| | `AssignedTechnicianId` | INT | Nullable FK → Technicians(Id), ON DELETE RESTRICT |

**Index**: `IX_WorkOrders_AssignedTechnicianId` on `WorkOrders.AssignedTechnicianId`

### Connection String Configuration

The connection string is configured in [`appsettings.json`](src/WorkOrderManagement.App/appsettings.json):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WorkOrderManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;",
    "SqliteFallback": "Data Source=workorders.db"
  }
}
```

**To change the database target**, edit the `DefaultConnection` value:

| Scenario | Connection String |
|---|---|
| **SQL Server (localhost, Windows Auth)** | `Server=localhost;Database=WorkOrderManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;` |
| **SQL Server (named instance)** | `Server=localhost\SQLEXPRESS;Database=WorkOrderManagementDb;Trusted_Connection=True;TrustServerCertificate=True;` |
| **SQL Server (username/password)** | `Server=localhost;Database=WorkOrderManagementDb;User Id=sa;Password=YourPass;TrustServerCertificate=True;` |
| **SQLite (embedded file)** | `Data Source=workorders.db` |

The application **automatically detects** whether the connection string targets SQL Server or SQLite and configures the correct EF Core provider at startup.

### Automatic Database Initialization

**No manual database setup is required.** On application startup (`App.xaml.cs`):

1. **SQL Server** → `EnsureCreatedAsync()` creates the database and all tables if they don't exist.
2. **SQLite** → `MigrateAsync()` applies pending Code-First migrations.
3. **Seed data** → If the `Technicians` table is empty, `DatabaseSeeder` inserts 4 technicians and 8 work orders (including deliberately overdue items for testing Business Rule #2).

### Prerequisites for SQL Server

1. **SQL Server** must be installed and running on `localhost` (the default instance).
   - SQL Server 2017 or later is recommended.
   - Windows Authentication must be enabled.
2. Verify the server is accessible:
   ```bash
   sqlcmd -S localhost -E -Q "SELECT @@VERSION"
   ```
3. The application will create the `WorkOrderManagementDb` database automatically — no need to create it manually.

---

## Setup & Run

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10 or 11
- SQL Server (localhost) — or use SQLite by changing the connection string

### Steps

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd WorkOrderManagement
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Configure the database** *(optional — defaults work out of the box for SQL Server on localhost)*

   Edit `src/WorkOrderManagement.App/appsettings.json` if your SQL Server instance differs from `localhost`.

5. **Run the application**
   ```bash
   dotnet run --project src/WorkOrderManagement.App
   ```
   Or open `WorkOrderManagement.slnx` in **Visual Studio 2022** and press **F5**.

The database, tables, and demo data are created automatically on first launch.

---

## Features

### Work Orders
- ✅ View all work orders in a sortable, filterable DataGrid
- ✅ Add / Edit / Delete work orders with confirmation dialogs
- ✅ Assign or unassign technicians via dropdown
- ✅ Change priority (Low / Medium / High) and status (Open / In Progress / Completed)
- ✅ Visual colour-coded priority and status badges

### Technicians
- ✅ View all technicians with specialty
- ✅ Add / Edit / Delete technicians
- ✅ Deletion blocked when technician has assigned work orders (with user-friendly message)

### Dashboard
- ✅ Live summary cards: Total, Open, In Progress, Completed, High Priority, Overdue

### Filtering
- ✅ Filter by Status (All / Open / In Progress / Completed)
- ✅ Filter by Priority (All / Low / Medium / High)
- ✅ Combined status + priority filtering
- ✅ Clear Filters button to reset

### UX
- ✅ Loading indicators during async operations
- ✅ Empty state messages when no data
- ✅ Confirmation dialogs for destructive actions
- ✅ Inline validation error messages
- ✅ Dark, modern UI theme

---

## Business Rules

### Rule #1 — High Priority Date Enforcement

> **When a work order is created (or updated) with Priority = High, `TargetCompletionDate` is automatically set to `today + 1 day`.**

- Applied in `WorkOrderService.ApplyHighPriorityRule()` — enforced in the service layer, not the UI.
- The date picker is disabled when High priority is selected in the dialog.
- A visual warning note explains this behaviour to the user.
- The rule also fires on Update to prevent bypassing via create-then-edit.

### Rule #2 — Overdue Completion Warning

> **When marking a work order as Completed and the `TargetCompletionDate` is in the past, a confirmation dialog is shown.**

- `WorkOrderService.IsOverdue(workOrder)` returns `true` when `TargetCompletionDate.Date < today`.
- The ViewModel checks overdue status before saving when Status = Completed.
- **No** → save is cancelled, status remains unchanged.
- **Yes** → work order is completed normally.
- Overdue definition: `TargetCompletionDate.Date < DateTime.Today` (same-day completion is not overdue).

---

## Running Tests

```bash
dotnet test
```

**Test results: 28 tests — 0 failed, 28 passed**

| Test Class | Tests | Coverage Area |
|---|---|---|
| `HighPriorityRuleTests` | 4 | High priority date rule (create + update) |
| `OverdueCompletionTests` | 5 | Overdue detection, Yes/No confirmation flow |
| `WorkOrderFilteringTests` | 5 | Status, priority, combined, and no-filter |
| `TechnicianAssignmentTests` | 5 | Assignment, invalid technician, deletion conflict |
| `ValidationTests` | 9 | Missing fields, max lengths, trimming |

---

## Key Design Decisions Summary

| Decision | Rationale |
|---|---|
| **WPF + MVVM** | Rich native Windows UI; MVVM enables testable ViewModels without UI dependencies |
| **4-Project Clean Architecture** | Enforces Dependency Inversion; business rules are framework-agnostic |
| **Repository Pattern** | Abstracts EF Core behind interfaces; enables mocking in tests |
| **EF Core Code-First Migrations** | Schema is version-controlled; auto-applied on startup |
| **SQL Server (primary) + SQLite (fallback)** | Enterprise-grade primary DB; portable fallback for easy setup |
| **Auto provider detection** | Connection string analysis auto-selects SQL Server vs SQLite |
| **`IDateTimeProvider`** | Makes time-dependent logic deterministic in unit tests |
| **High-priority rule on Update** | Prevents bypass via create-then-edit; documented intentional choice |
| **`DeleteBehavior.Restrict`** | Prevents accidental cascade-delete of work orders when removing technicians |
| **Server-side filtering** | `IQueryable` filtering pushes WHERE clauses to SQL — no in-memory filtering |
| **No CQRS / MediatR** | Assignment explicitly discourages over-engineering; simple service → repository layering |
| **Scoped services + Singleton VMs** | Standard DI lifetime pattern for WPF apps using the generic host |

---

## Database Migration Commands (for developers)

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project src/WorkOrderManagement.Infrastructure \
  --startup-project src/WorkOrderManagement.Infrastructure

# Apply migrations
dotnet ef database update \
  --project src/WorkOrderManagement.Infrastructure \
  --startup-project src/WorkOrderManagement.Infrastructure

# Revert last migration
dotnet ef migrations remove \
  --project src/WorkOrderManagement.Infrastructure \
  --startup-project src/WorkOrderManagement.Infrastructure
```
