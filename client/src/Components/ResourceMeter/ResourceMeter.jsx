/* eslint-disable no-unused-vars */
import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import './ResourceMeter.css';

/**
 * A circular meter that displays the usage of a resource with a color indication
 * of its usage level.
 *
 * @param {Object} props - The component's properties.
 * @param {string} props.resourceAvailability - The name of the availability of the resource.
 * @param {string} props.resourceName - The name of the resource.
 * @param {number} props.usage - The percentage of the resource that is currently being used.
 * @returns {ReactElement} A ReactElement representing the component.
 */
function ResourceMeter({ resourceAvailability, resourceName, usage }) {
    /**
     * Returns the color of the resource meter based on the usage percentage.
     *  - Green for below 70%
     *  - Yellow for 70–89%
     *  - Red for 90% and above
     * @param {number} usage - The percentage of the resource that is currently being used.
     * @returns {string} The color of the resource meter.
     */
    const getColor = (usage) => {
        if (usage >= 70 && usage < 90) return '#ffc107'; // Yellow for 70–89%
        if (usage >= 90) return '#f44336'; // Red for 90% and above
        return '#4caf50'; // Green for below 70%
    };
    const [animatedUsage, setAnimatedUsage] = useState(usage);
    const [color, setColor] = useState(getColor(usage)); // Track color in state

    
    
/** 
 * This useEffect handles the animation steps for seamlessly transitioning the usage percentage.
 * It updates the animatedUsage state to create a smooth transition effect.
*/
    useEffect(() => {
        const animationDuration = 300; // Duration in milliseconds
        const steps = 30; // Number of steps for smoother animation
        const stepTime = animationDuration / steps;
    
        const step = (usage - animatedUsage) / steps; // Use the latest `animatedUsage` value
    
        let currentUsage = animatedUsage; // Initialize with the current animated usage
    
        const interval = setInterval(() => {
            currentUsage += step;
    
            // Stop the animation when close to the target to prevent overshooting
            if ((step > 0 && currentUsage >= usage) || (step < 0 && currentUsage <= usage)) {
                clearInterval(interval);
                setAnimatedUsage(usage); // Ensure it matches the target usage
                setColor(getColor(usage)); // Final color for the target usage
            } else {
                setAnimatedUsage(currentUsage); // Update animated usage
                setColor(getColor(currentUsage)); // Update color dynamically
            }
        }, stepTime);
    
        return () => clearInterval(interval);
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [usage]);
    

    return (
        <div className='container'>
            <h2>{resourceName}</h2>
            <div className="resource-meter">
                <div
                    className="meter-circle"
                    style={{
                        background: `conic-gradient(${color} ${animatedUsage}%, #eee ${animatedUsage}% 100%)`,
                    }}
                >
                    <div className="inner-circle">
                        <h2 className="percentage">
                            {animatedUsage > 0 && animatedUsage < 1 
                            ? animatedUsage.toFixed(2) 
                            : Math.round(animatedUsage)}%
                        </h2>

                    </div>
                </div>
            </div>
            <h2>{resourceAvailability}</h2>
        </div>
    );
}

ResourceMeter.propTypes = {
    usage: PropTypes.number.isRequired,
    resourceName: PropTypes.string.isRequired,
    resourceAvailability: PropTypes.string.isRequired,
};

export default ResourceMeter;
