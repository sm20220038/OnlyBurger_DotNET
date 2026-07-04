# OnlyBurger Web API

An ASP.NET Core Web API for online ordering of burgers and fast food. It supports two
roles — **Customer** and **Administrator (restaurant staff)** — with JWT authentication,
role-based authorization, a SQL Server database via EF Core, and a clean **CQRS**
architecture.

---

## Tech stack

| Concern            | Choice                                                        |
| ------------------ | ------------------------------------------------------------- |
| Framework          | ASP.NET Core Web API (.NET 10)                                |
| Persistence        | Entity Framework Core 10 + Microsoft SQL Server               |
| Auth               | JWT bearer tokens, role-based authorization (`User`, `Admin`) |
| Password storage   | PBKDF2 (SHA-256) — built-in, no third-party dependency        |
| Architecture       | CQRS with a lightweight custom dispatcher (one handler each)  |
| API docs / testing | Swagger UI (with bearer auth)                                 |

> **Why a custom CQRS dispatcher instead of MediatR?** It keeps the project free of
> external messaging libraries, makes the command/query separation explicit and easy to
> read, and demonstrates the pattern from first principles. See
> [`Cqrs/`](OnlyBurger.Api/Cqrs).

> **Frontend:** a React + Vite single-page app lives in [`onlyburger-web/`](onlyburger-web).
> Start this API first, then follow [onlyburger-web/README.md](onlyburger-web/README.md).
> In development the frontend proxies `/api` to this backend, and CORS is configured for
> the Vite dev origin.

---

## Architecture (CQRS)

Every state change is a **Command**; every read is a **Query**. Each has exactly one
handler. Controllers depend only on `IDispatcher`, which routes a message to its handler.

```
HTTP Controller ──> IDispatcher ──> ICommandHandler<TCommand,TResult>   (writes)
                                └──> IQueryHandler<TQuery,TResult>       (reads)
                                          │
                                          └──> AppDbContext (EF Core / SQL Server)
```

Handlers are auto-discovered and registered at startup
([`CqrsRegistration.AddCqrs`](OnlyBurger.Api/Cqrs/CqrsRegistration.cs)).

### Project layout

```
OnlyBurger.Api/
├── Auth/            JWT service, PBKDF2 password hasher, current-user accessor
├── Common/          App exceptions + global exception-handling middleware
├── Controllers/     Auth, Products, Cart, Orders
├── Cqrs/            ICommand/IQuery markers, handlers, dispatcher, registration
├── Data/            AppDbContext, DbInitializer (migrate + seed), Migrations/
├── Domain/          Entities (User, Product, Order, OrderItem, CartItem) + Enums
└── Features/        One folder per area; each command/query lives with its handler
    ├── Auth/        RegisterUserCommand, LoginUserCommand
    ├── Products/    Create/Update/Delete commands, GetProducts/GetProductById queries
    ├── Cart/        Add/Update/Remove commands, GetCart query
    └── Orders/      Create/Update/Delete/Approve/Reject/Pay commands,
                     GetOrders/GetOrderById/GetUserOrders queries
```

The commands and queries map directly to the assignment spec:

- **Commands:** `CreateProductCommand`, `UpdateProductCommand`, `DeleteProductCommand`,
  `CreateOrderCommand`, `UpdateOrderCommand`, `DeleteOrderCommand`, `ApproveOrderCommand`,
  `RejectOrderCommand`, `RegisterUserCommand`, `LoginUserCommand` (plus cart + pay commands).
- **Queries:** `GetProductsQuery`, `GetProductByIdQuery`, `GetOrdersQuery`,
  `GetOrderByIdQuery`, `GetUserOrdersQuery`, `GetCartQuery`.

---

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server. The default connection string uses **SQL Server LocalDB**
  (`(localdb)\MSSQLLocalDB`), which ships with Visual Studio. To use another instance,
  edit `ConnectionStrings:DefaultConnection` in
  [`appsettings.json`](OnlyBurger.Api/appsettings.json).

### Run

```bash
cd OnlyBurger.Api
dotnet run
```

On startup the app **applies migrations and seeds** the database automatically, so no
manual database step is required. Then open Swagger:

```
https://localhost:<port>/swagger
```

(The exact port is printed in the console and configured in
`Properties/launchSettings.json`.)

### Managing migrations manually (optional)

```bash
dotnet tool install --global dotnet-ef      # once
cd OnlyBurger.Api
dotnet ef migrations add <Name>
dotnet ef database update
```

