/* eslint-disable no-unused-vars */
import React from 'react';
import './Home.css';
import { Link, useNavigate } from 'react-router';
import { useState, useEffect } from 'react';
import axios from 'axios';
//TODO: Add react icons below

function Home(props) {
    const navigate = useNavigate();
    useEffect(() => {
        const validateToken = async () => {
            const token = localStorage.getItem('X-Auth-Token');
            if (!token) {
                alert("No token found, redirecting to login...");
                navigate('/'); // Redirect to login
                return;
            }

            try {
                const response = await axios.get('http://localhost:5256/api/user/validate', {
                    headers: {
                        'X-Auth-Token': token // Add token as a header
                    }
                });

                if (response.status === 200) {
                    console.log("Token is valid. Rendering the page...");
                    // Token is valid; proceed with rendering
                } else {
                    alert("Invalid token. Redirecting to login...");
                    navigate('/'); // Redirect to login if token is invalid
                }
            } catch (error) {
                alert("Validation failed:", error);
                navigate('/'); // Redirect to login on validation failure
            }
        };

        validateToken(); // Call the async function
    }, [navigate]);// Empty dependency array to ensure this runs only once on component mount
    return (
        <>
            <h1>Grove Street, home...at least it was before I fucked everything up</h1>
        </>
    );
}

export default Home;