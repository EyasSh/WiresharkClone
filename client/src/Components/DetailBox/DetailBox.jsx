/* eslint-disable no-unused-vars */
// DetailBox.jsx
import React from 'react';
import PropTypes from 'prop-types';
import '../../index.css';
import './DetailBox.css';
import JsonParser from '../../Services/JsonParser';

/**
 * DetailBox component renders a modal window with packet details.
 * 
 * @param {Object} props - The properties object.
 * @param {Object} props.packet - The packet object containing the information to display.
 * @param {function} props.onClose - The function to call when the modal is closed.
 * 
 * @description The component renders packet details and lengths in a modal window.
 * Clicking outside the modal or on the close button will close the modal.
 * The component is used within the Packet component to show details when clicked.
 */
function DetailBox({ packet, onClose }) {
  console.log('🔍 DetailBox render, packet =', packet);
  if (!packet) {console.log('🔍 DetailBox render, packet is null'); return null;}

  return (
    <div className="detail-box">
      <span
        className="close"
        onClick={onClose}
        style={{ cursor: 'pointer', float: 'right' }}
      >
        ✕
      </span>
      <h3>Packet Details</h3>
      <div className="detail-box-content">
        <p><strong>Source IP:</strong> {packet.sourceIP}</p>
        <p><strong>Destination IP:</strong> {packet.destinationIP}</p>
        <p><strong>Protocol:</strong> {packet.protocol}</p>
        <p><strong>Timestamp:</strong> {packet.timestamp}</p>
        <p><strong>Description:</strong> {packet.description}</p>
        <p><strong>Application Layer Text:</strong>  { packet.applicationLayerText ? JsonParser(packet.applicationLayerText): 'N/A'}</p>
        <h3>Lengths</h3>
        <p>Header Length: {packet.headerLength} bytes</p>
        <p>Total Length: {packet.totalLength} bytes</p>
      </div>
    </div>
  );
}

DetailBox.propTypes = {
  packet: PropTypes.object,
  onClose: PropTypes.func.isRequired,
};

export default DetailBox;
