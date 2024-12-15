/* eslint-disable no-unused-vars */
import React,{ useState} from 'react';
import Input from '../Input';
import './Login.css';

/**
 * Login component renders a form with email and password inputs.
 * It uses the Input component to handle user input.
 * The state of the email and password is managed using useState hooks.
 * The form is styled using the 'Login-Container' class.
 */

function Login() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    return (
        <div className='Login-Container'>
        
            <Input 
                className='Login-Input'
                type="email" 
                placeholder="Email" 
                value={email} 
                action={(e) => setEmail(e)} 
                required 
            />
            
            
            <Input 
                className='Login-Input'
                type="password" 
                placeholder="Password" 
                value={password} 
                action={(e) => setPassword(e)} 
                required 
            />
        </div>
    );
}

export default Login;