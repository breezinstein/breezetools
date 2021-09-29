using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Breezinstein.Tools.UI
{
    public class ImageFilledOverTime : MonoBehaviour
    {
        [SerializeField]
        Image filledImage;

        void Update()
        {
            filledImage.fillAmount = Mathf.Repeat(Time.unscaledTime, 1f);
        }
    }
}