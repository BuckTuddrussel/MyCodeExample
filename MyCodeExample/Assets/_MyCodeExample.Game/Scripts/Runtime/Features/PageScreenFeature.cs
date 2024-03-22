using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MyCodeExample.Managers;
using MyCodeExample.Collections.SystemManager;
using MyCodeExample.Threading;
using MyCodeExample.Game.Events;
using MyCodeExample.Game.Services;
using MyCodeExample.Game.UI.Screens;
using UnityEngine;

namespace MyCodeExample.Game.Features
{
    public sealed class PageScreenFeature : Feature, IFeatureInitializable, IFeatureEnableable, IFeatureUpdateable
    {
        // This should be moved to Config - remote one or local 
        public const int DISPLAY_ENTRIES_COUNT = 5;

        private readonly List<Page> _pages = new List<Page>();

        private SafeCancellationTokenSource _pageSetupCTS;
        private SafeCancellationTokenSource _switchPageCTS;

        private PageDataService _pageDataService;
        private UIScreenManager _screenManager;
        private PageScreen _pageScreen;
        private EventBus _eventBus;

        private int _totalEntries;
        private int _currentPageIndex;

        bool IFeatureEnableable.FeatureAutoEnable => true;

        void IFeatureInitializable.Initialize()
        {
            SystemManager.TryGetService(out _pageDataService);
            SystemManager.TryGetManager(out _screenManager);

            _eventBus = SystemManager.EventBus;
            _pageScreen = _screenManager.Get<PageScreen>();
        }

        void IFeatureEnableable.OnFeatureEnable()
        {
            var screenControlsData = _pageScreen.GetScreenControls();
            screenControlsData.PreviousPageButton.OnClick += OnPreviousPageButtonClicked;
            screenControlsData.NextPageButton.OnClick += OnNextPageButtonClicked;

            SetupAndLoadFirstPage().Forget();
        }

        void IFeatureEnableable.OnFeatureDisable()
        {
            _pageSetupCTS.TryCancelAndDispose();
            _switchPageCTS.TryCancelAndDispose();
            _pageDataService.InvalidateCache();
            _currentPageIndex = 0;
            _totalEntries = 0;

            var screenControlsData = _pageScreen.GetScreenControls();
            screenControlsData.PreviousPageButton.OnClick -= OnPreviousPageButtonClicked;
            screenControlsData.NextPageButton.OnClick -= OnNextPageButtonClicked;
        }

        void IFeatureUpdateable.Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
            {
                OnPreviousPageButtonClicked();
                _pageScreen.GetScreenControls().PreviousPageButton.PlayOnClickAnimation();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
            {
                OnNextPageButtonClicked();
                _pageScreen.GetScreenControls().NextPageButton.PlayOnClickAnimation();
            }
        }

        private int GetLastPageIndex()
        {
            UnityEngine.Debug.Assert(_pages.Count > 0, "The page list is empty.");
            return _pages.Count - 1;
        }

        private void OnPreviousPageButtonClicked()
        {
            if (_currentPageIndex > 0)
            {
                _currentPageIndex--;
                GoToPage(_currentPageIndex).Forget();
            }
        }

        private void OnNextPageButtonClicked()
        {
            if (_currentPageIndex < GetLastPageIndex())
            {
                _currentPageIndex++;
                GoToPage(_currentPageIndex).Forget();
            }
        }

        private void HandlePreviousSwitchPageRequest()
        {
            _switchPageCTS.TryCancelAndDispose();
            _switchPageCTS = new SafeCancellationTokenSource();
        }

        private async Task GoToPage(int pageIndex)
        {
            UpdateScreenControls();
            HandlePreviousSwitchPageRequest();

            var page = _pages[pageIndex];
            if (_pageDataService.TryGetPageData(page.StartIndex, page.EntriesCount, out var result))
            {
                _pageScreen.PreformFullPageTransition(result);
            }
            else
            {
                _pageScreen.InitiatePageTransition();
                var requestPageData = _pageDataService.TryRequestPageData(page.StartIndex, page.EntriesCount,
                    _switchPageCTS);
                result = await requestPageData.MonitorIgnoreCancellation();

                if (requestPageData.IsCompletedSuccessfully)
                {
                    _pageScreen.FinalizePageTransition(result);
                }
                else if (!requestPageData.IsCanceled)
                {
                    _eventBus.Publish(new SwitchPageContentErrorEvent());
                }
            }

            _switchPageCTS.Dispose();
        }

        private async Task SetupAndLoadFirstPage()
        {
            _pageSetupCTS = new SafeCancellationTokenSource();

            var requestDataTask = _pageDataService.RequestDataAvailableCount(_pageSetupCTS);
            var count = await requestDataTask.MonitorIgnoreCancellation();

            if (requestDataTask.IsCompletedSuccessfully)
            {
                var lastPageEntries = count % DISPLAY_ENTRIES_COUNT;
                var totalPages = count / DISPLAY_ENTRIES_COUNT;
                _totalEntries = count;

                var entryIndex = 0;
                for (var pageIndex = 0; pageIndex < totalPages; pageIndex++)
                {
                    _pages.Add(new Page(entryIndex, DISPLAY_ENTRIES_COUNT));
                    entryIndex += DISPLAY_ENTRIES_COUNT;
                }

                if (lastPageEntries > 0)
                {
                    var lastPageStartIndex = _totalEntries - lastPageEntries;
                    _pages.Add(new Page(lastPageStartIndex, lastPageEntries));
                }

                _pageScreen.Setup(DISPLAY_ENTRIES_COUNT);


                var goToPageTask = await GoToPage(0).Monitor();
                if (goToPageTask.IsCompletedSuccessfully)
                    _eventBus.Publish(new InitialPageContentReadyEvent());
                else
                    _eventBus.Publish(new SwitchPageContentErrorEvent());
            }
            else
            {
                _eventBus.Publish(new SetupPageContentErrorEvent());
            }

            _pageSetupCTS.Dispose();
        }

        private void UpdateScreenControls()
        {
            _pageScreen.UpdateScreenControls(new PageScreen.UpdateControlsData()
            {
                PageNavigationState = DeterminePageState()
            });
        }

        private PageScreen.PageNavigationState DeterminePageState()
        {
            PageScreen.PageNavigationState pageNavigationState;
            if (_pages.Count == 0)
            {
                pageNavigationState = PageScreen.PageNavigationState.None;
            }
            else if (_currentPageIndex == 0)
            {
                pageNavigationState = PageScreen.PageNavigationState.First;
            }
            else if (_currentPageIndex >= GetLastPageIndex())
            {
                pageNavigationState = PageScreen.PageNavigationState.Last;
            }
            else
            {
                pageNavigationState = PageScreen.PageNavigationState.Middle;
            }

            return pageNavigationState;
        }

        void IDisposable.Dispose()
        {
            Disable();
        }

        private readonly struct Page
        {
            public readonly int StartIndex;
            public readonly int EntriesCount;

            public Page(int startIndex, int entriesCount)
            {
                StartIndex = startIndex;
                EntriesCount = entriesCount;
            }
        }
    }
}