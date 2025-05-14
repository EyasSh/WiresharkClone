/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-unused-vars */
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import axios from 'axios';
import packetHubConnection from '../Sockets/packetHub';
import Packet from '../PacketBox/Packet';
import './Home.css';
import DetailBox from '../DetailBox/DetailBox';

function Home(props) {
  const navigate = useNavigate();
  const [sid, setSid] = useState('');
  const [stat, setStatus] = useState('');
  const [loading, setLoading] = useState(true);
  const [packetsArr, setPackets] = useState([]);
  const [isFirstCapture, setIsFirstCapture] = useState(true);
  const [isMounted, setIsMounted] = useState(false);
  const [pressedPacket, setPressedPacket] = useState(-1);

  useEffect(() => {
    setIsMounted(true);

    // 1. Validate token
    const validateToken = async () => {
        const token = localStorage.getItem('X-Auth-Token') || undefined;
        if (!token) {
            alert("No token found, redirecting to login...");
            navigate('/');
            return;
        }

        try {
            const response = await axios.get('http://localhost:5256/api/user/validate', {
                headers: {
                    'X-Auth-Token': token
                }
            });

            if (response.status !== 200) {
                alert("Invalid token. Redirecting to login...");
                navigate('/');
            }
        } catch (error) {
            alert("Validation failed:", error);
            navigate('/');
        }
    };

    // 2. Function to fetch packets
    const fetchPackets = async () => {
        if (packetHubConnection.state === "Connected") {
            try {
                console.log("Fetching packets...");
                await packetHubConnection.invoke("GetPackets");
                console.log("Packets request sent");
            } catch (err) {
                console.error(`Error: ${err}`);
            }
        } else {
            console.log("SignalR not connected");
        }
    };

    // 3. Setup listener for incoming packets
    const initializePacketListener = () => {
        packetHubConnection.off("ReceivePackets"); // Ensure no duplicate listeners
        packetHubConnection.on("ReceivePackets", (newPackets) => {
            console.log("Packets received:", newPackets);
            if (isMounted) {
                setPackets((oldPackets) => [...oldPackets, ...newPackets]);
                localStorage.setItem('packets', JSON.stringify(packetsArr));
            }
        });
    };

    // Validate token once
    validateToken();

    // Ensure SignalR is connected before setting up listeners
    if (packetHubConnection.state === "Connected") {
        console.log("SignalR already connected. Initializing listener...");
        initializePacketListener();
    } else {
        console.error("SignalR connection not established!");
    }

    let intervalId;

    if (isFirstCapture && isMounted) {
        fetchPackets(); // Fetch once immediately
        setIsFirstCapture(false);
    } else if (isMounted) {
        intervalId = setInterval(fetchPackets, 60000);
    }

    return () => {
        setIsMounted(false);
        if (intervalId) {
            clearInterval(intervalId);
        }
        packetHubConnection.off("ReceivePackets"); // Remove listener on unmount
        console.log("Home component unmounted, stopped fetching.");
    };
}, [navigate, isFirstCapture, isMounted]); // Removed `packetsArr` from dependencies
  // 4. Render
  return (
    <div className='Home-Container'>
      <div className='Header'>
        <h1>Packet Analyzer</h1>
        <p className='Description'>
          Packets Colored <strong style={{color: 'yellow'}}>Yellow</strong> are suspicious
        </p>
        <p className='Description'>
          Packets Colored <strong style={{color: 'red'}}>Red</strong> are potentially malicious
        </p>
      </div>
        <div className='Items'>
            <span className='Item'>Source IP</span>
            <span className='Item'>Source Port</span>
            <span className='Item'>Packet Type</span>
            <span className='Item'>Destination IP</span>
            <span className='Item'>Destination Port</span>
        </div>
      {/* Map through the packet array and display each packet */}
      {packetsArr.map((packet, index) => (
        <Packet
          key={index}
          // You can rename or pass whichever fields you want:
          // For example, let's pass:
          //   - packetType => the protocol or ipVersion
          packetType={`Protocol: ${packet.protocol} | IP Version: ${packet.ipVersion}`}
          sourceIP={packet.sourceIP}
          destinationIP={packet.destinationIP}
          sourcePort={packet.sourcePort}
          destinationPort={packet.destinationPort}
          protocol={packet.protocol}
          // If you want to display the timestamp, you can pass it
          // to 'packetDescription' or add a new prop in Packet.
          packetDescription={packet.timestamp}
            clickHandler={() => {
                setPressedPacket(index);
                console.log(`Packet ${index} clicked`);
                // You can add more logic here if needed
            }}
        />
      ))}
        {pressedPacket !== -1 && (
        <DetailBox
            key={pressedPacket}                  // <–– this forces a full remount on each packet
            packet={packetsArr !==undefined || packetsArr !==null ? packetsArr[pressedPacket] : null}
            onClose={() => setPressedPacket(-1)}
        />
    )}
    </div>
  );
}

export default Home;
