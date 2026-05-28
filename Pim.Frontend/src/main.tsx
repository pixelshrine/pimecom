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
  status: ProductStatus;
  version: number;
  createdAt?: string;
};

type ProductForm = {
  sku: string;
  name: string;
  description: string;
  imageUrl: string;
  color: string;
  size: string;
  material: string;
  status: ProductStatus;
};

const emptyForm: ProductForm = {
  sku: "",
  name: "",
  description: "",
  imageUrl: "",
  color: "",
  size: "",
  material: "",
  status: "Draft"
};

const productStatuses = [
  "Draft",
  "In Progress",
  "Pending Review",
  "Approved",
  "Ready for Publish",
  "Published",
  "Discontinued",
  "Archived",
  "Recalled"
] as const;

type ProductStatus = (typeof productStatuses)[number];

const apiBaseUrl = import.meta.env.VITE_PIM_API_URL ?? "http://localhost:5158";

function App() {
  const [products, setProducts] = React.useState<Product[]>([]);
  const [selectedProduct, setSelectedProduct] = React.useState<Product | null>(null);
  const [form, setForm] = React.useState<ProductForm>(emptyForm);
  const [isLoading, setIsLoading] = React.useState(true);
  const [isSaving, setIsSaving] = React.useState(false);
  const [message, setMessage] = React.useState<string | null>(null);
  const [error, setError] = React.useState<string | null>(null);

  async function loadProducts() {
    try {
      setIsLoading(true);
      setError(null);
      const response = await fetch(`${apiBaseUrl}/api/products`);

      if (!response.ok) {
        throw new Error("PIM products could not be loaded.");
      }

      setProducts((await response.json()) as Product[]);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong.");
    } finally {
      setIsLoading(false);
    }
  }

  React.useEffect(() => {
    loadProducts();
  }, []);

  function selectProduct(product: Product) {
    setSelectedProduct(product);
    setForm({
      sku: product.sku,
      name: product.name,
      description: product.description,
      imageUrl: product.imageUrl,
      color: product.color,
      size: product.size,
      material: product.material,
      status: normalizeStatus(product.status)
    });
    setMessage(null);
    setError(null);
  }

  function startNewProduct() {
    setSelectedProduct(null);
    setForm(emptyForm);
    setMessage(null);
    setError(null);
  }

  async function saveProduct(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSaving(true);
    setError(null);
    setMessage(null);

    try {
      const url = selectedProduct
        ? `${apiBaseUrl}/api/products/${selectedProduct.id}`
        : `${apiBaseUrl}/api/products`;

      const response = await fetch(url, {
        method: selectedProduct ? "PUT" : "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(form)
      });

      if (!response.ok) {
        throw new Error("The product could not be saved.");
      }

      setMessage(
        selectedProduct
          ? "Product updated and ProductUpdated event published."
          : "Product created and ProductCreated event published."
      );

      await loadProducts();
      if (!selectedProduct) {
        setForm(emptyForm);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong.");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <main className="workspace">
      <section className="sidebar" aria-label="PIM product list">
        <div className="header-row">
          <div>
            <p className="eyebrow">PIM Admin</p>
            <h1>Products</h1>
          </div>
          <button className="icon-button" onClick={startNewProduct} type="button" title="New product">
            +
          </button>
        </div>

        {isLoading && <p className="state">Loading products...</p>}
        {!isLoading && products.length === 0 && (
          <p className="state">Create the first product to publish it to Ecom.</p>
        )}

        <div className="list">
          {products.map((product) => (
            <button
              className={selectedProduct?.id === product.id ? "list-item active" : "list-item"}
              key={product.id}
              onClick={() => selectProduct(product)}
              type="button"
            >
              <span>
                <strong>{product.name}</strong>
                <small>{product.sku}</small>
                <b>{product.status}</b>
              </span>
              <em>v{product.version}</em>
            </button>
          ))}
        </div>
      </section>

      <section className="editor" aria-label="Product editor">
        <div className="editor-heading">
          <p className="eyebrow">Catalog Source</p>
          <h2>{selectedProduct ? "Edit Product" : "Create Product"}</h2>
        </div>

        <form onSubmit={saveProduct}>
          <label>
            SKU
            <input
              required
              maxLength={80}
              onChange={(event) => setForm({ ...form, sku: event.target.value })}
              value={form.sku}
            />
          </label>

          <label>
            Name
            <input
              required
              maxLength={200}
              onChange={(event) => setForm({ ...form, name: event.target.value })}
              value={form.name}
            />
          </label>

          <label>
            Status
            <select
              onChange={(event) =>
                setForm({ ...form, status: normalizeStatus(event.target.value) })
              }
              value={form.status}
            >
              {productStatuses.map((status) => (
                <option key={status} value={status}>
                  {status}
                </option>
              ))}
            </select>
          </label>

          <label>
            Image URL
            <input
              onChange={(event) => setForm({ ...form, imageUrl: event.target.value })}
              value={form.imageUrl}
            />
          </label>

          <div className="field-grid">
            <label>
              Color
              <input
                maxLength={120}
                onChange={(event) => setForm({ ...form, color: event.target.value })}
                value={form.color}
              />
            </label>

            <label>
              Size
              <input
                maxLength={120}
                onChange={(event) => setForm({ ...form, size: event.target.value })}
                value={form.size}
              />
            </label>
          </div>

          <label>
            Material
            <input
              maxLength={200}
              onChange={(event) => setForm({ ...form, material: event.target.value })}
              value={form.material}
            />
          </label>

          <label>
            Description
            <textarea
              required
              onChange={(event) => setForm({ ...form, description: event.target.value })}
              rows={7}
              value={form.description}
            />
          </label>

          <div className="actions">
            <button disabled={isSaving} type="submit">
              {isSaving ? "Saving..." : selectedProduct ? "Save changes" : "Create product"}
            </button>
            <button className="secondary" onClick={startNewProduct} type="button">
              Clear
            </button>
          </div>
        </form>

        {message && <p className="notice">{message}</p>}
        {error && <p className="notice error">{error}</p>}
      </section>

      <aside className="preview" aria-label="Image preview">
        <div className="image-frame">
          {form.imageUrl ? <img src={form.imageUrl} alt="" /> : <span>No image URL</span>}
        </div>
        <dl>
          <div>
            <dt>Selected ID</dt>
            <dd>{selectedProduct?.id ?? "New product"}</dd>
          </div>
          <div>
            <dt>Current Version</dt>
            <dd>{selectedProduct?.version ?? "Not published"}</dd>
          </div>
          <div>
            <dt>Status</dt>
            <dd>{form.status}</dd>
          </div>
          <div>
            <dt>Color</dt>
            <dd>{form.color || "Not set"}</dd>
          </div>
          <div>
            <dt>Size</dt>
            <dd>{form.size || "Not set"}</dd>
          </div>
          <div>
            <dt>Material</dt>
            <dd>{form.material || "Not set"}</dd>
          </div>
        </dl>
      </aside>
    </main>
  );
}

function normalizeStatus(value: string): ProductStatus {
  return productStatuses.find((status) => status === value) ?? "Draft";
}

ReactDOM.createRoot(document.getElementById("root")!).render(<App />);
