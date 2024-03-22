using UnityEngine;
using DG.Tweening;

namespace MyCodeExample.Game.UI
{
    public class LoadingIndicator : MonoBehaviour
    {
        [Header(InspectorConstants.HEADER_PREFAB_CONFIGURATION_PLAYMODE_IGNORED)]
        [SerializeField] private float _spinningTweenTime = 1.5F;
        
        private RectTransform _transform;
        private Tween _spinningTween;

        private void Awake()
        {
            _transform = GetComponent<RectTransform>();
           
            _spinningTween = _transform.DORotate(new Vector3(0, 0, 360), _spinningTweenTime, RotateMode.FastBeyond360)
                .SetRelative(true).SetLoops(-1).SetEase(Ease.Linear).Pause();
        }

        private void OnEnable()
        {
            _transform.rotation = Quaternion.Euler(Vector3.zero);
            _spinningTween.Restart();
        }

        private void OnDisable()
        {
            _spinningTween.Pause();
        }

        private void OnDestroy()
        {
            _spinningTween.Kill();
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}