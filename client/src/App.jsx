/* eslint-disable react/prop-types */ 
// App.jsx
// App.jsx
import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route, useLocation } from 'react-router';
import './index.css'
import './App.css';
import './Components/Button/Button.css'; // Specific button styles come after index.css
import Login from './Components/Login/Login';
import Signup from './Components/signup/Signup';
import Home from './Components/Home/Home';
import Nav from './Components/Nav/Nav';
import Packets from './Components/Packets/Packets';
import hubConnection from './Components/Sockets/SignalR'; // Import the SignalR connection
import packetHubConnection from './Components/Sockets/packetHub';
import Notifications from './Components/Notifications/Notifications';
import Performance from './Components/Performance/Performance';
import Loading from './Components/Logo/Loading';
import VirusChecker from './Components/Security/VirusChecker';

/**
 * The main App component that handles the SignalR connection and rendering of the main content.
 * It establishes a connection to the SignalR hub when the component mounts and sets up event listeners for the ConnectNotification event.
 * If the connection is established successfully, it sets the connection state to 'Connected' and sets the sid and psid states with the connection ID.
 * If there is an error connecting to the hub, it sets the connection state to 'Disconnected' and displays an error notification.
 * It also renders a notification component if a notification exists.
 * @returns {ReactElement} The main App component.
 */
export default function App() {
    const [connection, setConnection] = useState(null);
    const [sid, setSid] = useState(() => localStorage.getItem('sid') || null);
    const [notification,setNotification] = useState(null);

    const [packetConnection, setPacketConnection] = useState(null);
    const [psid, setPSid] = useState(() => localStorage.getItem('psid') || null);
    const [pnotification,setPNotification] = useState(null);

    useEffect(() => {
        /**
         * Establishes a connection to the SignalR hub. If the connection is established successfully, 
         * it sets the connection state to 'Connected' and sets the sid and psid states with the connection ID.
         * If there is an error connecting to the hub, it sets the connection state to 'Disconnected' and 
         * displays an error notification.
         * 
         * @function startConnection
         * @returns {void}
         */
        // ...existing code...
        const startConnection = async () => {
            try {
                if (hubConnection.state === 'Disconnected') {
                    await hubConnection.start();
                    setConnection(hubConnection);
                } else {
                    setConnection(hubConnection);
                }

                if (packetHubConnection.state === 'Disconnected') {
                    await packetHubConnection.start();
                    setPacketConnection(packetHubConnection);
                } else {
                    setPacketConnection(packetHubConnection);
                }
                console.log('Connected to SignalR hub');
                // Listen for the ConnectNotification event
                hubConnection.on('ConnectNotification', (connectionId, status) => {
                    console.log(`Connection ID: ${connectionId}, Status: ${status}`);
                    setSid(connectionId);
                    localStorage.setItem('sid', connectionId);
                    setNotification({
                        severity: status === 'ok' ? 'ok' : 'err',
                        children: status === 'ok'
                            ? 'Initialized Session'
                            : 'Connection error occurred.',
                    });
                });
                // Use packetHubConnection here, not packetConnection
                packetHubConnection.on('ConnectNotification', (connectionId, status) => {
                    console.log(`Connection ID: ${connectionId}, Status: ${status}`);
                    setPSid(connectionId);
                    localStorage.setItem('psid', connectionId);
                    setPNotification({
                        severity: status === 'ok' ? 'ok' : 'err',
                        children: status === 'ok'
                            ? 'Initialized Session'
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
    
        let likesLightTheme = window.matchMedia('(prefers-color-scheme: light)').matches
        if (likesLightTheme) {
            document.documentElement.setAttribute('data-theme', 'light');
        }
        else {
            document.documentElement.setAttribute('data-theme', 'dark');
        }
    
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
            
            {pnotification && (
                <Notifications severity={pnotification.severity}>
                    {pnotification.children}
                </Notifications>
            )}
            <Main hubConnection={connection} sid={sid} packetConnection={packetConnection} psid={psid} />
        </Router>
    );
}


/**
 * The main component of the application. It renders the navigation bar and the routes.
 * The navigation bar is only visible if the current path is not in the excludedPaths array.
 * The routes are defined below.
 * @param {Object} props The props object
 * @param {Object} props.hubConnection The SignalR hub connection object
 * @param {string} props.sid The session ID of the user
 * @param {Object} props.packetConnection The SignalR packet connection object
 * @param {string} props.psid The session ID of the user for the packet connection
 * @returns {ReactElement} The main component
 */
export function Main({ hubConnection, sid, packetConnection, psid }) {
    const location = useLocation();
    const excludedPaths = ['/', '/signup']; // Paths where Nav should not be visible

    return (
        <>
            {!excludedPaths.includes(location.pathname) && <Nav hubConnection={hubConnection} sid={sid} packetHub={packetConnection} />}
            <Routes>
                <Route path="/" element={<Login />} />
                <Route path="/signup" element={<Signup />} />
                <Route path="/home" element={packetConnection
                    ? <Home hubConnection={packetConnection} sid={psid} />
                    : <Loading />} />
                <Route path="/performance" element={<Performance hubConnection={hubConnection} sid={sid} />} />
                <Route path="/packets" element={<Packets hubConnection={hubConnection} sid={sid} />} />
                <Route path='/security/*' element={<VirusChecker />} />
            </Routes>
        </>
    );
}
