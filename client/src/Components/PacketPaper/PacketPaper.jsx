/* eslint-disable no-unused-vars */
import {useState} from 'react';
import '../../index.css';
import PropTypes from 'prop-types';
/**
 * PacketPaper component renders a section titled "Packet Paper".
 * 
 * @param {Object} props - The properties object.
 * @param {Array} props.packet - An array containing packet data to be displayed.
 * 
 * @returns {ReactElement} A React component that displays the section with the packet data.
 * PacketPaper component renders a section titled "Packet Report".
 * 
 * @param {{ sourceIP, destinationIP, protocol, sourcePort, destinationPort, isMalicious, isSuspicious, timestamp, packetDescription, headerLength, totalLength }} packet
 */
function PacketPaper({ packet }) {
  return (
    <div className="paper-container">

      <div className="packet-paper"  style={{border: packet.isMalicious ? '5px solid red' : packet.isSuspicious ? '5px solid orange' : '2px solid green'}}>
        
        <p ><strong>Source IP:</strong> {packet.sourceIP}</p>
        <p><strong>Destination IP:</strong> {packet.destinationIP}</p>
        <p><strong>Protocol:</strong> {packet.protocol}</p>
        <p><strong>Source Port:</strong> {packet.sourcePort}</p>
        <p><strong>Destination Port:</strong> {packet.destinationPort}</p>
        <p>
          <strong>Danger Level:</strong>{' '}
          {packet.isMalicious ? 'High' 
            : packet.isSuspicious ? 'Medium' 
            : 'Low'}
        </p>
        <p><strong>Timestamp:</strong> {packet.timestamp}</p>
        <p><strong>Description:</strong> {packet.description}</p>
        <p><strong>Header Length:</strong> {packet.headerLength}</p>
        <p><strong>Total Length:</strong> {packet.totalLength}</p>
      </div>
    </div>
  );
}

PacketPaper.propTypes = {
  packet: PropTypes.shape({
    sourceIP: PropTypes.string,
    destinationIP: PropTypes.string,
    protocol: PropTypes.string,
    sourcePort: PropTypes.number,
    destinationPort: PropTypes.number,
    isMalicious: PropTypes.bool,
    isSuspicious: PropTypes.bool,
    timestamp: PropTypes.string,
    packetDescription: PropTypes.string,
    headerLength: PropTypes.number,
    totalLength: PropTypes.number,
    description: PropTypes.string,
  }).isRequired,
};

export default PacketPaper;
