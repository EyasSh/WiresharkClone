/* eslint-disable react/prop-types */
/* eslint-disable no-undef */
/* eslint-disable no-unused-vars */
import React,{ useState} from 'react';
import './Notifications.css';

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