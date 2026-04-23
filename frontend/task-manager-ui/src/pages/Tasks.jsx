import { useCallback, useEffect, useMemo, useState } from 'react'
import api from '../api/client.js'
import TaskCard from '../components/TaskCard.jsx'
import TaskForm from '../components/TaskForm.jsx'

const FILTERS = [
  { id: 'all', label: 'All' },
  { id: 'Pending', label: 'Pending' },
  { id: 'InProgress', label: 'In progress' },
  { id: 'Done', label: 'Done' }
]

export default function Tasks() {
  const [tasks, setTasks] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [filter, setFilter] = useState('all')
  const [formOpen, setFormOpen] = useState(false)
  const [editingTask, setEditingTask] = useState(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const params = filter === 'all' ? {} : { status: filter }
      const { data } = await api.get('/api/tasks', { params })
      setTasks(data)
    } catch (err) {
      setError(err?.response?.data?.message || 'Failed to load tasks.')
    } finally {
      setLoading(false)
    }
  }, [filter])

  useEffect(() => {
    load()
  }, [load])

  const openCreate = () => {
    setEditingTask(null)
    setFormOpen(true)
  }
  const openEdit = (task) => {
    setEditingTask(task)
    setFormOpen(true)
  }
  const closeForm = () => {
    setFormOpen(false)
    setEditingTask(null)
  }

  const handleSubmit = async (values) => {
    if (editingTask) {
      const { data } = await api.put(`/api/tasks/${editingTask.id}`, values)
      setTasks((prev) => prev.map((t) => (t.id === data.id ? data : t)))
    } else {
      const { data } = await api.post('/api/tasks', {
        title: values.title,
        description: values.description,
        dueDate: values.dueDate
      })
      setTasks((prev) => [data, ...prev])
    }
    closeForm()
  }

  const handleDelete = async (task) => {
    if (!confirm(`Delete "${task.title}"?`)) return
    await api.delete(`/api/tasks/${task.id}`)
    setTasks((prev) => prev.filter((t) => t.id !== task.id))
  }

  const stats = useMemo(() => {
    const counts = { total: tasks.length, Pending: 0, InProgress: 0, Done: 0 }
    tasks.forEach((t) => {
      if (counts[t.status] !== undefined) counts[t.status] += 1
    })
    return counts
  }, [tasks])

  return (
    <div className="max-w-5xl mx-auto px-4 py-6">
      <div className="flex items-center justify-between mb-4">
        <div>
          <h1 className="text-2xl font-semibold">Your tasks</h1>
          <p className="text-sm text-slate-600">
            {stats.total} total &middot; {stats.Pending} pending &middot; {stats.InProgress} in progress &middot; {stats.Done} done
          </p>
        </div>
        <button
          onClick={openCreate}
          className="px-4 py-2 rounded-md bg-brand-500 text-white hover:bg-brand-600"
        >
          + New task
        </button>
      </div>

      <div className="flex gap-2 mb-4 flex-wrap">
        {FILTERS.map((f) => (
          <button
            key={f.id}
            onClick={() => setFilter(f.id)}
            className={
              'px-3 py-1.5 rounded-full text-sm border transition ' +
              (filter === f.id
                ? 'bg-brand-500 text-white border-brand-500'
                : 'bg-white text-slate-700 border-slate-300 hover:bg-slate-50')
            }
          >
            {f.label}
          </button>
        ))}
      </div>

      {error && <p className="mb-4 text-sm text-rose-600">{error}</p>}

      {loading ? (
        <p className="text-slate-500">Loading...</p>
      ) : tasks.length === 0 ? (
        <div className="rounded-lg border border-dashed border-slate-300 p-10 text-center text-slate-500">
          No tasks yet. Click "New task" to create one.
        </div>
      ) : (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {tasks.map((t) => (
            <TaskCard key={t.id} task={t} onEdit={openEdit} onDelete={handleDelete} />
          ))}
        </div>
      )}

      <TaskForm
        open={formOpen}
        initial={editingTask}
        onClose={closeForm}
        onSubmit={handleSubmit}
      />
    </div>
  )
}
