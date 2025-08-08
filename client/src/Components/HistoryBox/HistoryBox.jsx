/* eslint-disable no-unused-vars */
import { useState} from 'react';
import PropTypes from 'prop-types';
import './HistoryBox.css';
HistoryBox.propTypes = {
    fileName: PropTypes.string.isRequired,
    date: PropTypes.string.isRequired,
    result: PropTypes.string.isRequired,
}

/**
 * Renders a styled box with a filename, date of check, and the result of the check.
 * The date is formatted to DD-MM-YYYY.
 * @param {Object} props
 * @param {string} props.fileName - The name of the file.
 * @param {string} props.date - The date of the check in the form YYYY-MM-DDTHH:MM:SS.SSSZ.
 * @param {string} props.result - The result of the check.
 * @returns {ReactElement} A React component displaying the file check history in a styled box.
 */
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