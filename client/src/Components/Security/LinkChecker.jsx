import { useState} from 'react';
import './VirusChecker.css';
import axios from 'axios';
import Input from '../Input';
import Button from '../Button/Button';

function LinkChecker() {
    const [url, setUrl] = useState('');
    const [urlStatus, setUrlStatus] = useState(''); // State for displaying the result message
    
    
    /**
     * Given the statistical data from VirusTotal, determines the status message
     * to be displayed for the user. The status message will be one of the following:
     * 
     * - "This URL is harmful. It was flagged as malicious by several engines."
     * - "This URL is suspicious. It requires further caution."
     * - "This URL is harmless. No issues were found."
     * - "No sufficient data available for this URL."
     * 
     * @param {Object} stats An object with 3 properties: malicious, suspicious, and harmless.
     * Each of these properties represents the number of engines that flagged the URL
     * as malicious, suspicious, or harmless, respectively.
     */
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

/**
 * Asynchronously handles the URL checking process by sending a POST request
 * to the server to validate the URL against VirusTotal. 
 * On success, updates the URL status message based on the server's response.
 * 
 * The function sends the URL as a raw string in the request body, 
 * along with necessary headers for content type and authentication.
 * 
 * If the response status is 200, it extracts statistical data from the response
 * and uses it to generate a user-friendly status message which is then displayed.
 * 
 * In case of an error, logs the error details to the console.
 */
    const handleUrl = async()=>{
        const user  = localStorage.getItem('user');

        try {
            const email = user ? JSON.parse(user).email : '';
            const res = await axios.post(
                'http://localhost:5256/api/user/domain',
                {
                    url,      // or url: url
                    email     // will be "" if not logged in
                },
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
    return (
        <>
            <div className='Link-Container'>
                <h1>URL Checker</h1>
                <Input type='url' placeholder='Enter URL' value={url} action={(e)=>setUrl(e)} required={true} />
                <Button content='Check URL' action={async()=>await handleUrl()} status='signup' />
                <b><span className='url-status'>{urlStatus}</span></b>
            </div>
        </>
    );
}

export default LinkChecker;