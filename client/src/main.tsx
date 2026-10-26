import { createRoot } from 'react-dom/client'
import { App } from './App'

// Start React and show the App inside <div id="root"> in index.html
createRoot(document.getElementById('root')!).render(<App />)
