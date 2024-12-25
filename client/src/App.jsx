// eslint-disable-next-line no-unused-vars
import { useState } from 'react'
import {BrowserRouter as Router, Routes, Route} from 'react-router'
import './App.css'
import Login from './Components/Login/Login'
import Signup from './Components/signup/signup'
import Home from './Components/Home/Home'

function App() {

  return (
    <>
      <Router>
        <Routes>
          <Route path='/' element={<Login></Login>}></Route>
          <Route path='/signup' element={<Signup></Signup>}></Route>
          <Route path='/home' element={<Home></Home>}></Route>
        </Routes>
      </Router>
    </>
  )
}

export default App
