/* eslint-disable no-unused-vars */
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import axios from 'axios';
import Notifications from '../Notifications/Notifications';
import * as signalR from '@microsoft/signalr';
import Nav from '../Nav/Nav';
import Input from '../Input';
import hubConnection from '../Sockets/SignalR';
function Home(props) {
    const navigate = useNavigate();
    const [sid, setSid] = useState('');
    const [stat, setStatus] = useState('');
    const [loading, setLoading] = useState(true); // New loading state

    useEffect(() => {
        const validateToken = async () => {
            const token = localStorage.getItem('X-Auth-Token');
            if (!token) {
                alert("No token found, redirecting to login...");
                navigate('/'); // Redirect to login
                return;
            }

            try {
                const response = await axios.get('http://localhost:5256/api/user/validate', {
                    headers: {
                        'X-Auth-Token': token // Add token as a header
                    }
                });

                if (response.status === 200) {
                    console.log("Token is valid. Rendering the page...");
                    // Token is valid; proceed with rendering
                    const connection =hubConnection

                    // Start the connection
                    connection
                        .start()
                        .then(() => {
                            console.log("Connected to SignalR Hub");
                        })
                        .catch(err => console.error("SignalR Connection Error:", err));

                    // Handle the ConnectNotification event
                    connection.on("ConnectNotification", (connectionId, status) => {
                        console.log(`Connected with ID: ${connectionId}, Status: ${status}`);
                        setSid(connectionId);
                        setStatus(status.toLowerCase());
                        localStorage.setItem("sid", connectionId);
                        localStorage.setItem("status", status.toLowerCase());
                        setLoading(false); // Stop loading once data is set
                    });
                } else {
                    alert("Invalid token. Redirecting to login...");
                    navigate('/'); // Redirect to login if token is invalid
                }
            } catch (error) {
                alert("Validation failed:", error);
                navigate('/'); // Redirect to login on validation failure
            }
        };

        validateToken(); // Call the async function
    }, [navigate]);

    return (
        <div className='Home-Container'>
            <Nav  />
            {/* Render Notifications only when loading is false and stat is set */}
            {!loading && stat && (
                <Notifications severity={stat}>
                    Successfully Connected to Socket Service
                </Notifications>
            )}
            <h1>Grove Street, home...at least it was before I fucked everything up</h1>
        </div>
    );
}

export default Home;
