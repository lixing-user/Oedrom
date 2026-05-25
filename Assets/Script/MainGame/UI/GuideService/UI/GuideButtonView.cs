using UnityEngine;
using Zenject;

public class GuideButtonView : MonoBehaviour
{
    [Inject] private GuidePanelPresenter _guidePresenter;
    private ButtonView _buttonView;

    private void Awake()
    {
        _buttonView = GetComponent<ButtonView>();
    }

    private void OnEnable()
    {
        _buttonView.OnClicked += HandleGuideClicked;
    }

    private void OnDisable()
    {
        _buttonView.OnClicked -= HandleGuideClicked;
    }

    private void HandleGuideClicked()
    {
        _guidePresenter.OpenGuide();
    }
}
