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

        public MessageBoxEvent eventCallback;

        void Start()
        {
            button1.onClick.AddListener(() =>
            {
                eventCallback.Invoke(1);
                //AudioManager.Instance.PlaySFX("ui_select");
                Close();
            });
            button2.onClick.AddListener(() =>
            {
                eventCallback.Invoke(2);
                //AudioManager.Instance.PlaySFX("ui_select");
                Close();
            });
           button3.onClick.AddListener(() =>
            {
                eventCallback.Invoke(3);
                //AudioManager.Instance.PlaySFX("ui_select");
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
            switch (messageInfo.numberOfButtons)
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
            headerText.text = messageInfo.header;
            messageText.text = messageInfo.message;
            buttonText1.text = messageInfo.button1Text;
            buttonText2.text = messageInfo.button2Text;
            buttonText3.text = messageInfo.button3Text;

        }
        public void Close()
        {
            eventCallback.RemoveAllListeners();
            WindowManager.Instance.CloseMessageBox();
        }
    }

    [Serializable]
    public class MessageBoxEvent : UnityEvent<int> { } 
}