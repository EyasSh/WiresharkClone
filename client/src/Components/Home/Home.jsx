/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
// src/Home/Home.jsx

import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import axios from 'axios';
import './Home.css';
import Packet from '../PacketBox/Packet';
import DetailBox from '../DetailBox/DetailBox';
import FilterButton from '../FilterButton/FilterButton';

/**
 * Home component is the main page of the application. It displays a list of packets
 * captured by the server, with an option to filter by protocol. When a packet is
 * selected, it shows the packet details in a modal box.
 */
export default function Home({ hubConnection }) {
  const navigate = useNavigate();

  // ─── raw data & filter ───────────────────────────────────
  const [packetsArr, setPacketsArr] = useState([]);
  const [filter, setFilter] = useState('All');
  const [filteredPackets, setFilteredPackets] = useState([]);
  const [selectedIndex, setSelectedIndex] = useState(-1);
  const [email] = useState(
    localStorage.getItem('user')
      ? JSON.parse(localStorage.getItem('user')).email
      : ''
  );

  // ─── auth check ──────────────────────────────────────────
  useEffect(() => {
    (async () => {
      const token = localStorage.getItem('X-Auth-Token');
      if (!token) return navigate('/');
      try {
        const { status } = await axios.get(
          'http://localhost:5256/api/user/validate',
          { headers: { 'X-Auth-Token': token } }
        );
        if (status !== 200) navigate('/');
      } catch {
        navigate('/');
      }
    })();
  }, [navigate]);

  // ─── SignalR: Listen and fetch packets ───────────────────
  useEffect(() => {
    if (!hubConnection || hubConnection.state !== 'Connected') return;

    // Set up the event handler BEFORE invoking GetPackets
    hubConnection.off('ReceivePackets');
    hubConnection.on('ReceivePackets', newPackets => {
      console.log("Received new packets:", newPackets);
      setPacketsArr(newPackets); // Replace with latest packets from server
    });

    // Fetch packets once and then every 60 seconds
    const fetchOnce = () => {
      hubConnection.invoke('GetPackets', email).catch(console.error);
    };
    fetchOnce();
    const id = setInterval(fetchOnce, 60_000);

    // Cleanup
    return () => {
      clearInterval(id);
      hubConnection.off('ReceivePackets');
    };
  }, [hubConnection, email]);

  // ─── Filtering ───────────────────────────────────────────
  useEffect(() => {
    const result = packetsArr.filter(p => {
      if (filter === 'All') return true;
      if (filter === 'Suspicious and Malicious') return p.isSuspicious || p.isMalicious;
      return p.protocol === filter;
    });

    setFilteredPackets(result);
    if (result.length === 0) setSelectedIndex(-1);
  }, [packetsArr, filter]);

  // Clear detail when filter changes
  useEffect(() => {
    setSelectedIndex(-1);
  }, [filter]);

  // ─── pick the packet for the detail box ──────────────────
  const selectedPacket =
    selectedIndex >= 0 && selectedIndex < filteredPackets.length
      ? filteredPackets[selectedIndex]
      : null;

  return (
    <div className="Home-Container">
      <header className="Header">
        <div className='Danger-Container'>
          <span>Malicious Packet</span>
          <div className="Malicious-Color"></div>
        </div>
        <div className='Danger-Container'>
          <span>Suspicious Packet</span>
          <div className="Suspicious-Color"></div>
        </div>
        <div className='Danger-Container'>
          <span>Safe Packet</span>
          <div className='Safe-Color-Container'>
            <div className="Safe-Color-Light"></div>
            <div className="Safe-Color-Dark"></div>
          </div>
        </div>
      </header>

      <div className="Filter-Container">
        <FilterButton
          selected={filter}
          onChange={setFilter}
        />
      </div>

      <div className="Packets-List">
        <div className='Item-Labels'>
         <span>Source IP</span>
         <span>Source Port</span>
         <span>Protocol</span>
         <span>Destination IP</span>
         <span>Destination Port</span>
        </div>
        {filteredPackets.map((p, i) => (
          <Packet
            key={`${p.protocol}-${p.timestamp}-${i}`}
            index={i}
            clickHandler={() => setSelectedIndex(i)}
            packetType={`Protocol: ${p.protocol} | IP v${p.ipVersion}`}
            sourceIP={p.sourceIP}
            destinationIP={p.destinationIP}
            sourcePort={p.sourcePort}
            destinationPort={p.destinationPort}
            protocol={p.protocol}
            packetDescription={p.description}
            timestamp={p.timestamp}
            isSuspicious={p.isSuspicious}
            isMalicious={p.isMalicious}
            isSelected={selectedIndex === i}
          />
        ))}
      </div>
      {selectedPacket && (
        <DetailBox
          packet={selectedPacket}
          index={selectedIndex}
          onClose={() => setSelectedIndex(-1)}
        />
      )}
    </div>
  );
}
