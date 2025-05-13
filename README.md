# Netflix Clone Project: Developer TODO Checklist

## Iteration 0: Project Setup & Shared Core

### Chunk 0.1: Solution & Project Structure
- [ ] Create .NET Solution `NetflixClone`
- [ ] Create `NetflixClone.Shared` Class Library project (.NET 9.0)
- [ ] Create `NetflixClone.FCS` ASP.NET Core Web API project (Minimal APIs, .NET 9.0)
- [ ] Create `NetflixClone.UAS` ASP.NET Core Web API project (Minimal APIs, .NET 9.0)
- [ ] Create `NetflixClone.APIGateway` ASP.NET Core Empty project (.NET 9.0)

### Chunk 0.2: Shared Library - Result Pattern
- [ ] In `NetflixClone.Shared`: Implement non-generic `Result` class (`IsSuccess`, `Error`, static factories)
- [ ] In `NetflixClone.Shared`: Implement generic `Result<TValue>` class (inherits `Result`, `Value` property, static factories)

### Chunk 0.3: Shared Library - Custom Exception Handling Middleware
- [ ] In `NetflixClone.Shared`: Implement `GlobalExceptionHandlerMiddleware` (catches exceptions, logs, returns RFC 7807 `ProblemDetails`)
- [ ] In `NetflixClone.Shared`: Create `UseGlobalExceptionHandler` extension method for `IApplicationBuilder`

### Chunk 0.4: Shared Library - MediatR & Basic Logging Behavior
- [ ] Add `MediatR` NuGet package to `NetflixClone.Shared`
- [ ] Add `Serilog` NuGet package to `NetflixClone.Shared`
- [ ] In `NetflixClone.Shared`: Implement `LoggingBehavior<TRequest, TResponse>` for MediatR (logs start/end of request handling)

### Chunk 0.5: Basic Service Setup (FCS & UAS)
- [ ] Add `NetflixClone.Shared` as a project reference to `NetflixClone.FCS.csproj`
- [ ] Add `NetflixClone.Shared` as a project reference to `NetflixClone.UAS.csproj`
- [ ] Add `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `MediatR` (e.g. `MediatR.Extensions.Microsoft.DependencyInjection`) NuGet packages to `NetflixClone.FCS`
- [ ] Add `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `MediatR` NuGet packages to `NetflixClone.UAS`
- [ ] **FCS `Program.cs`**:
    - [ ] Configure Serilog as the logging provider (bootstrap and host integration)
    - [ ] Register MediatR services and open generic `LoggingBehavior`
    - [ ] Register `GlobalExceptionHandlerMiddleware`
    - [ ] Add `app.UseSerilogRequestLogging()`
    - [ ] Add a test GET `/ping` endpoint
- [ ] Create `appsettings.json` for FCS with basic Serilog configuration
- [ ] **UAS `Program.cs`**:
    - [ ] Configure Serilog as the logging provider (bootstrap and host integration)
    - [ ] Register MediatR services and open generic `LoggingBehavior`
    - [ ] Register `GlobalExceptionHandlerMiddleware`
    - [ ] Add `app.UseSerilogRequestLogging()`
    - [ ] Add a test GET `/ping` endpoint
- [ ] Create `appsettings.json` for UAS with basic Serilog configuration

## Iteration 1: FCS - Database & Genre Management (No API yet)

### Chunk 1.1: FCS - Database Setup (PostgreSQL & DbUp)
- [ ] Add `Dapper`, `Npgsql`, `dbup-postgresql` NuGet packages to `NetflixClone.FCS`
- [ ] In FCS: Create folder `Database/Migrations`
- [ ] In FCS: Create `Database/Migrations/0001_CreateGenresTable.sql` (ID SERIAL PK, Name VARCHAR UNIQUE)
- [ ] In FCS `Program.cs`: Configure DbUp to run migrations on startup (ensure DB exists, log to console)
- [ ] In FCS `appsettings.Development.json`: Add `FCS_DB` PostgreSQL connection string

### Chunk 1.2: FCS - Genre Entity & Stored Procedures
- [ ] In FCS: Create `Entities/Genre.cs` (record `Genre(int Id, string Name)`)
- [ ] In FCS: Create `Database/Migrations/0002_GenreSPs.sql`:
    - [ ] `sp_CreateGenre(p_Name VARCHAR)` - returns created Genre (ID, Name)
    - [ ] `sp_GetGenres()` - returns all Genres (ID, Name) ordered by Name

