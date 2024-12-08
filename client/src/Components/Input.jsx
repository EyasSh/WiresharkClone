/* eslint-disable no-unused-vars */
/* eslint-disable react/prop-types */
import React from 'react';
import { useState, useEffect, useCallback } from 'react';
function Input(props) 
{
     
    let [inputType, setInputType] = useState(props.type);
    const fn= props?.func;
    
   
    return (
        <div>
            <input type={inputType} onChange={(e) => fn(e.target.value)}></input>
        </div>
    );
}

export default Input;