import { Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useCart } from '../context/CartContext'

export default function Navbar() {
  const { isAuthenticated, isAdmin, user, logout } = useAuth()
  const { itemCount, clearLocal } = useCart()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    clearLocal()
    navigate('/')
  }

  return (
    <header className="navbar">
      <div className="navbar-inner container">
        <Link to="/" className="brand">
          <span className="brand-mark">OB</span>
          <span className="brand-name">OnlyBurger</span>
        </Link>

        <nav className="nav-links">
          <NavLink to="/" end className="nav-link">
            Menu
          </NavLink>

          {isAuthenticated && (
            <>
              <NavLink to="/cart" className="nav-link">
                Cart{itemCount > 0 ? <span className="badge">{itemCount}</span> : null}
              </NavLink>
              <NavLink to="/orders" className="nav-link">
                My Orders
              </NavLink>
            </>
          )}

          {isAdmin && (
            <>
              <NavLink to="/admin/products" className="nav-link">
                Manage Menu
              </NavLink>
              <NavLink to="/admin/orders" className="nav-link">
                Manage Orders
              </NavLink>
            </>
          )}
        </nav>

        <div className="nav-account">
          {isAuthenticated ? (
            <>
              <span className="nav-user">
                {user.username}
                {isAdmin ? <span className="role-tag">Admin</span> : null}
              </span>
              <button type="button" className="btn btn-outline btn-sm" onClick={handleLogout}>
                Log out
              </button>
            </>
          ) : (
            <>
              <Link to="/login" className="btn btn-outline btn-sm">
                Log in
              </Link>
              <Link to="/register" className="btn btn-primary btn-sm">
                Sign up
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  )
}
