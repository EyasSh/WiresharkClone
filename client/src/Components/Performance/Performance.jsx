/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
import React from 'react';
import './Performance.css'; // Add consistent styles for performance page
import '../ResourceMeter/ResourceMeter' // Import the ResourceMeter component
import ResourceMeter from '../ResourceMeter/ResourceMeter';
const platformObj={
    "RAM":"Windows Only!",
    "Disk":"Windows Only!",
    "CPU":"Cross Platform"
}
function Performance(props) {
    const connection = props.hubConnection;
    const sid = props.sid;
    const RamName="RAM";
    const CpuName="CPU";
    const DiskName="Disk";
    return (
        <div className="Performance-Container">
            <ResourceMeter usage={70} resourceName={CpuName} resourceAvailability={platformObj.CPU}/>
            <ResourceMeter usage={70} resourceName={RamName} resourceAvailability={platformObj.RAM}/>
            <ResourceMeter usage={80} resourceName={DiskName} resourceAvailability={platformObj.Disk}/>
        </div>
    );
}

export default Performance;