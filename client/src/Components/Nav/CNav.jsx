
import { useNavigate } from 'react-router';
import { FaLink ,FaFile, FaHistory } from 'react-icons/fa';
import { useState} from 'react';
import './Nav.css';

/**
 * CNav is a secondary Nav component which renders a navigation bar with options for file check, file history, and link check.
 * The active navigation item is highlighted and clicking a navigation item changes the active state 
 * and navigates to the corresponding page using the useNavigate hook.
 * The component uses useState to manage the active navigation item state.
 */
function CNav() {
    const navigate = useNavigate();
    const [activeItem, setActiveItem] = useState('security');
    /**
     * Handles a click event on a navigation item.
     * Sets the activeItem state to the item that was clicked and navigates to the corresponding page using the useNavigate hook.
     * @param {string} item The navigation item that was clicked.
     */
   const handleClick = (item) => {
        setActiveItem(item);
        if (item === 'security') {
            navigate('/security');
        } else if (item === 'history') {
            navigate('/security/history');
        } else if (item === 'link') {
            navigate('/security/link');
        }
   }
    return (
        <div className='CNavCont'>
            <nav className='CNavItems'>


            <div onClick={() => handleClick('security')} className={`NavItem ${activeItem === 'security' ? 'active' : ''}`}>
                <FaFile size={20} title='File Check' /> 
            </div>
            <div onClick={() => handleClick('history')} className={`NavItem ${activeItem === 'history' ? 'active' : ''}`}>
                <FaHistory size={20} title='File History' /> 
            </div>
            <div onClick={() => handleClick('link')} className={`NavItem ${activeItem === 'link' ? 'active' : ''}`}>
                <FaLink size={20} title='Check Link' /> 
            </div>
            </nav>
        </div>
    );
}

export default CNav;
