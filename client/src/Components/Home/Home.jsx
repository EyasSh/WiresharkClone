/* eslint-disable no-unused-vars */
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import axios from 'axios';
import Notifications from '../Notifications/Notifications';
import * as signalR from '@microsoft/signalr';
import Nav from '../Nav/Nav';
import Input from '../Input';
import hubConnection from '../Sockets/SignalR';
import ResourceMeter from '../ResourceMeter/ResourceMeter';
import Packet from '../PacketBox/Packet';
import './Home.css';
function Home(props) {
    const navigate = useNavigate();
    const [sid, setSid] = useState('');
    const [stat, setStatus] = useState('');
    const [loading, setLoading] = useState(true); // New loading state
    const [packets, setPackets] = useState([]); // New packets state
    useEffect(() => {
        const validateToken = async () => {
            const token = localStorage.getItem('X-Auth-Token') || undefined;
            if (!token) {
                alert("No token found, redirecting to login...");
                navigate('/'); // Redirect to login
                return;
            }

            try {
                if(token===undefined){
                    alert("No token found, redirecting to login...");
                    navigate('/'); // Redirect to login
                    return;
                }
                const response = await axios.get('http://localhost:5256/api/user/validate', {
                    headers: {
                        'X-Auth-Token': token // Add token as a header
                    }
                });

                if (response.status !== 200) {
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
            <div className='Header'>
                <h1>Packet Analyzer</h1>
                <p className='Description'>Packets Colored <strong style={{color: 'yellow'}}>Yellow</strong> are suspicious</p>
                <p className='Description'>Packets Colored <strong style={{color: 'red'}}>Red</strong> are potentially malicious </p>
            </div>
           <Packet />
           <Packet />
           <Packet />
           <Packet />
           <Packet />
           <Packet />
        </div>
    );
}

export default Home;
