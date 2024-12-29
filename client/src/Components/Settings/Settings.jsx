/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
import React from 'react';
import { FaMoon, FaSun } from 'react-icons/fa';
import './Settings.css';

/**
 * Settings component renders a modal with theme toggle functionality.
 * 
 * @param {Object} props - The properties passed to the component.
 * @param {boolean} props.isOpen - Determines if the modal is open or closed.
 * @param {function} props.onClose - Function to call when the modal is closed.
 * @param {ReactNode} props.children - Additional components or elements to render within the modal.
 * 
 * @description The component uses a modal overlay to allow users to toggle between light and dark themes. 
 * Clicking outside the modal or on the close button will close the modal. The current theme is displayed 
 * with an icon and a text label indicating the mode switch.
 */

function Settings({ isOpen, onClose, children }) {  
    const [isLightMode, setIsLightMode] = React.useState(
        window.matchMedia('(prefers-color-scheme: light)').matches
    );

    const toggleTheme = () => {
        const newMode = isLightMode ? 'dark' : 'light';
        setIsLightMode(!isLightMode);
        document.documentElement.setAttribute('data-theme', newMode);
    };

    if (!isOpen) return null; // Render nothing if modal is closed

    return (
        <div className="ModalOverlay" onClick={onClose}>
            <div
                className="ModalContent"
                onClick={(e) => e.stopPropagation()} // Prevent closing when clicking inside modal
            >
                <button className="CloseButton" onClick={onClose}>
                    X
                </button>
                <h2>Settings</h2>
                <div className="ThemeToggle">
                    <div className="Icon" onClick={toggleTheme}>
                    <span>Switch to {isLightMode ? 'Dark' : 'Light'} Light/Dark Mode</span>
                        {isLightMode ? <FaMoon size={24} title="Dark Mode" /> : <FaSun size={24} title="Light Mode" />}
                    </div>
                    
                </div>
            </div>
        </div>
    );
}

export default Settings;
