using System;
using System.Windows.Forms;

namespace trading_bot_prototype
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Load += new EventHandler(this.Form1_Load);
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
                {
                    WriteLog("로그인 성공");

                    // 로그인 정보 가져오기
                    string userId = axKHOpenAPI1.GetLoginInfo("USER_ID");
                    string userName = axKHOpenAPI1.GetLoginInfo("USER_NAME");
                    string accountListRaw = axKHOpenAPI1.GetLoginInfo("ACCNO"); // 계좌번호 목록
                    string serverType = axKHOpenAPI1.GetLoginInfo("GetServerGubun"); // 0: 실서버, 1: 모의투자
                    string[] accountList = accountListRaw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    cmbAccounts.Items.Clear();
                    cmbAccounts.Items.AddRange(accountList);
                    if (cmbAccounts.Items.Count > 0)
                        cmbAccounts.SelectedIndex = 0;

                    // Label에 실제 값 세팅
                    lblUserId.Text = $"ID: {userId}";
                    lblUserName.Text = $"이름: {userName}";
                    lblServerType.Text = $"서버: {(serverType == "1" ? "모의투자" : "실서버")}";

                    // 출력
                    WriteLog($"사용자 ID: {userId}");
                    WriteLog($"사용자 이름: {userName}");
                    WriteLog("계좌 목록:");
                    foreach (string acc in accountList)
                    {
                        WriteLog($"- {acc}");
                    }
                    WriteLog($"서버 종류: {(serverType == "1" ? "모의투자" : "실서버")}");
                }
                else
                {
                    WriteLog($"로그인 실패 - 에러코드: {e.nErrCode}");
                }
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

            btnCheckBalance.Click += (s, e) =>
            {
                if (cmbAccounts.SelectedItem == null)
                {
                    WriteLog("계좌를 선택하세요.");
                    return;
                }

                string account = cmbAccounts.SelectedItem.ToString();

                axKHOpenAPI1.SetInputValue("계좌번호", account);
                axKHOpenAPI1.SetInputValue("비밀번호", ""); // 보통 공란, 실서버에서 다르면 수정
                axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00"); // PC
                axKHOpenAPI1.SetInputValue("조회구분", "1"); // 합산

                int result = axKHOpenAPI1.CommRqData("예수금요청", "opw00001", 0, "9000");

                if (result == 0)
                    WriteLog("예수금 조회 요청 성공");
                else
                    WriteLog("예수금 조회 요청 실패");
            };

            axKHOpenAPI1.OnReceiveTrData += (s, e) =>
            {
                if (e.sRQName == "예수금요청")
                {
                    string cash = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "예수금");
                    cash = cash.Trim();
                    WriteLog($"현재 매수 가능 예수금: {cash}원");

                    // label로도 표시
                    lblBalance.Text = $"예수금: {cash}원";
                }
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
