import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import axios from 'axios';
import packetHubConnection from '../Sockets/packetHub';
import Packet from '../PacketBox/Packet';
import DetailBox from '../DetailBox/DetailBox';
import './Home.css';

function Home() {
  const navigate = useNavigate();
  const [packetsArr, setPackets] = useState([]);
  const [pressedPacket, setPressedPacket] = useState(-1);

  // 1️⃣ Token validation on mount
  useEffect(() => {
    const validateToken = async () => {
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
    };
    validateToken();
  }, [navigate]);

  // 2️⃣ SignalR listener: push every batch into state
  useEffect(() => {
    if (packetHubConnection.state !== 'Connected') {
      console.error('SignalR not connected!');
      return;
    }

    packetHubConnection.off('ReceivePackets');
    packetHubConnection.on('ReceivePackets', (newPackets) => {
      console.log('Packets received:', newPackets);
      setPackets((old) => {
        const updated = [...old, ...newPackets];
        localStorage.setItem('packets', JSON.stringify(updated));
        return updated;
      });
    });

    return () => {
      packetHubConnection.off('ReceivePackets');
    };
  }, []); // run once

  // 3️⃣ Fire off GetPackets and start the 60s interval
  useEffect(() => {
    const fetchOnce = () => {
      if (packetHubConnection.state === 'Connected') {
        packetHubConnection.invoke('GetPackets').catch(console.error);
      }
    };

    fetchOnce();
    const id = setInterval(fetchOnce, 60_000);
    return () => clearInterval(id);
  }, []); // run once
useEffect(() => {
    if(pressedPacket >= 0 && pressedPacket < packetsArr.length) {
      const packet = packetsArr[pressedPacket];
      console.log('Selected packet:', packet);
      console.log(pressedPacket)
    }
},[pressedPacket, packetsArr]);
  // 4️⃣ Derive the selected packet
  const selectedPacket =
    pressedPacket >= 0 && pressedPacket < packetsArr.length
      ? packetsArr[pressedPacket]
      : null;

  return (
    <div className="Home-Container">
      <div className='Header'>
        <h1>Packet Analyzer</h1>
        <p className='Description'>
          Packets Colored <strong style={{color: 'yellow'}}>Yellow</strong> are suspicious
        </p>
        <p className='Description'>
          Packets Colored <strong style={{color: 'red'}}>Red</strong> are potentially malicious
        </p>
      </div>
      {packetsArr.map((p, idx) => (
        <Packet
          key={idx}
          packetType={`Protocol: ${p.protocol} | IP v${p.ipVersion}`}
          sourceIP={p.sourceIP}
          destinationIP={p.destinationIP}
          sourcePort={p.sourcePort}
          destinationPort={p.destinationPort}
          protocol={p.protocol}
          packetDescription={p.timestamp}
          index={idx}
          clickHandler={(i) => setPressedPacket(i)}
        />
      ))}

      {selectedPacket && (
        <DetailBox
          packet={selectedPacket}
          index={pressedPacket}
          onClose={() => setPressedPacket(-1)}
        />
      )}
    </div>
  );
}

export default Home;
