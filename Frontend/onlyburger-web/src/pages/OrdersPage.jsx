import { useCallback, useEffect, useState } from 'react'
import { useLocation } from 'react-router-dom'
import { api } from '../api/client'
import { formatDateTime, formatPrice } from '../utils/format'
import StatusBadge from '../components/StatusBadge'
import Message from '../components/Message'

export default function OrdersPage() {
  const location = useLocation()
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState(location.state?.placed ? 'Your order has been placed.' : '')
  const [editing, setEditing] = useState(null)
  const [editValue, setEditValue] = useState('')

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const data = await api.getMyOrders()
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

  const startEdit = (order) => {
    setEditing(order.id)
    setEditValue(order.deliveryLocation)
  }

  const saveEdit = async (orderId) => {
    await run(async () => {
      await api.updateOrder(orderId, { deliveryLocation: editValue.trim() })
      setEditing(null)
    })
  }

  return (
    <section>
      <h1>My orders</h1>
      <Message type="success">{notice}</Message>
      <Message type="error">{error}</Message>

      {loading ? (
        <p className="muted">Loading orders...</p>
      ) : orders.length === 0 ? (
        <p className="muted">You have not placed any orders yet.</p>
      ) : (
        <div className="order-list">
          {orders.map((order) => {
            const isPending = order.status === 'Pending'
            const isUnpaid = order.paymentStatus === 'Unpaid'
            return (
              <article key={order.id} className="order-card">
                <header className="order-head">
                  <div>
                    <h3>Order #{order.id}</h3>
                    <span className="muted">{formatDateTime(order.orderDateTime)}</span>
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
                  {editing === order.id ? (
                    <div className="edit-delivery">
                      <input
                        type="text"
                        value={editValue}
                        onChange={(event) => setEditValue(event.target.value)}
                      />
                      <button
                        type="button"
                        className="btn btn-primary btn-sm"
                        onClick={() => saveEdit(order.id)}
                      >
                        Save
                      </button>
                      <button
                        type="button"
                        className="btn btn-outline btn-sm"
                        onClick={() => setEditing(null)}
                      >
                        Cancel
                      </button>
                    </div>
                  ) : (
                    <span className="muted">Deliver to: {order.deliveryLocation}</span>
                  )}
                  <span className="order-total">Total {formatPrice(order.totalPrice)}</span>
                </div>

                <div className="order-actions">
                  {isUnpaid && order.status !== 'Rejected' && (
                    <button
                      type="button"
                      className="btn btn-primary btn-sm"
                      onClick={() => run(() => api.payOrder(order.id))}
                    >
                      Pay now
                    </button>
                  )}
                  {isPending && editing !== order.id && (
                    <button
                      type="button"
                      className="btn btn-outline btn-sm"
                      onClick={() => startEdit(order)}
                    >
                      Edit delivery
                    </button>
                  )}
                  {isPending && (
                    <button
                      type="button"
                      className="btn btn-danger btn-sm"
                      onClick={() => run(() => api.deleteOrder(order.id))}
                    >
                      Cancel order
                    </button>
                  )}
                </div>
              </article>
            )
          })}
        </div>
      )}
    </section>
  )
}
