using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Breezinstein.Tools.UI
{
    public class MessaageBox : MonoBehaviour
    {
		public TextMeshProUGUI headerText;

        public TextMeshProUGUI messageText;

        public Button button1;
		public TextMeshProUGUI buttonText1;

        public Button button2;
		public TextMeshProUGUI buttonText2;

        public Button button3;
		public TextMeshProUGUI buttonText3;

        public MessageTemplate messageInfo;

        void Start()
        {
            button1.onClick.AddListener(() =>
            {
                messageInfo.Action1?.Invoke();
                Close();
            });
            button2.onClick.AddListener(() =>
            {
                messageInfo.Action2?.Invoke();
                Close();
            });
           button3.onClick.AddListener(() =>
            {
                messageInfo.Action3?.Invoke();
                Close();
            });
        }

        void OnEnable()
        {
            if (messageInfo != null)
            {
                SetUpButtons();
                SetupText();
                transform.SetAsLastSibling();
            }
        }

        void SetUpButtons()
        {
            switch (messageInfo.NumberOfButtons)
            {
                case 1:
               
                    button1.gameObject.SetActive(true);
                    button2.gameObject.SetActive(false);
                    button3.gameObject.SetActive(false);
                    break;
                case 2:
				default:
                    button1.gameObject.SetActive(true);
                    button2.gameObject.SetActive(true);
                    button3.gameObject.SetActive(false);
                    break;
               case 3:
                    button1.gameObject.SetActive(true);
                    button2.gameObject.SetActive(true);
                    button3.gameObject.SetActive(true);
                    break;
            }
        }

        void SetupText()
        {
            headerText.text = messageInfo.Header;
            messageText.text = messageInfo.Message;
            buttonText1.text = messageInfo.Button1Text;
            buttonText2.text = messageInfo.Button2Text;
            buttonText3.text = messageInfo.Button3Text;

        }
        public void Close()
        {
            WindowManager.Instance.CloseMessageBox();
        }
    }
}