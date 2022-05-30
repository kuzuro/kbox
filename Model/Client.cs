using kbox.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
                connState = "접속 실패";
                mw.appendLog(connState);
                Stop();
            }
        }


        static public void Stop()
        {
            if(serverSocket != null)
            {
                serverSocket.Disconnect(false);
            }
            
            clientSocket.Disconnect(false);

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
                clientSocket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallBack), serverSocket);

            }
            catch (Exception ex)
            {
                connState = string.Concat("실패 : ", ex.Message);
                mw.appendLog(connState);
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
                    string data = EncodingConverter.ConvertString(mw.mainEncodingSelect, buffer).Replace("\0", ""); ;
                    mw.appendLog(data);
                }


            }
            catch(Exception ex)
            {
                connState = string.Concat("실패 : ", ex.Message);
                mw.appendLog(connState);
                Stop();
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
                    //serverSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, new AsyncCallback(SendCallBack), clientSocket);

                }
                else
                {
                    mw.appendLog("접속이 끊어졌습니다.");
                }

            }
            catch (Exception ex)
            {

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




        static private void SendCallBack(IAsyncResult IAR)
        {

        }





    }
}
