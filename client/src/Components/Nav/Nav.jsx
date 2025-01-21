/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
import React, { useState } from 'react';
import {FaLock, FaUser, FaHome, FaCog } from 'react-icons/fa';
import { FiLogOut } from 'react-icons/fi'; // Import the logout icon
import {IoAnalytics, IoAnalyticsSharp} from 'react-icons/io5';
import './Nav.css';
import { useNavigate } from 'react-router';
import Logo from '../Logo/Logo';
import Settings from '../Settings/Settings';

function Nav({ hubConnection, sid }) {
    const [activeItem, setActiveItem] = useState('home');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const navigate = useNavigate();
    
    const handleClick = async (item) => {
        setActiveItem(item);

        if (item === 'logout') {
            try {
                if (hubConnection.state === 'Connected') {
                    await hubConnection.stop();
                    console.log(`Disconnected. SID: ${sid}`);
                }

                localStorage.clear(); // Clear session data
                navigate('/'); // Navigate to login
            } catch (err) {
                console.error('Error during logout:', err);
            }
        }
        else if (item === 'settings') {
            setIsModalOpen(true); // Open the modal when settings is clicked
        } else if (item === 'profile') {
            navigate('/profile', { state: { sid } }); // Pass SID only
        } else if (item === 'home') {
            navigate('/home', { state: { sid } }); // Pass SID only
        }
        else if (item === 'performance') {
            navigate('/performance', { state: { sid } }); // Pass SID only
        }
        else if(item==='security'){
            navigate('/security'); 
        }
    };
    const closeModal = () => {
        setIsModalOpen(false); // Close the modal
    };
    return (
        <div className="NavBox">
            <Logo flex="row" />
            <nav className="NavItems">
                <div className={`NavItem ${activeItem === 'home' ? 'active' : ''}`} onClick={() => handleClick('home')}>
                    <FaHome size={24} title="Home" />
                </div>
                <div className={`NavItem ${activeItem === 'performance' ? 'active' : ''}`} onClick={() => handleClick('performance')}>
                    <IoAnalyticsSharp size={24} title="Performance" />
                </div>
                <div className={`NavItem ${activeItem === 'security' ? 'active' : ''}`} onClick={() => handleClick('performance')}>
                    <FaLock size={24} title="Security" />
                </div>
                <div
                    className={`NavItem ${activeItem === 'profile' ? 'active' : ''}`}
                    onClick={() => handleClick('profile')}
                >
                    <FaUser size={24} title="Profile" />
                </div>
                <div
                    className={`NavItem ${activeItem === 'settings' ? 'active' : ''}`}
                    onClick={() => handleClick('settings')}
                >
                    <FaCog size={24} title="Settings" />
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
