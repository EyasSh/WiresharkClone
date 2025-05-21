// src/Home/Home.jsx

import{ useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import axios from 'axios';
import packetHubConnection from '../Sockets/packetHub';
import Packet from '../PacketBox/Packet';
import DetailBox from '../DetailBox/DetailBox';
import FilterButton from '../FilterButton/FilterButton';
import './Home.css';

export default function Home() {
  const navigate = useNavigate();

  // ─── raw data & filter ───────────────────────────────────
  const [packetsArr, setPacketsArr] = useState([]);
  const [filter, setFilter]       = useState('All');
  const [filteredPackets, setFilteredPackets] = useState([]);
  const [selectedIndex, setSelectedIndex]     = useState(-1);

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

  // ─── SignalR listener ─────────────────────────────────────
  useEffect(() => {
    if (packetHubConnection.state !== 'Connected') return;
    packetHubConnection.off('ReceivePackets');
    packetHubConnection.on('ReceivePackets', newPackets => {
      setPacketsArr(old => {
        const upd = [...old, ...newPackets];
        localStorage.setItem('packets', JSON.stringify(upd));
        return upd;
      });
    });
    return () => packetHubConnection.off('ReceivePackets');
  }, []);

  // ─── initial fetch + interval ─────────────────────────────
  useEffect(() => {
    const fetchOnce = () => {
      if (packetHubConnection.state === 'Connected') {
        packetHubConnection.invoke('GetPackets').catch(console.error);
      }
    };
    fetchOnce();
    const id = setInterval(fetchOnce, 60_000);
    return () => clearInterval(id);
  }, []);

  // ─── THIS is the one & only filter effect ────────────────
  useEffect(() => {
    const result = packetsArr.filter(p =>
      filter === 'All' ? true : p.protocol === filter
    );
    console.log(
      `[FilterEffect] filter="${filter}" raw=${packetsArr.length} → filtered=${result.length}`
    );
    setFilteredPackets(result);
    setSelectedIndex(-1); // clear any detail when the list changes
  }, [packetsArr, filter]);

  // ─── pick the packet for the detail box ──────────────────
  const selectedPacket =
    selectedIndex >= 0 && selectedIndex < filteredPackets.length
      ? filteredPackets[selectedIndex]
      : null;

  return (
    <div className="Home-Container">
      <header className="Header">
        <h1>Packet Analyzer</h1>
        <p className="Description">
          Packets Colored <strong style={{ color: 'yellow' }}>Yellow</strong> are suspicious
        </p>
        <p className="Description">
          Packets Colored <strong style={{ color: 'red' }}>Red</strong> are potentially malicious
        </p>
      </header>

      <div className="Filter-Container">
        <FilterButton
          selected={filter}
          onChange={setFilter}
        />
      </div>

      <div className="Packets-List">
        {filteredPackets.map((p, i) => (
          <Packet
            // use a key that changes whenever filter toggles back to All
            key={`${p.protocol}-${p.timestamp}-${i}`}
            packetType={`Protocol: ${p.protocol} | IP v${p.ipVersion}`}
            sourceIP={p.sourceIP}
            destinationIP={p.destinationIP}
            sourcePort={p.sourcePort}
            destinationPort={p.destinationPort}
            protocol={p.protocol}
            packetDescription={p.timestamp}
            index={i}
            clickHandler={() => setSelectedIndex(i)}
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
