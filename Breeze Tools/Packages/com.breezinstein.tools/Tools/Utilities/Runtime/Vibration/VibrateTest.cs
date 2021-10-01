using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Breezinstein.Tools
{
    public class VibrateTest : MonoBehaviour
    {
        [SerializeField]
        private Toggle toggle;
        [SerializeField]
        private Button vibratePop;
        [SerializeField]
        private Button vibratePeek;
        [SerializeField]
        private Button vibrateNope;
        [SerializeField]
        private Button vibrate;
        [SerializeField]
        private TMP_InputField vibrateDuration;

        private void Awake()
        {
            VibrationManager.Init();
        }
        private void OnEnable()
        {
            toggle.isOn = VibrationManager.VibrateEnabled;
            toggle.onValueChanged.AddListener(VibrationManager.ToggleVibration);
            vibratePop.onClick.AddListener(VibrationManager.VibratePop);
            vibratePeek.onClick.AddListener(VibrationManager.VibratePeek);
            vibrateNope.onClick.AddListener(VibrationManager.VibrateNope);
            vibrate.onClick.AddListener(() => VibrationManager.Vibrate(long.Parse(vibrateDuration.text)));
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveAllListeners();
            vibratePop.onClick.RemoveAllListeners();
            vibratePeek.onClick.RemoveAllListeners();
            vibrateNope.onClick.RemoveAllListeners();
            vibrate.onClick.RemoveAllListeners();
        }
    }



}
