# ECommerce API

ASP.NET Core Web API for user accounts and a product catalog, organized as Clean Architecture.

## Features

- Registration and login with BCrypt password hashes. Login accepts a username or an email address.
- JWT access tokens and rotating refresh tokens stored in SQL Server.
- Logout that revokes the caller's active refresh tokens, and a current-user endpoint.
- Product list, get-by-id, and a paged query with keyword search, price range, and sort.
- Product create, update, and delete limited to the `Admin` role, with a seller-ownership check on update and delete.
- FluentValidation for product create and update requests.
- Exception-handling middleware that maps application exceptions to HTTP status codes.
- Swagger UI when the host environment is Development.
- Entity Framework Core migrations applied automatically on startup, plus an admin user seed.

## Tech stack

- .NET 10 (`net10.0`) and ASP.NET Core
- Entity Framework Core 10.0.5 with the SQL Server provider
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.6
- BCrypt.Net-Next 4.1.0
- AutoMapper 12.0.1
- FluentValidation 12.1.1 and FluentValidation.AspNetCore 11.3.1
- Swashbuckle.AspNetCore 6.5.0

The solution file is `ECommerce.slnx`.

## Architecture

Dependencies point inward: **Api → Infrastructure → Application → Domain**. Domain does not reference the other projects.

| Project | Role |
| --- | --- |
| `src/ECommerce.Domain` | Entities only: `User`, `Product`, and `RefreshToken`. |
| `src/ECommerce.Application` | Use cases and contracts: DTOs, FluentValidation validators, AutoMapper profiles, repository and service interfaces, `AuthService`, and `ProductService`. |
| `src/ECommerce.Infrastructure` | EF Core `AppDbContext`, migrations, repositories, `UnitOfWork`, JWT generation, the current-user accessor, and `DbSeeder`. `DependencyInjection.AddInfrastructure` registers those implementations and the application services. |
| `src/ECommerce.Api` | Host and HTTP layer: `Program.cs`, `AuthController`, `ProductsController`, and `ExceptionHandlingMiddleware`. |

`Program.cs` is the composition root. It calls `AddApplication()` (validators and AutoMapper) and `AddInfrastructure()`.

## Configuration and secrets

`src/ECommerce.Api/appsettings.json` keeps non-secret defaults. `JwtSettings:Key` and `AdminSeed:Password` are empty in that file. The process reads the real values from the standard ASP.NET Core configuration stack. Later sources replace earlier ones:

1. `appsettings.json`
2. `appsettings.{Environment}.json` (`appsettings.Development.json` is gitignored)
3. [User secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), only when the environment is Development
4. Environment variables
5. Command-line arguments

`dotnet run` uses `src/ECommerce.Api/Properties/launchSettings.json`, which sets `ASPNETCORE_ENVIRONMENT` to `Development`, so user secrets and `appsettings.Development.json` load and Swagger is enabled. User secrets are not loaded when the environment is Production. Use environment variables there.

Startup throws `InvalidOperationException` before the database is opened when:

- `JwtSettings:Key` is missing or shorter than 32 bytes (HMAC-SHA256)
- `JwtSettings:Issuer`, `JwtSettings:Audience`, or `JwtSettings:ExpiresInMinutes` is missing or invalid
- `ConnectionStrings:DefaultConnection` is missing

The admin password is required only when the database has no user with role `Admin`. Later startups leave the existing admin row unchanged. The password is hashed with BCrypt and is not written back to configuration.

| Setting | Environment variable | Purpose |
| --- | --- | --- |
| `JwtSettings:Key` | `JwtSettings__Key` | Signing key, at least 32 bytes |
| `JwtSettings:Issuer` | `JwtSettings__Issuer` | Token issuer. Committed default: `ECommerceApi` |
| `JwtSettings:Audience` | `JwtSettings__Audience` | Token audience. Committed default: `ECommerceApiUsers` |
| `JwtSettings:ExpiresInMinutes` | `JwtSettings__ExpiresInMinutes` | Access-token lifetime. Committed default: `60` |
| `AdminSeed:Password` | `AdminSeed__Password` | Initial admin password |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | SQL Server connection string |

