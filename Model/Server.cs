using kbox.EnumClass;
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

        static public List<String> userList = new List<String>();
        static public int connCount = 0;

        static public Socket ServerSocket;
        static private List<Socket> ClientSocketList;
        static private byte[] buffer;
        static private int bufferSize = 1024;

        static MainWindow mw = (MainWindow)System.Windows.Application.Current.MainWindow;


        static Encoding UTF8 = Encoding.UTF8;
        static Encoding UNICODE = Encoding.Unicode;
        static Encoding EUCKR = Encoding.GetEncoding(51949);



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


                    userList.Clear();

                    ServerInfo();

                    /*
                    // 접속자 목록화
                    for (int i = 0; i < ClientSocketList.Count; i++)
                    {
                        userList.Add(ClientSocket.RemoteEndPoint.ToString());
                    }

                    // 접속자 갱신
                    connCount = ClientSocketList.Count;
                    */
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

                string data = "";
                if (mw.mainEncodingSelect == EncodingSelector.UTF8)
                {
                    data = UTF8.GetString(buffer).Replace("\0", "").Trim();
                }
                else if (mw.sendEncodingSelect == EncodingSelector.UNICODE)
                {
                    data = UNICODE.GetString(buffer).Replace("\0", "").Trim();
                }
                else
                {
                    data = EUCKR.GetString(buffer).Replace("\0", "").Trim();
                }

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

                userList.Clear();

                ServerInfo();
                /*
                // 접속자 목록화
                for (int i = 0; i < ClientSocketList.Count; i++)
                {
                    userList.Add(ClientSocket.RemoteEndPoint.ToString());
                }

                // 접속자 갱신
                connCount = ClientSocketList.Count;
                */

            }
            else {

                ClientSocket.Disconnect(false);
                ClientSocketList.Remove(ClientSocket);
                mw.appendLog(string.Concat("[접속 해제 : ", ClientSocket.RemoteEndPoint.ToString(), "]"));

                userList.Clear();

                ServerInfo();

                /*
                // 접속자 목록화
                for (int i = 0; i < ClientSocketList.Count; i++)
                {
                    userList.Add(ClientSocketList[i].RemoteEndPoint.ToString());
                }

                // 접속자 갱신
                connCount = ClientSocketList.Count;
                */
            }
        }


        /// <summary>
        /// 자동 전송
        /// </summary>
        private static void AutoSend()
        {
            byte[] sendData = Encoder(mw.sendEncodingSelect, mw.autoSendContent);

            for (int i = 0; i < ClientSocketList.Count; i++)
            {
                ClientSocketList[i].Send(sendData, sendData.Length, SocketFlags.None);
            }
        }


        /// <summary>
        /// 전송
        /// </summary>
        /// <param name="sendMsg"></param>
        public static void Send(string sendMsg)
        {
            byte[] sendData = Encoder(mw.sendEncodingSelect, sendMsg);

            for (int i = 0; i < ClientSocketList.Count; i++)
            {
                ClientSocketList[i].Send(sendData, sendData.Length, SocketFlags.None);
            }
        }



        /// <summary>
        /// 인코딩 변경
        /// </summary>
        /// <param name="endcode"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] Encoder(EncodingSelector endcode, string data)
        {
            byte[] result;

            if (endcode == EncodingSelector.UTF8)
            {
                result = UTF8.GetBytes(data);
            }
            else if (endcode == EncodingSelector.UNICODE)
            {
                result = UNICODE.GetBytes(data);
            }
            else
            {
                result = EUCKR.GetBytes(data);
            }

            return result;
        }



        /// <summary>
        /// 접속자가 발생하거나 메시지가 오면 동작
        /// </summary>
        private static void ServerInfo()
        {


            // 접속자 갱신
            connCount = ClientSocketList.Count;
        }

    }
}
