import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useCart } from '../context/CartContext'
import { formatPrice } from '../utils/format'
import Message from '../components/Message'

export default function CartPage() {
  const { cart, updateItem, removeItem, refresh } = useCart()
  const navigate = useNavigate()

  const [deliveryLocation, setDeliveryLocation] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const isEmpty = cart.items.length === 0

  const changeQuantity = async (productId, quantity) => {
    if (quantity < 1) {
      return
    }
    setError('')
    try {
      await updateItem(productId, quantity)
    } catch (err) {
      setError(err.message)
    }
  }

  const remove = async (productId) => {
    setError('')
    try {
      await removeItem(productId)
    } catch (err) {
      setError(err.message)
    }
  }

  const placeOrder = async (event) => {
    event.preventDefault()
    setError('')
    setBusy(true)
    try {
      await api.createOrder(deliveryLocation.trim())
      await refresh()
      navigate('/orders', { state: { placed: true } })
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <section>
      <h1>Your cart</h1>
      <Message type="error">{error}</Message>

      {isEmpty ? (
        <div className="empty-state">
          <p>Your cart is empty.</p>
          <Link to="/" className="btn btn-primary">
            Browse the menu
          </Link>
        </div>
      ) : (
        <div className="cart-layout">
          <div className="cart-items">
            {cart.items.map((item) => (
              <div key={item.id} className="cart-row">
                <div className="cart-row-main">
                  <span className="cart-row-name">{item.productName}</span>
                  <span className="muted">{formatPrice(item.unitPrice)} each</span>
                </div>
                <div className="stepper">
                  <button
                    type="button"
                    className="stepper-btn"
                    aria-label="Decrease quantity"
                    onClick={() => changeQuantity(item.productId, item.quantity - 1)}
                  >
                    -
                  </button>
                  <span className="stepper-value">{item.quantity}</span>
                  <button
                    type="button"
                    className="stepper-btn"
                    aria-label="Increase quantity"
                    onClick={() => changeQuantity(item.productId, item.quantity + 1)}
                  >
                    +
                  </button>
                </div>
                <span className="cart-row-total">{formatPrice(item.lineTotal)}</span>
                <button
                  type="button"
                  className="btn btn-outline btn-sm"
                  onClick={() => remove(item.productId)}
                >
                  Remove
                </button>
              </div>
            ))}
          </div>

          <aside className="cart-summary">
            <h2>Checkout</h2>
            <div className="summary-row">
              <span>Total</span>
              <strong>{formatPrice(cart.totalPrice)}</strong>
            </div>
            <form onSubmit={placeOrder} className="form">
              <label className="field">
                <span>Delivery location</span>
                <input
                  type="text"
                  value={deliveryLocation}
                  onChange={(event) => setDeliveryLocation(event.target.value)}
                  placeholder="Street, number, city"
                  required
                />
              </label>
              <button type="submit" className="btn btn-primary btn-block" disabled={busy}>
                {busy ? 'Placing order' : 'Place order'}
              </button>
            </form>
            <p className="field-hint">Payment is simulated after the order is placed.</p>
          </aside>
        </div>
      )}
    </section>
  )
}
