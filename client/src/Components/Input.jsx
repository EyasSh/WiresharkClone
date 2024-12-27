/* eslint-disable no-unused-vars */
/* eslint-disable react/prop-types */
import React, { useState } from 'react';
import { FaEye, FaEyeSlash } from 'react-icons/fa';
import '../index.css';

/**
 * @function Input
 * @description A react component which renders an input box based on given props.
 * @param {Object} props - The properties of the input box.
 * @prop {string} [type=text] - The type of the input box.
 * @prop {string} [placeholder] - The placeholder of the input box.
 * @prop {string} [value] - The value of the input box.
 * @prop {function} [action] - The function to be called when the input box changes.
 * @prop {boolean} [required=false] - Whether the input box is required to be filled or not.
 * @prop {boolean} [showPasswordToggle=false] - Whether to show a password toggle button for password input.
 * @returns {ReactElement} - The rendered input box.
 */
function Input(props) {
    const [inputType, setInputType] = useState(props.type || "text");

    const handleTogglePassword = () => {
        setInputType((prev) => (prev === "password" ? "text" : "password"));
    };

    return (
        <div style={{ display: "flex", alignItems: "center", position: "relative", width: "100%" }}>
            <input
                className={props.className}
                placeholder={props.placeholder}
                type={inputType}
                value={props.value || ""}
                onChange={(e) => props.action(e.target.value)}
                required={props.required}
                style={{
                    paddingRight: props.showPasswordToggle && props.type === "password" ? "2rem" : "initial",
                    boxSizing: "border-box",
                    width: "100%", // Ensure it fills the container without overflow
                }}
            />
            {props.showPasswordToggle && props.type === "password" && (
                <span
                    onClick={handleTogglePassword}
                    style={{
                        cursor: "pointer",
                        position: "absolute",
                        right: "1rem",
                        top: "41%",
                        transform: "translateY(-50%)",
                    }}
                >
                    {inputType === "password" ? <FaEye /> : <FaEyeSlash />}
                </span>
            )}
        </div>
    );
}

export default Input;
