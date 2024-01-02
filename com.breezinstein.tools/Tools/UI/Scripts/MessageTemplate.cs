using System;

namespace Breezinstein.Tools.UI
{
    /// <summary>
    /// Represents a message template for UI interactions.
    /// </summary>
    public class MessageTemplate
    {
        /// <summary>
        /// Gets the header of the message.
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// Gets the main content of the message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the text for the first button.
        /// </summary>
        public string Button1Text { get; }

        /// <summary>
        /// Gets the text for the second button.
        /// </summary>
        public string Button2Text { get; }

        /// <summary>
        /// Gets the text for the third button.
        /// </summary>
        public string Button3Text { get; }

        /// <summary>
        /// Gets the number of buttons in the message.
        /// </summary>
        public int NumberOfButtons { get; }

        /// <summary>
        /// Gets the action to be performed when the first button is clicked.
        /// </summary>
        public Action Action1 { get; }

        /// <summary>
        /// Gets the action to be performed when the second button is clicked.
        /// </summary>
        public Action Action2 { get; }

        /// <summary>
        /// Gets the action to be performed when the third button is clicked.
        /// </summary>
        public Action Action3 { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageTemplate"/> class.
        /// </summary>
        /// <param name="header">The header of the message.</param>
        /// <param name="message">The main content of the message.</param>
        /// <param name="button1Text">The text for the first button. Defaults to "Yes".</param>
        /// <param name="action1">The action to be performed when the first button is clicked.</param>
        /// <param name="button2Text">The text for the second button.</param>
        /// <param name="action2">The action to be performed when the second button is clicked.</param>
        /// <param name="button3Text">The text for the third button.</param>
        /// <param name="action3">The action to be performed when the third button is clicked.</param>
        public MessageTemplate(string header, string message, string button1Text = "Yes", Action action1 = null, string button2Text = null, Action action2 = null, string button3Text = null, Action action3 = null)
        {
            Header = header;
            Message = message;
            Button1Text = button1Text;
            Button2Text = button2Text;
            Button3Text = button3Text;
            Action1 = action1;
            Action2 = action2;
            Action3 = action3;

            NumberOfButtons = (button1Text != null ? 1 : 0) + (button2Text != null ? 1 : 0) + (button3Text != null ? 1 : 0);
        }
    }
}