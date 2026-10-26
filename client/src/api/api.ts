// These describe the shape of the data we send to and get from the backend.

export type Role = 'User' | 'SuperUser' | 'Admin'

// The logged-in user and their login token
export type Session = { token: string; username: string; role: Role }

export type User = { id: number; username: string; role: Role }

// What we send to create or update a user. The password is optional when updating.
export type UserRequest = { username: string; password?: string; role: Role }

// The logged-in user is saved in sessionStorage, which the browser deletes when the tab is closed.
export function loadSession(): Session | null {
  const json = sessionStorage.getItem('session')
  if (json === null) return null
  return JSON.parse(json)
}

export function saveSession(session: Session | null) {
  if (session === null) {
    sessionStorage.removeItem('session')
  } else {
    sessionStorage.setItem('session', JSON.stringify(session))
  }
}

// Sends a request to the backend and returns the answer.
// If we are logged in, the token is sent along, so the backend knows who we are.
async function request(method: string, path: string, body?: object): Promise<any> {
  const headers = new Headers()
  headers.set('Content-Type', 'application/json')

  const session = loadSession()
  if (session !== null) {
    headers.set('Authorization', 'Bearer ' + session.token)
  }

  const response = await fetch(path, {
    method: method,
    headers: headers,
    body: body ? JSON.stringify(body) : undefined,
  })

  // Something went wrong: show the server's message, or the status text (like "Forbidden")
  if (!response.ok) {
    const message = await response.text()
    throw new Error(message || response.statusText)
  }

  // 204 means "it worked, but there is nothing to send back"
  if (response.status === 204) return null

  return response.json()
}

export function login(username: string, password: string): Promise<Session> {
  return request('POST', '/api/login', { username, password })
}

export function getUsers(): Promise<User[]> {
  return request('GET', '/api/users')
}

export function createUser(user: UserRequest): Promise<User> {
  return request('POST', '/api/users', user)
}

export function updateUser(id: number, user: UserRequest): Promise<User> {
  return request('PUT', '/api/users/' + id, user)
}

export function deleteUser(id: number): Promise<void> {
  return request('DELETE', '/api/users/' + id)
}
