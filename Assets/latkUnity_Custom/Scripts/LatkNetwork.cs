using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using BestHTTP.SocketIO;

public class LatkNetwork : MonoBehaviour {

    public LightningArtist latk;
    public string serverAddress = "vr.fox-gieg.com";
    public int serverPort = 8080;
    public bool doDebug = true;
    public float scaler = 1f;

    private SocketManager socketManager;
    private string socketAddress;
    private bool connected = false;

    public bool getConnectionStatus() {
        return connected;
    }

	private void Start() {
        socketAddress = "http://" + serverAddress + ":" + serverPort + "/socket.io/:8443";
        initSocketManager(socketAddress);
    }

    private void initSocketManager(string uri) {
        socketManager = new SocketManager(new Uri(uri));
        socketManager.Socket.AutoDecodePayload = false;
        socketManager.Socket.On("error", socketError);
        socketManager.Socket.On("connect", socketConnected);
        socketManager.Socket.On("reconnect", socketConnected);

        socketManager.Socket.On("newFrameFromServer", receivedLocalSocketMessage);
    }

    void socketConnected(Socket socket, Packet packet, params object[] args) {
        connected = true;

        if (doDebug) {
            Debug.Log(DateTime.Now + " Connected to server.");
        }
    }

    void socketError(Socket socket, Packet packet, params object[] args) {
        connected = false;
        if (doDebug) {
            Debug.LogError(DateTime.Now + " Failed to connect to server.");

            if (args.Length > 0) {
                Error error = args[0] as Error;
                if (error != null) {
                    switch (error.Code) {
                        case SocketIOErrors.User:
                            Debug.LogError("Socket Error Type: Exception in an event handler.");
                            break;
                        case SocketIOErrors.Internal:
                            Debug.LogError("Socket Error Type: Internal error.");
                            break;
                        default:
                            Debug.LogError("Socket Error Type: Server error.");
                            break;
                    }
                    Debug.LogError(error.ToString());
                    return;
                }
            }
            Debug.LogError("Could not parse error.");
        }
    }

    void receivedLocalSocketMessage(Socket socket, Packet packet, params object[] args) {
        string eventName = "data";
        string jsonString;
        if (packet.SocketIOEvent == SocketIOEventTypes.Event) {
            eventName = packet.DecodeEventName();
            jsonString = packet.RemoveEventName(true);
        } else if (packet.SocketIOEvent == SocketIOEventTypes.Ack) {
            jsonString = packet.ToString();
            jsonString = jsonString.Substring(1, jsonString.Length-2);
        } else {
            jsonString = packet.ToString();
        }

        if (doDebug) {
            Debug.Log(DateTime.Now + " - " + "Local Socket Event Name: " + eventName + " - Message: " + jsonString);
        }

        switch (eventName) {
            case "newFrameFromServer":
                // ~ ~ ~ ~ ~ ~ ~ ~ ~ 
                JSONNode data = JSON.Parse(jsonString);
                if (doDebug) Debug.Log("Receiving new frame " + data[0]["index"] + " with " + data.Count + " strokes.");

                for (var i = 0; i < data.Count; i++) {
                    List<Vector3> points = getPointsFromJson(data[i]["points"], scaler);
                    latk.inputInstantiateStroke(latk.mainColor, points);
                }

                int index = data[0]["index"].AsInt;
                //int last = layers.Count - 1;
                //if (newStrokes.Count > 0 && layers.Count > 0 && layers[last].frames) layers[last].frames[index] = newStrokes;
                // ~ ~ ~ ~ ~ ~ ~ ~ ~
                break;
        }
    }

    public void sendStrokeData(string data) {
        socketManager.Socket.Emit("clientStrokeToServer", data);
    }

    private void OnApplicationQuit() {
        socketManager.Close();
        if (doDebug) {
            Debug.Log("Closed connection");
        }
    }

    public List<Vector3> getPointsFromJson(JSONNode ptJson, float scaler) {
        List<Vector3> returns = new List<Vector3>();
        for (var j = 0; j < ptJson.Count; j++) {
            var co = ptJson[j]["co"];

            //if (j === 0 || !useMinDistance || (useMinDistance && origVerts[j].distanceTo(origVerts[j-1]) > minDistance)) {
            returns.Add(new Vector3(co[0].AsFloat, co[1].AsFloat, co[2].AsFloat) * scaler);
            //}
        }
        return returns;
    }

}
