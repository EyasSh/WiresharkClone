import React from 'react';
import { useState, useEffect, useCallback } from 'react';
function Input(props) 
{
    let [inputType, setInputType] = useState(props.type);
    let func= props.func;
    return (
        <div>
            <input type={inputType} onChange={(e) => func(e.target.value)}></input>
        </div>
    );
}

export default Input;