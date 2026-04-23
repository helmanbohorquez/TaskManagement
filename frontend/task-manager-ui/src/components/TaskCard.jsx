const statusStyles = {
  Pending: 'bg-amber-100 text-amber-800',
  InProgress: 'bg-blue-100 text-blue-800',
  Done: 'bg-emerald-100 text-emerald-800'
}

function formatDate(iso) {
  try {
    // Due dates are stored as UTC midnight; format in UTC so the displayed day
    // matches the one the user picked, regardless of their local timezone.
    return new Date(iso).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      timeZone: 'UTC'
    })
  } catch {
    return iso
  }
}

export default function TaskCard({ task, onEdit, onDelete, onDragStart, onDragEnd, isDragging }) {
  const handleDragStart = (e) => {
    e.dataTransfer.effectAllowed = 'move'
    e.dataTransfer.setData('text/plain', task.id)
    onDragStart?.(task)
  }

  const handleDragEnd = () => {
    onDragEnd?.(task)
  }

  return (
    <article
      draggable
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
      className={
        'rounded-lg border border-slate-200 bg-white p-4 shadow-sm hover:shadow transition cursor-grab active:cursor-grabbing ' +
        (isDragging ? 'opacity-50' : '')
      }
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <h3 className="font-medium text-slate-900 truncate">{task.title}</h3>
          {task.description ? (
            <p className="mt-1 text-sm text-slate-600 line-clamp-3">{task.description}</p>
          ) : null}
        </div>
        <div className="shrink-0">
          {task.isExpired ? (
            <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-semibold bg-rose-100 text-rose-800 border border-rose-200">
              Expired
            </span>
          ) : (
            <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${statusStyles[task.status] || 'bg-slate-100 text-slate-800'}`}>
              {task.status}
            </span>
          )}
        </div>
      </div>

      <div className="mt-3 flex items-center justify-between text-sm">
        <span className={task.isExpired ? 'text-rose-600 font-medium' : 'text-slate-500'}>
          Due {formatDate(task.dueDate)}
        </span>
        <div className="flex gap-2">
          <button
            onClick={() => onEdit(task)}
            className="px-2.5 py-1 rounded-md border border-slate-300 text-slate-700 hover:bg-slate-100"
          >
            Edit
          </button>
          <button
            onClick={() => onDelete(task)}
            className="px-2.5 py-1 rounded-md bg-rose-600 text-white hover:bg-rose-700"
          >
            Delete
          </button>
        </div>
      </div>
    </article>
  )
}
