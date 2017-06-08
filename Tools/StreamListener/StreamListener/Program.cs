using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System;


namespace StreamListener
{
    class Program
    {
    
        private static string recievedData;
        private static int messageLen = 1024;

        public static float currentRPM;
        public static float currentSpeed;
        public static float lastTimeStamp = 0f;

        private static Socket tcpServer;
        private static Socket connectedClient;

        private static bool isConnected = false;
        private static bool isWaiting = false;
        private static byte[] buffer;

        public static void Awake(int port)
        {
            tcpServer = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
            
            tcpServer.Bind(new IPEndPoint(IPAddress.Any, port));
            isConnected = false;
            tcpServer.Listen(100);
            buffer = new byte[messageLen];
            tcpServer.BeginAccept(new AsyncCallback(AcceptCallback), tcpServer);
            Console.WriteLine("listening on port " + port);
        }

        static void AcceptCallback(System.IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            
            Socket handler = listener.EndAccept(ar);
            connectedClient = handler;
            isConnected = true;
        }

        private static void Update()
        {
            if (isConnected && !isWaiting)
            {
                connectedClient.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), connectedClient);
                isWaiting = true;
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                int bytesRead = client.EndReceive(ar);
                isWaiting = false;

                if (bytesRead > 0)
                {
                    Console.Write(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                    /*float[] data = new float[16];
                    System.Buffer.BlockCopy(buffer, 0, data, 0, messageLen);

                    Console.WriteLine("EMSSetSpeed, " + data[1] + ", " + data[0]);
                    Console.WriteLine("EngineSpeed, " + data[2] + ", " + data[0]);
                    Console.WriteLine("GearPosActual, " + data[3] + ", " + data[0]);
                    Console.WriteLine("GearPosTarget, " + data[4] + ", " + data[0]);
                    Console.WriteLine("AcceleratorPedalPos, " + data[5] + ", " + data[0]);
                    Console.WriteLine("DeceleratorPedalPos, " + data[6] + ", " + data[0]);
                    Console.WriteLine("RollRate, " + data[7] + ", " + data[0]);
                    Console.WriteLine("SteeringWheelAngle, " + data[8] + ", " + data[0]);
                    Console.WriteLine("VehicleSpeed, " + data[9] + ", " + data[0]);
                    Console.WriteLine("VehicleSpeedOverGnd, " + data[10] + ", " + data[0]);
                    Console.WriteLine("WheelSpeedFrL, " + data[11] + ", " + data[0]);
                    Console.WriteLine("WheelSpeedFrR, " + data[12] + ", " + data[0]);
                    Console.WriteLine("WheelSpeedReL, " + data[13] + ", " + data[0]);
                    Console.WriteLine("WheelSpeedReR, " + data[14] + ", " + data[0]);
                    Console.WriteLine("YawRate, " + data[15] + ", " + data[0]);
                    */
                }
            }
            catch(Exception e) {
                Console.Error.WriteLine("ERR" );
            }
            finally
            {
                isWaiting = false;
            }

        }

        public static void OnDestroy()
        {
            tcpServer.Shutdown(SocketShutdown.Both);
            tcpServer.Close();
        }

    static void Main(string[] args)
        {
            int port = 9000;
            if(args.Length > 0)
            {
                port = int.Parse(args[0]);
            }
            Awake(port);
            while(true)
                {
                    Update();
                }
                OnDestroy();
            }
    }
}
