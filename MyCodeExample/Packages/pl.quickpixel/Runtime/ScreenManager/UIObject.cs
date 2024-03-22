using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using QuickPixel.Collections;
using QuickPixel.Collections.Unity;

namespace QuickPixel.ScreenManager
{
    /// <summary>
    ///     Simplified version of UIObject visible on scene
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster))]
    public abstract class UIObject : MonoBehaviour
    {
        [field: SerializeField] [field: ReadOnlyInInspector]
        internal UIObjectState UIObjectState;

        public QuickPixel.ScreenManager.ScreenManager ScreenManager { get; private set; }
        public UIComponents UIComponents { get; private set; }

        public bool IsTransitioning
            => UIObjectState.HasAnyFlag(UIObjectState.TransitioningIn | UIObjectState.TransitioningOut);

        public void Show()
        {
            ScreenManager.Show(this);
        }

        public void Hide()
        {
            ScreenManager.Hide(this);
        }

        public void DestroyScreen()
        {
            ScreenManager.DestroyUIObject(this);
        }

        internal void Setup(QuickPixel.ScreenManager.ScreenManager screenManager)
        {
            ScreenManager = screenManager;
            UIComponents = new UIComponents(this);

            SetVisibility(false);
        }

        protected internal virtual void Init()
        {
        }

        protected virtual IEnumerator OnAnimationIn()
        {
            UIComponents.CanvasGroup.alpha = 1f;
            yield break;
        }

        protected virtual IEnumerator OnAnimationOut()
        {
            UIComponents.CanvasGroup.alpha = 0f;
            yield break;
        }

        internal IEnumerator OnAnimationInInner()
        {
            SetTransitioning(UIObjectState.TransitioningIn);
            yield return OnAnimationIn();
            SetTransitioning(UIObjectState.TransitioningFinished);

            //MK Note: Simplified UI Manager is not maintaining focus, so we have to activate object manually
            SetTransitioning(UIObjectState.Interactable);
        }

        internal IEnumerator OnAnimationOutInner()
        {
            SetTransitioning(UIObjectState.TransitioningOut);
            yield return OnAnimationOut();
            SetTransitioning();
        }

        protected internal void SetTransitioning(UIObjectState isTransitioning = 0)
        {
            switch (isTransitioning)
            {
                case UIObjectState.TransitioningIn:
                    UIObjectState = UIObjectState.TransitioningIn | UIObjectState.Visible;
                    SetVisibility(true);
                    break;
                case UIObjectState.TransitioningOut:
                    UIObjectState = UIObjectState.TransitioningOut | UIObjectState.Visible;
                    UIComponents.CanvasGroup.blocksRaycasts = false;
                    break;
                case UIObjectState.Interactable:
                {
                    UIObjectState.RemoveFlag(UIObjectState.NonInteractable);
                    UIObjectState.AddFlag(UIObjectState.Interactable);
                    UIComponents.CanvasGroup.blocksRaycasts = true;
                    break;
                }
                case UIObjectState.NonInteractable:
                    UIObjectState.AddFlag(UIObjectState.NonInteractable);
                    UIObjectState.RemoveFlag(UIObjectState.Interactable);
                    UIComponents.CanvasGroup.blocksRaycasts = false;
                    EventSystem.current.SetSelectedGameObject(null);
                    break;
                case UIObjectState.TransitioningFinished:
                    UIObjectState.RemoveFlag(UIObjectState.TransitioningOut | UIObjectState.TransitioningIn);
                    break;
                default:
                    SetVisibility(false);
                    UIObjectState = 0;
                    break;
            }
        }

        protected internal void SetVisibility(bool isVisible)
        {
            if (isVisible)
            {
                UIComponents.Canvas.enabled = true;
                UIComponents.Canvas.overrideSorting = true;
                UIComponents.Canvas.sortingOrder = 0;
            }
            else
            {
                UIComponents.Canvas.enabled = false;
                UIComponents.CanvasGroup.blocksRaycasts = false;
                UIComponents.Canvas.sortingOrder = int.MinValue;
            }
        }
    }

    [Flags]
    public enum UIObjectState
    {
        Visible = 1 << 0,
        Interactable = 1 << 1,
        TransitioningIn = 1 << 2,
        TransitioningOut = 1 << 3,
        TransitioningFinished = 1 << 4,


        NonInteractable = 1 << 8
    }
}