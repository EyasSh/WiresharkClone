/* eslint-disable no-unused-vars */
import React, { useState } from 'react';
import PropTypes from 'prop-types';
import './Packet.css';

function Packet({
  ...props
}) {

  const [Type, setPacketType] = useState(props.packetType);
  const [SourceIP, setSourceIP] = useState(props.sourceIP);
  const [DestinationIP, setDestinationIP] = useState(props.destinationIP);
  const [SourcePort, setSourcePort] = useState(props.sourcePort);
  const [DestinationPort, setDestinationPort] = useState(props.destinationPort);
  const [Protocol, setProtocol] = useState(props.protocol);
  const [PacketDescription, setPacketDescription] = useState(props. packetDescription);
  const handleClick = () => {
    if (props.clickHandler && props.index>=0) {
      props.clickHandler(props.index);
    }
  }

  return (
    <div className={`Packet-Container`} onClick={()=>handleClick()}>
      <span>{SourceIP}</span>
      <span>{SourcePort}</span>
      <span>{Type}</span>
      <span>{DestinationIP}</span>
      <span>{DestinationPort}</span>
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
  clickHandler: PropTypes.func,
  index: PropTypes.number,
};

export default Packet;
