/* eslint-disable no-unused-vars */
import React, { useState } from 'react';
import PropTypes from 'prop-types';
import './Packet.css';

/**
 * Packet component renders a network packet's basic details.
 * 
 * @param {Object} props - The properties object.
 * @param {string} props.packetType - The type of the packet.
 * @param {string} props.sourceIP - The source IP address of the packet.
 * @param {string} props.destinationIP - The destination IP address of the packet.
 * @param {number} props.sourcePort - The source port of the packet.
 * @param {number} props.destinationPort - The destination port of the packet.
 * @param {string} props.protocol - The protocol used by the packet.
 * @param {string} props.packetDescription - A description of the packet.
 * @param {function} props.clickHandler - A function to handle click events on the packet.
 * @param {number} props.index - The index of the packet in the list.
 * 
 * @returns {ReactElement} A React component displaying the packet details in a styled container.
 */
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
  /**
   * Handles click events on the packet by calling the clickHandler function
   * with the packet's index if it exists and the index is valid.
   * @function
   */
  const handleClick = () => {
  console.log('Packet clicked!', props.index);
  props.clickHandler(props.index);
}


  return (
    <div className={props.isMalicious && props.isSuspicious ? "Packet-Container-Error":props.isMalicious ? "Packet-Container-Error" : props.isSuspicious ? "Packet-Container-Warning":  "Packet-Container"} onClick={()=>handleClick()}
      style={{backgroundColor: props.isSelected ? '#0a0aff': undefined}}
    >
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
  isSuspicious: PropTypes.bool,
  isMalicious: PropTypes.bool,
  isSelected: PropTypes.bool,
};

export default Packet;
