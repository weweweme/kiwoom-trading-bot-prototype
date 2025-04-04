using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace trading_bot_prototype
{
    /// <summary>
    /// 종목 검색 및 조회 기능 담당 서비스
    /// </summary>
    public class StockService
    {
        private readonly KiwoomApiWrapper _api;
        private readonly Logger _logger;
        private readonly PriceFormatter _formatter;

        private readonly TextBox _txtStockName;
        private readonly ListBox _lstStockCandidates;
        private readonly TextBox _txtStockCode;

        private readonly Dictionary<string, string> _nameToCode;

        public StockService(
            KiwoomApiWrapper api,
            Logger logger,
            PriceFormatter formatter,
            StockUIContext ui,
            Dictionary<string, string> nameToCode)
        {
            _api = api;
            _logger = logger;
            _formatter = formatter;
            _txtStockName = ui.TxtStockName;
            _lstStockCandidates = ui.LstStockCandidates;
            _txtStockCode = ui.TxtStockCode;
            _nameToCode = nameToCode;
        }

        /// <summary>
        /// 종목명 텍스트 입력 시 후보 필터링
        /// </summary>
        public void OnStockNameChanged()
        {
            string keyword = _txtStockName.Text.Trim();
            _lstStockCandidates.Items.Clear();

            if (keyword.Length < 1) return;

            var filtered = _nameToCode
                .Where(kv => kv.Key.Contains(keyword))
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key} ({kv.Value})")
                .ToList();

            _lstStockCandidates.Items.AddRange(filtered.ToArray());
        }

        /// <summary>
        /// 종목 후보 선택 시 코드 추출 및 텍스트박스에 입력
        /// </summary>
        public void OnStockCandidateSelected()
        {
            if (_lstStockCandidates.SelectedItem == null) return;

            string selected = _lstStockCandidates.SelectedItem.ToString();
            string code = selected.Split('(', ')')[1]; // "삼성전자 (005930)" → 005930
            _txtStockCode.Text = code;
        }

        /// <summary>
        /// 종목코드 기반으로 정보 조회 요청
        /// </summary>
        public void RequestStockInfo()
        {
            string code = _txtStockCode.Text.Trim();
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
        }

        /// <summary>
        /// 종목정보 수신 후 출력 처리
        /// </summary>
        public void HandleStockInfoResult(AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            string trCode = e.sTrCode;
            string rqName = e.sRQName;

            string code = _api.GetCommData(trCode, rqName, 0, "종목코드").Trim();
            string name = _api.GetCommData(trCode, rqName, 0, "종목명").Trim();
            string open = _api.GetCommData(trCode, rqName, 0, "시가").TrimStart('0');
            string high = _api.GetCommData(trCode, rqName, 0, "고가").TrimStart('0');
            string low = _api.GetCommData(trCode, rqName, 0, "저가").TrimStart('0');
            string upper = _api.GetCommData(trCode, rqName, 0, "상한가").TrimStart('0');
            string basePrice = _api.GetCommData(trCode, rqName, 0, "기준가").TrimStart('0');
            string floatRate = _api.GetCommData(trCode, rqName, 0, "유통비율").Trim();

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
    }
}
