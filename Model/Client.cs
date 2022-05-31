using kbox.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace kbox.Model
{
    static public class Client
    {

        static private string IP;
        static private int PORT;

        static public string connState = "접속 대기";

        static private bool clientState = false;

        static private Socket clientSocket;
        static private Socket serverSocket;

        static private byte[] buffer;
        static private int bufferSize = 1024;

        static MainWindow mw = (MainWindow)System.Windows.Application.Current.MainWindow;


        static public void Start(string _IP, int _PORT)
        {
            IP = _IP;
            PORT = _PORT;

            buffer = new byte[bufferSize];

            try
            {
                Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                connState = "접속 실패";
                mw.AddLog(connState);
                Stop();
            }
        }


        static public void Stop()
        {
            try
            {
                if (serverSocket != null)
                {
                    serverSocket.Disconnect(false);
                }

                if(clientSocket != null)
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
        static private void Connect()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.BeginConnect(IP, PORT, new AsyncCallback(CallBack), serverSocket);

            mw.startFlag = true;
            clientState = true;
        }


        static private void CallBack(IAsyncResult IAR)
        {
            try
            {
                Socket socket = (Socket)IAR.AsyncState;
                IPEndPoint ipep = (IPEndPoint)socket.RemoteEndPoint;

                connState = "접속중";

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


        static private void ReceiveCallBack(IAsyncResult IAR)
        {
            try
            {
                Socket socket = (Socket)IAR.AsyncState;
                int readSize = socket.EndReceive(IAR);

                if(readSize > 0)
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


        static private void Receive(int readSize)
        {
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), clientSocket);

            // 받았던 데이터의 크기만큼 반복해서 0으로 초기화
            for (int i = 0; i < readSize; i++)
            {
                buffer[i] = (byte)0;
            }
        }


        /// <summary>
        /// 메시지 전송
        /// </summary>
        /// <param name="sendMsg"></param>
        static public void Send(string sendMsg)
        {
            try
            {
                if(clientSocket.Connected)
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
        static public bool State()
        {
            return clientState;
        }


    }
}
