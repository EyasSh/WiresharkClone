import './VirusChecker.css';
import { BrowserRouter as Routes, Route } from 'react-router';
import FileChecker from './FileChecker';
import FileHistory from './FileHistory';
import LinkChecker from './LinkChecker';
import CNav from '../Nav/CNav';


/**
 * VirusChecker component renders a navigation bar and sets up routes for various security checking components.
 * It includes routes for file checking, link checking, and file history.
 * The component uses the CNav component for navigation and the Routes component for managing route paths.
 * 
 * @returns {ReactElement} The VirusChecker component with nested routes for different security checks.
 */
function VirusChecker() {

    return (
        <>
            <CNav />
            
                <Routes>
                    <Route index path="/" element={<FileChecker />} />
                    <Route path="/link" element={<LinkChecker />} />
                    <Route path="/history" element={<FileHistory />} />
                </Routes>
            
        </>
    );
}

export default VirusChecker;