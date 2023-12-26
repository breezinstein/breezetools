using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Breeze's Tools/Helpers/Set Image Color")]
public class ChangeImageColor : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Color color;
    public void SetImageColor()
    {
        if (image != null)
        {
            image.color = color;
        }
    }
}
