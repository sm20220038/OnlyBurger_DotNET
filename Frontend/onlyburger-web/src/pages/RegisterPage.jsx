import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useCart } from '../context/CartContext'
import Message from '../components/Message'

export default function RegisterPage() {
  const { register } = useAuth()
  const { refresh } = useCart()
  const navigate = useNavigate()

  const [form, setForm] = useState({ username: '', email: '', phoneNumber: '', password: '' })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const update = (field) => (event) => setForm({ ...form, [field]: event.target.value })

  const handleSubmit = async (event) => {
    event.preventDefault()
    setError('')
    setBusy(true)
    try {
      await register(form.username.trim(), form.email.trim(), form.phoneNumber.trim(), form.password)
      await refresh()
      navigate('/', { replace: true })
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <section className="auth-page">
      <div className="auth-card">
        <h1>Create your account</h1>
        <p className="muted">Sign up to start ordering.</p>

        <Message type="error">{error}</Message>

        <form onSubmit={handleSubmit} className="form">
          <label className="field">
            <span>Username</span>
            <input
              type="text"
              value={form.username}
              onChange={update('username')}
              autoComplete="username"
              required
            />
          </label>
          <label className="field">
            <span>Email</span>
            <input
              type="email"
              value={form.email}
              onChange={update('email')}
              autoComplete="email"
              required
            />
          </label>
          <label className="field">
            <span>Phone number</span>
            <input
              type="tel"
              value={form.phoneNumber}
              onChange={update('phoneNumber')}
              autoComplete="tel"
              placeholder="+381 64 123 4567"
              required
            />
            <small className="field-hint">So the restaurant can reach you about your delivery.</small>
          </label>
          <label className="field">
            <span>Password</span>
            <input
              type="password"
              value={form.password}
              onChange={update('password')}
              autoComplete="new-password"
              minLength={6}
              required
            />
            <small className="field-hint">At least 6 characters.</small>
          </label>
          <button type="submit" className="btn btn-primary btn-block" disabled={busy}>
            {busy ? 'Creating account' : 'Sign up'}
          </button>
        </form>

        <p className="auth-switch">
          Already have an account? <Link to="/login">Log in</Link>
        </p>
      </div>
    </section>
  )
}
