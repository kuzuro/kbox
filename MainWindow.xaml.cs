using kbox.EnumClass;
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
        
        public bool startFlag = false;

        public bool autoSendFlag = false;
        public string autoSendContent = "";

        private Thread connCountThread;
        private Thread connStateThread;

        public RunTypeSelector runTypeSelector;
        public EncodingSelector mainEncodingSelect;
        public EncodingSelector sendEncodingSelect;
        

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
        private void btn_minimized_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        /// <summary>
        /// 프로그램 종료
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            programExit();
        }


        /// <summary>
        /// 서버 및 클라이언트 종료 후 프로그램 종료
        /// </summary>
        public void programExit()
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

            autoSendMsg.Text = Properties.Settings.Default.AUTO_SEND_MSG;
            autoSendFlag = Properties.Settings.Default.AUTO_SEND_FLAG;
            
            controller.IsEnabled = false;

        }


        /// <summary>
        /// 시작/종료
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                RunTypeSelector rt = (RunTypeSelector)runType.SelectedIndex;

                if (!startFlag)
                {

                    string IP = ipAddr.Text.Trim();
                    int PORT = int.Parse(portNum.Text.Trim());


                    // 아이피 정합성 검사
                    IPAddress ipCheck;
                    bool vaild = !string.IsNullOrEmpty(IP) && IPAddress.TryParse(IP, out ipCheck);

                    if(!vaild)
                    {
                        appendLog("잘못된 IP 형식");
                        return;
                    }

                    log.Document.Blocks.Clear();


                    if (rt == RunTypeSelector.Server)
                    {
                        Server.Start(IP, PORT);

                        // 프로그램(서버)에 접속한 클라이언트 확인
                        connCountThread = new Thread(getConnCount);
                        connCountThread.IsBackground = true;
                        connCountThread.Start();
                    }
                    else
                    {
                        Client.Start(IP, PORT);

                        // 서버에 접속한 상태 표시
                        connStateThread = new Thread(getConnState);
                        connStateThread.IsBackground = true;
                        connStateThread.Start();

                    }

                    startFlag = true;

                    RunSetting();
                    SettingSave();

                    openBtn.Content = "중지";
                }
                else
                {
                    if (rt == RunTypeSelector.Server)
                    {
                        Server.Stop();

                        connCountThread.Abort();
                    }
                    else
                    {
                        Client.Stop();
                    }

                    log.Document.Blocks.Clear();
                    receiveData.Text = "데이터 없음";

                    state.Text = "미접속";

                    startFlag = false;
                    RunSetting();

                    openBtn.Content = "시작";
                }

            }
            catch (Exception ex)
            {
                appendLog("시작 실패");
            }
        }


        /// <summary>
        /// 로그에 출력
        /// </summary>
        /// <param name="content"></param>
        public void appendLog(string content)
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
        private void getConnCount()
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
        private void getConnState()
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
        /// 실행 타입 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void runType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(runType.SelectedIndex)
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
        private void mainEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (mainEncoding.SelectedIndex)
            {
                case 0:
                    mainEncodingSelect = EncodingSelector.UTF8;
                    break;

                case 1:
                    mainEncodingSelect = EncodingSelector.UNICODE;
                    break;

                case 2:
                    mainEncodingSelect = EncodingSelector.EUCKR;
                    break;
            }
        }


        /// <summary>
        /// 전송 인코딩 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (sendEncoding.SelectedIndex)
            {
                case 0:
                    sendEncodingSelect = EncodingSelector.UTF8;
                    break;

                case 1:
                    sendEncodingSelect = EncodingSelector.UNICODE;
                    break;

                case 2:
                    sendEncodingSelect = EncodingSelector.EUCKR;
                    break;
            }
        }


        /// <summary>
        /// 전송
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void send_Click(object sender, RoutedEventArgs e)
        {

            RunTypeSelector rt = (RunTypeSelector)runType.SelectedIndex;

            if (rt == RunTypeSelector.Server)
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
        private void autoSend_Click(object sender, RoutedEventArgs e)
        {
            if (!autoSendFlag)
            {
                autoSendFlag = true;
                autoSend.Content = "자동 전송 ON";
                autoSendContent = autoSendMsg.Text.Trim();
            }
            else
            {
                autoSendFlag = false;
                autoSend.Content = "자동 전송 OFF";
                autoSendContent = "";
            }
        }


        /// <summary>
        /// 로그 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cleanButton_click(object sender, RoutedEventArgs e)
        {
            log.Document.Blocks.Clear();
            receiveData.Text = "데이터 없음";
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

            Properties.Settings.Default.Save();
        }


        /// <summary>
        /// 실행 상태에 따라 컨트롤 활성/비활성화
        /// </summary>
        private void RunSetting()
        {
            runType.IsEnabled = !startFlag;
            ipAddr.IsEnabled = !startFlag;
            portNum.IsEnabled = !startFlag;
            mainEncoding.IsEnabled = !startFlag;
            controller.IsEnabled = startFlag;
        }


        /// <summary>
        /// 포트번호 입력 마스크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void portNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
