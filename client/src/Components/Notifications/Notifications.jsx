/* eslint-disable react/prop-types */
/* eslint-disable no-undef */
/* eslint-disable no-unused-vars */
import React,{ useState} from 'react';
import './Notifications.css';

/**
 * Notifications component renders a notification with a severity level.
 * 
 * @param {Object} props - The properties object.
 * @param {string} [props.severity='err'] - The severity level of the notification. Can be "warn", "warning", "err", "error", or "ok".
 * @param {ReactNode} props.children - The content to display within the notification.
 * 
 * @description If the provided severity does not match allowed values, it defaults to "err". The component applies a corresponding class name to the notification based on its severity level.
 */
function Notifications(props) {
    const {severity='err', children} = props;
    return (
        <div className='Notifications-Container'>
            <div className={`Notifications-Severity-Indicator ${severity.toLowerCase()}`} ></div>
            <h3 className='Notifications-Text'>{children}</h3>
        </div>
    );
}

export default Notifications;