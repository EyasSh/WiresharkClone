/* eslint-disable no-unused-vars */
/* eslint-disable react/prop-types */
import React, { act } from 'react';
import { useState, useEffect, useCallback } from 'react';
/**
 * @function Input
 * @description A react component which renders an input box based on given props.
 * @param {Object} props - The properties of the input box.
 * @prop {string} [type=text] - The type of the input box.
 * @prop {string} [placeholder] - The placeholder of the input box.
 * @prop {string} [value] - The value of the input box.
 * @prop {function} [action] - The function to be called when the input box changes.
 * @prop {boolean} [required=false] - Whether the input box is required to be filled or not.
 * @returns {ReactElement} - The rendered input box.
 */
function Input(props) 
{
    const [inputType, setInputType] = useState(props.type);
    const fn= props?.action;
    const handleChange = (e)=>{
        fn(e.target.value);
    }
   
    return (
        <div>
             <input 
             className={props.className}
            placeholder={props.placeholder} 
            type={props.type || "text"} 
            value={props.value || ""} 
            onChange={async(e) => await props.action(e.target.value)} 
            required={props.required} 
        />
        </div>
    );
}

export default Input;