### Chunk 1.3: FCS - Dapper Context (Shared)
- [ ] Add `Npgsql` NuGet package to `NetflixClone.Shared` (if not already transitively included and used directly)
- [ ] In `NetflixClone.Shared`: Create `Data/DapperContext.cs` (constructor takes `IConfiguration`, `connectionStringName`; `CreateConnection()` method returns `NpgsqlConnection`)
- [ ] In FCS `Program.cs`: Register `DapperContext` for DI (singleton, using "FCS_DB" connection string name)

### Chunk 1.4: FCS - MediatR for Genres (Internal Logic)
- [ ] In FCS: Create `Features/Admin/Genres/Commands/CreateGenre/CreateGenreCommand.cs` (record `(string Name)`, `IRequest<Result<Genre>>`)
- [ ] In FCS: Create `Features/Admin/Genres/Commands/CreateGenre/CreateGenreCommandHandler.cs` (inject `DapperContext`, call `sp_CreateGenre`, return `Result<Genre>`)
- [ ] In FCS: Create `Features/Admin/Genres/Queries/GetGenres/GetGenresQuery.cs` (record `{}`, `IRequest<Result<IEnumerable<Genre>>>`)
- [ ] In FCS: Create `Features/Admin/Genres/Queries/GetGenres/GetGenresQueryHandler.cs` (inject `DapperContext`, call `sp_GetGenres`, return `Result<IEnumerable<Genre>>`)

## Iteration 2: FCS - Film Entity & Core CRUD (No API yet)

### Chunk 2.1: FCS - Films Table & Entity
- [ ] In FCS: Create `Database/Migrations/0003_CreateFilmsTable.sql`:
    - [ ] `Films` table (ID BIGSERIAL PK, Name TEXT, ReleaseDate DATE, DirectorName TEXT, PictureURL TEXT, TrailerURL TEXT, GenreIDs INTEGER[], CreatedAt TIMESTAMPTZ, IsDeleted BOOLEAN, DeletedAt TIMESTAMPTZ)
    - [ ] Index on `IsDeleted`
    - [ ] (Optional) GIN Index on `GenreIDs`
- [ ] In FCS: Create `Entities/Film.cs` (record `Film` with all corresponding properties)

### Chunk 2.2: FCS - Film CRUD Stored Procedures
- [ ] In FCS: Create `Database/Migrations/0004_FilmCRUD_SPs.sql`:
    - [ ] `sp_CreateFilm` (all properties except ID, CreatedAt, IsDeleted, DeletedAt) - returns created Film
    - [ ] `sp_GetFilmById(p_Id BIGINT)` - returns Film where IsDeleted = FALSE
    - [ ] `sp_UpdateFilm(p_Id BIGINT`, all updatable properties) - returns updated Film
    - [ ] `sp_SoftDeleteFilm(p_Id BIGINT)` - sets IsDeleted=TRUE, DeletedAt=NOW(), returns deleted FilmID

### Chunk 2.3: FCS - MediatR for Film CRUD (Internal Logic)
- [ ] In FCS: Create `Features/Admin/Films/Commands/CreateFilm/CreateFilmCommand.cs` (record with film properties, `IRequest<Result<Film>>`)
- [ ] In FCS: Create `Features/Admin/Films/Commands/CreateFilm/CreateFilmCommandHandler.cs` (inject `DapperContext`, call `sp_CreateFilm`)
- [ ] In FCS: Create `Features/Admin/Films/Queries/GetFilmById/GetFilmByIdQuery.cs` (record `(long FilmId)`, `IRequest<Result<Film>>`)
- [ ] In FCS: Create `Features/Admin/Films/Queries/GetFilmById/GetFilmByIdQueryHandler.cs` (inject `DapperContext`, call `sp_GetFilmById`)
- [ ] In FCS: Create `Features/Admin/Films/Commands/UpdateFilm/UpdateFilmCommand.cs` (record with FilmId & properties, `IRequest<Result<Film>>`)
- [ ] In FCS: Create `Features/Admin/Films/Commands/UpdateFilm/UpdateFilmCommandHandler.cs` (inject `DapperContext`, call `sp_UpdateFilm`)
- [ ] In FCS: Create `Features/Admin/Films/Commands/SoftDeleteFilm/SoftDeleteFilmCommand.cs` (record `(long FilmId)`, `IRequest<Result<long?>>`)
- [ ] In FCS: Create `Features/Admin/Films/Commands/SoftDeleteFilm/SoftDeleteFilmCommandHandler.cs` (inject `DapperContext`, call `sp_SoftDeleteFilm`)

## Iteration 3: FCS - Admin APIs

