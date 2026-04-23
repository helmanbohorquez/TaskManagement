import { useCallback, useEffect, useMemo, useState } from 'react'
import api from '../api/client.js'
import BoardColumn from '../components/BoardColumn.jsx'
import TaskForm from '../components/TaskForm.jsx'

const COLUMNS = [
  { status: 'Pending', title: 'To do' },
  { status: 'InProgress', title: 'In progress' },
  { status: 'Done', title: 'Done' }
]

export default function Tasks() {
  const [tasks, setTasks] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [formOpen, setFormOpen] = useState(false)
  const [editingTask, setEditingTask] = useState(null)
  const [draggingTaskId, setDraggingTaskId] = useState(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const { data } = await api.get('/api/tasks')
      setTasks(data)
    } catch (err) {
      setError(err?.response?.data?.message || 'Failed to load tasks.')
    } finally {
      setLoading(false)
    }
  }, [])

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

  const handleMoveTask = useCallback(async (taskId, newStatus) => {
    // Always clear the drag state: the source card usually unmounts on the
    // optimistic re-render, so its `dragend` event never fires.
    setDraggingTaskId(null)

    const current = tasks.find((t) => t.id === taskId)
    if (!current || current.status === newStatus) return

    const previous = tasks
    setTasks((prev) =>
      prev.map((t) => (t.id === taskId ? { ...t, status: newStatus } : t))
    )

    try {
      const { data } = await api.put(`/api/tasks/${taskId}`, {
        title: current.title,
        description: current.description,
        dueDate: current.dueDate,
        status: newStatus
      })
      setTasks((prev) => prev.map((t) => (t.id === data.id ? data : t)))
    } catch (err) {
      setTasks(previous)
      setError(err?.response?.data?.message || 'Failed to move task.')
    }
  }, [tasks])

  const tasksByStatus = useMemo(() => {
    const groups = { Pending: [], InProgress: [], Done: [] }
    tasks.forEach((t) => {
      if (groups[t.status]) groups[t.status].push(t)
    })
    return groups
  }, [tasks])

  const totalCount = tasks.length

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-semibold">Your board</h1>
          <p className="text-sm text-slate-600">
            {totalCount} total &middot; drag cards between columns to change status
          </p>
        </div>
        <button
          onClick={openCreate}
          className="px-4 py-2 rounded-md bg-brand-500 text-white hover:bg-brand-600"
        >
          + New task
        </button>
      </div>

      {error && (
        <div className="mb-4 flex items-start justify-between gap-3 rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
          <span>{error}</span>
          <button
            onClick={() => setError('')}
            className="text-rose-500 hover:text-rose-700"
            aria-label="Dismiss error"
          >
            &times;
          </button>
        </div>
      )}

      {loading ? (
        <p className="text-slate-500">Loading...</p>
      ) : totalCount === 0 ? (
        <div className="rounded-lg border border-dashed border-slate-300 p-10 text-center text-slate-500">
          No tasks yet. Click "New task" to create one.
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-3">
          {COLUMNS.map((col) => (
            <BoardColumn
              key={col.status}
              status={col.status}
              title={col.title}
              tasks={tasksByStatus[col.status]}
              draggingTaskId={draggingTaskId}
              onDropTask={handleMoveTask}
              onDragStartTask={(task) => setDraggingTaskId(task.id)}
              onDragEndTask={() => setDraggingTaskId(null)}
              onEditTask={openEdit}
              onDeleteTask={handleDelete}
            />
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
