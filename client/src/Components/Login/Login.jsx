/* eslint-disable no-unused-vars */
import React,{ useState} from 'react';
import Input from '../Input';
import './Login.css';
import Logo from '../Logo/Logo';
import Button from '../Button/Button';
import Message from '../Message/Message.jsx';
import axios from 'axios';
import { useNavigate } from 'react-router';



/**
 * @function Login
 * @description A react component which renders a login form and handles login to the system.
 * @returns {ReactElement} The rendered login form.
 */
function Login() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const navigate = useNavigate();
    const handleSubmit = async () => {
       
    
        try {
            const response = await axios.post('http://localhost:5256/api/user/login', {
                email,
                password,
            });
    
            if (response.status === 200) {
                localStorage.setItem("X-Auth-Token", response.headers["x-auth-token"]);
                localStorage.setItem("user", JSON.stringify(response.data));
                navigate('/home');
            }
        } catch (error) {
            throw error
        }
    };
    
    const handleSignup = ()=>{
        alert('Sign Up');
    }
    return (
        <>
        
        <div className='Login-Container'>
            <Logo flex='column' />
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
                showPasswordToggle={true}
            />
            <Button action={()=>handleSubmit()} content={'Login'} status='action'/>
            <div className='signup-div'>
                <h4>Don&apos;t have an account?</h4>
                <Button link='/signup' content={'Sign Up'} status='signup'/>
            </div>
        </div>
        </>
    );
}

export default Login;