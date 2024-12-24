/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
import React from 'react';
import './Message.css';
function Message(props) {
    const { severity, children } = props;

    // Normalize severity to "normal" if it doesn't match allowed values
    const normalizedSeverity = (severity !== "warn" || severity !== "warning" || severity !== "err" || severity !== "error") 
        ? severity 
        : "normal";

    return (
        <div>
            <h5 className={normalizedSeverity}>{children}</h5>
        </div>
    );
}

export default Message;