### Chunk 3.1: FCS - Setup Basic JWT Authentication (Self-Handled)
- [ ] Add JWT Bearer NuGet packages (e.g., `Microsoft.AspNetCore.Authentication.JwtBearer`) to FCS
- [ ] In FCS `appsettings.json`: Define JWT settings (Key, Issuer, Audience)
- [ ] In FCS `Program.cs`: Configure JWT Bearer authentication services (`AddAuthentication`, `AddJwtBearer`)
- [ ] In FCS `Program.cs`: Add `app.UseAuthentication()` and `app.UseAuthorization()`
- [ ] (Placeholder/Optional) Create a simple utility/endpoint in FCS to generate a test admin token for development.

### Chunk 3.2: FCS - Admin Film Upload API
- [ ] In FCS: Define `DTOs/Admin/UploadFilmRequest.cs` (Name, ReleaseDate, DirectorName, (string) PictureUrl, TrailerUrl, GenreIds)
- [ ] In FCS: Create Admin API endpoint `POST /api/v1/admin/films` in `Endpoints/AdminFilmEndpoints.cs` (or similar):
    - [ ] Use Minimal API `MapPost`
    - [ ] Inject `IMediator`
    - [ ] Map `UploadFilmRequest` to `CreateFilmCommand`
    - [ ] Send command, handle `Result`, return 201 Created with film or ID / 400 Bad Request
    - [ ] Apply `[Authorize(Roles = "Administrator")]` (or equivalent fluent `.RequireAuthorization(p => p.RequireRole("Administrator"))`)

### Chunk 3.3: FCS - Admin Film Edit API
- [ ] In FCS: Define `DTOs/Admin/EditFilmRequest.cs` (Name, ReleaseDate, DirectorName, PictureUrl, TrailerUrl, GenreIds)
- [ ] In FCS: Create Admin API endpoint `PUT /api/v1/admin/films/{filmId:long}`:
    - [ ] Map `filmId` and `EditFilmRequest` to `UpdateFilmCommand`
    - [ ] Send command, handle `Result`, return 200 OK or 204 No Content / 404 Not Found / 400
    - [ ] Apply `[Authorize(Roles = "Administrator")]`

### Chunk 3.4: FCS - Admin Film Soft Delete API
- [ ] In FCS: Create Admin API endpoint `DELETE /api/v1/admin/films/{filmId:long}`:
    - [ ] Map `filmId` to `SoftDeleteFilmCommand`
    - [ ] Send command, handle `Result`, return 204 No Content / 404 Not Found
    - [ ] Apply `[Authorize(Roles = "Administrator")]`

### Chunk 3.5: FCS - Admin Genre Management APIs
- [ ] In FCS: Define `DTOs/Admin/CreateGenreRequest.cs` (Name)
- [ ] In FCS: Create Admin API endpoint `POST /api/v1/admin/genres`:
    - [ ] Map `CreateGenreRequest` to `CreateGenreCommand`
    - [ ] Send command, handle `Result`, return 201 Created / 400
    - [ ] Apply `[Authorize(Roles = "Administrator")]`
- [ ] In FCS: Create Admin API endpoint `GET /api/v1/admin/genres`:
    - [ ] Create and send `GetGenresQuery`
    - [ ] Handle `Result`, return 200 OK with list of genres
    - [ ] Apply `[Authorize(Roles = "Administrator")]`

## Iteration 4: FCS - User-Facing APIs

### Chunk 4.1: FCS - Film Discovery Stored Procedures
- [ ] In FCS: Create `Database/Migrations/0005_FilmDiscovery_SPs.sql`:
    - [ ] `sp_GetLatestFilmsByGenre(p_GenreId INT, p_Count INT)` - returns `p_Count` latest non-deleted films by `CreatedAt DESC`.
    - [ ] `sp_GetFilmsByGenrePaginated(p_GenreId INT, p_PageNumber INT, p_PageSize INT)` - returns paginated non-deleted films for genre, ordered by `CreatedAt DESC`. Include total count for pagination.
    - [ ] `sp_SearchFilmsByNamePaginated(p_Query TEXT, p_PageNumber INT, p_PageSize INT)` - returns paginated non-deleted films matching name (e.g., `ILIKE '%query%'` or FTS), ordered by relevance or `CreatedAt DESC`. Include total count.

### Chunk 4.2: FCS - MediatR for Film Discovery
- [ ] In FCS: Define `DTOs/PaginatedResult.cs` record `PaginatedResult<T>(IEnumerable<T> Items, int PageNumber, int PageSize, int TotalCount)`
- [ ] In FCS `Features/Public/Films/Queries/GetLatestFilmsByGenre`:
    - [ ] `GetLatestFilmsByGenreQuery.cs` (record `(int GenreId, int Count)`, `IRequest<Result<IEnumerable<Film>>>`)
    - [ ] `GetLatestFilmsByGenreQueryHandler.cs` (call `sp_GetLatestFilmsByGenre`)
