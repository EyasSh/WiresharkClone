/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
// Profile.jsx
import React from 'react';
import './Profile.css'; // Add consistent styles for profile page
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

function Profile({hubConnection}) {
    // Retrieve the user data and parse it
    const storedData = JSON.parse(localStorage.getItem('user')); // Get the stored data
    const name = storedData?.user?.name.split(' ')[0] || 'Guest'; // Safely access the nested user name
    const sid = localStorage.getItem('sid');
    const connection = hubConnection;

    return (
        <div className="Profile-Container">
            <h1>Profile Page</h1>
            <p>Welcome to the profile page {name}!</p>
            <p>Connection State: {connection?.state || 'No Connection'}</p>
            <p>Session ID: {sid}</p>
        </div>
    );
}


export default Profile;
