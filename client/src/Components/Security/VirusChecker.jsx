/* eslint-disable no-unused-vars */
import {useState} from 'react';
import './VirusChecker.css';
import Input from '../Input'
import Button from '../Button/Button';

function VirusChecker(props) {
    const [url, setUrl] = useState('');
    const [file, setFile] = useState(null);
    const handleUrl = ()=>{
        alert('Checking Virus in URL');
    }
    const handleFile = ()=>{
        alert('Checking Virus in File');
    }
    return (
        <div className='Vc-Container'>
            <h1 className='Vc-heading'>Check if a link is malicious</h1>
            <div className='Link-Container'>
                <Input type='url' placeholder='Enter URL'action={(e)=>setUrl(e)} required={true} />
                <Button content='Check URL' action={()=>handleUrl()} status='signup' />
            </div>
            <h1>Or Check if a file is malicious</h1>
            <div className='File-Container'>
                <Input type='file' action={(e)=>setFile(e)} required={true} />
                <Button content='Check File' action={()=>handleFile()} status='signup' />
            </div>
            

            
        </div>
    );
}

export default VirusChecker;