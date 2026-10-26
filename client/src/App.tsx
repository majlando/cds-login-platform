import { useState } from 'react'
import { loadSession, saveSession, type Session } from './api/api'
import { LoginPage } from './pages/LoginPage'
import { UsersPage } from './pages/UsersPage'

// Shows the login page, or the users page when someone is logged in
export function App() {
  // session is null when nobody is logged in
  const [session, setSession] = useState(loadSession())

  function logIn(newSession: Session) {
    saveSession(newSession)
    setSession(newSession)
  }

  function logOut() {
    saveSession(null)
    setSession(null)
  }

  if (session === null) {
    return <LoginPage onLogin={logIn} />
  }
  return <UsersPage session={session} onLogout={logOut} />
}
