using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace MyCodeExample.Game.UI.Screens
{
    public class ItemEntry : MonoBehaviour
    {
        [Header(InspectorConstants.HEADER_PREFAB_REFERENCES)] 
        [SerializeField] private LayoutElement _layoutElement;
        [SerializeField] private TextMeshProUGUI _numberTMP;
        [SerializeField] private TextMeshProUGUI _textTMP;
        [SerializeField] private Image _badgeImage;
        [SerializeField] private Image _glowImage;

        [Header(InspectorConstants.HEADER_PREFAB_CONFIGURATION_PLAYMODE_IGNORED)] 
        [SerializeField] private Color _glowColor = new Color(1f, 0.9686275f, 0.7411765f, 1);
        [Header("Glow Intro")] 
        [SerializeField] private float _glowIntroTweenTime = 1.35f;
        [SerializeField] private float _glowIntroToLoopDelayTime = 0.15f;
        [SerializeField] private Ease _glowIntroEasing = Ease.Linear;
        [Header("Glow Loop")] 
        [SerializeField] private float _glowMinAlpha = 0.25f;
        [SerializeField] private float _glowLoopTweenTime = 1.25f;
        [SerializeField] private Ease _glowLoopEasing = Ease.InCubic;

        private Sequence _glowIntroSequence;
        private Tween _glowLoopTween;
        private Color _transparentGlowColor;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _transparentGlowColor = new Color(_glowColor.r, _glowColor.g, _glowColor.b, 0);

            _glowLoopTween = _glowImage.DOFade(_glowMinAlpha, _glowLoopTweenTime)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(false)
                .SetEase(_glowLoopEasing);

            _glowIntroSequence = DOTween.Sequence(this)
                .SetDelay(_glowIntroTweenTime)
                .Join
                (
                    _glowImage.DOFade(1, _glowLoopTweenTime)
                        .SetEase(_glowIntroEasing)
                        .SetDelay(_glowIntroToLoopDelayTime)
                )
                .AppendCallback(() => { _glowLoopTween.Restart(); })
                .SetAutoKill(false)
                .SetEase(_glowIntroEasing);

            Disable();
        }

        // Note: Use IN to prevent defensive copy and pass non-alloc struct as reference in readonly mode
        public void Setup(in Data data)
        {
            _glowImage.color = _transparentGlowColor;
            _numberTMP.text = data.DisplayIndex;
            _textTMP.text = data.Description;
            _badgeImage.sprite = data.Sprite;

            if (data.Special)
            {
                EnableGlow();
            }
            else
            {
                DisableGlow();
            }

            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
            DisableGlow();
        }

        private void DisableGlow()
        {
            _glowImage.enabled = false;
            _glowIntroSequence.Pause();
            _glowLoopTween.Pause();
        }

        private void EnableGlow()
        {
            _glowImage.enabled = true;
            _glowIntroSequence.Restart();
        }
        
        public readonly struct Data
        {
            public readonly Sprite Sprite;
            public readonly string DisplayIndex;
            public readonly string Description;
            public readonly bool Special;
        
            public Data(int displayIndex, Sprite sprite, DataItem dataItem)
            {
                DisplayIndex = displayIndex.ToString();
                Sprite = sprite;
                Description = dataItem.Description;
                Special = dataItem.Special;
            }
        }
    }
}