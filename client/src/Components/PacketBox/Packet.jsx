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

  const [Type, setPacketType] = useState(packetType);
  const [SourceIP, setSourceIP] = useState(sourceIP);
  const [DestinationIP, setDestinationIP] = useState(destinationIP);
  const [SourcePort, setSourcePort] = useState(sourcePort);
  const [DestinationPort, setDestinationPort] = useState(destinationPort);
  const [Protocol, setProtocol] = useState(protocol);
  const [PacketDescription, setPacketDescription] = useState(packetDescription);


  return (
    <div className={`Packet-Container`}>
      <spam>{SourceIP}</spam>
      <spam>{SourcePort}</spam>
      <spam>{Type}</spam>
      <spam>{DestinationIP}</spam>
      <spam>{DestinationIP}</spam>
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
