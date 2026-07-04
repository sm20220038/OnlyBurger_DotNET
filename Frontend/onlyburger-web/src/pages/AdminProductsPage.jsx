import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/client'
import { formatPrice } from '../utils/format'
import Message from '../components/Message'

const EMPTY_FORM = { name: '', description: '', price: '' }

export default function AdminProductsPage() {
  const [products, setProducts] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [form, setForm] = useState(EMPTY_FORM)
  const [editingId, setEditingId] = useState(null)
  const [busy, setBusy] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const data = await api.getProducts()
      setProducts(data || [])
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    load()
  }, [load])

  const update = (field) => (event) => setForm({ ...form, [field]: event.target.value })

  const resetForm = () => {
    setForm(EMPTY_FORM)
    setEditingId(null)
  }

  const submit = async (event) => {
    event.preventDefault()
    setError('')
    setNotice('')
    setBusy(true)
    const payload = {
      name: form.name.trim(),
      description: form.description.trim(),
      price: Number(form.price),
    }
    try {
      if (editingId) {
        await api.updateProduct(editingId, payload)
        setNotice(`Updated ${payload.name}.`)
      } else {
        await api.createProduct(payload)
        setNotice(`Added ${payload.name}.`)
      }
      resetForm()
      await load()
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  const startEdit = (product) => {
    setEditingId(product.id)
    setForm({
      name: product.name,
      description: product.description,
      price: String(product.price),
    })
  }

  const remove = async (product) => {
    setError('')
    setNotice('')
    try {
      await api.deleteProduct(product.id)
      setNotice(`Deleted ${product.name}.`)
      if (editingId === product.id) {
        resetForm()
      }
      await load()
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <section>
      <h1>Manage menu</h1>
      <Message type="success">{notice}</Message>
      <Message type="error">{error}</Message>

      <div className="admin-layout">
        <form onSubmit={submit} className="form admin-form">
          <h2>{editingId ? 'Edit product' : 'Add product'}</h2>
          <label className="field">
            <span>Name</span>
            <input type="text" value={form.name} onChange={update('name')} maxLength={100} required />
          </label>
          <label className="field">
            <span>Description</span>
            <textarea
              value={form.description}
              onChange={update('description')}
              maxLength={500}
              rows={3}
            />
          </label>
          <label className="field">
            <span>Price (RSD)</span>
            <input
              type="number"
              value={form.price}
              onChange={update('price')}
              min="1"
              step="1"
              required
            />
          </label>
          <div className="form-actions">
            <button type="submit" className="btn btn-primary" disabled={busy}>
              {editingId ? 'Save changes' : 'Add product'}
            </button>
            {editingId && (
              <button type="button" className="btn btn-outline" onClick={resetForm}>
                Cancel
              </button>
            )}
          </div>
        </form>

        <div className="admin-table-wrap">
          {loading ? (
            <p className="muted">Loading products...</p>
          ) : (
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Description</th>
                  <th className="num">Price</th>
                  <th className="actions">Actions</th>
                </tr>
              </thead>
              <tbody>
                {products.map((product) => (
                  <tr key={product.id}>
                    <td>{product.name}</td>
                    <td className="muted">{product.description}</td>
                    <td className="num">{formatPrice(product.price)}</td>
                    <td className="actions">
                      <button
                        type="button"
                        className="btn btn-outline btn-sm"
                        onClick={() => startEdit(product)}
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        className="btn btn-danger btn-sm"
                        onClick={() => remove(product)}
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </section>
  )
}
