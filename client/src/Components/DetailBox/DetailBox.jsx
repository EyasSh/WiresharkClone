/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
import {useState, useEffect} from 'react';
import PropTypes from 'prop-types';


function DetailBox(props) {
    const packet = props.packet!=null && props.packet!=undefined ? props.packet : null;
    if(!packet) {
        return null;
    }
    return (
        <div
        
       className="detail-box"
     >
       <span className="close" onClick={props.onClose}>x</span>
            <h1>Packet Details</h1>
            <div className="detail-box-content">
                <p><strong>Source IP:</strong> {packet?.sourceIP}</p>
                <p><strong>Destination IP:</strong> {packet?.destinationIP}</p>
                <p><strong>Protocol:</strong> {packet?.protocol}</p>
                <p><strong>Length:</strong> {packet?.length} bytes</p>
                <p><strong>Timestamp:</strong> {packet?.timestamp}</p>
                <h3>Lengths</h3>
                <p>Header Length: {packet?.headerLength} bytes</p>
                <p>Total Length: {packet?.totalLength} bytes</p>
            </div>
        </div>
    );
}
DetailBox.propTypes = {
    packet: PropTypes.object,
    index: PropTypes.number,
    onClose: PropTypes.func,
};
export default DetailBox;