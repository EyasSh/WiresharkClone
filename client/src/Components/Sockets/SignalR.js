import * as signalR from '@microsoft/signalr';

const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5256/hub") // Replace with your server's SignalR Hub URL
    .withAutomaticReconnect()
    .build();


export default hubConnection