using kbox.EnumClass;
using kbox.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

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
        static private string receiveMsg = "";

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

                serverState = true;
                mw.startFlag = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Stop();
            }
        }

        /// <summary>
        /// 서버 종료
        /// </summary>
        static public void Stop()
        {
            if(ClientSocketList != null)
            {
                for (int i = 0; i < ClientSocketList.Count; i++)
                {
                    if (ClientSocketList[i].Connected)
                    {
                        ClientSocketList[i].Disconnect(false);
                    }

                    ClientSocketList[i].Dispose();
                }
            }

            ClientSocketList = null;

            if(ServerSocket != null)
            {
                ServerSocket.Dispose();
            }

            serverState = false;
            mw.startFlag = false;
        }


        /// <summary>
        /// 서버 동작 상태
        /// </summary>
        /// <returns></returns>
        static public bool State()
        {
            return serverState;
        }


        static private void Accept(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (ClientSocketList != null)
                {
                    Socket clientSocket = e.AcceptSocket;
                    ClientSocketList.Add(clientSocket);
                    buffer = new byte[bufferSize];

                    // 서버 접속을 감지
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    args.SetBuffer(buffer, 0, bufferSize);
                    args.UserToken = ClientSocketList;
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(Receive);  // 감지되면 메서드 실행
                    clientSocket.ReceiveAsync(args);

                    e.AcceptSocket = null;
                    ServerSocket.AcceptAsync(e);

                    ServerInfo();
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
                Stop();
            }
        }


        /// <summary>
        /// 서버 접속이 감지되면 실행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static private void Receive(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                Socket ClientSocket = (Socket)sender;

                if (ClientSocket.Connected && (e.BytesTransferred > 0))
                {
                    buffer = e.Buffer;

                    receiveMsg = EncodingConverter.ConvertString(mw.mainEncodingSelect, buffer).Replace("\0", "");
                    mw.AddLog(receiveMsg);

                    e.SetBuffer(buffer, 0, bufferSize);
                    ClientSocket.ReceiveAsync(e);

                    // 자동 전송
                    if (mw.autoSendFlag)
                    {
                        AutoSend();
                    }

                    ServerInfo();
                    
                    // 사용한 버퍼를 0으로 초기화
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = 0;
                    }                
                }
                else
                {
                    if (ClientSocketList != null)
                    {
                        ClientSocketList.Remove(ClientSocket);
                    }

                    mw.AddLog(string.Concat("[접속 해제 : ", ClientSocket.RemoteEndPoint.ToString(), "]"));

                    ServerInfo();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());

                Stop();                
                ServerInfo();
            }
        }


        /// <summary>
        /// 자동 전송
        /// </summary> 
        static private void AutoSend()
        {

            if(mw.autoSendSelector == AutoSendSelector.Receive)
            {
                // 받은 내용을 그대로 클라이언트에게 전달
                // 채팅방처럼 동일한 내용을 공유해야할 경우 사용

                byte[] sendData = EncodingConverter.ConvertByte(mw.mainEncodingSelect, receiveMsg);

                for (int i = 0; i < ClientSocketList.Count; i++)
                {
                    ClientSocketList[i].Send(sendData, sendData.Length, SocketFlags.None);
                }
            }
            else
            {
                // 서버가 입력한 값 전달
                // 해당 기능을 이용하여 연산 한 값을 리턴하거나 DB 조건조회하여 데이터를 건낼 수 있음

                byte[] sendData = EncodingConverter.ConvertByte(mw.sendEncodingSelect, mw.autoSendMsgContent);

                for (int i = 0; i < ClientSocketList.Count; i++)
                {
                    ClientSocketList[i].Send(sendData, sendData.Length, SocketFlags.None);
                }
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
                ClientSocketList[i].Send(sendData, 0, sendData.Length, SocketFlags.None);
            }
        }


        /// <summary>
        /// 접속자가 발생하거나 메시지가 오면 동작
        /// </summary>
        static private void ServerInfo()
        {
            // 접속자 갱신
            if (ClientSocketList != null)
            {
                connCount = ClientSocketList.Count;
            }
            else
            {
                connCount = 0;
            }
        }


    }
}
