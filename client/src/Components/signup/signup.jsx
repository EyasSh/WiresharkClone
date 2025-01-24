/* eslint-disable no-unused-vars */
import React, { useState } from 'react';
import Input from '../Input';
import './signup.css';
import "../Login/Login.css";
import Button from '../Button/Button';
import axios from 'axios';
import Logo from '../Logo/Logo';



/**
 * A react component which renders a sign up form and handles sign up to the system.
 * The form has fields for name, email, password, and date of birth. When the form is
 * submitted, it sends a POST request to the user controller's sign up action. If the
 * request is successful, it will alert the response message. If the request fails, it
 * will alert the error response message.
 * @returns {ReactElement} The rendered sign up form.
 */
function Signup() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [name, setName] = useState('');
    const [date, setDate] = useState(new Date()); // Initially a Date object

    /**
     * Handles the sign up form submission by sending a POST request to the user
     * controller's sign up action. If the request is successful, it will alert the
     * response message. If the request fails, it will alert the error response
     * message.
     */
    const handleSignup = async () => {
        try {
            const response = await axios.post('http://localhost:5256/api/user/signup', {
                name,
                email,
                password,
                date: date.toISOString().split('T')[0], // Format as YYYY-MM-DD
            });

            if (response.status === 200) {
                alert(`${response.data}`);
            }
        } catch (e) {
            alert(`Sign Up Failed ${e.response ? e.response.data : e.message}`);
        }
    };

    return (
        <div className='Signup-Container'>
            <Logo flex='column' />
            <Input 
                className='Signup-Input'
                type="text" 
                placeholder="Name" 
                value={name} 
                action={(e) => setName(e)} 
                required 
            />
            <Input 
                className='Signup-Input'
                type="email" 
                placeholder="Email" 
                value={email} 
                action={(e) => setEmail(e)} 
                required 
            />
            <Input 
                className='Signup-Input'
                type="password" 
                placeholder="Password" 
                value={password} 
                action={(e) => setPassword(e)} 
                required 
                showPasswordToggle={true}
            />
            <Input
                className='Signup-Input'
                type="date" 
                placeholder="Date of Birth" 
                value={date.toISOString().split('T')[0]} // Convert Date object to YYYY-MM-DD
                action={(e) => setDate(new Date(e))} // Convert input string to Date object
                required
            />
            <Button status='signup' content='Sign Up' action={async () => await handleSignup()} />
        </div>
    );
}

export default Signup;
