// Display helpers for prices and dates.

export function formatPrice(value) {
  const number = Number(value ?? 0)
  // Serbian dinar is shown as a whole number with a thousands separator, e.g. "1.060 RSD".
  return `${number.toLocaleString('sr-RS', { maximumFractionDigits: 0 })} RSD`
}

export function formatDateTime(value) {
  if (!value) {
    return ''
  }
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }
  return date.toLocaleString()
}

// Convention-based product image paths. Drop an image named "<id>.<ext>" into
// public/products/ and it will show automatically. Any of these extensions work.
// See public/products/README.md.
const IMAGE_EXTENSIONS = ['jpg', 'jpeg', 'png', 'webp', 'avif']

export function productImageCandidates(productId) {
  return IMAGE_EXTENSIONS.map((ext) => `/products/${productId}.${ext}`)
}
