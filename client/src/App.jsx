/* eslint-disable react/prop-types */ 
// App.jsx
// App.jsx
import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route, useLocation } from 'react-router';
import './index.css'
import './App.css';
import './Components/Button/Button.css'; // Specific button styles come after index.css
import Login from './Components/Login/Login';
import Signup from './Components/Signup/Signup';
import Home from './Components/Home/Home';
import Nav from './Components/Nav/Nav';
import Profile from './Components/Profile/Profile';
import hubConnection from './Components/Sockets/SignalR'; // Import the SignalR connection
import Notifications from './Components/Notifications/Notifications';
import Performance from './Components/Performance/Performance';
import Loading from './Components/Logo/Loading';
import VirusChecker from './Components/Security/VirusChecker';

export default function App() {
    const [connection, setConnection] = useState(null);
    const [sid, setSid] = useState(() => localStorage.getItem('sid') || null);
    const [notification,setNotification] = useState(null);

    useEffect(() => {
        const startConnection = async () => {
            try {
                if (hubConnection.state === 'Connected') {
                    setConnection(hubConnection);
                    return;
                }

                await hubConnection.start();
                setConnection(hubConnection); // Pass the connected instance
                console.log('Connected to SignalR hub');
                  // Listen for the ConnectNotification event
                  hubConnection.on('ConnectNotification', (connectionId, status) => {
                    console.log(`Connection ID: ${connectionId}, Status: ${status}`);
                    setSid(connectionId);
                    localStorage.setItem('sid', connectionId);
                     // Display notification
                     setNotification({
                        severity: status === 'ok' ? 'ok' : 'err',
                        children: status === 'ok'
                            ? 'Connected successfully to the hub!'
                            : 'Connection error occurred.',
                    });
                  });
            } catch (error) {
                console.error('Error connecting to SignalR hub:', error);
                setNotification({
                    severity: 'err',
                    children: `Error: ${error.message || 'Failed to connect to the hub.'}`,
                });
            }
        };

        startConnection();
    }, []);

    if (!connection) {
        return <Loading />; // Wait until the connection is ready
    }

    return (
        <Router>
            {/* Render notification if it exists */}
            {notification && (
                <Notifications severity={notification.severity}>
                    {notification.children}
                </Notifications>
            )}
            <Main hubConnection={connection} sid={sid} />
        </Router>
    );
}

// Main Component
export function Main({ hubConnection, sid }) {
    const location = useLocation();
    const excludedPaths = ['/', '/signup']; // Paths where Nav should not be visible

    return (
        <>
            {!excludedPaths.includes(location.pathname) && <Nav hubConnection={hubConnection} sid={sid} />}
            <Routes>
                <Route path="/" element={<Login />} />
                <Route path="/signup" element={<Signup />} />
                <Route path="/home" element={<Home hubConnection={hubConnection} sid={sid} />} />
                <Route path="/performance" element={<Performance hubConnection={hubConnection} sid={sid} />} />
                <Route path="/profile" element={<Profile hubConnection={hubConnection} sid={sid} />} />
                <Route path='security' element={<VirusChecker />} />
            </Routes>
        </>
    );
}
