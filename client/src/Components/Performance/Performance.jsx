/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
import React, { useEffect, useState } from 'react';
import './Performance.css'; // Add consistent styles for performance page
import ResourceMeter from '../ResourceMeter/ResourceMeter';

const platformObj = {
    RAM: "Windows Only!",
    Disk: "Windows Only!",
    CPU: "Cross Platform"
};

function Performance({ hubConnection }) {
    const [metrics, setMetrics] = useState({
        cpuUsage: 5,
        ramUsage: 80,
        diskUsage: 90
    });

    useEffect(() => {
        let isMounted = true; // Prevent state updates on unmounted component

        const fetchMetrics = async () => {
            if (hubConnection.state === "Connected") {
                try {
                    console.log("Fetching metrics...");
                    await hubConnection.invoke("GetMetrics"); // Request metrics from the server
                    console.log("Metrics request sent");
                } catch (error) {
                    console.error("Error fetching metrics:", error);
                }
            } else {
                console.log("Connection not ready");
            }
        };

        const initializeListener = () => {
            hubConnection.off("ReceiveMetrics"); // Ensure no duplicate listeners
            console.log("Registering ReceiveMetrics listener...");
            hubConnection.on("ReceiveMetrics", (cpuUsage, ramUsage, diskUsage) => {
                console.log("Metrics received:", { cpuUsage, ramUsage, diskUsage });
                if (isMounted) {
                    setMetrics({ cpuUsage, ramUsage, diskUsage });
                }
            });
        };

        if (hubConnection.state === "Connected") {
            console.log("SignalR already connected. Initializing listener...");
            initializeListener(); // Register listener if already connected
        } else {
            console.error("SignalR connection not established!");
        }

        // Fetch metrics every 2 seconds
        const intervalId = setInterval(fetchMetrics, 500);

        return () => {
            isMounted = false; // Prevent updates to unmounted component
            clearInterval(intervalId); // Cleanup interval
            hubConnection.off("ReceiveMetrics"); // Remove listener on unmount
            console.log("Performance component unmounted and listener removed");
        };
    }, [hubConnection]); // Add `hubConnection` as a dependency

    return (
        <div className="Performance-Container">
            <ResourceMeter 
                usage={metrics.cpuUsage} 
                resourceName="CPU" 
                resourceAvailability={platformObj.CPU} 
            />
            <ResourceMeter 
                usage={metrics.ramUsage} 
                resourceName="RAM" 
                resourceAvailability={platformObj.RAM} 
            />
            <ResourceMeter 
                usage={metrics.diskUsage} 
                resourceName="Disk" 
                resourceAvailability={platformObj.Disk} 
            />
        </div>
    );
}

export default Performance;
