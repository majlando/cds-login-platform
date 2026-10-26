import { useState, type FormEvent } from 'react'
import { login, type Session } from '../api/api'

type Props = {
  onLogin: (session: Session) => void
}

export function LoginPage({ onLogin }: Props) {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')

  async function submit(event: FormEvent) {
    event.preventDefault() // Stop the browser from reloading the page
    try {
      const session = await login(username, password)
      onLogin(session)
    } catch (err: any) {
      setError(err.message)
    }
  }

  return (
    <main>
      <h1>Log in</h1>
      <form onSubmit={submit}>
        <input placeholder="Username" value={username} onChange={e => setUsername(e.target.value)} />
        <input type="password" placeholder="Password" value={password} onChange={e => setPassword(e.target.value)} />
        <button>Log in</button>
      </form>
      {error && <p className="error">{error}</p>}
    </main>
  )
}
