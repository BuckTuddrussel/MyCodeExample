using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using QuickPixel.ScreenManager;

namespace MyCodeExample.Game.UI.Screens
{
    public sealed class StartupScreen : UIObject
    {
        [Header(InspectorConstants.HEADER_SCENE_REFERENCES)]
        [SerializeField] private Image _backgroundImage;

        [Header(InspectorConstants.HEADER_PREFAB_CONFIGURATION)]
        [SerializeField] private float _backgroundAlphaTweenTime = 1f;
        [SerializeField] private float _backgroundMaxAlpha = 0.5f;
        
        protected override void Init()
        {
            _backgroundImage.SetColorAlpha(0);
        }

        protected override IEnumerator OnAnimationOut()
        {
            yield return _backgroundImage.DOFade(_backgroundMaxAlpha, _backgroundAlphaTweenTime);
        }
    }
}