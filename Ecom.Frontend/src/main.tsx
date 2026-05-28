import React from "react";
import ReactDOM from "react-dom/client";
import "./styles.css";

type Product = {
  id: string;
  sku: string;
  name: string;
  description: string;
  imageUrl: string;
  color: string;
  size: string;
  material: string;
  version: number;
};

type Route =
  | { name: "catalog" }
  | { name: "product"; id: string };

const apiBaseUrl = import.meta.env.VITE_ECOM_API_URL ?? "http://localhost:5197";

function App() {
  const [route, setRoute] = React.useState<Route>(readRoute());

  React.useEffect(() => {
    function handlePopState() {
      setRoute(readRoute());
    }

    window.addEventListener("popstate", handlePopState);
    return () => window.removeEventListener("popstate", handlePopState);
  }, []);

  function navigate(nextRoute: Route) {
    const path = nextRoute.name === "catalog" ? "/" : `/products/${nextRoute.id}`;
    window.history.pushState(null, "", path);
    setRoute(nextRoute);
  }

  return route.name === "catalog" ? (
    <CatalogPage onOpenProduct={(id) => navigate({ name: "product", id })} />
  ) : (
    <ProductPage id={route.id} onBack={() => navigate({ name: "catalog" })} />
  );
}

function CatalogPage({ onOpenProduct }: { onOpenProduct: (id: string) => void }) {
  const [products, setProducts] = React.useState<Product[]>([]);
  const [isLoading, setIsLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    async function loadProducts() {
      try {
        setIsLoading(true);
        setError(null);

        const response = await fetch(`${apiBaseUrl}/api/products`);

        if (!response.ok) {
          throw new Error("The product catalog could not be loaded.");
        }

        setProducts((await response.json()) as Product[]);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Something went wrong.");
      } finally {
        setIsLoading(false);
      }
    }

    loadProducts();
  }, []);

  return (
    <main className="page catalog-page">
      <StoreHeader />

      <section className="catalog-hero">
        <div>
          <p className="eyebrow">New Season</p>
          <h1>Everyday essentials, ready to move.</h1>
        </div>
        <p>
          A curated storefront powered by the published catalog from PIM. Only buyer-ready
          products appear here.
        </p>
      </section>

      {isLoading && <p className="state">Loading catalog...</p>}
      {error && <p className="state state-error">{error}</p>}
      {!isLoading && !error && products.length === 0 && (
        <p className="state">No published products are available yet.</p>
      )}

      <section className="product-grid" aria-label="Published products">
        {products.map((product) => (
          <button
            className="product-tile"
            key={product.id}
            onClick={() => onOpenProduct(product.id)}
            type="button"
          >
            <span className="tile-image">
              <img src={product.imageUrl || fallbackImage} alt="" />
            </span>
            <span className="tile-copy">
              <small>{product.sku}</small>
              <strong>{product.name}</strong>
              <span>View product</span>
            </span>
          </button>
        ))}
      </section>
    </main>
  );
}

function ProductPage({ id, onBack }: { id: string; onBack: () => void }) {
  const [product, setProduct] = React.useState<Product | null>(null);
  const [isLoading, setIsLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    async function loadProduct() {
      try {
        setIsLoading(true);
        setError(null);

        const response = await fetch(`${apiBaseUrl}/api/products/${id}`);

        if (response.status === 404) {
          throw new Error("This product is not available.");
        }

        if (!response.ok) {
          throw new Error("The product could not be loaded.");
        }

        setProduct((await response.json()) as Product);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Something went wrong.");
      } finally {
        setIsLoading(false);
      }
    }

    loadProduct();
  }, [id]);

  return (
    <main className="page product-page">
      <StoreHeader />

      <button className="back-button" onClick={onBack} type="button">
        Back to catalog
      </button>

      {isLoading && <p className="state">Loading product...</p>}
      {error && <p className="state state-error">{error}</p>}

      {product && (
        <section className="product-detail">
          <div className="gallery">
            <img src={product.imageUrl || fallbackImage} alt={product.name} />
          </div>

          <div className="purchase-panel">
            <p className="eyebrow">{product.sku}</p>
            <h1>{product.name}</h1>
            <p className="description">
              {product.description || "Product details are coming soon."}
            </p>

            <div className="option-row">
              <span>Color</span>
              <p>{product.color || "As shown"}</p>
            </div>

            <div className="option-row">
              <span>Size</span>
              <p>{product.size || "One size"}</p>
            </div>

            <button className="primary-action" type="button">
              Add to bag
            </button>

            <dl className="detail-list">
              <div>
                <dt>Material</dt>
                <dd>{product.material || "Product material details are coming soon"}</dd>
              </div>
              <div>
                <dt>Shipping</dt>
                <dd>Free standard shipping on published catalog items</dd>
              </div>
              <div>
                <dt>Returns</dt>
                <dd>30-day returns for unused products</dd>
              </div>
            </dl>
          </div>
        </section>
      )}
    </main>
  );
}

function StoreHeader() {
  return (
    <header className="store-header">
      <strong>Pimmerce</strong>
      <nav aria-label="Store navigation">
        <span>Catalog</span>
        <span>New</span>
        <span>Support</span>
      </nav>
    </header>
  );
}

function readRoute(): Route {
  const match = window.location.pathname.match(/^\/products\/([^/]+)$/);

  if (match) {
    return { name: "product", id: match[1] };
  }

  return { name: "catalog" };
}

const fallbackImage =
  "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=1200&q=80";

ReactDOM.createRoot(document.getElementById("root")!).render(<App />);
