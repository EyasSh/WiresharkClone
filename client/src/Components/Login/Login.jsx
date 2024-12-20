/* eslint-disable no-unused-vars */
import React,{ useState} from 'react';
import Input from '../Input';
import './Login.css';
import Logo from '../Logo/Logo';
import Button from '../Button/Button';

/**
 * Login component renders a form with email and password inputs.
 * It uses the Input component to handle user input.
 * The state of the email and password is managed using useState hooks.
 * The form is styled using the 'Login-Container' class.
 */

function Login() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const handleSubmit= ()=>{
        console.log(email,password);
    }
    const handleSignup = ()=>{
        console.log('Sign Up');
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
            />
            <Button action={()=>handleSubmit()} content={'Login'} status='action'/>
            <div className='signup-div'>
                <h4>Don&apos;t have an account?</h4>
                <Button link='/signup' content={'Sign Up'} status='signup' action={()=>handleSignup()}/>
            </div>
        </div>
        </>
    );
}

export default Login;