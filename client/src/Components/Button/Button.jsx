/* eslint-disable no-unused-vars */
/* eslint-disable react/prop-types */
import React from 'react';
import './button.css';
import { Link } from 'react-router';

function Button(props) {
    const statusClass = statusHandler(props.status);
    const buttonContent = props.content.toString();
    
    if (props.link) {
        return (
            <Link to={props.link}>
                <button className={statusClass || "default-class"}>
                    {buttonContent}
                </button>
            </Link>
        );
    } else {
        return (
            <button className={statusClass || "default-class"} onClick={props.action}>
                {buttonContent}
            </button>
        );
    }
}

function statusHandler(status) {
    switch (status) {
        case "action":
            return "action";
        case "purchase":
            return "purchase";
        case "signup":
            return "signup";
        default:
            return "";
    }
}

export default Button;
