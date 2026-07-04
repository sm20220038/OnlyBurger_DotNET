// Colored pill for order status and payment status values.
export default function StatusBadge({ value }) {
  const key = String(value || '').toLowerCase()
  return <span className={`status-badge status-${key}`}>{value}</span>
}
