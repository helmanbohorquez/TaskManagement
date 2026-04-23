# User story

> **As a** busy professional,
> **I want to** sign up for an account, log in, and manage my personal tasks (title, description, status, and due date),
> **so that** I can track what I need to do without mixing my list with anyone else's.

## Acceptance criteria

- I can create an account with an email and a strong password.
- I can log in and stay logged in until my token expires.
- I can create a task with a title, optional description, and due date. Newly created tasks start as `Pending`.
- I can list all my tasks, optionally filtered by status (`Pending`, `InProgress`, `Done`).
- I can edit any of my tasks, including changing the status.
- I can delete my tasks.
- I cannot see, edit, or delete tasks that belong to other users.
- The UI is responsive and works well on mobile and desktop.
