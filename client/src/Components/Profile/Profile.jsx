/* eslint-disable no-unused-vars */
// Profile.jsx
import React from 'react';
import './Profile.css'; // Add consistent styles for profile page

function Profile() {
    // Retrieve the user data and parse it
    const storedData = JSON.parse(localStorage.getItem('user')); // Get the stored data
    const name = storedData?.user?.name.split(' ')[0] || 'Guest'; // Safely access the nested user name

    return (
        <div className="Profile-Container">
            <h1>Profile Page</h1>
            <p>Welcome to the profile page {name}!</p>
        </div>
    );
}


export default Profile;
