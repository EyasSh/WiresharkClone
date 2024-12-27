/* eslint-disable no-unused-vars */
import React, { useState } from 'react';
import Logo from '../Logo/Logo';
import { FaHome, FaCog } from 'react-icons/fa';
import { FiLogOut } from 'react-icons/fi'; // Import the logout icon
import './Nav.css';
import Settings from '../Settings/Settings';

function Nav(props) {
    const [activeItem, setActiveItem] = useState('home');
    const [isModalOpen, setIsModalOpen] = useState(false); // Modal visibility state

    const handleClick = (item) => {
        setActiveItem(item);
        if (item === 'settings') {
            setIsModalOpen(true); // Open the modal when settings is clicked
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
