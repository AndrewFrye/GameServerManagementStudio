import React, { useEffect, useState } from 'react';
import './App.css';
import Connector from './signalr-connection';

function App() {
    const [message, setMessage] = useState("initial value");

    useEffect(() => {
        // Listen for the "ReceiveData" event
        const connection = Connector;
        connection.connection.on("ReceiveData", (receivedMessage) => {
            setMessage(receivedMessage);
        });

        // Cleanup on unmount
        return () => {
            connection.connection.off("ReceiveData");
        };
    }, []);

    const sendMessage = () => {
        // Example of sending a message (if implemented in the backend)
        Connector.connection.invoke("SendMessage", (new Date()).toISOString())
            .catch(err => console.error("Error sending message:", err));
    };

    return (
        <div className="App">
            <span>Message from SignalR: <span style={{ color: "green" }}>{message}</span></span>
            <br />
            <button onClick={sendMessage}>Send Date</button>
        </div>
    );
}

export default App;