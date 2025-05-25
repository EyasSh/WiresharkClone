import { useState, useEffect } from 'react';
import './PacketReport.css';
import axios from 'axios';
import PacketPaper from '../PacketPaper/PacketPaper';
import Loading from "../Logo/Loading";

function PacketReport() {
  const [packets, setPackets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchPackets = async () => {
      setLoading(true);
      try {
        const token = localStorage.getItem('X-Auth-Token');
        const response = await axios.get(
          'http://localhost:5256/api/user/packets',
          {
            headers: {
              'Content-Type': 'application/json',
              'X-Auth-Token': token,
            },
          }
        );
        if (response.status === 200) {
          setPackets(response.data);
          console.log(
            response.data.length
              ? 'Packets fetched successfully:'
              : 'No packets found',
            response.data
          );
        } else {
          console.warn('Unexpected status:', response.status);
        }
      } catch (err) {
        console.error('Error fetching packets:', err);
        setError(err);
      } finally {
        setLoading(false);
      }
    };

    fetchPackets();
  }, []);

  // optional: log when packets updates
  useEffect(() => {
    console.log('Packets state updated:', packets);
  }, [packets]);

  if (loading) {
    return (
      <div className="report-container">
        <h2>Packet Report</h2>
        <div className="no-packets"><h2>Loading packets…</h2></div>
        <Loading />

      </div>
    );
  }

  if (error) {
    return (
      <div className="report-container">
        <h2>Packet Report</h2>
        <div className="no-packets"><h2>Failed to load packets.</h2></div>
      </div>
    );
  }

  return (
    <div className="report-container">
      <h2>Packet Report</h2>
      {packets.length > 0 ? (
        packets.map((packet, index) => (
          <PacketPaper key={index} packet={packet} />
        ))
      ) : (
        <div className="no-packets">
          <h2>No Packets Found</h2>
        </div>
      )}
    </div>
  );
}

export default PacketReport;
