/* eslint-disable no-unused-vars */
import {useState, useEffect} from 'react';
import './PacketReport.css';
import axios from 'axios';
import PropTypes from 'prop-types';
import PacketPaper from '../PacketPaper/PacketPaper';
import { use } from 'react';



function PacketReport() {
    const [packets, setPackets] = useState([]);
    useEffect(() => {
        const fetchPackets = async () => {
            try {
                const token = localStorage.getItem('X-Auth-Token');

                const response = await axios.get('http://localhost:5256/api/user/packets', {
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Auth-Token': token,
                    },
                });
                setPackets(response.data);
                if(response.status === 200 && response.data.length > 0) {
                    console.log('Packets fetched successfully:', response.data);
                    setPackets(response.data);
                }
                else if(response.status === 200 && response.data.length === 0) {
                    console.log('No packets found');
                }
            } catch (error) {
                console.error('Error fetching packets:', error);
            }
        };

        fetchPackets();
    }, []);
 useEffect(() => {
        console.log('Packets state updated:', packets);
    }, [packets]);
    return (
        <div className='report-container'>
            <h2>Packet Report</h2>
            {packets.length > 0 ? (
                <>
                    {packets.map((packet, index) => (
                        <PacketPaper key={index} packet={packet} />
                    ))}
                </>
            ) : (
                <div className='no-packets'>
                    <h2>No Packets Found</h2>
                </div>
            )}
        </div>
    );
}

export default PacketReport;