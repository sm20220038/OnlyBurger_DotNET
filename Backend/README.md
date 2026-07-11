# OnlyBurger Web API

An ASP.NET Core Web API for online ordering of burgers and fast food. It supports two
roles — **Customer** and **Administrator (restaurant staff)** — with JWT authentication,
role-based authorization, a SQLite database via EF Core, real-time updates over SignalR, and
a clean **CQRS** architecture split across **Domain / Infrastructure / Presentation** projects.

---

## Tech stack

| Concern            | Choice                                                        |
| ------------------ | ------------------------------------------------------------- |
| Framework          | ASP.NET Core Web API (.NET 10)                                |
| Persistence        | Entity Framework Core 10 + SQLite                             |
| Auth               | JWT bearer tokens, role-based authorization (`User`, `Admin`) |
| Password storage   | PBKDF2 (SHA-256) — built-in, no third-party dependency        |
| Architecture       | CQRS with **MediatR** + **Repository / Unit of Work** over EF Core |
| Real-time          | **SignalR** (live order + delivery tracking)                  |
| API docs / testing | Swagger UI (with bearer auth)                                 |

> **CQRS is implemented with [MediatR](https://github.com/jbogard/MediatR).** Every write is a
> `IRequest<T>` command and every read is an `IRequest<T>` query, each with a single
> `IRequestHandler`. Controllers depend only on `IMediator`. Handlers never touch the
> `DbContext` directly — they go through the **Repository / Unit of Work** layer
> (interfaces in the **Domain** project under [`Repositories/`](OnlyBurger.Domain/Repositories),
> EF Core implementations in **Infrastructure** under [`Data/Repositories/`](OnlyBurger.Infrastructure/Data/Repositories)
> + [`Data/UnitOfWork.cs`](OnlyBurger.Infrastructure/Data/UnitOfWork.cs)), so a handler can touch
> several repositories and commit them together with one `SaveChangesAsync`.

> **Frontend:** a React + Vite single-page app lives in [`onlyburger-web/`](onlyburger-web).
> Start this API first, then follow [onlyburger-web/README.md](onlyburger-web/README.md).
> In development the frontend proxies `/api` to this backend, and CORS is configured for
> the Vite dev origin.

---

## Architecture (CQRS)

Every state change is a **Command**; every read is a **Query** — each is a MediatR
`IRequest<T>` with exactly one `IRequestHandler`. Controllers depend only on `IMediator`,
which routes the request to its handler. Handlers depend on `IUnitOfWork`, never on the
`DbContext` directly.

```
HTTP Controller ──> IMediator ──> IRequestHandler<TRequest,TResult>   (command or query)
                                          │
                                          └──> IUnitOfWork ──> IRepository<T> ──> EF Core (SQLite)
                                                    │
                                                    └──> SaveChangesAsync()  (one transaction)
```

Handlers are auto-discovered and registered at startup by MediatR
(`AddMediatR(... RegisterServicesFromAssembly ...)` in [`Program.cs`](OnlyBurger.Api/Program.cs)).

### Project layout (Clean Architecture — 3 projects)

Dependencies point inward only: **Presentation → Infrastructure → Domain**. The Domain project
has no dependencies at all.

```
Backend/
├── OnlyBurger.Domain/            ← pure domain, no dependencies
│   ├── Entities/                 User, Product, Cart, CartItem, Order, OrderItem, Student
│   ├── Enums/                    CartStatus, OrderStatus, PaymentStatus, DeliveryStatus, UserRole
│   └── Repositories/             IRepository<T>, IUnitOfWork, per-entity repository interfaces
│
├── OnlyBurger.Infrastructure/    ← references Domain (EF Core, MediatR, SignalR)
│   ├── Data/                     AppDbContext, UnitOfWork, Repositories/, DbInitializer, Migrations/
│   ├── Features/                 CQRS commands/queries + MediatR handlers + DTOs
│   │   ├── Auth/                 RegisterUserCommand, LoginUserCommand
│   │   ├── Products/             Create/Update/Delete commands, GetProducts/GetProductById queries
│   │   ├── Cart/                 Add/Update/Remove commands, GetCart query
│   │   └── Orders/               Create/Update/Delete/Approve/Reject/Pay/UpdateDeliveryStatus + queries
│   ├── Realtime/                 SignalR OrderTrackingHub + IOrderNotifier (live order/delivery push)
│   ├── Auth/                     JWT token service + PBKDF2 password hasher
│   └── Common/Exceptions/        Application exception types
│
└── OnlyBurger.Api/               ← references Infrastructure (the web host)
    ├── Controllers/              Auth, Products, Cart, Orders (depend only on IMediator)
    ├── Auth/                     ICurrentUser + CurrentUser (reads the caller from HttpContext)
    ├── Common/Middleware/        Global exception-handling middleware (→ problem+json)
    └── Program.cs                DI wiring, JWT bearer, CORS, Swagger, SignalR hub mapping
```

Web-pipeline concerns that read `HttpContext` (the current-user accessor and the exception
middleware) live in **Presentation**; the reusable JWT/hashing services stay in Infrastructure.

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

The `DbContext` and migrations live in the **Infrastructure** project, so point `dotnet ef` at it
with the **Api** project as the startup host (run from the `Backend/` folder):

```bash
dotnet tool install --global dotnet-ef      # once
dotnet ef migrations add <Name>  --project OnlyBurger.Infrastructure --startup-project OnlyBurger.Api
dotnet ef database update        --project OnlyBurger.Infrastructure --startup-project OnlyBurger.Api
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
| POST   | `/{id}/approve`  | Admin             | Approve a pending order (→ live push)      |
| POST   | `/{id}/reject`   | Admin             | Reject a pending order (→ live push)       |
| POST   | `/{id}/delivery-status` | Admin      | Advance delivery status (→ live push)      |

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
- **Cart** — `Id, UserId, Status, CreatedAt, CheckedOutAt`
- **CartItem** — `Id, CartId, ProductId, Quantity`
- **Order** — `Id, UserId, CartId, DeliveryLocation, OrderDateTime, TotalPrice, Status, PaymentStatus, DeliveryStatus`
- **OrderItem** — `Id, OrderId, ProductId, Quantity, UnitPrice`

Enums (stored as text): `CartStatus { Active, CheckedOut }`,
`OrderStatus { Pending, Approved, Rejected }`, `PaymentStatus { Unpaid, Paid }`,
`DeliveryStatus { Pending, Preparing, OutForDelivery, Delivered }`, `UserRole { User, Admin }`.

**Cart lifecycle & Cart↔Order relation.** The cart is persisted in the database (it survives a
refresh or a server restart). A user has exactly one `Active` cart at a time; at checkout that
cart is marked `CheckedOut` and linked 1:1 to the `Order` it produced (`Order.CartId`) rather than
being deleted, so it is kept as history. See **[docs/architecture.md](docs/architecture.md)** for
the full ER, cart-lifecycle, delivery-status, and SignalR diagrams.

## Real-time delivery tracking (SignalR)

An approved order moves through `Preparing → OutForDelivery → Delivered`. Staff advance the status
via `POST /api/orders/{id}/delivery-status`; the change is pushed to the owning customer over the
**`OrderTrackingHub`** SignalR hub at **`/hubs/orders`** (authenticated with the JWT sent as an
`access_token` query value). The React "My orders" page listens for the `OrderUpdated` event and
re-renders live — no polling. Approving/rejecting an order pushes an update the same way.

---

## Error handling

A single middleware ([`ExceptionHandlingMiddleware`](OnlyBurger.Api/Common/Middleware/ExceptionHandlingMiddleware.cs))
turns business exceptions into consistent JSON responses: `400` validation, `401`
unauthorized, `403` forbidden, `404` not found, `409` conflict, `500` otherwise.
