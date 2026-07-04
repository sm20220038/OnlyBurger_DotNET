import { useState } from 'react'
import { productImageCandidates } from '../utils/format'

// Shows the product image when one exists in public/products/<id>.<ext>.
// It tries the common extensions in turn and falls back to a clean placeholder
// (the product name) when none of them load.
export default function ProductImage({ productId, name }) {
  const candidates = productImageCandidates(productId)
  const [index, setIndex] = useState(0)

  if (index >= candidates.length) {
    return (
      <div className="product-image placeholder" role="img" aria-label={name}>
        <span className="placeholder-text">{name}</span>
      </div>
    )
  }

  return (
    <div className="product-image">
      <img src={candidates[index]} alt={name} onError={() => setIndex((i) => i + 1)} />
    </div>
  )
}
