// Thin fetch wrapper around the OnlyBurger backend.
// Reads the JWT from localStorage and attaches it as a Bearer token.

const BASE = import.meta.env.VITE_API_BASE_URL ?? ''

const TOKEN_KEY = 'ob_token'

export function getToken() {
  return localStorage.getItem(TOKEN_KEY)
}

export function setToken(token) {
  if (token) {
    localStorage.setItem(TOKEN_KEY, token)
  } else {
    localStorage.removeItem(TOKEN_KEY)
  }
}

async function request(path, { method = 'GET', body, auth = true } = {}) {
  const headers = {}
  if (body !== undefined) {
    headers['Content-Type'] = 'application/json'
  }
  const token = getToken()
  if (auth && token) {
    headers['Authorization'] = `Bearer ${token}`
  }

  const response = await fetch(`${BASE}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  })

  if (response.status === 204) {
    return null
  }

  const text = await response.text()
  const data = text ? JSON.parse(text) : null

  if (!response.ok) {
    const message =
      (data && (data.detail || data.title)) || `Request failed with status ${response.status}`
    const error = new Error(message)
    error.status = response.status
    error.data = data
    throw error
  }

  return data
}

export const api = {
  // Auth
  login: (usernameOrEmail, password) =>
    request('/api/auth/login', { method: 'POST', body: { usernameOrEmail, password }, auth: false }),
  register: (username, email, password) =>
    request('/api/auth/register', { method: 'POST', body: { username, email, password }, auth: false }),

  // Products
  getProducts: () => request('/api/products', { auth: false }),
  getProduct: (id) => request(`/api/products/${id}`, { auth: false }),
  createProduct: (product) => request('/api/products', { method: 'POST', body: product }),
  updateProduct: (id, product) => request(`/api/products/${id}`, { method: 'PUT', body: product }),
  deleteProduct: (id) => request(`/api/products/${id}`, { method: 'DELETE' }),

  // Cart
  getCart: () => request('/api/cart'),
  addToCart: (productId, quantity) =>
    request('/api/cart/items', { method: 'POST', body: { productId, quantity } }),
  updateCartItem: (productId, quantity) =>
    request(`/api/cart/items/${productId}`, { method: 'PUT', body: { quantity } }),
  removeCartItem: (productId) => request(`/api/cart/items/${productId}`, { method: 'DELETE' }),

  // Orders (customer)
  createOrder: (deliveryLocation) =>
    request('/api/orders', { method: 'POST', body: { deliveryLocation } }),
  getMyOrders: () => request('/api/orders/mine'),
  getOrder: (id) => request(`/api/orders/${id}`),
  updateOrder: (id, body) => request(`/api/orders/${id}`, { method: 'PUT', body }),
  deleteOrder: (id) => request(`/api/orders/${id}`, { method: 'DELETE' }),
  payOrder: (id) => request(`/api/orders/${id}/pay`, { method: 'POST' }),

  // Orders (admin)
  getAllOrders: () => request('/api/orders'),
  approveOrder: (id) => request(`/api/orders/${id}/approve`, { method: 'POST' }),
  rejectOrder: (id) => request(`/api/orders/${id}/reject`, { method: 'POST' }),
}
