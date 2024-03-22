using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MyCodeExample.Game
{
    [RequireComponent(typeof(Image))]
    public sealed class MockupTester : MonoBehaviour
    {
        private Image _image;
        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.SetColorAlpha(0);
        }
        
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.F1))
            {
                MockupAlphaTransition(0f);
            }
            else if (Input.GetKeyUp(KeyCode.F2))
            {
                MockupAlphaTransition(.5f);
            }
            else if (Input.GetKeyUp(KeyCode.F3))
            {
                MockupAlphaTransition(.75f);
            }
            else if (Input.GetKeyUp(KeyCode.F4))
            {
                MockupAlphaTransition(1f);
            }
        }

        private void MockupAlphaTransition(float alpha)
        {
            _image.DOKill();
            _image.DOFade(alpha, 0.15f).SetEase(Ease.Linear);
        }
    }
}
