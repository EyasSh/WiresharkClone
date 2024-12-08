// eslint-disable-next-line no-unused-vars
import { useState } from 'react'
import {BrowserRouter as Router, Routes, Route} from 'react-router'
import './App.css'

function App() {

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