The committed connection string uses Windows integrated security and contains no password:

```text
Server=localhost;Database=ECommerceDb;Trusted_Connection=True;TrustServerCertificate=True;
```

On Linux or macOS, replace it with a SQL authentication connection string through user secrets or `ConnectionStrings__DefaultConnection`. Do not commit that password.

### Local development with user secrets

From `src/ECommerce.Api`:

```bash
dotnet user-secrets set "JwtSettings:Key" "replace-with-at-least-32-bytes-of-random-text"
dotnet user-secrets set "AdminSeed:Password" "replace-with-a-local-admin-password"
```

Optional SQL authentication connection string:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ECommerceDb;User Id=sa;Password=<password>;TrustServerCertificate=True;"
```

### Gitignored appsettings.Development.json

Copy `src/ECommerce.Api/appsettings.Development.example.json` to `src/ECommerce.Api/appsettings.Development.json` and fill in the values there instead of user secrets. That copy is listed in `.gitignore`.

### Other environments

```bash
export JwtSettings__Key="replace-with-at-least-32-bytes-of-random-text"
export AdminSeed__Password="replace-with-an-admin-password"
export ConnectionStrings__DefaultConnection="Server=localhost;Database=ECommerceDb;User Id=sa;Password=<password>;TrustServerCertificate=True;"
```

`AdminSeed__Password` can be omitted after an admin user exists.

A database that already has an admin user keeps that password hash. The seeder does not rotate it. Replace the row (or its hash) if that database was created while a password was still committed in source.

## How to run

Requirements: the .NET 10 SDK, and a SQL Server instance the connection string can reach.

```bash
cd src/ECommerce.Api
dotnet user-secrets set "JwtSettings:Key" "replace-with-at-least-32-bytes-of-random-text"
dotnet user-secrets set "AdminSeed:Password" "replace-with-a-local-admin-password"
dotnet run
```

On startup the host calls `Database.MigrateAsync()`, which applies the migrations in `src/ECommerce.Infrastructure/Migrations` (`InitialCreate`, then `AddRefreshTokens`). It then seeds an admin user when none exists:

- Username: `admin`
- Full name: `System Administrator`
- Role: `Admin`
- Password: `AdminSeed:Password`
- Email: empty, so sign in with the username

Kestrel listens on `http://0.0.0.0:5000` (`Kestrel:Endpoints:Http:Url`). In Development, Swagger UI is at [http://localhost:5000/swagger](http://localhost:5000/swagger).

You do not need `dotnet ef database update` for a normal start. The design-time factory `AppDbContextFactory` reads `appsettings.json`, the optional environment JSON file, the API project's user secrets, and environment variables. It looks for the API project at `../ECommerce.Api` from `src/ECommerce.Infrastructure`, in the current directory, or at `src/ECommerce.Api` from the repository root.

## API endpoints

Routes come from `[Route("api/[controller]")]`. Matching is case-insensitive.

### Auth — `/api/auth`

| Method | Path | Auth | Behavior |
| --- | --- | --- | --- |
| `POST` | `/api/auth/register` | Anonymous | Creates a user with role `User`. Body: `username`, `fullName`, `email`, `password`. Username, email, and full name are stored in lower case. `200` with `"User created successfully."`, or `400` with `"Username or email already exists."` |
| `POST` | `/api/auth/login` | Anonymous | Body: `identifier` and `password`. An identifier that contains `@` is treated as an email; otherwise as a username. `200` with `accessToken` and `refreshToken`. `401` for unknown user, inactive user, or bad password. |
| `POST` | `/api/auth/refresh` | Anonymous | Body: `refreshToken`. Revokes that token and returns a new pair. `401` when the token is missing, expired, or already revoked. |
| `POST` | `/api/auth/logout` | Bearer | Revokes the caller's refresh tokens that are still active and unexpired. |
| `GET` | `/api/auth/me` | Bearer | Returns `userId`, `username`, and `role` taken from the JWT. |

