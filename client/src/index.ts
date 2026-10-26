import { serve } from 'bun'
import index from './index.html'

// Where the C# backend is running. In Docker this is set in compose.yaml.
const API_URL = process.env.API_URL || 'http://localhost:5000'

// The port the website runs on: 3000 while developing, 8001 in Docker (set in the Dockerfile)
const PORT = Number(process.env.PORT || 3000)

// A small Bun web server. It does two things:
// 1. Sends the React website to the browser.
// 2. Passes every /api request on to the C# backend, and sends the answer back.
//    This way the browser only ever talks to one address.
const server = serve({
  hostname: '0.0.0.0',
  port: PORT,
  routes: {
    '/api/*': request => {
      const path = new URL(request.url).pathname // For example "/api/users"
      return fetch(API_URL + path, {
        method: request.method,
        headers: request.headers, // Includes the login token
        body: request.body,
      })
    },
    '/*': index,
  },
  development: process.env.NODE_ENV !== 'production', // Live reload while developing
})

console.log('Running at ' + server.url)
