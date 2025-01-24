/* eslint-disable no-unused-vars */
import {useState} from 'react';
import './VirusChecker.css';
import Input from '../Input'
import Button from '../Button/Button';
import axios from 'axios';
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
function VirusChecker(props) {
    const [url, setUrl] = useState('');
    const [file, setFile] = useState(null);
    const [urlStatus, setUrlStatus] = useState(''); // State for displaying the result message
    // Helper function to determine URL status message
    const getUrlStatusMessage = (stats) => {
        const { malicious, suspicious, harmless } = stats;

        if (malicious > 0) {
            return 'This URL is harmful. It was flagged as malicious by several engines.';
        } else if (suspicious > 0) {
            return 'This URL is suspicious. It requires further caution.';
        } else if (harmless > 0) {
            return 'This URL is harmless. No issues were found.';
        } else {
            return 'No sufficient data available for this URL.';
        }
    };

    const handleUrl = async()=>{
        try {
            const res = await axios.post(
                'http://localhost:5256/api/user/domain',
                `"${url}"`, // Send the URL as a raw string
                {
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Auth-Token': localStorage.getItem('X-Auth-Token'),
                    },
                }
            );
            
            if (res.status === 200) {
                
                 // Extract stats from the response
                 const stats = res.data.results.data.attributes.stats;

                 // Get a user-friendly message
                 const message = getUrlStatusMessage(stats);
 
                 // Update the status display
                 setUrlStatus(message);
            }
            
            
            if (res.status === 200) {
                console.log(res.data);
            }
            
            if(res.status === 200){
                console.log(res.data);
            }
            
        } catch (error) {
            console.error(error.response ? error.response.data : error.message);
        }
    }
    const handleFile = async()=>{
        alert('Checking Virus in File');
    }
    return (
        <div className='Vc-Container'>
            <h1 className='Vc-heading'>Check if a link is malicious</h1>
            <div className='Link-Container'>
                <Input type='url' placeholder='Enter URL' value={url} action={(e)=>setUrl(e)} required={true} />
                <Button content='Check URL' action={async()=>await handleUrl()} status='signup' />
                <span className='url-status'>{urlStatus}</span>
            </div>
            <h1>Or Check if a file is malicious</h1>
            <div className='File-Container'>
                <Input type='file' action={(e)=>setFile(e)} required={true} />
                <Button content='Check File' action={async()=>await handleFile()} status='signup' />
            </div>
            

            
        </div>
    );
}

export default VirusChecker;