/* eslint-disable no-unused-vars */
import React, { useState } from 'react';
import Logo from '../Logo/Logo';
import { FaUser,FaHome, FaCog } from 'react-icons/fa';
import { FiLogOut } from 'react-icons/fi'; // Import the logout icon
import './Nav.css';
import Settings from '../Settings/Settings';
import { useNavigate } from 'react-router';
import * as signalR from '@microsoft/signalr';
import hubConnection from '../Sockets/SignalR';


/**
 * Nav component renders the navigation bar with Home, Settings, and Logout options.
 * It manages the active navigation item state and modal visibility for settings.
 * Handles user interactions to navigate between pages, open settings modal,
 * and logout, including stopping a SignalR connection and clearing session data.
 * 
 * @param {Object} props - The properties passed to the component.
 */

function Nav(props) {
    const [activeItem, setActiveItem] = useState('home');
    const [isModalOpen, setIsModalOpen] = useState(false); // Modal visibility state
    const navigate = useNavigate();
    const sid = localStorage.getItem('sid');const handleClick = async (item) => {
        setActiveItem(item);
    
        if (item === 'logout') {
            const sid = localStorage.getItem('sid'); // Get the saved session ID
    
            try {
                if (hubConnection.state === signalR.HubConnectionState.Connected) {
                    // Stop the SignalR connection
                    await hubConnection.stop();
                    alert(`SignalR connection stopped. sid: ${sid}`);
                }
    
                // Clear local storage
                localStorage.removeItem('sid');
                localStorage.removeItem('user');
                localStorage.removeItem('status');
    
                // Navigate to the login page
                navigate('/');
            } catch (err) {
                console.error("Error during logout process:", err);
            }
        } else if (item === 'settings') {
            setIsModalOpen(true); // Open the modal when settings is clicked
        } else if (item === 'profile') {
            // Navigate to the profile page
            navigate('/profile');
        } else if (item === 'home') {
            // Navigate to the home page
            navigate('/home');
        }
    };
    

    const closeModal = () => {
        setIsModalOpen(false); // Close the modal
    };

    return (
        <div className="NavBox">
            <Logo flex="row" />
            <nav className="NavItems">
                <div
                    className={`NavItem ${activeItem === 'home' ? 'active' : ''}`}
                    onClick={() => handleClick('home')}
                >
                    <FaHome size={24} title="Home" />
                </div>
                <div
                    className={`NavItem ${activeItem === 'settings' ? 'active' : ''}`}
                    onClick={() => handleClick('settings')}
                >
                    <FaCog size={24} title="Settings" />
                </div>
                <div
                    className={`NavItem ${activeItem === 'profile' ? 'active' : ''}`}
                    onClick={() => handleClick('profile')}
                >
                    <FaUser size={24} title="Profile" />
                </div>
                <div
                    className={`NavItem ${activeItem === 'logout' ? 'active' : ''}`}
                    onClick={() => handleClick('logout')}
                >
                    <FiLogOut size={24} title="Logout" />
                </div>
            </nav>
             {/* Modal Component */}
             <Settings isOpen={isModalOpen} onClose={closeModal}>
                <h2>Settings</h2>
                <button onClick={closeModal}>Close</button>
            </Settings>
        </div>
    );
}

export default Nav;
