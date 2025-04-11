import React, {useEffect, useState} from 'react';
import './App.css';
import Connector from './signalr-connection';
import '@mantine/core/styles.css';
import {Button, MantineProvider, Select} from '@mantine/core';


function App() {
    const [serverInfos, setServerInfos] = useState([]);

    useEffect(() => {
        const connection = Connector;

        connection.connection.on("ReceiveData", (receivedPacket) => {
            onDataRecieved(receivedPacket);
        });

        connection.connection.on("Registered", () => {
            fetchServerInfos();
        });

        return () => {
            connection.connection.off("ReceiveData");
            connection.connection.off("Registered");
        };
    }, []);

    useEffect(() => {
        console.log("serverInfos updated:", serverInfos);
    }, [serverInfos]);

    const onDataRecieved = (packetStr) => {
        console.log("Received packet string:", packetStr);
        try {
            const packet = JSON.parse(packetStr);
            console.log("Parsed packet:", packet);

            if (packet.Command === "ReturnServerInfos") {
                const serverInfoStrs = JSON.parse(packet.Message);
                console.log("Parsed server info strings:", serverInfoStrs);

                const updatedServerInfos = serverInfoStrs.map(serverInfoStr => {
                    const serverInfo = JSON.parse(serverInfoStr);
                    console.log("Parsed server info:", serverInfo);
                    return {
                        label: serverInfo.GameInfo.InstanceId,
                        value: serverInfo.GameInfo.InstanceId
                    };
                });

                console.log("Updated server infos:", updatedServerInfos);
                setServerInfos(updatedServerInfos);
            }
        } catch (error) {
            console.error("Error processing received packet:", error);
        }
    };

    const sendMessage = (packet) => {
        Connector.connection.invoke("SendToService", JSON.stringify(packet))
            .catch(err => console.error("Error sending message:", err));
    };

    const fetchServerInfos = () => {
        const packet = {
            "Source": "Web Client",
            "Command": "FetchServerInfos",
        };
        sendMessage(packet);
    };

    return (

        <MantineProvider defaultColorScheme="auto">
            <div className="ServerSelectionDiv">
                <Select placeholder="Select A Server" nothingFoundMessage="Nothing found..."
                        data={serverInfos} searchable/>
                <Button >
                    Start
                </Button>
            </div>
        </MantineProvider>
    );
}

export default App;