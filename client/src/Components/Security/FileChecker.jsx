/* eslint-disable no-unused-vars */
import { useState} from 'react';
import Input from '../Input';
import Button from '../Button/Button';
import axios from 'axios';
import './VirusChecker.css';

/**
 * Renders a file upload input field and a button to check the uploaded file.
 * The file is sent to the server to be checked against VirusTotal.
 * If the server returns a 200 status, the file status is updated with a user-friendly message
 * indicating whether the file is malicious or not.
 * If an error occurs, the error is logged to the console.
 * @returns {ReactElement} A React component with a file upload input field and a button to check the file.
 */
function FileChecker() {
    const [fileStatus, setFileStatus] = useState('');
    const [file, setFile] = useState(null);
     // Helper function to create a readable message for the file status
  const getFileStatusMessage = (maliciousCount, undetectedCount) => {
    // Adjust this logic or wording as needed
    if (maliciousCount > 0) {
      return 'This file is malicious. It was flagged by one or more scanning engines.';
    }
    return 'This file appears safe. No issues were detected.';
  };


  /**
   * Handles the file upload and sends it to the server for analysis.
   * On success, updates the file status with a user-friendly message
   * indicating whether the file is malicious or not.
   * If an error occurs, logs the error to the console.
   */
  const handleFile = async () => {
    // Ensure a file is selected
    if (!file) {
      alert('Please select a file first.');
      return;
    }

    try {
      // Build the FormData to send the file
      const formData = new FormData();
      // "file" must match the name expected by your .NET controller
      formData.append('file', file);
      const user = JSON.parse(localStorage.getItem('user'));
      const email = user.email;
      formData.append('email', email);
      formData.append('userId', user.id); // Assuming you store user ID in localStorage
      

      const res = await axios.post(
        'http://localhost:5256/api/user/file',
        formData,
        {
          headers: {
            // Let Axios set the content type (including boundary) automatically
            'X-Auth-Token': localStorage.getItem('X-Auth-Token'),
          },
        }
      );

      if (res.status === 200) {
        // The server returns malicious, undetected, and message
        const { malicious, undetected, message } = res.data;
        // Or you can create a custom message based on counts
        const userFriendlyMessage = getFileStatusMessage(malicious, undetected);

        setFileStatus(userFriendlyMessage);
      }
    } catch (error) {
      console.error(error.response ? error.response.data : error.message);
    }
  };
    return (
        <>
            <div className='File-Container'>
                <h1>File Checker</h1>
                <Input type='file' action={(e)=>setFile(e)} required={true} />
                <Button content='Check File' action={async()=>await handleFile()} status='signup' />
                <b><span className='url-status'>{fileStatus}</span></b>
            </div>
        </>
    );
}

export default FileChecker;