/* eslint-disable no-unused-vars */
import React, { useState } from 'react';
import PropTypes from 'prop-types';
import './Packet.css';

function Packet({
  packetType,
  sourceIP,
  destinationIP,
  sourcePort,
  destinationPort,
  protocol,
  packetDescription
}) {
  // Track whether the box is collapsed or expanded
  const [isCollapsed, setIsCollapsed] = useState(true);
  const [Type, setPacketType] = useState(packetType);
  const [SourceIP, setSourceIP] = useState(sourceIP);
  const [DestinationIP, setDestinationIP] = useState(destinationIP);
  const [SourcePort, setSourcePort] = useState(sourcePort);
  const [DestinationPort, setDestinationPort] = useState(destinationPort);
  const [Protocol, setProtocol] = useState(protocol);
  const [PacketDescription, setPacketDescription] = useState(packetDescription);

  // Toggles the collapsed state
  const toggleCollapse = () => {
    setIsCollapsed(!isCollapsed);
  };

  return (
    <div className={`Packet-Container ${isCollapsed ? 'collapsed' : 'expanded'}`}>
      {/* Header Row — always visible */}
      <div className="Packet-Header" onClick={toggleCollapse}>
        <span className="Packet-Type">
          {packetType || 'Some Packet'}
        </span>
        {/* Arrow indicator. Down if collapsed, up if expanded */}
        <span className="Toggle-Icon">
          {isCollapsed ? '▼' : '▲'}
        </span>
      </div>

      {/* Details — only shown when expanded */}
      {!isCollapsed && (
        <div className="Packet-Details">
            
          <p><strong>Source IP:</strong> {sourceIP || 'Source IP'}</p>
          <p><strong>Destination IP:</strong> {destinationIP || 'Destination IP'}</p>
          <p><strong>Source Port:</strong> {sourcePort || 'Source Port'}</p>
          <p><strong>Destination Port:</strong> {destinationPort || 'Destination Port'}</p>
          <p><strong>Protocol:</strong> {protocol || 'Protocol'}</p>
          <p><strong>Description:</strong> {packetDescription || 'Packet Description'}</p>
        </div>
      )}
    </div>
  );
}

Packet.propTypes = {
  packetType: PropTypes.string,
  sourceIP: PropTypes.string,
  destinationIP: PropTypes.string,
  sourcePort: PropTypes.number,
  destinationPort: PropTypes.number,
  protocol: PropTypes.string,
  packetDescription: PropTypes.string
};

export default Packet;
