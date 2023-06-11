using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Breezinstein.Tools
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class FPSCounter : MonoBehaviour
    {
        const float fpsMeasurePeriod = 0.5f;
        private int m_FpsAccumulator = 0;
        private float m_FpsNextPeriod = 0;
        private float m_MinFps;
        private float m_MaxFps;
        private int m_CurrentFps;
        const string display = "{0} FPS";
        private TextMeshProUGUI m_Text;
        private float currentTime;
        private readonly StringBuilder m_StringBuilder = new StringBuilder(16);

        private void Start()
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
            m_Text = GetComponent<TextMeshProUGUI>();
        }


        private void Update()
        {
            // measure average frames per second
            m_FpsAccumulator++;
            currentTime = Time.realtimeSinceStartup;
            if (currentTime > m_FpsNextPeriod)
            {
                m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
                m_FpsAccumulator = 0;
                m_FpsNextPeriod = currentTime + fpsMeasurePeriod;

                if (m_CurrentFps < m_MinFps)
                {
                    m_MinFps = m_CurrentFps;
                }
                if (m_CurrentFps > m_MaxFps)
                {
                    m_MaxFps = m_CurrentFps;
                }

                //display the min, current and max fps on different lines in the same string
                m_StringBuilder.Clear();
                m_StringBuilder.AppendLine(string.Format(display, m_CurrentFps));
                m_StringBuilder.AppendLine(string.Format(display, m_MinFps));
                m_StringBuilder.AppendLine(string.Format(display, m_MaxFps));
                m_Text.text = m_StringBuilder.ToString();

            }
        }
    }
}
