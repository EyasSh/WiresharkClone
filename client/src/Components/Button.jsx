/* eslint-disable no-unused-vars */
/* eslint-disable react/prop-types */
// eslint-disable-next-line no-unused-vars
import  React, { useState } from 'react';

function Button(props) {
    // eslint-disable-next-line no-unused-vars, react/prop-types
    const [buttonClass, setButtonClass] = useState(props.class);
    const [buttonText, setButtonText] = useState(props.text);
    return (
        <div>
            <button className={buttonClass}>{buttonText}</button>
        </div>
    );
}

export default Button;