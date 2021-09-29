using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breezinstein.Tools.UI
{
    public class RotateOverTime : MonoBehaviour
    {
        [SerializeField] bool x;
        [SerializeField] bool y;
        [SerializeField] bool z = true;
        [SerializeField] float speedMultiplier = 1f;

        void Update()
        {
            Vector3 rot = Vector3.zero;
            if (x)
            {
                rot.x = -Mathf.Repeat(Time.unscaledTime * 100f, 360f) * speedMultiplier;
                //transform.rotation = Quaternion.Euler ( -Mathf.Repeat (Time.unscaledTime * 100f, 360f),0f,0f);
            }
            if (y)
            {
                rot.y = -Mathf.Repeat(Time.unscaledTime * 100f, 360f) * speedMultiplier;
                //transform.rotation = Quaternion.Euler (0f,  -Mathf.Repeat (Time.unscaledTime * 100f, 360f),0f);
            }
            if (z)
            {
                rot.z = -Mathf.Repeat(Time.unscaledTime * 100f, 360f) * speedMultiplier;
                //transform.rotation = Quaternion.Euler (0f, 0f, -Mathf.Repeat (Time.unscaledTime * 100f, 360f));
            }
            transform.rotation = Quaternion.Euler(rot);
        }
    }
}