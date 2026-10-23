# Products API: Clean Architecture, CQRS, Versioning, and EF Core

This project is split into four layers: `Orders.Domain`, `Orders.Application`,
`Orders.Infrastructure`, and `Orders.Api`. The products endpoints use CQRS
queries and commands, request DTO validation, and EF Core with SQL Server.
The database is created and seeded when the API starts.

## Clean Architecture and CQRS products API

- `POST /api/products` creates a product from a JSON body containing `name`
  and a positive `price`. The response is `201 Created` with a link to the
  new product. Products use the seeded Electronics category.
- `GET /api/products/{id}` returns a product by ID or `404 Not Found`.
- `GET /api/products` returns the existing paged product response; optional
  `pageNumber`, `pageSize`, and `search` query parameters are supported.
- The API validates the create-product DTO and sends commands and queries
  through application handlers. Domain entities live in `Orders.Domain`;
  EF Core and SQL Server configuration live in `Orders.Infrastructure`.

## Endpoints

### Existing versioned API

- `GET /api/v1/products` returns the original product shape:

  ```json
  {
    "id": 1,
    "name": "Laptop",
    "price": 25000
  }
  ```

- `GET /api/v2/products` adds `categoryName` without changing the V1 contract.

### EF Core products API

- `GET /api/products?pageNumber=1&pageSize=10&search=Lap` returns a page of
  products, including the category name, concurrency token, `createdAt`, and
  `updatedAt` timestamps. `pageSize` is limited to 100. Search matches
  product-name prefixes.
- `PUT /api/products/{id}` updates only `name`, `price`, and `categoryId`.
  Send the `concurrencyToken` returned by the GET endpoint:

  ```json
  {
    "name": "Laptop Pro",
    "price": 27000,
    "categoryId": 1,
    "concurrencyToken": "10000000-0000-0000-0000-000000000001"
  }
  ```

  A successful update returns the new token. If another request has already
  changed the product, the API returns `409 Conflict`; reload the product and
  retry with its latest token.
- `DELETE /api/products/{id}` demonstrates soft deletion. Deleted products
  are excluded from normal product and category queries.

## EF Core concepts used

- The products list uses `AsNoTracking`, a `Select` projection, and `Skip` /
  `Take` pagination. The category name is selected in the same database query,
  avoiding one query per product.
- `ProductsDbContext` configures global `HasQueryFilter` filters for
  `IsDeleted`. Its save pipeline converts product deletes to updates and
  automatically maintains `CreatedAt` and `UpdatedAt` for products and
  categories when records are created or changed.
- The `Name` column has an index because product-name prefix search is a
  frequent lookup. The API's `StartsWith` search uses that indexed field.
- The application-managed `ConcurrencyToken` is configured as an EF Core
  concurrency token. PUT sets the request token as the tracked property's
  original value, allowing EF Core to detect stale updates and report
  `DbUpdateConcurrencyException`.
- `EnsureCreated` and seed data keep this training sample runnable without a
  migration setup. For a production application, use EF Core migrations.

## Run the API

Install the .NET 10 SDK and SQL Server LocalDB, then run these commands from
the repository root:

```powershell
dotnet restore .\OrdersApiExample.slnx
dotnet run --project .\Orders.Api\Orders.Api.csproj
```

The default connection string is in `Orders.Api\appsettings.json`. Override
`ConnectionStrings__Products` to use a different SQL Server instance.

## Run the tests

```powershell
dotnet test .\OrdersApiExample.slnx
```

The integration tests use an in-memory SQLite database and verify the
versioned response contracts, product creation and validation, lookup,
pagination, category projection, prefix search, stale-token handling, and
soft deletion.
