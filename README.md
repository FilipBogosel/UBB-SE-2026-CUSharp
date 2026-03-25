# MovieApp

MovieApp is a .NET 8 WPF desktop application built with MVVM, Entity Framework Core 8, SQL Server LocalDB, and xUnit/Moq tests. It implements a movie catalog, review workflow with extended reviews, weekly battle betting, badge/points gamification, mocked external critic data, and nested movie discussions.

## Solution Structure

- `MovieApp.UI`
  WPF desktop frontend, XAML views, MVVM view models, commands, and dependency injection bootstrapping.
- `MovieApp.Core`
  Domain models, EF Core `DbContext`, services, interfaces, seeding, and migrations.
- `MovieApp.Tests`
  xUnit and Moq test suite for the main service layer.

## Tech Stack

- .NET 8 / `net8.0-windows`
- WPF
- Entity Framework Core 8
- SQL Server LocalDB
- Microsoft.Extensions.DependencyInjection
- xUnit
- Moq

## Prerequisites

- Windows
- .NET 8 SDK or newer
- SQL Server LocalDB

## Database

The application uses this SQL Server LocalDB connection string:

```text
Server=(localdb)\mssqllocaldb;Database=MovieAppDb;Trusted_Connection=True;
```

The UI project applies migrations automatically on startup and then runs the database seeder.

## Seed Data

On first startup, the application seeds:

- 10 movies
- 3 users
- 6 badges based on the named badge rules in the requirements
- sample reviews
- sample comments
- sample user stats
- one active weekly battle

Hard-coded logged-in user:

```text
UserId = 1
```

## How To Run

1. Restore packages:

```powershell
dotnet restore MovieApp.sln
```

2. Build the solution:

```powershell
dotnet build MovieApp.sln
```

3. Run the WPF app:

```powershell
dotnet run --project .\MovieApp.UI\MovieApp.UI.csproj
```

The first launch will create/update the database and seed initial content automatically.

## How To Test

Run the test suite with:

```powershell
dotnet test .\MovieApp.Tests\MovieApp.Tests.csproj
```

## Migration

The initial EF Core migration has already been generated:

- `InitialCreate`

Migration files live under:

- `MovieApp.Core\Migrations`

## Implemented Features

### Catalog

- Browse all movies
- Live title search
- Genre filter
- Minimum rating slider filter
- Movie selection synchronized with detail/forum views

### Movie Details and Reviews

- Full movie details with poster, year, genre, and average rating
- Standard review submission
- Duplicate review prevention
- Extended review workflow with category ratings and texts
- External critic score mock integration
- Lexicon frequency analysis
- Polarization indicator

### Battle Arena

- Active weekly battle display
- Bet placement UI
- Available points display
- Battle validation rules and payout logic in service layer

### Forum

- Root comments per movie
- Nested replies
- Reverse chronological loading
- Inline reply editor

### Gamification

- Point accumulation and freezing/refund support
- Weekly score recalculation service
- Badge evaluation and awarding

## Notes On Source Inconsistencies

The provided text and diagrams were not perfectly aligned, so the implementation follows the explicit behavioral requirements first:

- The badge list names six badges even though one line mentions five badge types. The app seeds and evaluates all six named badges.
- The extra-review workflow requires a separate 500 to 12000 character extended text, but the listed ER fields did not include a dedicated column for it. A persisted `ExtraReviewContent` field was added to make that requirement representable.
- The category rating rules mention 0.5 increments, so category ratings are stored as floating-point values in the app model and service layer.

## Build Verification

The solution was validated with:

```powershell
dotnet build MovieApp.sln -c Debug
dotnet test .\MovieApp.Tests\MovieApp.Tests.csproj -c Debug --no-build
```
