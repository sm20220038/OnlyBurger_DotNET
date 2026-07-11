import { useCallback, useEffect, useRef, useState } from 'react'
import { useLocation } from 'react-router-dom'
import { api } from '../api/client'
import { createOrderHubConnection, ORDER_UPDATED_EVENT } from '../api/orderHub'
import { formatDateTime, formatPrice } from '../utils/format'
import StatusBadge from '../components/StatusBadge'
import DeliveryTracker from '../components/DeliveryTracker'
import Message from '../components/Message'

// Delivery-based views. "Active" hides delivered orders so the list stays focused on
// what's still on its way; "Delivered" brings the completed ones back.
const FILTERS = ['Active', 'Delivered', 'All']

const isDelivered = (order) => order.deliveryStatus === 'Delivered'

function matchesFilter(order, filter) {
  if (filter === 'Active') return !isDelivered(order)
  if (filter === 'Delivered') return isDelivered(order)
  return true
}

export default function OrdersPage() {
  const location = useLocation()
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState(location.state?.placed ? 'Your order has been placed.' : '')
  const [editing, setEditing] = useState(null)
  const [editValue, setEditValue] = useState('')
  const [liveOrderId, setLiveOrderId] = useState(null)
  const [filter, setFilter] = useState('Active')

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

  // Live order/delivery updates over SignalR. When the backend pushes an updated order,
  // merge it into the list in place — no refresh needed.
  const connectionRef = useRef(null)
  useEffect(() => {
    const connection = createOrderHubConnection()
    connectionRef.current = connection

    connection.on(ORDER_UPDATED_EVENT, (updated) => {
      setOrders((current) => {
        const exists = current.some((o) => o.id === updated.id)
        return exists
          ? current.map((o) => (o.id === updated.id ? updated : o))
          : [updated, ...current]
      })
      // Briefly highlight the order that just changed.
      setLiveOrderId(updated.id)
      setTimeout(() => setLiveOrderId((id) => (id === updated.id ? null : id)), 2500)
    })

    connection.start().catch(() => {
      /* tracking is a live enhancement; the page still works without it */
    })

    return () => {
      connection.off(ORDER_UPDATED_EVENT)
      connection.stop()
    }
  }, [])

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

  const counts = {
    Active: orders.filter((o) => matchesFilter(o, 'Active')).length,
    Delivered: orders.filter((o) => matchesFilter(o, 'Delivered')).length,
    All: orders.length,
  }
  const visibleOrders = orders.filter((o) => matchesFilter(o, filter))

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
        <>
          <div className="filter-bar">
            {FILTERS.map((option) => (
              <button
                key={option}
                type="button"
                className={`filter-chip ${filter === option ? 'active' : ''}`}
                onClick={() => setFilter(option)}
              >
                {option} ({counts[option]})
              </button>
            ))}
          </div>

          {visibleOrders.length === 0 ? (
            <p className="muted">
              {filter === 'Delivered'
                ? 'No delivered orders yet.'
                : 'No active orders — everything has been delivered.'}
            </p>
          ) : (
        <div className="order-list">
          {visibleOrders.map((order) => {
            const isPending = order.status === 'Pending'
            const isUnpaid = order.paymentStatus === 'Unpaid'
            return (
              <article
                key={order.id}
                className={`order-card ${liveOrderId === order.id ? 'order-card-live' : ''}`}
              >
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

                {order.status === 'Approved' && <DeliveryTracker status={order.deliveryStatus} />}

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
        </>
      )}
    </section>
  )
}
