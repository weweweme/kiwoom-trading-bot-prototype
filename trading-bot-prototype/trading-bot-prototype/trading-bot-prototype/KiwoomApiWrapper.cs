using System;
using System.Collections.Generic;
using AxKHOpenAPILib;

namespace trading_bot_prototype
{
    /// <summary>
    /// 키움증권 OpenAPI를 감싸는 래퍼 클래스
    /// </summary>
    public class KiwoomApiWrapper
    {
        private readonly AxKHOpenAPI _api;

        public KiwoomApiWrapper(AxKHOpenAPI api)
        {
            _api = api;
        }

        public string GetUserId() => _api.GetLoginInfo("USER_ID");
        public string GetUserName() => _api.GetLoginInfo("USER_NAME");
        public string GetServerType() => _api.GetLoginInfo("GetServerGubun");

        public string[] GetAccountList()
        {
            var raw = _api.GetLoginInfo("ACCNO");
            return raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public Dictionary<string, string> GetStockCodeNameMap()
        {
            var result = new Dictionary<string, string>();
            foreach (var market in new[] { "0", "10" }) // 코스피 + 코스닥
            {
                var codes = _api.GetCodeListByMarket(market).Split(';');
                foreach (var code in codes)
                {
                    if (string.IsNullOrWhiteSpace(code)) continue;
                    var name = _api.GetMasterCodeName(code).Trim();
                    if (!result.ContainsKey(name))
                        result[name] = code;
                }
            }
            return result;
        }

        public int RequestBalance(string account, string password)
        {
            _api.SetInputValue("계좌번호", account);
            _api.SetInputValue("비밀번호", password);
            _api.SetInputValue("비밀번호입력매체구분", "00"); // PC
            _api.SetInputValue("조회구분", "1"); // 합산
            return _api.CommRqData("예수금요청", "opw00001", 0, "9000");
        }

        public int RequestStockInfo(string code)
        {
            _api.SetInputValue("종목코드", code);
            return _api.CommRqData("종목정보요청", "opt10001", 0, "9100");
        }

        public int CommConnect() => _api.CommConnect();
        public int GetConnectState() => _api.GetConnectState();
    }
}
