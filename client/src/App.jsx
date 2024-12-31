/* eslint-disable no-unused-vars */
// App.jsx
import { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, useLocation } from 'react-router';
import './App.css';
import Login from './Components/Login/Login';
import Signup from './Components/Signup/Signup';
import Home from './Components/Home/Home';
import Nav from './Components/Nav/Nav';
import Profile from './Components/Profile/Profile';

function App() {
    return (
        <Router>
            <Main />
        </Router>
    );
}

// Main component to manage routes and navbar visibility
function Main() {
    const location = useLocation();
    const excludedPaths = ['/', '/signup']; // Paths where Nav should not be visible

    return (
        <>
            {/* Render Nav only if the current path is not in excludedPaths */}
            {!excludedPaths.includes(location.pathname) && <Nav />}
            <Routes>
                <Route path="/" element={<Login />} />
                <Route path="/signup" element={<Signup />} />
                <Route path="/home" element={<Home />} />
                <Route path="/profile" element={<Profile />} />
                {
                  /*Add these later:
                  <Route path="/link-checker" element={<LinkChecker />} />
                  <Route path="/file-checker" element={<FileChecker />} />
                  <Route path="/packet-analyzer" element={<PacketAnalyzer />} />
                  */ 

                }
                
            </Routes>
        </>
    );
}

export default App;
