using System.Windows.Forms;

namespace trading_bot_prototype
{
    /// <summary>
    /// 종목 검색 및 조회 기능에서 사용하는 UI 컨트롤들을 묶는 컨텍스트
    /// </summary>
    public class StockUIContext
    {
        public TextBox TxtStockName { get; set; }              // 종목명 입력
        public ListBox LstStockCandidates { get; set; }        // 후보 리스트
        public TextBox TxtStockCode { get; set; }              // 종목코드 입력
    }
}
