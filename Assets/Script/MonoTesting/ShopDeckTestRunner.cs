using UnityEngine;
using Zenject;

public class ShopDeckTestRunner : MonoBehaviour
{
    [Inject] private SignalBus _signalBus;
    void Start()
    {
        //_signalBus.Fire<BoardInitializedSignal>();
    }
}
