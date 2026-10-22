# ASP.NET Core API Versioning: Products

This project demonstrates how to evolve a products API without changing the
response contract used by existing clients.

## API endpoints

- `GET /api/v1/products` returns the original product shape:

  ```json
  {
    "id": 1,
    "name": "Laptop",
    "price": 25000
  }
  ```

- `GET /api/v2/products` returns the new shape with a category:

  ```json
  {
    "id": 1,
    "name": "Laptop",
    "price": 25000,
    "categoryName": "Electronics"
  }
  ```

The versions use separate response models and version-specific controller
actions. This keeps the V1 response unchanged while V2 evolves independently.
The sample uses a single fixed in-memory product.

## Run the API

Install the .NET 10 SDK, then run these commands from the repository root:

```powershell
dotnet restore .\OrdersApiExample.slnx
dotnet run --project .\Orders.Api\Orders.Api.csproj
```

Use the base URL printed by `dotnet run` to request either endpoint.

## Run the tests

```powershell
dotnet test .\OrdersApiExample.slnx
```

The integration tests verify both versioned routes and their independent JSON
response contracts.
