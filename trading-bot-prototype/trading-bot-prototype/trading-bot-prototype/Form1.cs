using System;
using System.Windows.Forms;

namespace trading_bot_prototype
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            // TODO: 이벤트 등록들 (로그인, 연결확인, 이벤트핸들러 등)
        }
    }
}
