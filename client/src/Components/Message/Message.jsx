/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
import React from 'react';
import './Message.css';
/**
 * Message component renders a message with a specific severity level.
 * 
 * @param {Object} props - The properties object.
 * @param {string} props.severity - The severity level of the message. Can be "warn", "warning", "err", "error", or "normal".
 * @param {ReactNode} props.children - The content to display within the message.
 * 
 * @description If the provided severity does not match allowed values, it defaults to "normal".
 * The component applies a corresponding class name to the message based on its severity level.
 */
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