- [ ] In FCS `Features/Public/Films/Queries/GetFilmsByGenrePaginated`:
    - [ ] `GetFilmsByGenrePaginatedQuery.cs` (record `(int GenreId, int PageNumber, int PageSize)`, `IRequest<Result<PaginatedResult<Film>>>`)
    - [ ] `GetFilmsByGenrePaginatedQueryHandler.cs` (call `sp_GetFilmsByGenrePaginated`, map to `PaginatedResult`)
- [ ] In FCS `Features/Public/Films/Queries/SearchFilmsByNamePaginated`:
    - [ ] `SearchFilmsByNamePaginatedQuery.cs` (record `(string Query, int PageNumber, int PageSize)`, `IRequest<Result<PaginatedResult<Film>>>`)
    - [ ] `SearchFilmsByNamePaginatedQueryHandler.cs` (call `sp_SearchFilmsByNamePaginated`, map to `PaginatedResult`)
- [ ] (Re-use `GetFilmByIdQuery` from Admin features for public access)

### Chunk 4.3: FCS - User-Facing Film Discovery APIs
- [ ] In FCS `Endpoints/PublicFilmEndpoints.cs` (or similar):
    - [ ] `GET /api/v1/genres/{genreId:int}/films/latest` (query param `count`, default 10):
        - [ ] Use `GetLatestFilmsByGenreQuery`. Allow anonymous or `[Authorize]`.
    - [ ] `GET /api/v1/genres/{genreId:int}/films` (query params `pageNumber`, `pageSize`):
        - [ ] Use `GetFilmsByGenrePaginatedQuery`. Allow anonymous or `[Authorize]`.
    - [ ] `GET /api/v1/films/search` (query params `name`, `pageNumber`, `pageSize`):
        - [ ] Use `SearchFilmsByNamePaginatedQuery`. Allow anonymous or `[Authorize]`.
    - [ ] `GET /api/v1/films/{filmId:long}`:
        - [ ] Use `GetFilmByIdQuery`. Allow anonymous or `[Authorize]`.

## Iteration 5: User Activity Service (UAS) - Core

### Chunk 5.1: UAS - Database Setup (PostgreSQL & DbUp)
- [ ] Add `Dapper`, `Npgsql`, `dbup-postgresql` NuGet packages to `NetflixClone.UAS`
- [ ] In UAS: Create folder `Database/Migrations`
- [ ] In UAS: Create `Database/Migrations/0001_CreateUserActivityTables.sql`:
    - [ ] `UserFavourites` (UserID TEXT, FilmID BIGINT, AddedDate TIMESTAMPTZ, FilmName_Replicated TEXT, FilmPictureURL_Replicated TEXT, PK(UserID, FilmID))
    - [ ] `UserWatchList` (UserID TEXT, FilmID BIGINT, AddedDate TIMESTAMPTZ, FilmName_Replicated TEXT, FilmPictureURL_Replicated TEXT, PK(UserID, FilmID))
    - [ ] (Note: UserID type based on JWT `sub` claim, TEXT is flexible)
- [ ] In UAS `Program.cs`: Configure DbUp to run migrations on startup
- [ ] In UAS `appsettings.Development.json`: Add `UAS_DB` PostgreSQL connection string

### Chunk 5.2: UAS - Entities
- [ ] In UAS: Create `Entities/UserFavourite.cs` (record with table columns)
- [ ] In UAS: Create `Entities/UserWatchListItem.cs` (record with table columns)

### Chunk 5.3: UAS - Dapper Context Registration
- [ ] In UAS `Program.cs`: Register `DapperContext` (from Shared) for DI (singleton, using "UAS_DB" connection string name)

### Chunk 5.4: UAS - Stored Procedures for Lists
- [ ] In UAS: Create `Database/Migrations/0002_UserActivity_SPs.sql`:
    - [ ] `sp_AddFavourite(p_UserID TEXT, p_FilmID BIGINT, p_FilmName TEXT, p_FilmPictureURL TEXT, p_AddedDate TIMESTAMPTZ)`
    - [ ] `sp_RemoveFavourite(p_UserID TEXT, p_FilmID BIGINT)`
    - [ ] `sp_GetFavourites(p_UserID TEXT)` - returns `UserFavourite` items, ordered by AddedDate DESC
    - [ ] `sp_AddWatchListItem(p_UserID TEXT, p_FilmID BIGINT, p_FilmName TEXT, p_FilmPictureURL TEXT, p_AddedDate TIMESTAMPTZ)`
    - [ ] `sp_RemoveWatchListItem(p_UserID TEXT, p_FilmID BIGINT)`
    - [ ] `sp_GetWatchList(p_UserID TEXT)` - returns `UserWatchListItem` items, ordered by AddedDate DESC

