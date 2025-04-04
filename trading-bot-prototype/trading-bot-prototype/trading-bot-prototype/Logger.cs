using System;
using System.Windows.Forms;

namespace trading_bot_prototype
{
    /// <summary>
    /// RichTextBox에 로그 메시지를 출력하는 Logger 클래스
    /// </summary>
    public class Logger
    {
        // 로그 메시지를 출력할 대상 RichTextBox
        private readonly RichTextBox _output;

        /// <summary>
        /// Logger 생성자
        /// </summary>
        /// <param name="output">로그를 출력할 RichTextBox 컨트롤</param>
        public Logger(RichTextBox output)
        {
            _output = output;
        }

        /// <summary>
        /// 로그 메시지를 RichTextBox에 추가하고 스크롤을 맨 아래로 이동
        /// </summary>
        /// <param name="message">출력할 로그 메시지</param>
        public void Log(string message)
        {
            _output.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
            _output.SelectionStart = _output.Text.Length;
            _output.ScrollToCaret();
        }
    }
}
