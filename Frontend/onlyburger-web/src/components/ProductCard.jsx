import { useState } from 'react'
import ProductImage from './ProductImage'
import { formatPrice } from '../utils/format'

export default function ProductCard({ product, canOrder, onAdd, onRequireLogin }) {
  const [quantity, setQuantity] = useState(1)
  const [busy, setBusy] = useState(false)

  const handleAdd = async () => {
    if (!canOrder) {
      onRequireLogin()
      return
    }
    setBusy(true)
    try {
      await onAdd(product.id, quantity)
      setQuantity(1)
    } finally {
      setBusy(false)
    }
  }

  return (
    <article className="product-card">
      <ProductImage productId={product.id} name={product.name} />
      <div className="product-body">
        <h3 className="product-name">{product.name}</h3>
        <p className="product-description">{product.description}</p>
        <div className="product-footer">
          <span className="product-price">{formatPrice(product.price)}</span>
          <div className="product-actions">
            <div className="stepper">
              <button
                type="button"
                className="stepper-btn"
                aria-label="Decrease quantity"
                onClick={() => setQuantity((q) => Math.max(1, q - 1))}
              >
                -
              </button>
              <span className="stepper-value">{quantity}</span>
              <button
                type="button"
                className="stepper-btn"
                aria-label="Increase quantity"
                onClick={() => setQuantity((q) => Math.min(1000, q + 1))}
              >
                +
              </button>
            </div>
            <button type="button" className="btn btn-primary" onClick={handleAdd} disabled={busy}>
              {busy ? 'Adding' : 'Add to cart'}
            </button>
          </div>
        </div>
      </div>
    </article>
  )
}
