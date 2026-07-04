import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../context/AuthContext'
import { useCart } from '../context/CartContext'
import ProductCard from '../components/ProductCard'
import Message from '../components/Message'

export default function MenuPage() {
  const { isAuthenticated } = useAuth()
  const { addItem } = useCart()
  const navigate = useNavigate()

  const [products, setProducts] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')

  useEffect(() => {
    let active = true
    api
      .getProducts()
      .then((data) => {
        if (active) {
          setProducts(data || [])
        }
      })
      .catch((err) => {
        if (active) {
          setError(err.message)
        }
      })
      .finally(() => {
        if (active) {
          setLoading(false)
        }
      })
    return () => {
      active = false
    }
  }, [])

  const handleAdd = async (productId, quantity) => {
    setError('')
    try {
      await addItem(productId, quantity)
      const product = products.find((p) => p.id === productId)
      setNotice(`Added ${quantity} x ${product ? product.name : 'item'} to your cart.`)
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <section>
      <div className="page-hero">
        <h1>Order your favorites</h1>
        <p>Handmade burgers and sides, ready in minutes. Build your order and check out.</p>
      </div>

      <Message type="success">{notice}</Message>
      <Message type="error">{error}</Message>

      {loading ? (
        <p className="muted">Loading menu...</p>
      ) : products.length === 0 ? (
        <p className="muted">No products on the menu yet.</p>
      ) : (
        <div className="product-grid">
          {products.map((product) => (
            <ProductCard
              key={product.id}
              product={product}
              canOrder={isAuthenticated}
              onAdd={handleAdd}
              onRequireLogin={() => navigate('/login', { state: { from: '/' } })}
            />
          ))}
        </div>
      )}
    </section>
  )
}
