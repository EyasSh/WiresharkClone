import { useState, useEffect } from 'react';
import './PacketReport.css';
import axios from 'axios';
import PacketPaper from '../PacketPaper/PacketPaper';
import Loading from "../Logo/Loading";

/**
 * PacketReport component fetches and displays a list of network packets.
 * 
 * @returns {React.ReactElement} A React component that renders the packet report page.
 * 
 * @description This component fetches packets from the server using an 
 * asynchronous request and displays them in a list format. If packets are 
 * being fetched, it shows a loading indicator. If an error occurs during 
 * the fetch, an error message is displayed. The component uses the 
 * `axios` library for HTTP requests and manages state with 
 * React hooks (`useState` and `useEffect`).
 */
function PacketReport() {
  const [packets, setPackets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [userId] = useState(
    localStorage.getItem('user')
      ? JSON.parse(localStorage.getItem('user')).id
      : ''
  );

  useEffect(() => {
    /**
     * Fetches packets from the server using an asynchronous request
     * and updates the state variables accordingly.
     * 
     * @description This function sends a GET request to the server's
     * /api/user/packets endpoint with the X-Auth-Token included in the
     * request headers. If the response is successful (status 200), it
     * updates the `packets` state variable with the received data and
     * logs a success message to the console. If the response is not
     * successful, it logs a warning message to the console. If an error
     * occurs during the request, it logs the error to the console and
     * updates the `error` state variable with the error value. Finally,
     * it sets the `loading` state variable to `false` regardless of the
     * outcome.
     */
    const fetchPackets = async () => {
      setLoading(true);
      try {
        const token = localStorage.getItem('X-Auth-Token');
        const url = `http://localhost:5256/api/user/packets?userId=${encodeURIComponent(userId)}`;
        const response = await axios.get(url, { headers: { 'X-Auth-Token': token } });

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
