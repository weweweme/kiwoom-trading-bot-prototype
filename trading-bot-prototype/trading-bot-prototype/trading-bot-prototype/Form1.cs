﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace trading_bot_prototype
{
    public partial class Form1 : Form
    {
        private Dictionary<string, string> nameToCode = new Dictionary<string, string>();
        private Logger _logger;

        public Form1()
        {
            InitializeComponent();
            this.Load += new EventHandler(this.Form1_Load);
            _logger = new Logger(rtxtLog);
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
                    _logger.Log("로그인 성공");

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
                    _logger.Log($"사용자 ID: {userId}");
                    _logger.Log($"사용자 이름: {userName}");
                    _logger.Log("계좌 목록:");
                    foreach (string acc in accountList)
                    {
                        _logger.Log($"- {acc}");
                    }
                    _logger.Log($"서버 종류: {(serverType == "1" ? "모의투자" : "실서버")}");

                    LoadStockNameDictionary();
                }
                else
                {
                    _logger.Log($"로그인 실패 - 에러코드: {e.nErrCode}");
                }
            };

            // 로그인 버튼 클릭 이벤트 핸들러
            btnLogin.Click += (s, e) =>
            {
                if (axKHOpenAPI1.CommConnect() == 0)
                    _logger.Log("로그인창 열기 성공");
                else
                    _logger.Log("로그인창 열기 실패");
            };

            // 연결 확인 버튼 클릭 이벤트 핸들러
            btnCheckConnect.Click += (s, e) =>
            {
                if (axKHOpenAPI1.GetConnectState() == 0)
                    _logger.Log("Open API 연결되어 있지 않습니다.");
                else
                    _logger.Log("Open API 연결 중입니다.");
            };

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

                axKHOpenAPI1.SetInputValue("계좌번호", account);
                axKHOpenAPI1.SetInputValue("비밀번호", password); // <- 여기!
                axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00"); // PC
                axKHOpenAPI1.SetInputValue("조회구분", "1"); // 합산

                int result = axKHOpenAPI1.CommRqData("예수금요청", "opw00001", 0, "9000");

                if (result == 0)
                    _logger.Log("예수금 조회 요청 성공");
                else
                    _logger.Log("예수금 조회 요청 실패");
            };

            axKHOpenAPI1.OnReceiveTrData += (s, e) =>
            {
                if (e.sRQName == "예수금요청")
                {
                    string cashRaw = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "예수금");
                    string cashFormatted = FormatPrice(cashRaw);

                    _logger.Log($"현재 매수 가능 예수금: {cashFormatted}원");
                    lblBalance.Text = $"예수금: {cashFormatted}원";
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
                    _logger.Log($"시가: {FormatPrice(open)}");
                    _logger.Log($"고가: {FormatPrice(high)}");
                    _logger.Log($"저가: {FormatPrice(low)}");
                    _logger.Log($"상한가: {FormatPrice(upper)}");
                    _logger.Log($"기준가: {FormatPrice(basePrice)}");
                    _logger.Log($"유통비율: {floatRate}");
                }
            };

            btnRequestStockInfo.Click += (s, e) =>
            {
                string code = txtStockCode.Text.Trim();

                if (string.IsNullOrWhiteSpace(code))
                {
                    _logger.Log("종목코드를 입력하세요.");
                    return;
                }

                axKHOpenAPI1.SetInputValue("종목코드", code);
                int result = axKHOpenAPI1.CommRqData("종목정보요청", "opt10001", 0, "9100");

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
                // "삼성전자우 (005935)" → "005935" 추출
                string code = selected.Split('(', ')')[1];
                txtStockCode.Text = code;
            };
        }

        private string FormatPrice(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "0";

            raw = raw.Trim().TrimStart('0');
            if (string.IsNullOrEmpty(raw))
                raw = "0";

            if (!long.TryParse(raw, out long val))
                return raw;

            return val.ToString("N0");
        }


        private void LoadStockNameDictionary()
        {
            nameToCode.Clear();

            var kospi = axKHOpenAPI1.GetCodeListByMarket("0").Split(';');
            var kosdaq = axKHOpenAPI1.GetCodeListByMarket("10").Split(';');

            foreach (var code in kospi.Concat(kosdaq))
            {
                if (string.IsNullOrWhiteSpace(code)) continue;
                string name = axKHOpenAPI1.GetMasterCodeName(code).Trim();

                if (!nameToCode.ContainsKey(name))
                    nameToCode[name] = code;
            }

            _logger.Log($"종목명 매핑 완료 - 총 {nameToCode.Count}건");
        }
    }
}
