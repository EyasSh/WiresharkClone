
import { useNavigate } from 'react-router';
import { FaLink ,FaFile, FaHistory } from 'react-icons/fa';
import { useState} from 'react';
import './Nav.css';

function CNav() {
    const navigate = useNavigate();
    const [activeItem, setActiveItem] = useState('security');
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
