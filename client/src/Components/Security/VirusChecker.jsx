/* eslint-disable no-unused-vars */
import {useState} from 'react';
import './VirusChecker.css';
import Input from '../Input'
import Button from '../Button/Button';
import axios from 'axios';
import { BrowserRouter as Router, Routes, Route } from 'react-router';
import FileChecker from './FileChecker';
import FileHistory from './FileHistory';
import LinkChecker from './LinkChecker';
import CNav from '../Nav/CNav';

/**
 * VirusChecker is a component that checks if a URL or a file is malicious.
 * It provides two input fields: one for entering a URL and one for selecting a file.
 * When the user clicks on the "Check URL" or "Check File" button, the component sends a request to the server to check if the URL or file is malicious.
 * The component displays a message indicating whether the URL or file is malicious or not.
 * If the URL or file is malicious, it displays a warning message.
 * If the URL or file is not malicious, it displays a success message.
 * If there is an error while checking the URL or file, it displays an error message.
 * The component also provides a button to clear the input fields.
 * The component is fully responsive and works well on different screen sizes.
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