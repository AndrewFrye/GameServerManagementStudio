import * as signalR from "@microsoft/signalr";
import config from "../config.json";

const URL = `http://${config.HOSTNAME}:${config.PORT}/iohub`;

class Connector {
    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(URL)
            .withAutomaticReconnect()
            .build();

        this.connection.on("Registered", async () => {
            await this.onRegistered();
        });

        this.connection.start()
            .then(() => console.log("Connected to SignalR hub"))
            .then(() => {this.connection.invoke("Register")})
            .catch(err => console.error("Connection error:", err));

        // Listen for the "ReceiveData" event
        this.connection.on("ReceiveData", async (message) => {
            await this.onReceiveData(message);
        });
    }

    async onReceiveData(message) {
        console.log("Data received:", message);
        // Add your custom logic here
    }

    async onRegistered() {
        console.log("Registered successfully");
        // Add your custom logic here
    }
}

// Export the singleton instance directly
const connectorInstance = new Connector();
export default connectorInstance;