// src/Home/Home.jsx

import{ useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import axios from 'axios';
import packetHubConnection from '../Sockets/packetHub';
import Packet from '../PacketBox/Packet';
import DetailBox from '../DetailBox/DetailBox';
import FilterButton from '../FilterButton/FilterButton';
import './Home.css';

/**
 * Home component is the main page of the application. It displays a list of packets
 * captured by the server, with an option to filter by protocol. When a packet is
 * selected, it shows the packet details in a modal box.
 *
 * @type {React.FC}
 */

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
        const upd = [...newPackets, ...old];
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
   const result = packetsArr.filter(p => {
  if (filter === 'All') return true;
  if (filter === 'Suspicious and Malicious') return p.isSuspicious || p.isMalicious;
  return p.protocol === filter;
});


   console.log(
     `[FilterEffect] filter="${filter}" raw=${packetsArr.length} → filtered=${result.length}`
   );
   if(result.length> 0) 
   {setFilteredPackets(result);}
   else{
      setFilteredPackets([]);
      setSelectedIndex(-1); // clear detail box if no packets match the filter
   }
 }, [packetsArr, filter]);


 // only clear the detail when the *filter* itself changes
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
        {filteredPackets.map((p, i) => (
          <Packet
            // use a key that changes whenever filter toggles back to All
            key={`${p.protocol}-${p.timestamp}-${i}`}
            index={i}
            clickHandler={() => { console.log('click', i); setSelectedIndex(i); }}
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
