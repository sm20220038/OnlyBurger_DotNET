import { useCallback, useEffect, useRef, useState } from 'react'
import { api } from '../api/client'
import { createOrderHubConnection, ADMIN_ORDERS_CHANGED_EVENT } from '../api/orderHub'
import { formatDateTime, formatPrice } from '../utils/format'
import StatusBadge from '../components/StatusBadge'
import Message from '../components/Message'

const FILTERS = ['All', 'Pending', 'Approved', 'Rejected']

// The delivery pipeline an admin walks an approved order through.
const DELIVERY_STEPS = ['Preparing', 'OutForDelivery', 'Delivered']
const DELIVERY_LABELS = {
  Preparing: 'Mark preparing',
  OutForDelivery: 'Mark on the way',
  Delivered: 'Mark delivered',
}
const DELIVERY_ORDER = ['Pending', 'Preparing', 'OutForDelivery', 'Delivered']

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

  // Live refresh: when a customer places a new order, the backend pushes to all admins.
  // Reload the list on that signal so new orders appear without a manual refresh.
  const connectionRef = useRef(null)
  useEffect(() => {
    const connection = createOrderHubConnection()
    connectionRef.current = connection

    connection.on(ADMIN_ORDERS_CHANGED_EVENT, () => {
      load()
    })

    connection.start().catch(() => {
      /* live refresh is an enhancement; the page still works without it */
    })

    return () => {
      connection.off(ADMIN_ORDERS_CHANGED_EVENT)
      connection.stop()
    }
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
                    {order.customerPhone && (
                      <>
                        {' | '}
                        <a href={`tel:${order.customerPhone}`}>📞 {order.customerPhone}</a>
                      </>
                    )}
                  </span>
                </div>
                <div className="order-badges">
                  <StatusBadge value={order.status} />
                  <StatusBadge value={order.paymentStatus} />
                  {order.status === 'Approved' && <StatusBadge value={order.deliveryStatus} />}
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

              {order.status === 'Approved' && (
                <div className="order-actions">
                  {DELIVERY_STEPS.map((step) => {
                    const done = DELIVERY_ORDER.indexOf(order.deliveryStatus) >= DELIVERY_ORDER.indexOf(step)
                    return (
                      <button
                        key={step}
                        type="button"
                        className="btn btn-outline btn-sm"
                        disabled={done}
                        onClick={() => run(() => api.setDeliveryStatus(order.id, step))}
                      >
                        {DELIVERY_LABELS[step]}
                      </button>
                    )
                  })}
                </div>
              )}
            </article>
          ))}
        </div>
      )}
    </section>
  )
}
