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
  packetDescription,
  clickHandler
}) {

  const [Type, setPacketType] = useState(packetType);
  const [SourceIP, setSourceIP] = useState(sourceIP);
  const [DestinationIP, setDestinationIP] = useState(destinationIP);
  const [SourcePort, setSourcePort] = useState(sourcePort);
  const [DestinationPort, setDestinationPort] = useState(destinationPort);
  const [Protocol, setProtocol] = useState(protocol);
  const [PacketDescription, setPacketDescription] = useState(packetDescription);
  const handleClick = () => {
    if (clickHandler) {
      clickHandler();
    }
  }

  return (
    <div className={`Packet-Container`} onClick={handleClick}>
      <span>{SourceIP}</span>
      <span>{SourcePort}</span>
      <span>{Type}</span>
      <span>{DestinationIP}</span>
      <span>{DestinationIP}</span>
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
  packetDescription: PropTypes.string,
  clickHandler: PropTypes.func
};

export default Packet;
