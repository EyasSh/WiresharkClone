/* eslint-disable no-unused-vars */
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import axios from 'axios';
import hubConnection from '../Sockets/SignalR';
import Packet from '../PacketBox/Packet';
import './Home.css';

function Home(props) {
  const navigate = useNavigate();
  const [sid, setSid] = useState('');
  const [stat, setStatus] = useState('');
  const [loading, setLoading] = useState(true);
  const [packetsArr, setPackets] = useState([]);
  const [isFirstCapture, setIsFirstCapture] = useState(true);

  useEffect(() => {
    let isMounted = true;

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

    // 2. Function to fetch packets from the server via SignalR
    const fetchPackets = async () => {
      try {
        if (hubConnection.state === "Connected") {
          console.log("Fetching packets...");
          await hubConnection.invoke("GetPackets");
          console.log("Packets request sent");
        } else {
          console.log("SignalR not connected");
        }
      } catch (err) {
        console.error(`Error: ${err}`);
      }
    };

    // 3. Initialize the listener for incoming packets
    const initializePacketListener = () => {
      // Remove any existing handler to avoid duplication
      hubConnection.off("ReceivePackets");

      hubConnection.on("ReceivePackets", (newPackets) => {
        console.log("Packets received:", newPackets);
        if (isMounted) {
          // Append the new packets to the old array
          setPackets(oldPackets => [...oldPackets, ...newPackets]);
          console.log(packetsArr)
        }
      });
    };

    // Validate the token once
    validateToken();

    // If SignalR is already connected, set up the listener
    if (hubConnection.state === "Connected") {
      console.log("SignalR already connected. Initializing listener...");
      initializePacketListener();
    } else {
      console.error("SignalR connection not established!");
    }

    // Decide whether to immediately fetch once or set interval
    let intervalId;
    if (isFirstCapture) {
      fetchPackets();       // Fetch once immediately
      setIsFirstCapture(false);
    } else {
      intervalId = setInterval(fetchPackets, 50);
    }

    // Clean up when component unmounts
    return () => {
      isMounted = false;
      clearInterval(intervalId);
      hubConnection.off("ReceivePackets");
      console.log("Home component unmounted and listener removed");
    };

    // Dependencies:
  }, [navigate, isFirstCapture,packetsArr]);

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
        />
      ))}
    </div>
  );
}

export default Home;
