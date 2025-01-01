/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
// Profile.jsx
import React from 'react';
import './Profile.css'; // Add consistent styles for profile page

function Profile({hubConnection}) {
    // Retrieve the user data and parse it
    const storedData = JSON.parse(localStorage.getItem('user')); // Get the stored data
    const name = storedData?.user?.name.split(' ')[0] || 'Guest'; // Safely access the nested user name
    const sid = localStorage.getItem('sid');

    return (
        <div className="Profile-Container">
            <h1>Profile Page</h1>
            <p>Welcome to the profile page {name}!</p>
            <p>Connection State: {hubConnection?.state || 'No Connection'}</p>
            <p>Session ID: {sid}</p>
        </div>
    );
}


export default Profile;
