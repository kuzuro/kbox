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


        static private Socket clientSocket;
        static private Socket ServerSocket;

        static private byte[] buffer;
        static private int bufferSize = 1024;

        static MainWindow mw = (MainWindow)System.Windows.Application.Current.MainWindow;

        static Encoding UTF8 = Encoding.UTF8;
        static Encoding UNICODE = Encoding.Unicode;
        static Encoding EUCKR = Encoding.GetEncoding(51949);



        static public void Start(string _IP, int _PORT)
        {
            IP = _IP;
            PORT = _PORT;

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

        }



        /// <summary>
        /// 접속 시작
        /// </summary>
        static private void Connect()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.BeginConnect(IP, PORT, new AsyncCallback(CallBack), clientSocket);
        }



        static private void CallBack(IAsyncResult IAR)
        {
            try
            {
                Socket socket = (Socket)IAR.AsyncState;
                IPEndPoint ipep = (IPEndPoint)socket.RemoteEndPoint;

                connState = "접속중";

                socket.EndAccept(IAR);
                ServerSocket = socket;
                ServerSocket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallBack), ServerSocket);

            }
            catch(Exception ex)
            {
                mw.appendLog("접속 실패");
                Connect();
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
                    string data = EncodingConverter.ConvertString(mw.mainEncodingSelect, buffer);
                    mw.appendLog(data);
                }


            }
            catch(Exception ex)
            {
                Connect();
            }
        }




        static public void Send(string sendMsg)
        {
            try
            {
                if(clientSocket.Connected)
                {

                    byte[] sendData = EncodingConverter.ConvertByte(mw.sendEncodingSelect, sendMsg);

                    clientSocket.Send(sendData, sendData.Length, SocketFlags.None);
                    //clientSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, new AsyncCallback(SendCallBack), clientSocket);

                }

            }
            catch (Exception ex)
            {

            }
        }


        static private void SendCallBack(IAsyncResult IAR)
        {

        }





    }
}