### Chunk 5.5: UAS - MediatR for List Management (Internal Logic)
- [ ] In UAS `Features/Favourites/Commands/AddFavourite`:
    - [ ] `AddFavouriteCommand.cs` (record `(string UserId, long FilmId, string FilmName, string FilmPictureUrl)`, `IRequest<Result<UserFavourite>>`)
    - [ ] `AddFavouriteCommandHandler.cs` (call `sp_AddFavourite`)
- [ ] In UAS `Features/Favourites/Commands/RemoveFavourite`:
    - [ ] `RemoveFavouriteCommand.cs` (record `(string UserId, long FilmId)`, `IRequest<Result<bool>>`)
    - [ ] `RemoveFavouriteCommandHandler.cs` (call `sp_RemoveFavourite`)
- [ ] In UAS `Features/Favourites/Queries/GetFavourites`:
    - [ ] `GetFavouritesQuery.cs` (record `(string UserId)`, `IRequest<Result<IEnumerable<UserFavourite>>>`)
    - [ ] `GetFavouritesQueryHandler.cs` (call `sp_GetFavourites`)
- [ ] In UAS `Features/WatchList/Commands/AddWatchListItem`:
    - [ ] `AddWatchListItemCommand.cs` (record `(string UserId, long FilmId, string FilmName, string FilmPictureUrl)`, `IRequest<Result<UserWatchListItem>>`)
    - [ ] `AddWatchListItemCommandHandler.cs` (call `sp_AddWatchListItem`)
- [ ] In UAS `Features/WatchList/Commands/RemoveWatchListItem`:
    - [ ] `RemoveWatchListItemCommand.cs` (record `(string UserId, long FilmId)`, `IRequest<Result<bool>>`)
    - [ ] `RemoveWatchListItemCommandHandler.cs` (call `sp_RemoveWatchListItem`)
- [ ] In UAS `Features/WatchList/Queries/GetWatchList`:
    - [ ] `GetWatchListQuery.cs` (record `(string UserId)`, `IRequest<Result<IEnumerable<UserWatchListItem>>>`)
    - [ ] `GetWatchListQueryHandler.cs` (call `sp_GetWatchList`)

## Iteration 6: UAS - User APIs

### Chunk 6.1: UAS - Setup Basic JWT Authentication
- [ ] Add JWT Bearer NuGet packages to UAS
- [ ] In UAS `appsettings.json`: Define JWT settings (Key, Issuer, Audience - ideally same as FCS)
- [ ] In UAS `Program.cs`: Configure JWT Bearer authentication services
- [ ] In UAS `Program.cs`: Add `app.UseAuthentication()` and `app.UseAuthorization()`

### Chunk 6.2: UAS - Favourites List APIs
- [ ] In UAS: Define `DTOs/AddToListRequest.cs` (record `(long FilmId, string FilmName, string FilmPictureUrl)`) - FilmName/PictureURL are for replication.
- [ ] In UAS `Endpoints/FavouritesEndpoints.cs` (or similar):
    - [ ] `POST /api/v1/me/favourites`:
        - [ ] Requires `[Authorize]`. Extract UserID from `HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value`.
        - [ ] Map `AddToListRequest` to `AddFavouriteCommand` (with UserID).
        - [ ] Send command, handle Result (201 Created/204 No Content or 400).
    - [ ] `DELETE /api/v1/me/favourites/{filmId:long}`:
        - [ ] Requires `[Authorize]`. Extract UserID.
        - [ ] Map to `RemoveFavouriteCommand`. Handle Result (204 No Content or 404).
    - [ ] `GET /api/v1/me/favourites`:
        - [ ] Requires `[Authorize]`. Extract UserID.
        - [ ] Use `GetFavouritesQuery`. Handle Result (200 OK).

### Chunk 6.3: UAS - Watch List APIs
- [ ] (Reuse `DTOs/AddToListRequest.cs` if applicable)
- [ ] In UAS `Endpoints/WatchListEndpoints.cs` (or similar):
    - [ ] `POST /api/v1/me/watchlist`:
        - [ ] Similar to AddFavourite; map to `AddWatchListItemCommand`.
    - [ ] `DELETE /api/v1/me/watchlist/{filmId:long}`:
        - [ ] Similar to RemoveFavourite; map to `RemoveWatchListItemCommand`.
    - [ ] `GET /api/v1/me/watchlist`:
        - [ ] Similar to GetFavourites; use `GetWatchListQuery`.

## Iteration 7: Inter-Service Communication (RabbitMQ)

