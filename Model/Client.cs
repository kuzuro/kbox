using socket_box.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace socket_box.Model
{
    static public class Client
    {

        private static string IP;
        private static int PORT;

        public static string connStateMsg = "접속 대기";

        private static bool clientState = false;

        private static Socket clientSocket;
        private static Socket serverSocket;

        private static byte[] buffer;
        private static int bufferSize = 1024;

        static MainWindow mw = (MainWindow)System.Windows.Application.Current.MainWindow;


        public static void Start(string _IP, int _PORT)
        {
            IP = _IP;
            PORT = _PORT;

            buffer = new byte[bufferSize];

            try
            {

                mw.startFlag = true;
                clientState = true;

                Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                connStateMsg = "접속 실패";
                mw.AddLog(connStateMsg);
                Stop();
            }
        }


        public static void Stop()
        {
            try
            {
                if (serverSocket != null)
                {
                    serverSocket.Disconnect(false);
                }

                if (clientSocket != null)
                {
                    clientSocket.Disconnect(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            mw.startFlag = false;
            clientState = false;
        }


        /// <summary>
        /// 접속 시작
        /// </summary>
        private static void Connect()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.BeginConnect(IP, PORT, new AsyncCallback(CallBack), serverSocket);
        }


        private static void CallBack(IAsyncResult IAR)
        {
            try
            {
                Socket socket = (Socket)IAR.AsyncState;
                IPEndPoint ipep = (IPEndPoint)socket.RemoteEndPoint;

                connStateMsg = "접속중";

                socket.EndConnect(IAR);

                clientSocket = socket;
                clientSocket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallBack), clientSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                Stop();
            }
        }


        private static void ReceiveCallBack(IAsyncResult IAR)
        {
            try
            {
                Socket socket = (Socket)IAR.AsyncState;
                int readSize = socket.EndReceive(IAR);

                if (readSize > 0)
                {
                    string data = EncodingConverter.ConvertString(mw.mainEncodingSelect, buffer).Replace("\0", "");
                    mw.AddLog(data);
                }

                Receive(readSize);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Stop();
            }
        }


        private static void Receive(int readSize)
        {
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), clientSocket);

            // 받았던 데이터의 크기만큼 반복해서 0으로 초기화
            for (int i = 0; i < readSize; i++)
            {
                buffer[i] = 0;
            }
        }


        /// <summary>
        /// 메시지 전송
        /// </summary>
        /// <param name="sendMsg"></param>
        public static void Send(string sendMsg)
        {
            try
            {
                if (clientSocket.Connected)
                {
                    byte[] sendData = EncodingConverter.ConvertByte(mw.sendEncodingSelect, sendMsg);
                    serverSocket.Send(sendData, sendData.Length, SocketFlags.None);
                }
                else
                {
                    mw.AddLog("접속이 끊어졌습니다.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        /// <summary>
        /// 클라이언트 동작 상태
        /// </summary>
        /// <returns></returns>
        public static bool State()
        {
            return clientState;
        }


    }
}
