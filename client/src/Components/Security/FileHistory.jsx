import { useState, useEffect} from 'react';
import axios from 'axios';
import HistoryBox from '../HistoryBox/HistoryBox';
import './VirusChecker.css';

function FileHistory() {
    const [files,setFiles] = useState([]);
    useEffect(() => {
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