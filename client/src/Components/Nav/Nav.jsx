/* eslint-disable no-unused-vars */
import React, { useState } from 'react';
import Logo from '../Logo/Logo';
import { FaHome, FaCog } from 'react-icons/fa';
import { FiLogOut } from 'react-icons/fi'; // Import the logout icon
import './Nav.css';

function Nav(props) {
    const [activeItem, setActiveItem] = useState('home'); // Track active item

    const handleClick = (item) => {
        setActiveItem(item); // Set the active item
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
        </div>
    );
}

export default Nav;
