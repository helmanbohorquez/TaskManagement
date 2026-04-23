import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import api, { storeToken, clearToken, getToken } from '../api/client.js'

const AuthContext = createContext(null)

function decodeJwtEmail(token) {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return payload.email || null
  } catch {
    return null
  }
}

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => getToken())
  const [email, setEmail] = useState(() => {
    const t = getToken()
    return t ? decodeJwtEmail(t) : null
  })

  useEffect(() => {
    if (token) storeToken(token)
    else clearToken()
  }, [token])

  const login = async (emailValue, password) => {
    const { data } = await api.post('/api/auth/login', { email: emailValue, password })
    setToken(data.token)
    setEmail(data.email)
    return data
  }

  const register = async (emailValue, password) => {
    const { data } = await api.post('/api/auth/register', { email: emailValue, password })
    setToken(data.token)
    setEmail(data.email)
    return data
  }

  const logout = () => {
    setToken(null)
    setEmail(null)
  }

  const value = useMemo(
    () => ({ token, email, isAuthenticated: Boolean(token), login, register, logout }),
    [token, email]
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export const useAuth = () => {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
