//TODO: Convert to selectable :)

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using QuickPixel;

namespace MyCodeExample.Game.UI
{
    [RequireComponent(typeof(Image))]
    public sealed class GenericButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler,
        IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [field: Header(InspectorConstants.HEADER_PREFAB_REFERENCES)]
        [field: SerializeField] public Image Image { get; private set; }
        [field: SerializeField] public TextMeshProUGUI Label { get; private set; }
        [Space]
        [Header(InspectorConstants.HEADER_PREFAB_CONFIGURATION_PLAYMODE_IGNORED)]
        [SerializeField] private float _bumpDownTweenTime = 0.15F;
        [SerializeField] private Vector3 _bumpDownScale = new Vector3(0.95f, 0.95f, 1f);
        [SerializeField] private float _bumpUpTweenTime = 0.15F;
        [SerializeField] private Vector3 _bumpUpScale = new Vector3(1.05f, 1.05f, 1f);
        [Space]
        [Header(InspectorConstants.HEADER_ASSETS_REFERENCES)]
        [SerializeField] private Sprite _btnDisabled;
        [SerializeField] private Sprite _btnNormal;
        [SerializeField] private Sprite _btnPressed;

        private EventSystem _eventSystem;
        private Sequence _bumpSequence;
        private RectTransform _buttonTransform;
        private ButtonState _buttonState;

        public event Action OnClick;
        
        private void Awake()
        {
            _buttonTransform = (RectTransform)transform;
            _eventSystem = EventSystem.current;
            
            _bumpSequence = DOTween.Sequence(_buttonTransform)               
                .Join(_buttonTransform.DOScale(_bumpDownScale, _bumpDownTweenTime))
                .Append(_buttonTransform.DOScale(Vector3.one, _bumpDownTweenTime))
                .AppendCallback(() =>
                {
                     _buttonState.RemoveFlag(ButtonState.Animating);
                    Image.sprite = _buttonState.HasFlagFast(ButtonState.Interactable) ? _btnNormal : _btnDisabled;
                    _eventSystem.SetSelectedGameObject(null);
                    
                    if (_buttonState.HasFlagFast(ButtonState.Interactable | ButtonState.PointerOverObject))
                    {
                        _buttonTransform.DOScale(_bumpUpScale, _bumpUpTweenTime);
                    }
                })
                .SetAutoKill(false)
                .SetEase(Ease.Linear)
                .Pause();
        }

        public void SetInteractable(bool isInteractable)
        {
            if (isInteractable)
            {
                _buttonState.AddFlag(ButtonState.Interactable);
            }
            else
            {
                _buttonState.RemoveFlag(ButtonState.Interactable);
            }

            Image.sprite = _buttonState.HasFlagFast(ButtonState.Interactable) ? _btnNormal : _btnDisabled;
        }

        public void PlayOnClickAnimation()
        {
            _buttonState.AddFlag(ButtonState.Animating);
            _buttonTransform.DOComplete();
            _bumpSequence.Restart();
        }
        
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!_buttonState.HasFlagFast(ButtonState.Interactable)) return;

            _buttonState.AddFlag(ButtonState.Animating);
            
            _buttonTransform.DOComplete();
            _bumpSequence.Restart();
            Image.sprite = _btnPressed;
            
            OnClick?.Invoke();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _buttonState.AddFlag(ButtonState.PointerOverObject);
            
            if(!_buttonState.HasFlagFast(ButtonState.Interactable) || _buttonState.HasFlagFast(ButtonState.Animating)) return;
            _buttonTransform.DOScale(_bumpUpScale, _bumpUpTweenTime);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _buttonState.RemoveFlag(ButtonState.PointerOverObject);
            
            if(!_buttonState.HasFlagFast(ButtonState.Interactable) || _buttonState.HasFlagFast(ButtonState.Animating)) return;
            _buttonTransform.DOScale(Vector3.one, _bumpDownTweenTime);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            _buttonState.RemoveFlag(ButtonState.PointerHeldObject);
            
            if(!_buttonState.HasFlagFast(ButtonState.Interactable) || _buttonState.HasFlagFast(ButtonState.Animating)) return;
            Image.sprite = _btnNormal;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            _buttonState.AddFlag(ButtonState.PointerHeldObject);
            
            if(!_buttonState.HasFlagFast(ButtonState.Interactable) || _buttonState.HasFlagFast(ButtonState.Animating)) return;
            Image.sprite = _btnPressed;
        }
        
        [Flags]
        private enum ButtonState : ushort
        {
            None = 0,
            Interactable = 1  << 0,
            Animating = 1  << 1,
            PointerOverObject = 1  << 2,
            PointerHeldObject = 1  << 3,
            All = ushort.MaxValue
        }
    }
}