### Chunk 7.1: Shared Library - RabbitMQ Configuration & Basic Publisher/Consumer
- [ ] Add `RabbitMQ.Client` NuGet package to `NetflixClone.Shared`.
- [ ] In `NetflixClone.Shared`: Create `Messaging/RabbitMqConnectionFactory.cs` (helper to create `IConnection`).
- [ ] In `NetflixClone.Shared`: Define `Messaging/IMessagePublisher.cs` interface.
- [ ] In `NetflixClone.Shared`: Implement `Messaging/RabbitMqMessagePublisher.cs`.
- [ ] In `NetflixClone.Shared`: Define a base class or helper for RabbitMQ consumers (e.g., `BackgroundService` based, handling connection, channel, queue declaration, message deserialization).
- [ ] Add RabbitMQ connection settings (Hostname, Username, Password, Port) to `appsettings.json` for FCS and UAS.

### Chunk 7.2: FCS - Event Definitions & Publishing
- [ ] In `NetflixClone.Shared`: Create `Events/FilmSoftDeletedEvent.cs` (record `(long FilmId)`).
- [ ] In `NetflixClone.Shared`: Create `Events/FilmDetailsUpdatedEvent.cs` (record `(long FilmId, string? Name, string? PictureUrl)`).
- [ ] In FCS `Program.cs`: Register `RabbitMqConnectionFactory` and `IMessagePublisher` (as singleton).
- [ ] In FCS `SoftDeleteFilmCommandHandler`: After successful DB op, inject `IMessagePublisher` and publish `FilmSoftDeletedEvent`.
- [ ] In FCS `UpdateFilmCommandHandler`: After successful DB op, inject `IMessagePublisher` and publish `FilmDetailsUpdatedEvent`.

### Chunk 7.3: UAS - Event Consumers
- [ ] In UAS: Create `Database/Migrations/0003_UAS_EventHandling_SPs.sql`:
    - [ ] `sp_RemoveFilmFromAllUserLists(p_FilmID BIGINT)` (deletes from `UserFavourites` and `UserWatchList` for the given `p_FilmID`).
    - [ ] `sp_UpdateReplicatedFilmDetailsInUserLists(p_FilmID BIGINT, p_FilmName TEXT, p_FilmPictureURL TEXT)` (updates replicated data).
- [ ] In UAS: Create `Messaging/Consumers/FilmSoftDeletedEventConsumer.cs`:
    - [ ] Inherits from shared base consumer or implements `IHostedService`.
    - [ ] Subscribes to `FilmSoftDeletedEvent` queue.
    - [ ] Deserializes message, injects `DapperContext`, calls `sp_RemoveFilmFromAllUserLists`.
- [ ] In UAS: Create `Messaging/Consumers/FilmDetailsUpdatedEventConsumer.cs`:
    - [ ] Subscribes to `FilmDetailsUpdatedEvent` queue.
    - [ ] Deserializes message, injects `DapperContext`, calls `sp_UpdateReplicatedFilmDetailsInUserLists`.
- [ ] In UAS `Program.cs`: Register `RabbitMqConnectionFactory`.
- [ ] In UAS `Program.cs`: Register event consumers as Hosted Services (`AddHostedService`).

## Iteration 8: API Gateway (YARP)

### Chunk 8.1: YARP Basic Setup
- [ ] Add `Yarp.ReverseProxy` NuGet package to `NetflixClone.APIGateway`.
- [ ] In `NetflixClone.APIGateway/Program.cs`:
    - [ ] `builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));`
    - [ ] `app.MapReverseProxy();`

### Chunk 8.2: YARP Configuration
- [ ] In `NetflixClone.APIGateway/appsettings.json`:
    - [ ] Define `ReverseProxy` configuration with `Routes` and `Clusters`.
    - [ ] Route for FCS (e.g., path `/api/fcs/{**catchall}` to `fcs-cluster`).
    - [ ] Route for UAS (e.g., path `/api/uas/{**catchall}` to `uas-cluster`).
    - [ ] Define `fcs-cluster` and `uas-cluster` pointing to local addresses of FCS and UAS (e.g., `http://localhost:FCS_PORT`, `http://localhost:UAS_PORT`).
    - [ ] (Future) Refine routes for cleaner public URLs (e.g., `/api/films/*` maps to FCS, `/api/me/*` maps to UAS).
- [ ] Ensure FCS and UAS `launchSettings.json` (or Docker config) define specific ports.

## Iteration 9: Cross-Cutting Concerns - Polish

### Chunk 9.1: Centralized Logging with SEQ
- [ ] Add `Serilog.Sinks.Seq` NuGet package to `NetflixClone.Shared` (or FCS & UAS directly).
- [ ] Modify Serilog configuration in FCS `Program.cs` and UAS `Program.cs` to include the `.WriteTo.Seq("http://localhost:SEQ_PORT_HOST")` sink.
- [ ] Add Seq URL to `appsettings.json` for FCS and UAS.
- [ ] Verify `LoggingBehavior` in `NetflixClone.Shared` provides good structured logs.
- [ ] Ensure Correlation IDs (e.g., from `HttpContext.TraceIdentifier`) are logged.

