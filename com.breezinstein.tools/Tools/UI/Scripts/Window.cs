using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Breezinstein.Tools.UI
{
    public class Window : MonoBehaviour
    {
        public int ID = -99;
        public string windowName = "default";

        Animator animator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = GetComponent<Animator>();
                }
                return _animator;
            }
        }

        private Animator _animator;

        public RectTransform RTrans
        {
            get
            {
                if (_rTrans == null)
                {
                    _rTrans = GetComponent<RectTransform>();
                }
                return _rTrans;
            }
        }

        RectTransform _rTrans;
        // Use this for initialization
        void Start()
        {
            if (ID == -99)
            {
                Debug.LogError("Please Assign An ID");
            }
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                HandleBackButton();
            }
        }

        public void PlayAnimation(string clipName)
        {
            if(animator == null)
            {
                Debug.LogWarning("Animator Not Found");
                return;
            }
            animator.Play(clipName);
        }

        public void DisableGameObject()
        {
            gameObject.SetActive(false);
        }

        public void Close()
        {
            WindowManager.Instance.CloseWindow(ID);
        }

        public virtual void HandleBackButton()
        {
            WindowManager.Instance.OpenPreviousWindow();
        }
    }

}