// Colored pill for order status, payment status, and delivery status values.
// Splits PascalCase enum names into words for display, e.g. "OutForDelivery" -> "Out For Delivery".
function humanize(value) {
  return String(value || '').replace(/([a-z])([A-Z])/g, '$1 $2')
}

export default function StatusBadge({ value }) {
  const key = String(value || '').toLowerCase()
  return <span className={`status-badge status-${key}`}>{humanize(value)}</span>
}
