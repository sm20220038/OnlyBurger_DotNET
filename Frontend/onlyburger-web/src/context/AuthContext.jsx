import { createContext, useContext, useMemo, useState } from 'react'
import { api, setToken } from '../api/client'

const USER_KEY = 'ob_user'

const AuthContext = createContext(null)

function loadStoredUser() {
  try {
    const raw = localStorage.getItem(USER_KEY)
    return raw ? JSON.parse(raw) : null
  } catch {
    return null
  }
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(loadStoredUser)

  const persist = (authResponse) => {
    const nextUser = {
      userId: authResponse.userId,
      username: authResponse.username,
      email: authResponse.email,
      role: authResponse.role,
    }
    setToken(authResponse.token)
    localStorage.setItem(USER_KEY, JSON.stringify(nextUser))
    setUser(nextUser)
    return nextUser
  }

  const login = async (usernameOrEmail, password) => {
    const result = await api.login(usernameOrEmail, password)
    return persist(result)
  }

  const register = async (username, email, password) => {
    const result = await api.register(username, email, password)
    return persist(result)
  }

  const logout = () => {
    setToken(null)
    localStorage.removeItem(USER_KEY)
    setUser(null)
  }

  const value = useMemo(
    () => ({
      user,
      isAuthenticated: Boolean(user),
      isAdmin: user?.role === 'Admin',
      login,
      register,
      logout,
    }),
    [user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
