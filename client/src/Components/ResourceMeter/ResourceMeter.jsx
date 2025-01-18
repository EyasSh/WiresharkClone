/* eslint-disable no-unused-vars */
import React from 'react';
import PropTypes from 'prop-types';
import './ResourceMeter.css';

function ResourceMeter({ resourceName,usage }) {
    // Determine color based on usage percentage
    const getColor = (usage) => {
        if (usage < 70) return '#4caf50';
        if (usage < 90) return '#ffc107';
        return '#f44336';
    };

    const color = getColor(usage);

    return (
        <div className='container'>
            <h4>{resourceName}</h4>
        <div className="resource-meter">
            <div
                className="meter-circle"
                style={{
                    background: `conic-gradient(${color} ${usage}%, #eee ${usage}% 100%)`,
                }}
            >
                <div className="inner-circle">
                    <span className="percentage">{usage}%</span>
                </div>
            </div>
        </div>
        </div>
    );
}

ResourceMeter.propTypes = {
    usage: PropTypes.number.isRequired,
    resourceName: PropTypes.string.isRequired,
};

export default ResourceMeter;
