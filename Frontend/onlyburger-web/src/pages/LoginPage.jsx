import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useCart } from '../context/CartContext'
import Message from '../components/Message'

export default function LoginPage() {
  const { login } = useAuth()
  const { refresh } = useCart()
  const navigate = useNavigate()
  const location = useLocation()
  const redirectTo = location.state?.from || '/'

  const [form, setForm] = useState({ usernameOrEmail: '', password: '' })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const update = (field) => (event) => setForm({ ...form, [field]: event.target.value })

  const handleSubmit = async (event) => {
    event.preventDefault()
    setError('')
    setBusy(true)
    try {
      await login(form.usernameOrEmail.trim(), form.password)
      await refresh()
      navigate(redirectTo, { replace: true })
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <section className="auth-page">
      <div className="auth-card">
        <h1>Log in</h1>
        <p className="muted">Welcome back. Sign in to place an order.</p>

        <Message type="error">{error}</Message>

        <form onSubmit={handleSubmit} className="form">
          <label className="field">
            <span>Username or email</span>
            <input
              type="text"
              value={form.usernameOrEmail}
              onChange={update('usernameOrEmail')}
              autoComplete="username"
              required
            />
          </label>
          <label className="field">
            <span>Password</span>
            <input
              type="password"
              value={form.password}
              onChange={update('password')}
              autoComplete="current-password"
              required
            />
          </label>
          <button type="submit" className="btn btn-primary btn-block" disabled={busy}>
            {busy ? 'Signing in' : 'Log in'}
          </button>
        </form>

        <p className="auth-switch">
          New here? <Link to="/register">Create an account</Link>
        </p>
      </div>
    </section>
  )
}
