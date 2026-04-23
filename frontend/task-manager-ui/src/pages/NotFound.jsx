import { Link } from 'react-router-dom'

export default function NotFound() {
  return (
    <div className="max-w-md mx-auto mt-24 text-center px-4">
      <h1 className="text-4xl font-semibold">404</h1>
      <p className="mt-2 text-slate-600">The page you're looking for doesn't exist.</p>
      <Link to="/" className="mt-4 inline-block text-brand-600 hover:underline">Go home</Link>
    </div>
  )
}
