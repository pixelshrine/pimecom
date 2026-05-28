# PIM Ecom Messaging

A small portfolio project for a backend .NET developer role. The system separates product management from the public e-commerce catalog and uses asynchronous product events to keep the Ecom read model in sync.

## Architecture

- `Pim` - ASP.NET Core write API for creating and updating products.
- `Ecom` - ASP.NET Core read API for storefront product listing and details.
- `Shared` - shared event contracts and DTOs.
- `MessageQueue` - placeholder service documenting the RabbitMQ infrastructure role.
- `Pim.Frontend` - React admin UI for creating and editing products.
- `Ecom.Frontend` - React storefront that displays products from the Ecom API.

## Flow

1. PIM creates or updates a product.
2. PIM publishes a product event to RabbitMQ.
3. Ecom consumes the event and updates its product read model.
4. PIM Admin lets you manage products in the browser.
5. Ecom Storefront reads from the Ecom API and displays the catalog/details page.

## Backend Features

- ASP.NET Core Web APIs
- EF Core SQL Server persistence
- RabbitMQ publish/consume flow
- HTTP development event fallback when RabbitMQ is not available
- Idempotent event processing with `ProcessedEvent`
- Product version checks for out-of-order updates
- OpenAPI endpoint support in development
- Separate PIM write model and Ecom read model

## Run Locally

Start infrastructure:

```powershell
docker compose up -d
```

Run the backend APIs:

```powershell
dotnet run --project Pim/Pim.csproj
dotnet run --project Ecom/Ecom.csproj
```

Run the React storefront:

```powershell
cd Pim.Frontend
npm install
npm run dev
```

```powershell
cd Ecom.Frontend
npm install
npm run dev
```

Default local URLs:

- PIM API: `http://localhost:5158`
- Ecom API: `http://localhost:5197`
- PIM Admin: `http://127.0.0.1:5174`
- Storefront: `http://127.0.0.1:5173`
- RabbitMQ management: `http://localhost:15672`

## Demo Flow

1. Start Docker, PIM API, Ecom API, PIM Admin, and Storefront.
2. Open PIM Admin at `http://127.0.0.1:5174`.
3. Create a product.
4. Open the Ecom storefront at `http://127.0.0.1:5173`.
5. The product should appear after Ecom consumes the RabbitMQ message.

When RabbitMQ is not running, PIM posts the event to Ecom's development-only fallback endpoint so the local demo still works. Keep RabbitMQ running when you want to study the real message-queue path.

You can also create a product directly through HTTP:

```http
POST http://localhost:5158/api/products
Content-Type: application/json

{
  "sku": "SHOE-001",
  "name": "Everyday Runner",
  "description": "A lightweight sneaker for daily city walking.",
  "imageUrl": "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=1200&q=80"
}
```

## Study Map

- Start with `Shared/Events/ProductEvents.cs` to understand the integration events.
- Read `Pim/Application/Services/ProductService.cs` to see where PIM publishes events.
- Read `Pim/Infrastructure/Messaging/RabbitMqBus.cs` to see RabbitMQ publishing.
- Read `Ecom/Infrastructure/Messaging/RabbitMqConsumer.cs` to see the background consumer.
- Read `Ecom/Services/EventRouter.cs` to see idempotency, version checks, and read model updates.
- Read `Pim.Frontend/src/main.tsx` and `Ecom.Frontend/src/main.tsx` to see how the two frontends stay separated.

## Verification

```powershell
dotnet build PimEcomMessaging.slnx
cd Pim.Frontend
npm run build
cd Ecom.Frontend
npm run build
```
