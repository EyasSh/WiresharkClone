/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
// Profile.jsx
import{ useEffect, useState } from 'react';
import './Packets.css'; // Add consistent styles for profile page
import PacketReport from '../PacketReport/PacketReport';

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
        <>
            <PacketReport />
        </>
    );
}


export default Packets;
