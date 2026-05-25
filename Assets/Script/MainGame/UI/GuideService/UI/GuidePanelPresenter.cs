using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


public class GuidePanelPresenter : IInitializable, IDisposable
{
    [Inject] private GuidePanelView _view;
    //private List<GuideChapter> _guideData;

    [Inject] private IGuideDataService _dataService;
    [Inject] private GuideRuntimeModel _model;

    private int _currentChapterIndex = 0;
    private int _currentPageIndex = 0;


    public void Initialize()
    {
        _dataService.LoadAllGuides();

        _view.OnTabClicked += HandleTabClicked;
        _view.OnPrevClicked += HandlePrevClicked;
        _view.OnNextClicked += HandleNextClicked;
        _view.OnBackRequested += CloseGuide;

        _view.Hide(); // 預設隱藏
    }

    public void Dispose()
    {
        _view.OnTabClicked -= HandleTabClicked;
        _view.OnPrevClicked -= HandlePrevClicked;
        _view.OnNextClicked -= HandleNextClicked;
        _view.OnBackRequested -= CloseGuide;
    }


    public void OpenGuide(int defaultChapter = 0)
    {
        _model.ResetState();
        _model.CurrentChapterIndex = defaultChapter;

        var tabDatas = _dataService.GetTabDisplayDatas();
        _view.InitializeTabs(tabDatas);

        _view.Show();
        RefreshView();
    }

    public void CloseGuide() 
    {
        _view.Hide();
    }


    private void HandleTabClicked(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= _dataService.GetChapterCount()) return;

        _model.CurrentChapterIndex = chapterIndex;
        _model.CurrentPageIndex = 0; // 切換章節時，重置回第一頁

        RefreshView();
    }

    private void HandlePrevClicked()
    {
        if (_model.CurrentPageIndex > 0)
        {
            _model.CurrentPageIndex--;
            RefreshView();
        }
    }

    private void HandleNextClicked()
    {
        var currentChapter = _dataService.GetChapter(_model.CurrentChapterIndex);
        if (_model.CurrentPageIndex < currentChapter.pages.Count - 1)
        {
            _model.CurrentPageIndex++;
            RefreshView();
        }
    }

    private void RefreshView()
    {
        var chapter = _dataService.GetChapter(_model.CurrentChapterIndex);
        var page = chapter.pages[_model.CurrentPageIndex];

        _view.HighlightTab(_model.CurrentChapterIndex);
        _view.UpdatePageContent(page, _model.CurrentPageIndex, chapter.pages.Count);
    }
}
