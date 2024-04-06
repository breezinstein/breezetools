using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Breezinstein.Tools
{
    /// <summary>
    /// FPSCounter is a MonoBehaviour that measures and displays the current, minimum, and maximum FPS.
    /// </summary>
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
        private float[] previousFPS = new float[16];
        private int previousFPSIndex = 0;

        /// <summary>
        /// Start is called before the first frame update.
        /// It initializes the FPS measurement period and gets the TextMeshProUGUI component.
        /// </summary>
        private void Start()
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
            m_Text = GetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// Update is called once per frame.
        /// It measures the average frames per second and updates the FPS display.
        /// </summary>
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

                previousFPS[previousFPSIndex] = m_CurrentFps;
                previousFPSIndex++;
                // loop index if above 16
                if (previousFPSIndex >= 16)
                {
                    previousFPSIndex = 0;
                }

                // calculate min and max fps
                m_MinFps = previousFPS[0];
                m_MaxFps = previousFPS[0];
                for (int i = 1; i < 16; i++)
                {
                    if (previousFPS[i] < m_MinFps)
                    {
                        m_MinFps = previousFPS[i];
                    }
                    if (previousFPS[i] > m_MaxFps)
                    {
                        m_MaxFps = previousFPS[i];
                    }
                }

                //display the min, current and max fps on different lines in the same string
                m_StringBuilder.Clear();
                m_StringBuilder.AppendLine(string.Format(display + " CUR", m_CurrentFps));
                m_StringBuilder.AppendLine(string.Format(display + " MIN", m_MinFps));
                m_StringBuilder.AppendLine(string.Format(display + " MAX", m_MaxFps));
                m_Text.text = m_StringBuilder.ToString();

            }
        }
    }
}
