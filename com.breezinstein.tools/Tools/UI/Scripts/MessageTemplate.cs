namespace Breezinstein.Tools.UI
{
    public class MessageTemplate
    {
        public string header;
        public string message;
        public string button1Text = "Yes";
        public string button2Text = "No";
        public string button3Text = "Maybe";
        public int numberOfButtons;

        public MessageTemplate(string _header, string _message, string _button1Text)
        {
            header = _header;
            message = _message;
            button1Text = _button1Text;
            numberOfButtons = 1;
        }

        public MessageTemplate(string _header, string _message, string _button1Text, string _button2Text)
        {
            header = _header;
            message = _message;
            button1Text = _button1Text;
            button2Text = _button2Text;
            numberOfButtons = 2;
        }

        public MessageTemplate(string _header, string _message, string _button1Text, string _button2Text, string _button3Text)
        {
            header = _header;
            message = _message;
            button1Text = _button1Text;
            button2Text = _button2Text;
            button3Text = _button3Text;
            numberOfButtons = 3;
        }
    }
}