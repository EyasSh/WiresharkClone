/* eslint-disable no-unused-vars */
import {useEffect, useState} from 'react';
import PropTypes from 'prop-types';
import './HistoryBox.css';
HistoryBox.propTypes = {
    fileName: PropTypes.string.isRequired,
    date: PropTypes.string.isRequired,
    result: PropTypes.string.isRequired,
}

function HistoryBox(props) {
    const [name, setName] = useState(props.fileName);
    const [date, setDate] = useState(
  props.date.split('T')[0].split('-').reverse().join('-')
);
 // Format date to DD-MM-YYYY
    const [result, setResult] = useState(props.result);
    return (
        <div className='HistoryBox'>
            <div className='HistoryBox-Name'>{name}</div>
            <div className='HistoryBox-Date'>{date}</div>
            <div className='HistoryBox-Result'>{result}</div>
            
        </div>
    );
}

export default HistoryBox;