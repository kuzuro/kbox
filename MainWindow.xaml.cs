﻿using kbox.EnumClass;
using kbox.Model;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace kbox
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {        

        // 프로그램의 동작 여부. Server와 Client에서 제어
        public bool startFlag = false;

        public bool autoSendFlag = false;
        public string autoSendMsgContent = "";

        private Thread connCountThread;
        private Thread connStateThread;

        public RunTypeSelector runTypeSelector;
        public EncodingSelector mainEncodingSelect;
        public EncodingSelector sendEncodingSelect;
        public AutoSendSelector autoSendSelector;
        

        public MainWindow()
        {
            InitializeComponent();

            Init();
        }


        #region 윈도우 기본


        /// <summary>
        /// 타이틀바 드래그
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }

        }


        /// <summary>
        /// 프로그램 최소화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_minimized_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        /// <summary>
        /// 프로그램 종료
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_close_Click(object sender, RoutedEventArgs e)
        {
            ProgramExit();
        }


        /// <summary>
        /// 서버 및 클라이언트 종료 후 프로그램 종료
        /// </summary>
        public void ProgramExit()
        {

            // 서버가 실행중이면 종료
            if (startFlag)
            {
                Server.Stop();
            }


            // 클라이언트가 실행중이면 종료
            // code



            Environment.Exit(0);
        }

        #endregion


        #region 컨트롤 제어


        /// <summary>
        /// 시작/종료
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!startFlag)
                {
                    Start();
                }
                else
                {
                    Stop();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        /// <summary>
        /// 실행 타입 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (runType.SelectedIndex)
            {
                case 0:
                    runTypeSelector = RunTypeSelector.Server;
                    break;

                case 1:
                    runTypeSelector = RunTypeSelector.Client;
                    break;
            }
        }


        /// <summary>
        /// 메인 인코딩 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Encoding_Selector((sender as ComboBox));
        }


        /// <summary>
        /// 전송 인코딩 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Encoding_Selector((sender as ComboBox));
        }


        /// <summary>
        /// 전송
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (runTypeSelector == RunTypeSelector.Server)
            {
                Server.Send(sendMsg.Text.Trim());
            }
            else
            {
                Client.Send(sendMsg.Text.Trim());
            }
        }


        /// <summary>
        /// 자동 전송 토글
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoSend_Click(object sender, RoutedEventArgs e)
        {
            AutoSendSetting();
        }


        /// <summary>
        /// 로그 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CleanButton_click(object sender, RoutedEventArgs e)
        {
            log.Document.Blocks.Clear();
            receiveData.Text = "데이터 없음";
        }


        /// <summary>
        /// 포트번호 입력 마스크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PortNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        /// <summary>
        /// 자동 전송 타입
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoSendType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(autoSendMsg == null)
            {
                return;
            }

            autoSendSelector = (AutoSendSelector)autoSendType.SelectedIndex;

            if(autoSendSelector == AutoSendSelector.Receive)
            {
                autoSendMsg.IsEnabled = false;
            } 
            else
            {
                autoSendMsg.IsEnabled = true;
            }

        }


        #endregion


        /// <summary>
        /// 초기화
        /// </summary>
        private void Init()
        {
            runType.SelectedIndex = Properties.Settings.Default.RUN_TYPE;
            runTypeSelector = (RunTypeSelector)Properties.Settings.Default.RUN_TYPE;

            // 프로퍼티에서 불러오도록 수정
            ipAddr.Text = Properties.Settings.Default.IP;
            portNum.Text = Properties.Settings.Default.PORT.ToString();

            openBtn.Visibility = Visibility.Visible;

            mainEncoding.SelectedIndex = Properties.Settings.Default.MAIN_ENCODING;
            mainEncodingSelect = (EncodingSelector)0;
                
            state.Text = "미접속";

            log.Document.Blocks.Clear();
            receiveData.Text = "데이터 없음";

            sendEncoding.SelectedIndex = Properties.Settings.Default.SEND_ENCODING;
            sendEncodingSelect = (EncodingSelector)Properties.Settings.Default.SEND_ENCODING;

            sendMsg.Text = "";

            autoSendType.SelectedIndex = Properties.Settings.Default.AUTO_SEND_TYPE;
            autoSendSelector = (AutoSendSelector)Properties.Settings.Default.AUTO_SEND_TYPE;

            if (autoSendSelector == AutoSendSelector.Receive)
            {
                autoSendMsg.IsEnabled = false;
            }
            else
            {
                autoSendMsg.IsEnabled = true;
            }

            autoSendMsg.Text = Properties.Settings.Default.AUTO_SEND_MSG;
            autoSendFlag = !Properties.Settings.Default.AUTO_SEND_FLAG;  // 토글 방식이라 false값으로 돌리고 함수 호출
            autoSendMsgContent = Properties.Settings.Default.AUTO_SEND_MSG;

            AutoSendSetting();

            controller.IsEnabled = false;
        }


        /// <summary>
        /// 동작 시작
        /// </summary>
        private void Start()
        {
            string IP = ipAddr.Text.Trim();
            int PORT = int.Parse(portNum.Text.Trim());

            // 아이피 정합성 검사
            bool vaild = !string.IsNullOrEmpty(IP) && IPAddress.TryParse(IP, out IPAddress ipCheck);

            if (!vaild)
            {
                AddLog("잘못된 IP 형식");
                return;
            }

            if (runTypeSelector == RunTypeSelector.Server)
            {
                Server.Start(IP, PORT);

                // 프로그램(서버)에 접속한 클라이언트 확인
                connCountThread = new Thread(GetConnCount)
                {
                    IsBackground = true
                };
                connCountThread.Start();
            }
            else
            {
                Client.Start(IP, PORT);

                // 서버에 접속한 상태 표시
                connStateThread = new Thread(GetConnState)
                {
                    IsBackground = true
                };
                connStateThread.Start();
            }

            RunSetting();
            SettingSave();
        }


        /// <summary>
        /// 동장 중지
        /// </summary>
        private void Stop()
        {
            startFlag = false;

            if (runTypeSelector == RunTypeSelector.Server)
            {
    
                Server.Stop();
            }
            else
            {                
                Client.Stop();
            }

            RunSetting();
        }


        /// <summary>
        /// 로그에 출력
        /// </summary>
        /// <param name="content"></param>
        public void AddLog(string content)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                log.AppendText(content + "\r");
                log.ScrollToEnd();
                receiveData.Text = content;
            }));
        }


        /// <summary>
        /// 서버 접속자 표시
        /// </summary>
        private void GetConnCount()
        {
            while(startFlag)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {
                    state.Text = string.Concat("접속(", Server.connCount, ")");
                }));

                Thread.Sleep(1000);
            }       
        }


        /// <summary>
        /// 접속 상태 표시
        /// </summary>
        private void GetConnState()
        {
            while (startFlag)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {
                    state.Text = string.Concat(Client.connState);
                }));

                Thread.Sleep(1000);
            }
        }


        /// <summary>
        /// 인코딩 변경
        /// </summary>
        /// <param name="combbox"></param>
        private void Encoding_Selector(ComboBox combbox)
        {
            // 초기화를 하지 않으면 하단 if문에서 에러
            EncodingSelector es = EncodingSelector.UTF8;


            switch (combbox.SelectedIndex)
            {
                case 0:
                    es = EncodingSelector.UTF8;
                    break;

                case 1:
                    es = EncodingSelector.UNICODE;
                    break;

                case 2:
                    es = EncodingSelector.EUCKR;
                    break;
            }

            if (combbox.Name.Equals("mainEncoding"))
            {
                mainEncodingSelect = es;
            }
            else
            {
                sendEncodingSelect = es;
            }
        }


        /// <summary>
        /// 설정 저장 - 서버나 클라이언트를 시작할때마다 저장
        /// </summary>
        private void SettingSave()
        {
            Properties.Settings.Default.RUN_TYPE = runType.SelectedIndex;
            Properties.Settings.Default.IP = ipAddr.Text.Trim();
            Properties.Settings.Default.PORT = int.Parse(portNum.Text.Trim());
            Properties.Settings.Default.MAIN_ENCODING = mainEncoding.SelectedIndex;
            Properties.Settings.Default.SEND_ENCODING = sendEncoding.SelectedIndex;
            Properties.Settings.Default.AUTO_SEND_MSG = autoSendMsg.Text.Trim();
            Properties.Settings.Default.AUTO_SEND_FLAG = autoSendFlag;
            Properties.Settings.Default.AUTO_SEND_TYPE = autoSendType.SelectedIndex;

            Properties.Settings.Default.Save();
        }


        /// <summary>
        /// 실행 상태에 따라 컨트롤 활성/비활성화
        /// </summary>
        private void RunSetting()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                runType.IsEnabled = !startFlag;
                ipAddr.IsEnabled = !startFlag;
                portNum.IsEnabled = !startFlag;
                mainEncoding.IsEnabled = !startFlag;
                controller.IsEnabled = startFlag;

                log.Document.Blocks.Clear();

                if (startFlag)
                {
                    openBtn.Content = "중지";
                }
                else
                {
                    openBtn.Content = "시작";
                    receiveData.Text = "데이터 없음";
                    state.Text = "미접속";
                }
            }));
        }


        /// <summary>
        /// 자동 전송 설정
        /// </summary>
        private void AutoSendSetting()
        {
            if (!autoSendFlag)
            {
                autoSendFlag = true;
                autoSend.Content = "자동 전송 ON";

                autoSendType.IsEnabled = false;
                autoSendMsgContent = autoSendMsg.Text.Trim();                
                autoSendMsg.IsEnabled = false;

                SettingSave();
            }
            else
            {
                autoSendFlag = false;
                autoSend.Content = "자동 전송 OFF";
                autoSendType.IsEnabled = true;
                autoSendMsg.IsEnabled = true;
            }
        }


    }
}
