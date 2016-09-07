/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System;

public class DataStreamListener : MonoBehaviour {
   
    private static string recievedData;
    private static int messageLen = 2048;

    public static float currentRPM;
    public static float currentSpeed;
    public static float lastTimeStamp = 0f;

    private bool isConnected = false;
    private bool isWaiting = false;
    private byte[] buffer;
    private string secondaryBuffer = "";
    private static Socket tcpServer;
    private static Socket connectedClient;

    public void Awake()
    {
        tcpServer = new Socket(AddressFamily.InterNetwork,
        SocketType.Stream, ProtocolType.Tcp);
        tcpServer.Bind(new IPEndPoint(IPAddress.Any, DataStreamServer.PORT));
        isConnected = false;
        tcpServer.Listen(100);
        buffer = new byte[messageLen];
        tcpServer.BeginAccept(new AsyncCallback(AcceptCallback), tcpServer);
    }

    void AcceptCallback(System.IAsyncResult ar)
    {
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        connectedClient = handler;
        isConnected = true;
    }

    private void Update()
    {
        if (isConnected && !isWaiting)
        {
            connectedClient.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), connectedClient);
            isWaiting = true;
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;

            int bytesRead = client.EndReceive(ar);
           
            if (bytesRead > 0)
            {
                string read = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                secondaryBuffer += read;
                int endLineIndex = secondaryBuffer.IndexOf("\n");
                while( endLineIndex >= 0)
                {
                    string[] data = secondaryBuffer.Substring(0, endLineIndex).Split(',');
                    secondaryBuffer = secondaryBuffer.Substring(endLineIndex + 1);
                    if(data[0].Trim() == "EngineSpeed")
                    {
                        currentRPM = float.Parse(data[1].Trim());
                    } else if(data[0].Trim() == "VehicleSpeed")
                    {
                        currentSpeed = float.Parse(data[1].Trim());
                    }
                    endLineIndex = secondaryBuffer.IndexOf("\n");
                }



                
            }
        }
        catch { }
        finally
        {
            isWaiting = false;
        }
        
    }

    public void OnDestroy()
    {
        tcpServer.Shutdown(SocketShutdown.Both);
        tcpServer.Close();
        connectedClient.Shutdown(SocketShutdown.Both);
        connectedClient.Close();
    }

    

}
