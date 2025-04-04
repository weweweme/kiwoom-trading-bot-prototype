using System.Windows.Forms;

namespace trading_bot_prototype
{
    /// <summary>
    /// 로그인, 계좌 목록, 예수금 조회 등 계좌 관련 기능 전담
    /// </summary>
    public class AccountService
    {
        private readonly KiwoomApiWrapper _api;
        private readonly Logger _logger;
        private readonly PriceFormatter _formatter;
        private readonly ComboBox _cmbAccounts;
        private readonly Label _lblUserId;
        private readonly Label _lblUserName;
        private readonly Label _lblServerType;
        private readonly Label _lblBalance;

        public AccountService(
            KiwoomApiWrapper api,
            Logger logger,
            PriceFormatter formatter,
            ComboBox cmbAccounts,
            Label lblUserId,
            Label lblUserName,
            Label lblServerType,
            Label lblBalance)
        {
            _api = api;
            _logger = logger;
            _formatter = formatter;
            _cmbAccounts = cmbAccounts;
            _lblUserId = lblUserId;
            _lblUserName = lblUserName;
            _lblServerType = lblServerType;
            _lblBalance = lblBalance;
        }

        public void HandleLoginSuccess()
        {
            string userId = _api.GetUserId();
            string userName = _api.GetUserName();
            string serverType = _api.GetServerType();
            string[] accountList = _api.GetAccountList();

            _cmbAccounts.Items.Clear();
            _cmbAccounts.Items.AddRange(accountList);
            if (_cmbAccounts.Items.Count > 0)
                _cmbAccounts.SelectedIndex = 0;

            _lblUserId.Text = $"ID: {userId}";
            _lblUserName.Text = $"이름: {userName}";
            _lblServerType.Text = $"서버: {(serverType == "1" ? "모의투자" : "실서버")}";

            _logger.Log($"사용자 ID: {userId}");
            _logger.Log($"사용자 이름: {userName}");
            _logger.Log("계좌 목록:");
            foreach (var acc in accountList)
                _logger.Log($"- {acc}");
            _logger.Log($"서버 종류: {(serverType == "1" ? "모의투자" : "실서버")}");
        }

        public void CheckConnectionStatus()
        {
            if (_api.GetConnectState() == 0)
                _logger.Log("Open API 연결되어 있지 않습니다.");
            else
                _logger.Log("Open API 연결 중입니다.");
        }

        public void RequestBalance(string account, string password)
        {
            int result = _api.RequestBalance(account, password);

            if (result == 0)
                _logger.Log("예수금 조회 요청 성공");
            else
                _logger.Log("예수금 조회 요청 실패");
        }

        public void HandleBalanceResult(string rawCash)
        {
            string formatted = _formatter.Format(rawCash);
            _logger.Log($"현재 매수 가능 예수금: {formatted}원");
            _lblBalance.Text = $"예수금: {formatted}원";
        }
    }
}
