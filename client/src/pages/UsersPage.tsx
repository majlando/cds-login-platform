import { useEffect, useState, type FormEvent } from 'react'
import { createUser, deleteUser, getUsers, updateUser, type Role, type Session, type User } from '../api/api'

// Which roles can the logged-in user create, edit and delete?
// This only decides which buttons are shown. The backend does the real check.
function rolesICanManage(myRole: Role): Role[] {
  if (myRole === 'Admin') return ['User', 'SuperUser', 'Admin']
  if (myRole === 'SuperUser') return ['User']
  return []
}

type Props = {
  session: Session
  onLogout: () => void
}

export function UsersPage({ session, onLogout }: Props) {
  const [users, setUsers] = useState<User[]>([])
  const [error, setError] = useState('')

  // The form. editingId is the id of the user being edited, or null when creating a new user.
  const [editingId, setEditingId] = useState<number | null>(null)
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState<Role>('User')

  const roles = rolesICanManage(session.role)
  const canManage = roles.length > 0

  async function loadUsers() {
    try {
      setUsers(await getUsers())
    } catch (err: any) {
      setError(err.message)
    }
  }

  // Load the users once, when the page opens
  useEffect(() => {
    loadUsers()
  }, [])

  // Fill the form with a user's data, so it can be edited
  function startEdit(user: User) {
    setEditingId(user.id)
    setUsername(user.username)
    setPassword('')
    setRole(user.role)
  }

  function resetForm() {
    setEditingId(null)
    setUsername('')
    setPassword('')
    setRole('User')
  }

  // Create a new user, or update the one we are editing
  async function submit(event: FormEvent) {
    event.preventDefault() // Stop the browser from reloading the page
    setError('')
    const user = { username: username, password: password, role: role }
    try {
      if (editingId === null) {
        await createUser(user)
      } else {
        await updateUser(editingId, user)
      }
      resetForm()
      await loadUsers()
    } catch (err: any) {
      setError(err.message)
    }
  }

  async function remove(user: User) {
    setError('')
    try {
      await deleteUser(user.id)
      await loadUsers()
    } catch (err: any) {
      setError(err.message)
    }
  }

  return (
    <main>
      <p>
        Logged in as <b>{session.username}</b> ({session.role}) <button onClick={onLogout}>Log out</button>
      </p>
      <h1>Users</h1>

      {/* Only SuperUsers and Admins see the form */}
      {canManage && (
        <form onSubmit={submit}>
          <input placeholder="Username" value={username} onChange={e => setUsername(e.target.value)} />
          <input
            type="password"
            placeholder={editingId === null ? 'Password' : 'New password (optional)'}
            value={password}
            onChange={e => setPassword(e.target.value)}
          />
          <select value={role} onChange={e => setRole(e.target.value as Role)}>
            {roles.map(r => <option key={r}>{r}</option>)}
          </select>
          <button>{editingId === null ? 'Create user' : 'Save'}</button>
          {editingId !== null && <button type="button" onClick={resetForm}>Cancel</button>}
        </form>
      )}

      {error && <p className="error">{error}</p>}

      <table>
        <thead>
          <tr>
            <th>Id</th>
            <th>Username</th>
            <th>Role</th>
            {canManage && <th></th>}
          </tr>
        </thead>
        <tbody>
          {users.map(user => (
            <tr key={user.id}>
              <td>{user.id}</td>
              <td>{user.username}</td>
              <td>{user.role}</td>
              {canManage && (
                <td>
                  {/* Only show the buttons for users we are allowed to manage */}
                  {roles.includes(user.role) && (
                    <>
                      <button onClick={() => startEdit(user)}>Edit</button>
                      <button onClick={() => remove(user)}>Delete</button>
                    </>
                  )}
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </main>
  )
}
