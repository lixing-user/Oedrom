using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public enum ShopSection { Buy, Sell, Repair }

public class ShopCameraControler : MonoBehaviour
{
    [Header("SceneCamera")]
    [SerializeField] private CinemachineCamera _buyCamera;
    [SerializeField] private CinemachineCamera _sellCamera;
    [SerializeField] private CinemachineCamera _repairCamera;

    private int _highPriority = 20;
    private int _lowPriority = 10;

    private void Start()
    {
        _buyCamera.Priority = _highPriority;
        _sellCamera.Priority = _lowPriority;
        _repairCamera.Priority = _lowPriority;
    }

    public void MoveToSection(ShopSection section)
    {
        switch (section)
        {
            case ShopSection.Buy: 
                HandleBuyPanel(); 
                break;
            case ShopSection.Sell:
                HandleSellPanel(); 
                break;
            case ShopSection.Repair:
                HandleRepairPanel();
                break;
        }
    }

    private void HandleBuyPanel()
    {
        _buyCamera.Priority = _highPriority;
        _sellCamera.Priority = _lowPriority;
        _repairCamera.Priority = _lowPriority;
    }
    private void HandleSellPanel() 
    {
        _buyCamera.Priority = _lowPriority;
        _sellCamera.Priority = _highPriority;
        _repairCamera.Priority = _lowPriority;
    }
    private void HandleRepairPanel()
    {
        _buyCamera.Priority = _lowPriority;
        _sellCamera.Priority = _lowPriority;
        _repairCamera.Priority = _highPriority;
    }
}
