/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;

public class ConnectionData
{
    public Socket client;
    public bool IC = false;
    public bool events = false;
}

public class DataStreamServer : PersistentUnitySingleton<DataStreamServer> {

    public const int PORT = 9001;
    public const int ALTPORT = 9000;
    public const int EVENTPORT = 9002;
    private static string recievedData;
    private static int messageLen = 4 + (4 * 15);

    private List<ConnectionData> connections;


    protected override void Awake()
    {
        base.Awake();
        connections = new List<ConnectionData>();
        var dbEndpoint = new IPEndPoint(IPAddress.Parse(NetworkController.settings.clientIp), PORT);
        var altEndpoint = new IPEndPoint(IPAddress.Parse(NetworkController.settings.altClientIP), ALTPORT);
        var eventEndpoint = new IPEndPoint(IPAddress.Parse(NetworkController.settings.eventClientIP), EVENTPORT);

        var dbClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var altClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var eventClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        dbClient.BeginConnect(dbEndpoint, new AsyncCallback(ConnectCallback), new ConnectionData() { client = dbClient, IC = true, events = false });
        altClient.BeginConnect(altEndpoint, new AsyncCallback(ConnectCallback), new ConnectionData() { client = altClient, IC = false, events = false });
        eventClient.BeginConnect(eventEndpoint, new AsyncCallback(ConnectCallback), new ConnectionData() { client = eventClient, IC = false, events = true });


    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            ConnectionData data = (ConnectionData)ar.AsyncState;
            data.client.EndConnect(ar);
            connections.Add(data);
        } catch (Exception e)
        {
            Debug.Log("DataStreamServer: " + e.Message);
        }
    }

    private void SendTCP(byte[] data, Socket client)
    {
        try
        {
            if (client.Connected)
                client.BeginSend(data, 0, data.Length, 0,
            new AsyncCallback(SendCallback), client);
        } catch (Exception e)
        {
            Debug.Log("DataStreamServer: " + e.Message);
        }
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {         
            Socket handler = (Socket)ar.AsyncState;
            handler.EndSend(ar);
        }
        catch (Exception e)
        {
            Debug.Log("DataStramServer: " + e.ToString());
        }
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();

        foreach (var client in connections)
        {
            client.client.Shutdown(SocketShutdown.Both);
            client.client.Close();
        }
    }

    public void Send(byte[] data)
    {
        foreach(var client in connections)
        {
           SendTCP(data, client.client);
        }

    }

    public void Send(float[] data)
    {
        byte[] senddata = new byte[messageLen];
        System.Buffer.BlockCopy(data, 0, senddata, 0, messageLen);
        Send(senddata);
    }

    //send as packed struct
    public void Send(FullDataFrame data)
    {
        byte[] dataBytes = new byte[messageLen];//declare byte array and initialize its size
        System.IntPtr ptr = Marshal.AllocHGlobal(messageLen);//pointer to byte array

        Marshal.StructureToPtr(data, ptr, true);
        Marshal.Copy(ptr, dataBytes, 0, messageLen);
        Marshal.FreeHGlobal(ptr);

        Send(dataBytes);
    }

    public void Send(string csvData)
    {
        byte[] dataBytes = Encoding.ASCII.GetBytes(csvData);
        Send(dataBytes);
    }

    public void SendAsText(FullDataFrame frame)
    {
        foreach (var connection in connections)
        {
            if (connection != null)
            {
                byte[] data;
                if (connection.IC)
                {
                    data = Encoding.ASCII.GetBytes(frame.ToICCSV());
                    SendTCP(data, connection.client);
                }
                else if (connection.events)
                {
                    //do nothing here, event frames sent on their own   
                }
                else
                {
                    data = Encoding.ASCII.GetBytes(frame.ToCSV());
                    SendTCP(data, connection.client);
                }
            }
            
        }
    }

    public void SendAsText(TriggeredEventFrame frame)
    {
        foreach (var connection in connections)
        {
            if (connection != null && connection.events)
            {
                byte[] data;
                data = Encoding.ASCII.GetBytes(frame.ToCSV());
                SendTCP(data, connection.client);
            }
        }
    }
    

}
