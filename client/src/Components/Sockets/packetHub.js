import * as signalR from '@microsoft/signalr';
const packetHubConnection = new signalR.HubConnectionBuilder()
.withUrl("http://localhost:5256/packetHub").withAutomaticReconnect().build(); 
export default packetHubConnection