using kbox.EnumClass;
using kbox.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

        private Thread userThread;
        private Thread connCountThread;

        public RunTypeSelector runTypeSelector;
        public EncodingSelector mainEncodingSelect;
        public EncodingSelector sendEncodingSelect;
        

        public MainWindow()
        {
            InitializeComponent();

            Init();
        }



        #region 윈도우 기본

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }

        }

        private void btn_minimized_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            programExit();
        }

        public void programExit()
        {
            Environment.Exit(1);
        }

        #endregion



        /// <summary>
        /// 초기화
        /// </summary>
        private void Init()
        {
            runType.SelectedIndex = 0;
            runTypeSelector = RunTypeSelector.Server;

            // 프로퍼티에서 불러오도록 수정
            ipAddr.Text = "127.0.0.1";
            portNum.Text = "8899";

            openBtn.Visibility = Visibility.Visible;
            closeBtn.Visibility = Visibility.Collapsed;

            mainEncoding.SelectedIndex = 0;
            mainEncodingSelect = EncodingSelector.UTF8;

            state.Text = "미접속";

            log.Document.Blocks.Clear();
            receiveData.Text = "데이터가 없습니다.";

            sendEncoding.SelectedIndex = 0;
            sendEncodingSelect= EncodingSelector.UTF8;

            sendMsg.Text = "";

            autoSendMsg.Text = "";
            autoSendFlag = false;
            
            controller.IsEnabled = false;

        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                RunTypeSelector rt = (RunTypeSelector)runType.SelectedIndex;

                if (!startFlag)
                {
                    if (rt == RunTypeSelector.Server)
                    {
                        Server.Start(ipAddr.Text.Trim(), int.Parse(portNum.Text.Trim()));

                        startFlag = true;

                        connCountThread = new Thread(getConnCount);
                        connCountThread.IsBackground = true;
                        connCountThread.Start();
                    }
                    else
                    {

                    }

                    RunSetting();
                    openBtn.Content = "중지";

                }
                else
                {
                    if (rt == RunTypeSelector.Server)
                    {
                        Server.Stop();
                        startFlag = false;

                        connCountThread.Abort();

                        log.Document.Blocks.Clear();
                        receiveData.Text = "데이터가 없습니다.";
                    }
                    else
                    {

                    }

                    RunSetting();
                    openBtn.Content = "시작";
                }

            }
            catch (Exception ex)
            {
                appendLog("시작할 수 없는 상태입니다.");
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
        /// 접속자 표시
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
            Server.Send(sendMsg.Text.Trim());
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
            receiveData.Text = "데이터가 없습니다.";
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

    }
}
