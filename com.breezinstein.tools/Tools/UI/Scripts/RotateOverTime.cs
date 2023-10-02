using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breezinstein.Tools.UI
{
    // A script that rotates an object over time in the x, y, and z directions
    public class RotateOverTime : MonoBehaviour
    {
        [SerializeField] bool x; // Should the object rotate around the x-axis?
        [SerializeField] bool y; // Should the object rotate around the y-axis?
        [SerializeField] bool z = true; // Should the object rotate around the z-axis?
        [SerializeField] float speedMultiplier = 1f; // Multiplier for the rotation speed

        private float xRot; // Current rotation around x-axis
        private float yRot; // Current rotation around y-axis
        private float zRot; // Current rotation around z-axis
        private float speed = 100f; // Base speed of rotation

        private void OnEnable()
        {
            // Reset rotation amounts
            xRot = 0f;
            yRot = 0f;
            zRot = 0f;
        }

        // Update is called once per frame
        void Update()
        {
            // If none of the rotation axes are checked, do nothing
            if (!x && !y && !z) return;

            // Calculate rotation amounts for each axis
            Vector3 rot = Vector3.zero;
            if (x)
            {
                xRot += -Time.unscaledDeltaTime * speed * speedMultiplier;
                rot.x = Mathf.Repeat(xRot, 360f);
            }
            if (y)
            {
                yRot += -Time.unscaledDeltaTime * speed * speedMultiplier;
                rot.y = Mathf.Repeat(yRot, 360f);
            }
            if (z)
            {
                zRot += -Time.unscaledDeltaTime * speed * speedMultiplier;
                rot.z = Mathf.Repeat(zRot, 360f);
            }

            // Apply the rotation to the object
            transform.rotation = Quaternion.Euler(rot);
        }
    }
}