/* eslint-disable no-unused-vars */
// DetailBox.jsx
import React from 'react';
import PropTypes from 'prop-types';
import './DetailBox.css';

function DetailBox({ packet, onClose }) {
  console.log('🔍 DetailBox render, packet =', packet);
  if (!packet) return null;

  return (
    <div
      className="detail-box"
       
    >
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
        <p><strong>Length:</strong> {packet.length} bytes</p>
        <p><strong>Timestamp:</strong> {packet.timestamp}</p>
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
