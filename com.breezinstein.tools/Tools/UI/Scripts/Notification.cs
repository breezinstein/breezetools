using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Breezinstein.Tools.UI
{
    public class Notification : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private Image filledImage;
        [SerializeField] private Button closeButton;
        private float timeSinceEnabled = 0f;
        public UnityEvent notificationCallBack;
        public NotificationItem message;
        void OnEnable()
        {
            transform.SetAsLastSibling();
            StartCoroutine(ShowNotification(message));
            timeSinceEnabled = 0f;
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        private IEnumerator ShowNotification(NotificationItem _message)
        {
            notificationText.text = _message.message;
            yield return new WaitForSeconds(_message.duration);
            if (notificationCallBack != null)
            {
                notificationCallBack.Invoke();
                Close();
            }
        }
        void Update()
        {
            if (filledImage != null)
            {
                timeSinceEnabled += Time.deltaTime;
                filledImage.fillAmount = (message.duration - timeSinceEnabled) / message.duration;
            }
        }
        public void Close()
        {
            notificationCallBack.RemoveAllListeners();
            gameObject.SetActive(false);
        }

    }
}