import * as signalR from "@microsoft/signalr";

const HOSTNAME = process.env.REACT_APP_HOSTNAME || "localhost";
const PORT = process.env.REACT_APP_PORT || "5001";
const URL = `http://${HOSTNAME}:${PORT}/iohub`;

class Connector {
    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(URL)
            .withAutomaticReconnect()
            .build();

        this.connection.start()
            .then(() => console.log("Connected to SignalR hub"))
            .then(() => {this.connection.invoke("Register")})
            .catch(err => console.error("Connection error:", err));

        // Listen for the "ReceiveData" event
        this.connection.on("ReceiveData", async (message) => {
            await this.onReceiveData(message);
        });
    }

    // Method to handle received data
    async onReceiveData(message) {
        console.log("Data received:", message);
        // Add your custom logic here
    }
}

// Export the singleton instance directly
const connectorInstance = new Connector();
export default connectorInstance;