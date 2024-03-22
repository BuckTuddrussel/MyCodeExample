using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using QuickPixel.ScreenManager;
using MyCodeExample.Game.Services;
using UnityEngine.Serialization;

namespace MyCodeExample.Game.UI.Screens
{
    public sealed class PageScreen : UIObject
    {
        [Header(InspectorConstants.HEADER_PREFAB_REFERENCES)]
        [SerializeField] private GameObject _pageContentRoot;
        [SerializeField] private CanvasGroup _pageContentCanvasGroup;
        [SerializeField] private GenericButton _previousPageButton;
        [SerializeField] private GenericButton _nextPageButton;
        [SerializeField] private LoadingIndicator _loadingIndicator;

        [Header(InspectorConstants.HEADER_PREFAB_CONFIGURATION)]
        [SerializeField] private float _screenFadeInTweenTime = 0.25F;
        [Space, Space]
        [SerializeField] private float _pageContentFadeInTweenDelayTime = 0.15F;
        [SerializeField] private float _pageContentFadeInTweenTime = 0.25F;
        [SerializeField] private Ease _pageContentFadeInEase = Ease.Linear;
        [Space]
        [SerializeField] private float _pageContentFadeOutTweenTime = 0.15F;
        [SerializeField] private Ease _pageContentFadeOutEase = Ease.Linear;

        [Header(InspectorConstants.HEADER_ASSETS_REFERENCES)]
        [SerializeField] private ItemEntry _itemEntryPrefab;
        [SerializeField] private PageScreenDataSO pageScreenDataSo;

        private readonly List<ItemEntry> _itemEntries = new List<ItemEntry>();
        private int _displayEntriesCount = 0;

        protected override void Init()
        {
            pageScreenDataSo.Init();
            _loadingIndicator.SetActive(false);
            SetPageControls(PageNavigationState.None);
        }

        protected override IEnumerator OnAnimationIn()
        {
            yield return UIComponents.CanvasGroup.DOFade(1f, _screenFadeInTweenTime);
        }

        // MK Note: Object pooling here is redundant
        public void Setup(int displayEntriesCount)
        {
            _displayEntriesCount = displayEntriesCount;

            foreach (var entry in _itemEntries)
            {
                Destroy(entry.gameObject);
            }

            for (int i = 0; i < displayEntriesCount; i++)
            {
                var itemEntry = Instantiate(_itemEntryPrefab, _pageContentRoot.transform);
                _itemEntries.Add(itemEntry);
            }
        }

        public void PreformFullPageTransition(IList<RequestPageDataResult> requestPageDataResult)
        {
            _pageContentCanvasGroup.DOKill();
            _pageContentCanvasGroup.DOFade(0f, _pageContentFadeOutTweenTime).SetEase(_pageContentFadeOutEase)
                .onComplete = () => { FinalizePageTransition(requestPageDataResult); };
        }

        public void InitiatePageTransition()
        {
            _pageContentCanvasGroup.DOKill();
            _pageContentCanvasGroup.DOFade(0f, _pageContentFadeOutTweenTime).SetEase(_pageContentFadeOutEase)
                .onComplete = () => { _loadingIndicator.SetActive(true); };
        }

        public void FinalizePageTransition(IList<RequestPageDataResult> requestPageDataResult)
        {
            _pageContentCanvasGroup.DOComplete();

            OnPageSetup(requestPageDataResult);

            _pageContentCanvasGroup.DOFade(1f, _pageContentFadeInTweenTime).SetDelay(_pageContentFadeInTweenDelayTime)
                .SetEase(_pageContentFadeInEase);
            _loadingIndicator.SetActive(false);
        }
        
        public void UpdateScreenControls(in UpdateControlsData updateControlsData)
        {
            SetPageControls(updateControlsData.PageNavigationState);
        }

        private void OnPageSetup(IList<RequestPageDataResult> requestPageDataResult)
        {
            var entriesCount = requestPageDataResult.Count;

            for (var i = 0; i < entriesCount; i++)
            {
                var entry = requestPageDataResult[i];
                var entryItem = _itemEntries[i];
                var dataItem = entry.DataItem;

                var itemEntryData = new ItemEntry.Data(entry.DisplayIndex,
                    pageScreenDataSo.GetCategorySprite(dataItem.Category),
                    dataItem);

                entryItem.Setup(itemEntryData);
            }

            for (var i = entriesCount; i < _displayEntriesCount; i++)
            {
                var entryItem = _itemEntries[i];
                entryItem.Disable();
            }
        }

        private void SetPageControls(PageNavigationState pageNavigationState)
        {
            if (pageNavigationState == PageNavigationState.First)
            {
                _previousPageButton.SetInteractable(false);
                _nextPageButton.SetInteractable(true);
            }
            else if (pageNavigationState == PageNavigationState.Middle)
            {
                _previousPageButton.SetInteractable(true);
                _nextPageButton.SetInteractable(true);
            }
            else if (pageNavigationState == PageNavigationState.Last)
            {
                _previousPageButton.SetInteractable(true);
                _nextPageButton.SetInteractable(false);
            }
            else
            {
                _previousPageButton.SetInteractable(false);
                _nextPageButton.SetInteractable(false);
            }
        }
        
        public PageControls GetScreenControls()
        {
            return new PageControls()
            {
                NextPageButton = _nextPageButton,
                PreviousPageButton = _previousPageButton
            };
        }
        
        public struct UpdateControlsData
        {
            public PageNavigationState PageNavigationState;
        }
        
        public struct PageControls
        {
            public GenericButton PreviousPageButton;
            public GenericButton NextPageButton;
        }

        public enum PageNavigationState
        {
            None,
            First,
            Middle,
            Last,
        }
    }
}