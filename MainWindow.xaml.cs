using kbox.EnumClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        
        bool connState = false;


        bool autoSendFlag = false;



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

            ipAddr.Text = "127.0.0.1";
            portNum.Text = "8899";

            openBtn.Visibility = Visibility.Visible;
            closeBtn.Visibility = Visibility.Collapsed;
            encoding.SelectedIndex = 0;

            state.Text = "미접속";

            log.Document.Blocks.Clear();
            receiveData.Text = "데이터가 없습니다.";

            encoding2.SelectedIndex = 0;
            sendMsg.Text = "";

            autoSendMsg.Text = "";
            autoSendFlag = false;

            
            controller.IsEnabled = false;
            recent.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#999999"));

        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                // 서버인지 클라이언트인지 확인
                RunTypeSelector rt = (RunTypeSelector)runType.SelectedIndex;

                

                if(rt == RunTypeSelector.Server)
                {
                    
                }
                else
                {
                    
                }


            }
            catch (Exception ex)
            {
                appendLog("시작할 수 없는 상태입니다.");
            }


        }


        public void appendLog(string logContent)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                log.AppendText(logContent + "\r");
                log.ScrollToEnd();

            }));
        }


    }
}
