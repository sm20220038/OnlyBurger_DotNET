import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import { api } from '../api/client'
import { useAuth } from './AuthContext'

const EMPTY_CART = { items: [], totalPrice: 0 }

const CartContext = createContext(null)

export function CartProvider({ children }) {
  const { isAuthenticated } = useAuth()
  const [cart, setCart] = useState(EMPTY_CART)
  const [loading, setLoading] = useState(false)

  const refresh = useCallback(async () => {
    if (!isAuthenticated) {
      setCart(EMPTY_CART)
      return
    }
    setLoading(true)
    try {
      const data = await api.getCart()
      setCart(data ?? EMPTY_CART)
    } catch {
      setCart(EMPTY_CART)
    } finally {
      setLoading(false)
    }
  }, [isAuthenticated])

  useEffect(() => {
    refresh()
  }, [refresh])

  const addItem = async (productId, quantity) => {
    const data = await api.addToCart(productId, quantity)
    setCart(data ?? EMPTY_CART)
  }

  const updateItem = async (productId, quantity) => {
    const data = await api.updateCartItem(productId, quantity)
    setCart(data ?? EMPTY_CART)
  }

  const removeItem = async (productId) => {
    const data = await api.removeCartItem(productId)
    setCart(data ?? EMPTY_CART)
  }

  const clearLocal = () => setCart(EMPTY_CART)

  // Number of unique products in the cart (distinct line items), not the summed quantity.
  const itemCount = useMemo(() => cart.items.length, [cart])

  const value = useMemo(
    () => ({ cart, itemCount, loading, refresh, addItem, updateItem, removeItem, clearLocal }),
    [cart, itemCount, loading, refresh],
  )

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>
}

export function useCart() {
  const context = useContext(CartContext)
  if (!context) {
    throw new Error('useCart must be used within a CartProvider')
  }
  return context
}
