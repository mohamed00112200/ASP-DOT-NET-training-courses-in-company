# ASP.NET Core API Versioning: Orders

This small training project demonstrates how to evolve an API without breaking
clients that still use its first version.

## The task

An order has a name and a decimal price. The API exposes the same order through
two URL versions:

- **V1** keeps `price` as a decimal.
- **V2** returns `price` as an object containing `amount` and `currency`.

Example V1 response from `GET /api/v1/orders/1`:

```json
{
  "id": 1,
  "name": "Laptop",
  "price": 25000.50
}
```

Example V2 response from `GET /api/v2/orders/1`:

```json
{
  "id": 1,
  "name": "Laptop",
  "price": {
    "amount": 25000.50,
    "currency": "EGP"
  }
}
```

The response models are deliberately separate. Changing the V1 `price` type
would break existing clients, so V2 gets its own contract while V1 stays
available.

## Run the API

Install the .NET 10 SDK, then run these commands from the repository root:

```powershell
dotnet restore .\OrdersApiExample.slnx
dotnet run --project .\Orders.Api\Orders.Api.csproj
```

Use the base URL printed by `dotnet run` (for example,
`http://localhost:5000`) with these requests:

```text
GET /api/v1/orders/1
GET /api/v2/orders/1
GET /api/v2/orders/999
```

The first two requests return the two response shapes above. The last returns
`404 Not Found`. The sample uses fixed in-memory data; it is for learning API
versioning, not a database example.

## Run the tests

```powershell
dotnet test .\OrdersApiExample.slnx
```

The tests cover the V1 and V2 JSON contracts, a missing order response, and the
in-memory service's missing-order behavior.

## What to notice

1. `OrdersController` supports both `1.0` and `2.0`.
2. `[MapToApiVersion]` selects the action for each URL version.
3. `OrderV1Response` and `OrderV2Response` keep the public contracts separate
   from the internal `Order` model.
4. `UrlSegmentApiVersionReader` reads the version from `/api/v{version}/...`.
5. The controller returns `404` for an unknown order instead of a successful
   response containing missing data.

## Suggested Day 1 explanation

> I added URL-based API versioning to an orders endpoint. V1 returns `price` as
> a decimal, and V2 returns it as a `Money`-style object with `amount` and
> `currency`. Keeping the V1 response unchanged protects older clients. The
> tests verify both response contracts and the not-found case.