### Products — `/api/products`

The controller requires a bearer token on every action. Create, update, and delete also require role `Admin`. Update and delete additionally require `Product.SellerId` to be the caller.

| Method | Path | Behavior |
| --- | --- | --- |
| `GET` | `/api/products` | Active products. Each item is `id`, `name`, and `price`. |
| `GET` | `/api/products/paged` | Active products, paged. Query: `search` (name, description, or SKU, case-insensitive), `pageNumber` (default 1), `pageSize` (default 10), `minPrice`, `maxPrice`, `sortBy` (`name` or `price`; any other value sorts by name), `isAscending` (default true). Response: `pageNumber`, `pageSize`, `totalItems`, `totalPages`, `items`. |
| `GET` | `/api/products/{id}` | Product by primary key. `404` when the row does not exist. |
| `POST` | `/api/products` | Admin. Body: `name`, `description`, `sku`, `price`, `stock`, `imageUrl`. The seller is the current user. `201` with `id`, `name`, and `price`. `409` when the SKU already exists. `400` when the seller is missing, price is not positive, or stock is negative. |
| `PUT` | `/api/products/{id}` | Admin and owner. Body: `name`, `price`. `200` with `id`, `name`, and `price`. |
| `DELETE` | `/api/products/{id}` | Admin and owner. `204` when the row is deleted. |

A missing or invalid bearer token returns `401`. A signed-in caller who does not have the `Admin` role receives `403` on create, update, and delete.

FluentValidation on create and update requires a non-empty name of at most 100 characters and a price greater than 0. Those failures return the framework validation result before the action runs.

`ExceptionHandlingMiddleware` returns JSON `{ "message": "..." }` for `BadRequestException` (400), `UnauthorizedAccessException` (401), `NotFoundException` (404), and `ConflictException` (409). Any other exception returns 500 `{ "message": "An unexpected error occurred." }`.

## Auth flow

1. Register with `POST /api/auth/register`, or sign in as the seeded `admin` user.
2. `POST /api/auth/login` checks the BCrypt hash and rejects inactive users.
3. The access token is an HMAC-SHA256 JWT. It includes the user id (`ClaimTypes.NameIdentifier`), username (`ClaimTypes.Name`), and role (`ClaimTypes.Role`). Issuer, audience, and lifetime come from `JwtSettings` (60 minutes unless overridden).
4. The refresh token is 64 random bytes, Base64-encoded, stored on the user, and expires one day after it is created.
5. Send `Authorization: Bearer <accessToken>` on `/api/auth/logout`, `/api/auth/me`, and every `/api/products` route.
6. `POST /api/auth/refresh` rotates the refresh token: the presented token is revoked with reason `"Replaced by new token"`, and the response contains a new access token and refresh token.
7. `POST /api/auth/logout` revokes each of the caller's refresh tokens that is not already revoked or expired, with reason `"User logout"`.

In Swagger, authorize with the `Bearer` scheme. The definition is an API-key header named `Authorization`, so the value includes the `Bearer ` prefix.

## Future improvements

- Add an automated test project. This repository does not have one.
- Add FluentValidation for registration and login. Only product create and update are validated that way.
- Give `Username` a unique index. Uniqueness is checked in `AuthService`; the database unique index is on `Email` only.
- Store refresh tokens hashed, and move the one-day lifetime out of `AuthService`.
- Return description, SKU, stock, and image URL from product responses. `ProductDto` currently exposes `id`, `name`, and `price`.
- Log exceptions in `ExceptionHandlingMiddleware` (the injected logger is unused) and align the role default: the entity and registration use `User`, while the EF model default is `Customer`.
- Upgrade AutoMapper 12.0.1. `dotnet build` reports advisory GHSA-rvv3-g6hj-g44x for that package.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE).
