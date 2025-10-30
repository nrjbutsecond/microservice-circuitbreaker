import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import AppWithRouting from './AppWithRouting.jsx'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <AppWithRouting />
  </StrictMode>,
)
