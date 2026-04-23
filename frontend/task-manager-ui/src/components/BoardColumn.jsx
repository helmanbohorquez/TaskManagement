import { useState } from 'react'
import TaskCard from './TaskCard.jsx'

const columnAccent = {
  Pending: 'border-t-amber-400',
  InProgress: 'border-t-blue-400',
  Done: 'border-t-emerald-400'
}

export default function BoardColumn({
  status,
  title,
  tasks,
  draggingTaskId,
  onDropTask,
  onDragStartTask,
  onDragEndTask,
  onEditTask,
  onDeleteTask
}) {
  const [isOver, setIsOver] = useState(false)

  const handleDragOver = (e) => {
    e.preventDefault()
    e.dataTransfer.dropEffect = 'move'
    if (!isOver) setIsOver(true)
  }

  const handleDragLeave = (e) => {
    // Only clear when leaving the column element itself, not when crossing
    // into a child (relatedTarget is null when leaving the window/column root).
    if (e.currentTarget.contains(e.relatedTarget)) return
    setIsOver(false)
  }

  const handleDrop = (e) => {
    e.preventDefault()
    setIsOver(false)
    const taskId = e.dataTransfer.getData('text/plain')
    if (taskId) onDropTask(taskId, status)
  }

  return (
    <section
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
      className={
        'flex flex-col min-w-0 rounded-lg border border-t-4 bg-slate-50 p-3 transition-colors ' +
        (columnAccent[status] || 'border-t-slate-300') + ' ' +
        (isOver ? 'bg-brand-50 border-brand-500' : 'border-slate-200')
      }
    >
      <header className="flex items-center justify-between mb-3 px-1">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-700">
          {title}
        </h2>
        <span className="text-xs font-medium text-slate-500 bg-white border border-slate-200 rounded-full px-2 py-0.5">
          {tasks.length}
        </span>
      </header>

      <div className="flex flex-col gap-3 min-h-24">
        {tasks.length === 0 ? (
          <div className="text-xs text-slate-400 italic text-center py-6 border-2 border-dashed border-slate-200 rounded-md">
            Drop tasks here
          </div>
        ) : (
          tasks.map((t) => (
            <TaskCard
              key={t.id}
              task={t}
              onEdit={onEditTask}
              onDelete={onDeleteTask}
              onDragStart={onDragStartTask}
              onDragEnd={onDragEndTask}
              isDragging={draggingTaskId === t.id}
            />
          ))
        )}
      </div>
    </section>
  )
}
