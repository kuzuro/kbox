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


        static Encoding UTF8 = Encoding.UTF8;
        static Encoding euckr = Encoding.GetEncoding(51949);



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

            ServerSocket.Dispose();

        }



        static private void Accept(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (ClientSocketList != null)
                {
                    connCount = ClientSocketList.Count;

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
                connCount = ClientSocketList.Count;

                buffer = e.Buffer;

                // 인코딩 추가
                string data = "";


                data = UTF8.GetString(buffer).Replace("\0", "").Trim();

                mw.appendLog(data);

                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = 0;
                }

                e.SetBuffer(buffer, 0, bufferSize);
                ClientSocket.ReceiveAsync(e);

                if(mw.autoSendFlag)
                {
                    Send();
                }

            } 
            else {

                ClientSocket.Disconnect(false);
                ClientSocketList.Remove(ClientSocket);
                mw.appendLog(string.Concat("[접속 해제 : ", ClientSocket.RemoteEndPoint.ToString(), "]"));
            }
        }


        private static void Send()
        {
            byte[] sendData;


            // 인코딩 분기

            sendData = UTF8.GetBytes(mw.autoSendContent);


            for (int i = 0; i < ClientSocketList.Count; i++)
            {
                ClientSocketList[i].Send(sendData, sendData.Length, SocketFlags.None);
            }



        }






    }
}
