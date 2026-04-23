import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'

export default function Navbar() {
  const { isAuthenticated, email, logout } = useAuth()
  const navigate = useNavigate()

  const onLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <header className="bg-white border-b border-slate-200">
      <div className="max-w-5xl mx-auto px-4 py-3 flex items-center justify-between">
        <Link to="/" className="text-lg font-semibold text-brand-700">Task Manager</Link>
        <nav className="flex items-center gap-4 text-sm">
          {isAuthenticated ? (
            <>
              <span className="text-slate-600 hidden sm:inline">{email}</span>
              <button
                onClick={onLogout}
                className="px-3 py-1.5 rounded-md bg-slate-900 text-white hover:bg-slate-800"
              >
                Log out
              </button>
            </>
          ) : (
            <>
              <Link to="/login" className="text-slate-700 hover:text-brand-600">Log in</Link>
              <Link to="/register" className="px-3 py-1.5 rounded-md bg-brand-500 text-white hover:bg-brand-600">
                Sign up
              </Link>
            </>
          )}
        </nav>
      </div>
    </header>
  )
}
