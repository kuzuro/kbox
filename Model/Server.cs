﻿using kbox.EnumClass;
using kbox.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace kbox.Model
{
    static public class Server
    {
        static private bool serverState = false;
        static public int connCount = 0;
        static public Socket ServerSocket;
        static private List<Socket> ClientSocketList;

        static private byte[] buffer;
        static private int bufferSize = 1024;
        static MainWindow mw = (MainWindow)System.Windows.Application.Current.MainWindow;



        /// <summary>
        /// 서버 시작
        /// </summary>
        static public void Start(string IP, int PORT)
        {

            try
            {
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ClientSocketList = new List<Socket>();

                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(IP), PORT);

                ServerSocket.Bind(ipep);
                ServerSocket.Listen(20);

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                args.Completed += new EventHandler<SocketAsyncEventArgs>(Accept);
                ServerSocket.AcceptAsync(args);

            }
            catch (Exception ex)
            {
                mw.appendLog("서버 시작 실패");
                Stop();
            }


        }

        static public bool State()
        {
            return serverState;
        }


        /// <summary>
        /// 서버 종료
        /// </summary>
        static public void Stop()
        {
            for (int i = 0; i < ClientSocketList.Count; i++)
            {

                if (ClientSocketList[i].Connected)
                {
                    ClientSocketList[i].Disconnect(false);
                }

                ClientSocketList[i].Dispose();
            }

            ClientSocketList = null;

            if(ServerSocket != null)
            {
                ServerSocket.Dispose();
            }
        }



        static private void Accept(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (ClientSocketList != null)
                {


                    Socket ClientSocket = e.AcceptSocket;
                    ClientSocketList.Add(ClientSocket);
                    buffer = new byte[bufferSize];

                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    args.SetBuffer(buffer, 0, bufferSize);
                    args.UserToken = ClientSocketList;
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(Receive);
                    ClientSocket.ReceiveAsync(args);

                    e.AcceptSocket = null;
                    ServerSocket.AcceptAsync(e);

                    ServerInfo();
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        static private void Receive(object sender, SocketAsyncEventArgs e)
        {
            Socket ClientSocket = (Socket)sender; 

            if(ClientSocket.Connected && (e.BytesTransferred > 0))
            {

                buffer = e.Buffer;

                string data = EncodingConverter.ConvertString(mw.mainEncodingSelect, buffer);
                
                mw.appendLog(data);

                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = 0;
                }

                e.SetBuffer(buffer, 0, bufferSize);
                ClientSocket.ReceiveAsync(e);


                // 자동 전송
                if(mw.autoSendFlag)
                {
                    AutoSend();
                }

                ServerInfo();

            }
            else {

                ClientSocket.Disconnect(false);
                ClientSocketList.Remove(ClientSocket);
                mw.appendLog(string.Concat("[접속 해제 : ", ClientSocket.RemoteEndPoint.ToString(), "]"));

                ServerInfo();
            }
        }


        /// <summary>
        /// 자동 전송
        /// </summary>
        static private void AutoSend()
        {
            byte[] sendData = EncodingConverter.ConvertByte(mw.sendEncodingSelect, mw.autoSendContent);

            for (int i = 0; i < ClientSocketList.Count; i++)
            {
                ClientSocketList[i].Send(sendData, sendData.Length, SocketFlags.None);
            }
        }


        /// <summary>
        /// 전송
        /// </summary>
        /// <param name="sendMsg"></param>
        static public void Send(string sendMsg)
        {
            byte[] sendData = EncodingConverter.ConvertByte(mw.sendEncodingSelect, sendMsg);

            for (int i = 0; i < ClientSocketList.Count; i++)
            {
                ClientSocketList[i].Send(sendData, sendData.Length, SocketFlags.None);
            }
        }


        /// <summary>
        /// 접속자가 발생하거나 메시지가 오면 동작
        /// </summary>
        static private void ServerInfo()
        {
            // 접속자 갱신
            connCount = ClientSocketList.Count;
        }

    }
}