### Chunk 9.2: MediatR Pipeline Behaviors - Validation (Optional, if implemented)
- [ ] If `FluentValidation` is used:
    - [ ] Add `FluentValidation.AspNetCore` (or `FluentValidation.DependencyInjectionExtensions`) to `NetflixClone.Shared`.
    - [ ] Create `Behaviors/ValidationBehavior.cs` in `NetflixClone.Shared`.
    - [ ] Register `ValidationBehavior` in MediatR pipeline in FCS and UAS `Program.cs`.
    - [ ] Create sample validators for some commands (e.g., `CreateFilmCommandValidator`).
    - [ ] Ensure ProblemDetails from `GlobalExceptionHandlerMiddleware` includes validation errors.

### Chunk 9.3: Refined Error Handling
- [ ] Review `GlobalExceptionHandlerMiddleware` for any specific exception types to handle differently.
- [ ] Confirm consistent use of `Result` pattern in all MediatR handlers.
- [ ] Ensure API endpoints correctly map `Result` failures to appropriate HTTP status codes and ProblemDetails.

### Chunk 9.4: Configuration Management
- [ ] Review all configuration sources (`appsettings.json`, environment variables).
- [ ] Ensure sensitive data (keys, passwords) can be overridden by environment variables or user secrets for development.

## Iteration 10: Authentication & Authorization - Refinement

### Chunk 10.1: Standardize JWT Configuration
- [ ] (Optional) Create a shared configuration model/class for JWT settings in `NetflixClone.Shared`.
- [ ] Ensure JWT settings (Key, Issuer, Audience) are consistent between FCS, UAS, and any token generation utility.

### Chunk 10.2: Role-Based Authorization
- [ ] Define roles (e.g., "Administrator", "User") as constants, possibly in `NetflixClone.Shared`.
- [ ] Verify Admin endpoints in FCS are correctly protected (e.g., `RequireAuthorization(p => p.RequireRole("Administrator"))`).
- [ ] Verify User endpoints in UAS (and public FCS endpoints if auth is added) use `RequireAuthorization()` (which implies authenticated user).
- [ ] Ensure test token generation utility can create tokens with appropriate roles.

### Chunk 10.3: Test Token Generation Utility
- [ ] Implement a secure, local-only method (e.g., a separate console app, or a dev-only endpoint in one service) to generate JWTs with specifiable UserID and roles for testing.
- [ ] **CRITICAL**: Ensure this utility is NOT deployed to any non-development environment.

## Iteration 11: Containerization (Docker & Docker Compose)

### Chunk 11.1: Dockerfiles for Services
- [ ] Create `Dockerfile` for `NetflixClone.FCS` (multi-stage build, expose port).
- [ ] Create `Dockerfile` for `NetflixClone.UAS` (multi-stage build, expose port).
- [ ] Create `Dockerfile` for `NetflixClone.APIGateway` (multi-stage build, expose port).

### Chunk 11.2: Docker Compose for Local Development
- [ ] Create `docker-compose.yml` at the solution root:
    - [ ] Define services for `fcs-app`, `uas-app`, `apigateway-app`.
    - [ ] Define service for `fcs-db` (PostgreSQL image, volume for data, environment vars for user/pass/db).
    - [ ] Define service for `uas-db` (PostgreSQL image, volume for data, environment vars).
    - [ ] Define service for `rabbitmq` (RabbitMQ image, ports).
    - [ ] Define service for `seq` (Seq image, volume for data, ports, accept EULA env var).
    - [ ] (Later) Define service for `redis`.
    - [ ] Configure health checks, environment variables (connection strings pointing to other services in compose network), ports, and `depends_on`.

### Chunk 11.3: Database Initialization in Docker
- [ ] Ensure DbUp migrations run correctly when services start within Docker Compose (services depend on DBs being ready).
- [ ] Add `0000_InitialSeedData.sql` (optional) to FCS migrations to seed initial Genres (e.g., Action, Comedy, Drama).

## Iteration 12: Azure Integration - Film Pictures

### Chunk 12.1: Azure Blob Storage Setup
- [ ] Add `Azure.Storage.Blobs` NuGet package to `NetflixClone.FCS`.
- [ ] In FCS: Create `Services/AzureBlobStorageService.cs` implementing `IFileStorageService` (or similar interface):
    - [ ] Method: `Task<string> UploadFileAsync(string containerName, string blobName, Stream content, string contentType)` returns public URL.
