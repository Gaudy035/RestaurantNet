# RestaurantNet

Full-stack restaurant management system. A web application for managing a restaurant
chain: locations, tables, bookings, menu, orders and deliveries — with a public
storefront and an admin panel.

Project is still work in progress. Authentication and parts of the admin panel are functional but ordering, menu, booking, and location management are currently still in development.

## Tech stack

| Layer    | Technology                                                               |
| -------- | ------------------------------------------------------------------------ |
| Backend  | ASP.NET Core (.NET 10), EF Core, PostgreSQL, JWT, BCrypt, Quartz, Scalar |
| Frontend | Next.js 16, React 19, TypeScript, Tailwind CSS v4, shadcn/ui, Bun        |
| Infra    | Docker Compose (PostgreSQL + backend + frontend)                         |
| Testing  | xUnit, Moq, EF Core InMemory/SQLite, Coverlet                            |

## Features

- JWT authentication (access/refresh tokens in HttpOnly cookies)
- Role-based access (`Admin`, `Employee`, `Client`)
- Client accounts created by employees or self-registered by the client
- Employee accounts created and managed by admins (admins are also employees)
- Admin panel: manage clients and employees (list, search, add, delete)
- Initial admin seeded automatically when no admins exist in database
- Background job for daily cleaning up expired/revoked refresh-token (Quartz)
- Scalar UI API docs in dev at `/docs`

### Planned / work in progress

The following are modeled in the database but not yet exposed in the UI or API:

- Menu (categories, items, ingredients, allergens)
- Orders (dine-in / pickup / delivery) and order status flow
- Tables, bookings, and multi-location support
- Delivery management

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Bun](https://bun.sh/) (or Node.js)
- [Docker](https://www.docker.com/) (optional, for the full stack)

## Getting started

### With Docker (recommended)

1. Copy the environment template and adjust values:

   ```sh
   cp .env.example .env
   ```

2. Build and run:

   ```sh
   docker compose up --build
   ```

   | Service  | URL                   |
   | -------- | --------------------- |
   | Frontend | http://localhost:3000 |
   | Backend  | http://localhost:8080 |
   | Postgres | localhost:5432        |

### Running locally (development)

**Backend**

```sh
cd backend
cp appsettings.example.json appsettings.json   # then set your DB / JWT values
dotnet run
```

**Frontend**

```sh
cd frontend
cp .env.example .env.local
bun install
bun run dev
```

## Environment variables

See `.env.example` for the full list. Key variables:

| Variable                     | Purpose                             |
| ---------------------------- | ----------------------------------- |
| `POSTGRES_USER`              | PostgreSQL user                     |
| `POSTGRES_PASSWORD`          | PostgreSQL password                 |
| `POSTGRES_DB`                | PostgreSQL database name            |
| `JWT_KEY`                    | JWT signing key (min 32 characters) |
| `ADMIN_EMAIL`                | Seeded admin account email          |
| `ADMIN_PASSWORD`             | Seeded admin account password       |
| `CORS_ALLOWED_ORIGINS`       | Allowed frontend origin(s)          |
| `NEXT_PUBLIC_API_URL_CLIENT` | API URL used in the browser         |
| `API_URL_SERVER`             | API URL used server-side            |

## Authentication

Access tokens are stored in HttpOnly cookies and expire after 15 minutes; refresh
tokens are rotated on use and stored in the database. Admin and client surfaces use
separate cookies (`admin_*` vs `client_*`).

## Testing

Run from the project root

```sh
dotnet test
```

## Project structure

```
backend/     ASP.NET Core Web API (controllers, services, EF entities, migrations, jobs, tests)
frontend/    Next.js app (storefront + admin panel)
```
