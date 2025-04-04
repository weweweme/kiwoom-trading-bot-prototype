namespace trading_bot_prototype
{
    /// <summary>
    /// 문자열 형태의 금액 데이터를 천 단위 콤마가 포함된 숫자 형식으로 변환하는 유틸 클래스
    /// </summary>
    public class PriceFormatter
    {
        /// <summary>
        /// 금액 문자열을 천 단위 콤마 포함 형식으로 변환
        /// </summary>
        /// <param name="raw">금액 문자열(예: "00000001234500")</param>
        /// <returns>변환된 문자열(예: "1,234,500")</returns>
        public string Format(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "0";

            raw = raw.Trim().TrimStart('0');
            if (string.IsNullOrEmpty(raw))
                raw = "0";

            return long.TryParse(raw, out var val)
                ? val.ToString("N0")
                : raw;
        }
    }
}
