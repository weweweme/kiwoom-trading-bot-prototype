using System.Windows.Forms;

namespace trading_bot_prototype
{
    /// <summary>
    /// AccountService에서 사용하는 UI 컨트롤들을 모은 구조체
    /// </summary>
    public class AccountUIContext
    {
        public ComboBox CmbAccounts { get; set; }
        public Label LblUserId { get; set; }
        public Label LblUserName { get; set; }
        public Label LblServerType { get; set; }
        public Label LblBalance { get; set; }
    }
}