- [ ] In FCS `appsettings.json`: Configure Azure Blob Storage connection string and default container name.
- [ ] In FCS `Program.cs`: Register `AzureBlobStorageService` for DI.

### Chunk 12.2: Modify Film Upload Logic in FCS
- [ ] Modify `DTOs/Admin/UploadFilmRequest.cs` in FCS to include `IFormFile? PictureFile { get; set; }` (and remove string PictureUrl).
- [ ] Modify `CreateFilmCommand` in FCS to accept `IFormFile? PictureFile`.
- [ ] Modify `CreateFilmCommandHandler` in FCS:
    - [ ] If `PictureFile` is present, upload using `AzureBlobStorageService`.
    - [ ] Store the returned URL in the `p_PictureURL` parameter for `sp_CreateFilm`.
- [ ] Modify Admin Film Upload API (`POST /api/v1/admin/films`) to handle `multipart/form-data` and bind `IFormFile`.
- [ ] (Optional) Modify `UpdateFilmCommand` and handler for picture updates.

## Iteration 13: Caching (Redis)

### Chunk 13.1: Redis Setup
- [ ] Add `Microsoft.Extensions.Caching.StackExchangeRedis` NuGet package to FCS and UAS.
- [ ] In FCS and UAS `appsettings.json`: Configure Redis connection string.
- [ ] In FCS and UAS `Program.cs`: Register Redis distributed caching (`services.AddStackExchangeRedisCache`).
- [ ] Add `redis` service to `docker-compose.yml` (Redis image, ports).

### Chunk 13.2: Caching in FCS
- [ ] Identify candidate MediatR query handlers in FCS (e.g., `GetFilmByIdQueryHandler`, `GetLatestFilmsByGenreQueryHandler`, etc.).
- [ ] In these handlers, inject `IDistributedCache`.
    - [ ] Implement cache check (get from cache).
    - [ ] If cache miss, fetch from DB, then store in cache with appropriate expiration.
- [ ] Implement cache invalidation in relevant command handlers (e.g., `UpdateFilmCommandHandler`, `SoftDeleteFilmCommandHandler` should invalidate cache for the affected film ID and potentially related genre/search caches).

### Chunk 13.3: Caching in UAS
- [ ] Identify candidate MediatR query handlers in UAS (e.g., `GetFavouritesQueryHandler`, `GetWatchListQueryHandler`).
- [ ] Implement caching logic similar to FCS using `IDistributedCache`.
- [ ] Implement cache invalidation in command handlers (add/remove items) and event consumers (film deleted/updated from FCS).

## Iteration 14: API Documentation & Final Polish

### Chunk 14.1: Swagger/OpenAPI
- [ ] Add `Swashbuckle.AspNetCore` NuGet package to FCS, UAS, and APIGateway.
- [ ] In FCS, UAS, APIGateway `Program.cs`: Configure `AddSwaggerGen` and `UseSwagger`/`UseSwaggerUI`.
    - [ ] For APIGateway, explore YARP's capabilities to aggregate downstream Swagger definitions if desired or serve its own.
- [ ] Add XML comments to DTOs and API endpoint methods for better Swagger documentation. Enable XML documentation file generation in `.csproj` files.
- [ ] Configure Swagger to use JWT Bearer token for authorizing requests in UI.

### Chunk 14.2: Code Review & Refinement
- [ ] Thoroughly review all code for consistency, best practices, error handling, and potential bugs.
- [ ] Refactor any complex or unclear sections.
- [ ] Ensure all `// TODO:` comments in the codebase are addressed.
- [ ] Verify logging provides sufficient detail for troubleshooting.

### Chunk 14.3: Testing Plan Review & Initial Implementation
- [ ] Review the testing plan from the original specification.
- [ ] Write a few sample unit tests for critical MediatR handlers in FCS and UAS (using Moq/NSubstitute).
- [ ] Write a few sample integration tests for key API endpoints in FCS and UAS (using `WebApplicationFactory` and Testcontainers for DBs if possible).

## Cross-Cutting Tasks (To be considered throughout all iterations)
- [ ] **Security**:
    - [ ] Consistent input validation (DTOs, command parameters).
    - [ ] Output encoding (ASP.NET Core handles much of this by default).
    - [ ] Ensure HTTPS is enforced in production environments (Kestrel default, Azure App Service default).
    - [ ] Review for any potential security vulnerabilities (OWASP Top 10).
- [ ] **CI/CD**: (Future, but keep in mind for easy setup)
    - [ ] Structure projects for easy build/test/deployment automation.
- [ ] **Testing**:
    - [ ] Incrementally add unit tests for new logic.
    - [ ] Incrementally add integration tests for new API endpoints and service interactions.