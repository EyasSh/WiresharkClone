import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import {BrowserRouter as Router, Routes, Route} from 'react-router'
import './App.css'

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
      <Router>
        <Routes>
          <Route path='/' element={<h1>/</h1>}></Route>
          <Route path='/signup' element={<h1>Sign-Up</h1>}></Route>
          <Route path='/home' element={<h1>Home</h1>}></Route>
        </Routes>
      </Router>
    </>
  )
}

export default App
