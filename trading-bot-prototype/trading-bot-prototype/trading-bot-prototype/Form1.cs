using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace trading_bot_prototype
{
    public partial class Form1 : Form
    {
        private readonly Dictionary<string, string> nameToCode;
        private readonly Logger _logger;
        private readonly PriceFormatter _formatter;
        private readonly KiwoomApiWrapper _api;
        private readonly AccountService _account;

        public Form1()
        {
            InitializeComponent();
            _logger = new Logger(rtxtLog);
            _formatter = new PriceFormatter();
            _api = new KiwoomApiWrapper(axKHOpenAPI1);

            _account = new AccountService(
                _api,
                _logger,
                _formatter,
                new AccountUIContext
                {
                    CmbAccounts = cmbAccounts,
                    LblUserId = lblUserId,
                    LblUserName = lblUserName,
                    LblServerType = lblServerType,
                    LblBalance = lblBalance
                });

            this.Load += Form1_Load;
            nameToCode = _api.GetStockCodeNameMap();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            // 로그인 완료 이벤트
            axKHOpenAPI1.OnEventConnect += (s, e) =>
            {
                if (e.nErrCode == 0)
                {
                    _logger.Log("로그인 성공");
                    _account.HandleLoginSuccess();
                }
                else
                {
                    _logger.Log($"로그인 실패 - 에러코드: {e.nErrCode}");
                }
            };

            // 예수금 조회 응답 이벤트
            axKHOpenAPI1.OnReceiveTrData += (s, e) =>
            {
                if (e.sRQName == "예수금요청")
                {
                    string cashRaw = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "예수금");
                    _account.HandleBalanceResult(cashRaw);
                }

                if (e.sRQName == "종목정보요청")
                {
                    string code = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드").Trim();
                    string name = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목명").Trim();
                    string open = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "시가").TrimStart('0');
                    string high = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "고가").TrimStart('0');
                    string low = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "저가").TrimStart('0');
                    string upper = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "상한가").TrimStart('0');
                    string basePrice = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "기준가").TrimStart('0');
                    string floatRate = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "유통비율").Trim();

                    _logger.Log($"[종목 정보]");
                    _logger.Log($"코드: {code}");
                    _logger.Log($"이름: {name}");
                    _logger.Log($"시가: {_formatter.Format(open)}");
                    _logger.Log($"고가: {_formatter.Format(high)}");
                    _logger.Log($"저가: {_formatter.Format(low)}");
                    _logger.Log($"상한가: {_formatter.Format(upper)}");
                    _logger.Log($"기준가: {_formatter.Format(basePrice)}");
                    _logger.Log($"유통비율: {floatRate}");
                }
            };

            btnLogin.Click += (s, e) =>
            {
                if (_api.CommConnect() == 0)
                    _logger.Log("로그인창 열기 성공");
                else
                    _logger.Log("로그인창 열기 실패");
            };

            btnCheckConnect.Click += (s, e) => _account.CheckConnectionStatus();

            btnCheckBalance.Click += (s, e) =>
            {
                if (cmbAccounts.SelectedItem == null)
                {
                    _logger.Log("계좌를 선택하세요.");
                    return;
                }

                string account = cmbAccounts.SelectedItem.ToString();
                string password = txtPassword.Text.Trim();

                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.Log("비밀번호를 입력하세요.");
                    return;
                }

                _account.RequestBalance(account, password);
            };

            btnRequestStockInfo.Click += (s, e) =>
            {
                string code = txtStockCode.Text.Trim();

                if (string.IsNullOrWhiteSpace(code))
                {
                    _logger.Log("종목코드를 입력하세요.");
                    return;
                }

                int result = _api.RequestStockInfo(code);

                if (result == 0)
                    _logger.Log($"종목 [{code}] 정보 조회 요청 성공");
                else
                    _logger.Log($"종목 [{code}] 정보 조회 요청 실패");
            };

            txtStockName.TextChanged += (s, e) =>
            {
                string keyword = txtStockName.Text.Trim();
                lstStockCandidates.Items.Clear();

                if (keyword.Length < 1) return;

                var filtered = nameToCode
                    .Where(kv => kv.Key.Contains(keyword))
                    .OrderBy(kv => kv.Key)
                    .Select(kv => $"{kv.Key} ({kv.Value})")
                    .ToList();

                lstStockCandidates.Items.AddRange(filtered.ToArray());
            };

            lstStockCandidates.SelectedIndexChanged += (s, e) =>
            {
                if (lstStockCandidates.SelectedItem == null) return;

                string selected = lstStockCandidates.SelectedItem.ToString();
                string code = selected.Split('(', ')')[1];
                txtStockCode.Text = code;
            };
        }
    }
}
