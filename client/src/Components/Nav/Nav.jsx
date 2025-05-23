/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
import React, { useState } from 'react';
import {FaLock, FaHome, FaCog, FaEnvelope } from 'react-icons/fa';
import { FiLogOut } from 'react-icons/fi'; // Import the logout icon
import {IoAnalytics, IoAnalyticsSharp} from 'react-icons/io5';
import './Nav.css';
import '../../index.css';
import { useNavigate } from 'react-router';
import Logo from '../Logo/Logo';
import Settings from '../Settings/Settings';

/**
 * Nav component renders a navigation bar with links to the home, profile, settings, logout, and performance pages.
 * It also contains a modal component that is opened when the settings button is clicked.
 * The component receives a hubConnection and a sid as props, which are used to disconnect the SignalR connection on logout.
 * The active navigation item is highlighted in the navigation bar.
 * The component uses the useNavigate hook to navigate to the corresponding page when a navigation item is clicked.
 * The component uses the useState hook to manage the active item and the isModalOpen state.
 * The component uses the useEffect hook to set the active item to 'home' when the component mounts.
 * @param {Object} props
 * @param {Object} props.hubConnection The SignalR hub connection object.
 * @param {string} props.sid The session ID of the user.
 * @returns {ReactElement} The Nav component.
 */
function Nav({ hubConnection, sid }) {
    const [activeItem, setActiveItem] = useState('home');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const navigate = useNavigate();
    
    /**
     * Handles a click event on a navigation item.
     * If the item is 'logout', it disconnects the SignalR connection, clears the session data, and navigates to the login page.
     * If the item is 'settings', it opens the settings modal.
     * If the item is 'packets', 'home', or 'performance', it navigates to the corresponding page and passes the session ID as state.
     * @param {string} item The navigation item that was clicked.
     */
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
        } else if (item === 'packets') {
            navigate('/packets', { state: { sid } }); // Pass SID only
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
    /**
     * Closes the settings modal.
     */
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
                <div className={`NavItem ${activeItem === 'security' ? 'active' : ''}`} onClick={() => handleClick('security')}>
                    <FaLock size={24} title="Security" />
                </div>
                <div
                    className={`NavItem ${activeItem === 'profile' ? 'active' : ''}`}
                    onClick={() => handleClick('packets')}
                >
                    <FaEnvelope size={24} title="Packet History" />
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