---

## Seeded data

A starter menu and one admin account are created on first run:

| Field    | Value                  |
| -------- | ---------------------- |
| Username | `admin`                |
| Password | `Admin123!`            |
| Email    | `admin@onlyburger.com` |

Starter products: Classic Burger, Cheeseburger, Double Burger, Chicken Burger, Fries,
Coca-Cola. (Admin credentials are configurable under `SeedAdmin` in `appsettings.json`.)

---

## Authentication

1. `POST /api/auth/register` (new customer) or `POST /api/auth/login` (e.g. the admin
   above) — both return a JWT.
2. In Swagger, click **Authorize** and paste the token (the `Bearer ` prefix is added
   for you). With `curl`, send `Authorization: Bearer <token>`.

Customers are always created with the `User` role. The `Admin` role exists only via the
seeded account (or by promoting a user directly in the database).

---

## Endpoints

### Auth — `/api/auth`
| Method | Route       | Access | Description                       |
| ------ | ----------- | ------ | --------------------------------- |
| POST   | `/register` | Public | Register a customer, returns JWT  |
| POST   | `/login`    | Public | Log in, returns JWT               |

### Products — `/api/products`
| Method | Route   | Access | Description           |
| ------ | ------- | ------ | --------------------- |
| GET    | `/`     | Public | List all products     |
| GET    | `/{id}` | Public | Get one product       |
| POST   | `/`     | Admin  | Create a product      |
| PUT    | `/{id}` | Admin  | Update a product      |
| DELETE | `/{id}` | Admin  | Delete a product      |

### Cart — `/api/cart` (authenticated; always the caller's own cart)
| Method | Route                 | Description                              |
| ------ | --------------------- | ---------------------------------------- |
| GET    | `/`                   | View cart contents + total               |
| POST   | `/items`              | Add a product (increments if present)    |
| PUT    | `/items/{productId}`  | Set the quantity of a cart line          |
| DELETE | `/items/{productId}`  | Remove a product from the cart           |

### Orders — `/api/orders`
| Method | Route            | Access            | Description                                |
| ------ | ---------------- | ----------------- | ------------------------------------------ |
| POST   | `/`              | Customer          | Create an order from the cart              |
| GET    | `/mine`          | Customer          | List the caller's orders                   |
| GET    | `/{id}`          | Owner or Admin    | Order details                              |
| PUT    | `/{id}`          | Owner (pending)   | Modify delivery location / items           |
| DELETE | `/{id}`          | Owner (pending)   | Delete a pending order                     |
| POST   | `/{id}/pay`      | Owner             | Simulate payment                           |
| GET    | `/`              | Admin             | List **all** orders                        |
| POST   | `/{id}/approve`  | Admin             | Approve a pending order                    |
| POST   | `/{id}/reject`   | Admin             | Reject a pending order                     |

See [`OnlyBurger.Api.http`](OnlyBurger.Api/OnlyBurger.Api.http) for ready-to-run sample
requests.

---

## Business rules

- **Total is always derived** from the order items — never trusted from the client.
- **Order date/time** is set automatically at creation (UTC).
- A customer can **modify or delete an order only while it is `Pending`**. Once `Approved`
  or `Rejected` it is locked (returns `400`).
- Only **administrators** manage the menu and change order status.
- Customers can only see and act on **their own** orders (enforced in the handlers, not
  just the route).
- **Payment is simulated** — `POST /{id}/pay` flips `PaymentStatus` to `Paid`.

---

## Data model

- **User** — `Id, Username, Email, PasswordHash, Role`
- **Product** — `Id, Name, Description, Price`
- **Order** — `Id, UserId, DeliveryLocation, OrderDateTime, TotalPrice, Status, PaymentStatus`
- **OrderItem** — `Id, OrderId, ProductId, Quantity, UnitPrice`
- **CartItem** — `Id, UserId, ProductId, Quantity`

Enums: `OrderStatus { Pending, Approved, Rejected }`,
`PaymentStatus { Unpaid, Paid }`, `UserRole { User, Admin }` (stored as text in SQL Server).

---

## Error handling

A single middleware ([`ExceptionHandlingMiddleware`](OnlyBurger.Api/Common/Middleware/ExceptionHandlingMiddleware.cs))
turns business exceptions into consistent JSON responses: `400` validation, `401`
unauthorized, `403` forbidden, `404` not found, `409` conflict, `500` otherwise.
