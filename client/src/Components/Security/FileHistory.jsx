import { useState, useEffect} from 'react';
import axios from 'axios';
import HistoryBox from '../HistoryBox/HistoryBox';
import './VirusChecker.css';

function FileHistory() {
    const [files,setFiles] = useState([]);
    useEffect(() => {
/**
 * Fetches the history of file checks for the current user from the server.
 * Retrieves the user's authentication token and userId from local storage,
 * then makes an API call to get the file history data.
 * Updates the state with the retrieved data, which includes file name,
 * date of check, and result.
 * Utilizes Axios for the API request and updates the `files` state
 * with the result data.
 */
        const fetchChecks = async () => {
            const token= localStorage.getItem('X-Auth-Token')
            const user = localStorage.getItem('user')
            const userId = JSON.parse(user).id;
            const result = await axios(`http://localhost:5256/api/user/history?userId=${userId}`, {
                headers: {
                    'X-Auth-Token': token,
                },
            });
            setFiles(result.data);
        };
        fetchChecks();
    }, []);
    return (
        <div className='FileHistoryCont'>
            <h1>File History</h1>
            <>
                {files.map((file, index) => (
                    <HistoryBox
                        key={index}
                        fileName={file.name}
                        date={file.date}
                        result={file.result}
                    />
                ))}
            </>
        </div>
    );
}

export default FileHistory;