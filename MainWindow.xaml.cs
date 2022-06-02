using socket_box.EnumClass;
using socket_box.Model;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace socket_box
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
        public AutoSendTargetSelector autoSendTarget;

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
            // 서버가 실행중이면 종료
            if (Server.State())
            {
                Server.Stop();
            }

            // 클라이언트가 실행중이면 종료
            if (Client.State())
            {
                Client.Stop();
            }

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
                default:
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
            Encoding_Selector(sender as ComboBox);
        }


        /// <summary>
        /// 전송 인코딩 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Encoding_Selector(sender as ComboBox);
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
            receiveData.Text = "";
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
            if (autoSendMsg == null)
            {
                return;
            }

            autoSendSelector = (AutoSendSelector)autoSendType.SelectedIndex;

            if (autoSendSelector == AutoSendSelector.Receive)
            {
                autoSendMsg.IsEnabled = false;
                allSend.IsEnabled = false;
                specifySend.IsEnabled = false;
            }
            else
            {
                autoSendMsg.IsEnabled = true;
                allSend.IsEnabled = true;
                specifySend.IsEnabled = true;
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

            ipAddr.Text = Properties.Settings.Default.IP;
            portNum.Text = Properties.Settings.Default.PORT.ToString();

            openBtn.Visibility = Visibility.Visible;

            mainEncoding.SelectedIndex = Properties.Settings.Default.MAIN_ENCODING;
            mainEncodingSelect = (EncodingSelector)Properties.Settings.Default.MAIN_ENCODING;

            state.Text = "미접속";

            log.Document.Blocks.Clear();
            receiveData.Text = "";

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

            autoSendMsg.IsEnabled = autoSendSelector == AutoSendSelector.Receive;


            autoSendMsg.Text = Properties.Settings.Default.AUTO_SEND_MSG;

            // bool에 의한 토글 방식이라 저장값에 not을 적용한 뒤 함수 호출
            autoSendFlag = !Properties.Settings.Default.AUTO_SEND_FLAG;
            autoSendMsg.Text = Properties.Settings.Default.AUTO_SEND_MSG;
            AutoSendSetting();

            autoSendTarget = (AutoSendTargetSelector)Properties.Settings.Default.AUTO_SEND_TARGET_TYPE;
            if ((AutoSendTargetSelector)Properties.Settings.Default.AUTO_SEND_TARGET_TYPE == 0)
            {
                allSend.IsChecked = true;
            }
            else
            {
                specifySend.IsChecked = true;
            }

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
            }
            else
            {
                Client.Start(IP, PORT);
            }

            RunSetting();
            SettingSave();

            ConnectCheck();
        }


        /// <summary>
        /// 중지
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
        /// 로그 출력
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
        /// 접속 상태 확인
        /// </summary>
        private void ConnectCheck()
        {
            if (runTypeSelector == RunTypeSelector.Server)
            {
                // 서버에 접속된 클라이언트 카운트
                Thread serverConnCountThread = new Thread(ServerConnCount)
                {
                    IsBackground = true
                };
                serverConnCountThread.Start();
            }
            else
            {
                // 서버와 접속 상태 표시
                Thread clientConnStateThread = new Thread(ClientConnState)
                {
                    IsBackground = true
                };
                clientConnStateThread.Start();
            }
        }


        /// <summary>
        /// 서버에 접속된 클라이언트 카운트
        /// </summary>
        private void ServerConnCount()
        {
            while(true)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {
                    state.Text = string.Concat("접속(", Server.connectCount, ")");
                }));

                if (!Server.State())
                {
                    RunSetting();
                    break;
                }

                Thread.Sleep(1000);
            }
        }


        /// <summary>
        /// 접속 상태 표시
        /// </summary>
        private void ClientConnState()
        {
            while (true)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {
                    state.Text = string.Concat(Client.connStateMsg);
                }));

                if (!Client.State())
                {
                    RunSetting();

                    AddLog("접속 종료");

                    break;
                }

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
                default:
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
            Properties.Settings.Default.AUTO_SEND_TARGET_TYPE = (bool)allSend.IsChecked ? 0 : 1;

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

                receiveData.Text = "";

                if (startFlag)
                {
                    openBtn.Content = "중지";
                    state.Text = "접속";
                }
                else
                {
                    openBtn.Content = "시작";                    
                    state.Text = "미접속";
                }

                if(startFlag && runTypeSelector == RunTypeSelector.Server)
                {
                    autoSendControl.IsEnabled = true;
                }
                else
                {                    
                    autoSendControl.IsEnabled = false;
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

                allSend.IsEnabled = false;
                specifySend.IsEnabled = false;

                SettingSave();
            }
            else
            {
                autoSendFlag = false;
                autoSend.Content = "자동 전송 OFF";
                autoSendType.IsEnabled = true;
                autoSendMsg.IsEnabled = true;

                allSend.IsEnabled = true;
                specifySend.IsEnabled = true;
            }
        }


        /// <summary>
        /// 자동 전송 대상 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoSendTarget_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)allSend.IsChecked)
            {
                autoSendTarget = (AutoSendTargetSelector)0;
            }
            else
            {
                autoSendTarget = (AutoSendTargetSelector)1;
            }

            SettingSave();
        }

    }
}
