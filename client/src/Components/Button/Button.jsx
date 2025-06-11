/* eslint-disable no-unused-vars */
/* eslint-disable react/prop-types */
import React from 'react';
import './button.css';
import { Link } from 'react-router';

/**
 * Button component renders a button with the specified content and
 * optional link or action.
 * 
 * @param {Object} props - The properties object.
 * @param {string} props.content - The content to display within the button.
 * @param {string} [props.status=''] - The status of the button. Can be "action", "purchase", or "signup".
 * @param {string} [props.link=''] - The link to navigate to when the button is clicked.
 * @param {function} [props.action=function(){}] - The action to take when the button is clicked.
 * 
 * @returns {ReactElement} The Button component.
 */
function Button(props) {
    const statusClass = props.status || '';
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
            <button className={statusClass || "signup"} onClick={props.action}>
                {buttonContent}
            </button>
        );
    }
}

export default Button;
