/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
// Profile.jsx
import React, { useEffect, useState } from 'react';
import './Packets.css'; // Add consistent styles for profile page
import hubConnection from '../Sockets/SignalR';

/**
 * Profile component displays the user's profile information.
 * 
 * @param {Object} params - The parameters object.
 * @param {Object} params.hubConnection - The SignalR connection object.
 * 
 * @returns {ReactElement} A React component that renders the profile page.
 * 
 * @description The component fetches the user's data from localStorage and displays
 * their first name. It shows the current state of the SignalR connection and the session ID.
 * If the user's name is not available, it defaults to 'Guest'.
 */

function Packets({hubConnection}) {
    // Retrieve the user data and parse it
    const [name, setName] = useState('Guest'); 
    const connection = hubConnection;
    useEffect(() => {
        const user = JSON.parse(localStorage.getItem('user'));
        setName(user?.name.split(' ')[0] || 'Guest');
    }, []); // Empty dependency array to run only once on mount

    return (
        <div className="Profile-Container">
            <h1>Packet History Page</h1>
            <p>Welcome to the profile page {name}!</p>
            <p>Connection State: {connection?.state || 'No Connection'}</p>
            <p>Session ID: {connection.connectionId}</p>
        </div>
    );
}


export default Packets;
