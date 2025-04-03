using System;
using System.Windows.Forms;

namespace trading_bot_prototype
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            // KHOpenAPI Control의 로그인 이벤트 핸들러
            axKHOpenAPI1.OnEventConnect += (s, e) =>
            {
                if (e.nErrCode == 0)
                    WriteLog("로그인 성공"); // 정상 처리
                else
                    WriteLog($"로그인 실패: {e.nErrCode}"); // 에러 발생
            };

            // 로그인 버튼 클릭 이벤트 핸들러
            btnLogin.Click += (s, e) =>
            {
                if (axKHOpenAPI1.CommConnect() == 0)
                    WriteLog("로그인창 열기 성공");
                else
                    WriteLog("로그인창 열기 실패");
            };

            // 연결 확인 버튼 클릭 이벤트 핸들러
            btnCheckConnect.Click += (s, e) =>
            {
                if (axKHOpenAPI1.GetConnectState() == 0)
                    WriteLog("Open API 연결되어 있지 않습니다.");
                else
                    WriteLog("Open API 연결 중입니다.");
            };
        }

        private void WriteLog(string message)
        {
            rtxtLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
            rtxtLog.SelectionStart = rtxtLog.Text.Length;
            rtxtLog.ScrollToCaret();
        }
    }
}
