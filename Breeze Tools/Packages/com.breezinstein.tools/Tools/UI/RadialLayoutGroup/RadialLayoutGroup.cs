using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
namespace Breezinstein.Tools.UI
{
    public class RadialLayoutGroup : LayoutGroup//, ILayoutGroup
    {

        public enum RadialLayoutStart { top, left, right, bottom };
        public RadialLayoutStart StartFrom;

        public float Offset;

        public float Radius = 1.0f;
        public float Arc = 360.0f;

        public override void SetLayoutHorizontal()
        {
            UpdateChildren();
        }

        public override void SetLayoutVertical()
        {

            UpdateChildren();
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            UpdateChildren();
        }
        public override void CalculateLayoutInputVertical()
        {
            UpdateChildren();
        }

        void UpdateChildren()
        {

            int i = 0;
            float angleStep = Arc / transform.childCount;
            Vector3 direction;


            if (StartFrom == RadialLayoutStart.bottom) direction = -transform.up;
            else if (StartFrom == RadialLayoutStart.right) direction = transform.right;
            else if (StartFrom == RadialLayoutStart.left) direction = -transform.right;
            else direction = transform.up;

            foreach (RectTransform t in transform)
            {
                t.position = transform.position + Quaternion.Euler(0, 0, Offset + angleStep * i) * direction * Radius;
                i++;
            }

        }

    }
}