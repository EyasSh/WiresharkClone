/* eslint-disable react/prop-types */
/* eslint-disable no-unused-vars */
import React, { useEffect, useState } from 'react';
import './Performance.css'; // Add consistent styles for performance page
import ResourceMeter from '../ResourceMeter/ResourceMeter';
import axios from 'axios';

const platformObj = {
    RAM: "Windows Only!",
    Disk: "Windows Only!",
    CPU: "Cross Platform"
};

/**
 * Performance component displays system metrics as resource meters.
 * It fetches metrics from the server every 2 seconds and updates the UI accordingly.
 * The component also handles the case where the SignalR connection is not established.
 * If the connection is not established, it will not fetch metrics and will not display anything.
 * 
 * @param {Object} hubConnection - The SignalR connection object.
 * 
 * @returns {React.ReactElement} A React component displaying system metrics as resource meters.
 */
function Performance({ hubConnection }) {
    const [metrics, setMetrics] = useState({
        cpuUsage: 0,
        ramUsage: 0,
        diskUsage: 0
    });
    const user = JSON.parse(localStorage.getItem('user'));
    const name = user?.name.split(' ')[0] || 'Guest';
    const email = user?.email;

    useEffect(() => {
        let isMounted = true;
    
        const fetchMetrics = async () => {
            if (hubConnection.state === "Connected") {
                try {
                    console.log("Fetching metrics...");
                    await hubConnection.invoke("GetMetrics");
                    console.log("Metrics request sent");
                } catch (error) {
                    console.error("Error fetching metrics:", error);
                }
            } else {
                console.log("SignalR connection not established!");
            }
        };
    
        const initializeListener = async() => {
            hubConnection.off("ReceiveMetrics"); // Remove old listener
            console.log("Registering ReceiveMetrics listener...");
            hubConnection.on("ReceiveMetrics",  async(cpuUsage, ramUsage, diskUsage) => {
                console.log("Metrics received:");
                if (isMounted) {
                    setMetrics({ cpuUsage, ramUsage, diskUsage });
                    if(cpuUsage>= 80 || ramUsage >= 80 || diskUsage >= 80) {
                       
                        
                        const res =  await axios.post(
                          "http://localhost:5256/api/user/usage",
                          {
                            CpuUsage: cpuUsage,
                            RamUsage: ramUsage,
                            DiskUsage: diskUsage,
                            Name: name,
                            Email: email
                          },
                          {
                            headers: {
                              'X-Auth-Token': localStorage.getItem('X-Auth-Token'),
                              'Content-Type': 'application/json'
                            }
                          }
                        );
                        
                          
                    }
                }
            });
        };
   
        if (hubConnection.state === "Connected") {
            console.log("SignalR already connected. Initializing listener...");
            initializeListener();
        } else {
            console.error("SignalR connection not established!");
        }
    
        const intervalId = setInterval(fetchMetrics, 2000);
    
        return () => {
            isMounted = false;
            clearInterval(intervalId);
            hubConnection.off("ReceiveMetrics"); // Remove listener on unmount
            console.log("Performance component unmounted and listener removed");
        };
    }, [hubConnection]);
    

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
