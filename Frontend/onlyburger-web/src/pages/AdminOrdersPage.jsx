import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/client'
import { formatDateTime, formatPrice } from '../utils/format'
import StatusBadge from '../components/StatusBadge'
import Message from '../components/Message'

const FILTERS = ['All', 'Pending', 'Approved', 'Rejected']

export default function AdminOrdersPage() {
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [filter, setFilter] = useState('All')

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const data = await api.getAllOrders()
      setOrders(data || [])
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    load()
  }, [load])

  const run = async (action) => {
    setError('')
    try {
      await action()
      await load()
    } catch (err) {
      setError(err.message)
    }
  }

  const visibleOrders = filter === 'All' ? orders : orders.filter((o) => o.status === filter)

  return (
    <section>
      <h1>Manage orders</h1>
      <Message type="error">{error}</Message>

      <div className="filter-bar">
        {FILTERS.map((option) => (
          <button
            key={option}
            type="button"
            className={`filter-chip ${filter === option ? 'active' : ''}`}
            onClick={() => setFilter(option)}
          >
            {option}
          </button>
        ))}
      </div>

      {loading ? (
        <p className="muted">Loading orders...</p>
      ) : visibleOrders.length === 0 ? (
        <p className="muted">No orders to show.</p>
      ) : (
        <div className="order-list">
          {visibleOrders.map((order) => (
            <article key={order.id} className="order-card">
              <header className="order-head">
                <div>
                  <h3>Order #{order.id}</h3>
                  <span className="muted">
                    Customer #{order.userId} | {formatDateTime(order.orderDateTime)}
                  </span>
                </div>
                <div className="order-badges">
                  <StatusBadge value={order.status} />
                  <StatusBadge value={order.paymentStatus} />
                </div>
              </header>

              <ul className="order-items">
                {order.items.map((item) => (
                  <li key={item.id}>
                    <span>
                      {item.quantity} x {item.productName}
                    </span>
                    <span>{formatPrice(item.lineTotal)}</span>
                  </li>
                ))}
              </ul>

              <div className="order-meta">
                <span className="muted">Deliver to: {order.deliveryLocation}</span>
                <span className="order-total">Total {formatPrice(order.totalPrice)}</span>
              </div>

              {order.status === 'Pending' && (
                <div className="order-actions">
                  <button
                    type="button"
                    className="btn btn-primary btn-sm"
                    onClick={() => run(() => api.approveOrder(order.id))}
                  >
                    Approve
                  </button>
                  <button
                    type="button"
                    className="btn btn-danger btn-sm"
                    onClick={() => run(() => api.rejectOrder(order.id))}
                  >
                    Reject
                  </button>
                </div>
              )}
            </article>
          ))}
        </div>
      )}
    </section>
  )
}
