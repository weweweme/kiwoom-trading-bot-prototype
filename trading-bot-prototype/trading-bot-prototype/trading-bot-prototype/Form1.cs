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
        private readonly StockService _stock;

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

            nameToCode = _api.GetStockCodeNameMap();

            _stock = new StockService(
                _api,
                _logger,
                _formatter,
                new StockUIContext
                {
                    TxtStockName = txtStockName,
                    TxtStockCode = txtStockCode,
                    LstStockCandidates = lstStockCandidates
                },
                nameToCode);

            this.Load += Form1_Load;
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

            // 데이터 수신 이벤트 (예수금, 종목정보)
            axKHOpenAPI1.OnReceiveTrData += (s, e) =>
            {
                if (e.sRQName == "예수금요청")
                {
                    string cashRaw = _api.GetCommData(e.sTrCode, e.sRQName, 0, "예수금");
                    _account.HandleBalanceResult(cashRaw);
                }

                if (e.sRQName == "종목정보요청")
                {
                    _stock.HandleStockInfoResult(e);
                }
            };

            // 버튼 클릭 이벤트
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

            btnRequestStockInfo.Click += (s, e) => _stock.RequestStockInfo();

            txtStockName.TextChanged += (s, e) => _stock.OnStockNameChanged();

            lstStockCandidates.SelectedIndexChanged += (s, e) => _stock.OnStockCandidateSelected();
        }
    }
}
