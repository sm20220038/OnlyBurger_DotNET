# OnlyBurger Web (React + Vite)

Frontend for the OnlyBurger ordering system. It talks to the ASP.NET Core backend in
`../OnlyBurger.Api`.

Customers can browse the menu, manage a cart, place and track orders, and simulate
payment. Administrators can manage the menu and approve or reject orders.

## Prerequisites

- Node.js 18 or newer
- The backend running (see `../OnlyBurger.Api`). By default it listens on
  `http://localhost:5169`.

## Run it

1. Start the backend first:

   ```bash
   cd ../OnlyBurger.Api
   dotnet run
   ```

2. Start the frontend:

   ```bash
   npm install
   npm run dev
   ```

3. Open the URL Vite prints (default `http://localhost:5173`).

### How it reaches the API

The Vite dev server proxies any request starting with `/api` to the backend at
`http://localhost:5169` (configured in `vite.config.js`). This means the browser makes
same-origin calls, so there are no CORS or HTTPS certificate prompts in development.

To call a backend on a different host or port, either change the proxy target in
`vite.config.js`, or set `VITE_API_BASE_URL` in `.env` to the full backend URL (the
backend already allows the dev origin via CORS).

## Logging in

Use the seeded admin account from the backend to see the admin features:

- Username: `admin`
- Password: `Admin123!`

Or register a new account to use it as a regular customer.

## Product images

Photos are optional and load by convention. Drop an image named after the product id
into `public/products/`, for example `public/products/1.jpg`. Until then, each product
shows a clean placeholder with its name. See `public/products/README.md` for details.

## Project structure

```
src/
├── api/client.js          fetch wrapper + all API calls, attaches the JWT
├── context/
│   ├── AuthContext.jsx    login/register/logout, stores the JWT
│   └── CartContext.jsx    cart state and item count shared across the app
├── components/            Navbar, ProtectedRoute, ProductCard, ProductImage, etc.
├── pages/
│   ├── MenuPage.jsx       browse + add to cart
│   ├── CartPage.jsx       cart + checkout
│   ├── OrdersPage.jsx     my orders, pay, edit, cancel (pending only)
│   ├── LoginPage.jsx / RegisterPage.jsx
│   ├── AdminProductsPage.jsx   menu CRUD (admin)
│   └── AdminOrdersPage.jsx     approve/reject orders (admin)
└── utils/format.js        price/date formatting, image path helper
```

## Build

```bash
npm run build      # production build into dist/
npm run preview    # preview the production build
```
