using System.Collections;
using UnityEngine;
using Zenject;

public class ShopFacade : MonoBehaviour
{
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly ShopApplicationService _shopService;
    [Inject] private readonly ShopScenePresenter _shopPresenter;
    [Inject] private readonly ShopHandUIManager _handUIManager;

 
    void Start()
    {
        StartCoroutine(InitializeShopAsync());
    }


    private IEnumerator InitializeShopAsync()
    {
        Debug.Log("[ShopBootstrapper] 開始初始化商店場景...");

        // 1. 領域邏輯就緒 (如果你沒有用 IInitializable，可以在這裡手動呼叫)
        _shopService.Initialize(); 

        // 2. 視覺與 UI 實體化 (等同於棋盤的 SpawnAllPieces)
        _shopPresenter.InitializeView();
        _handUIManager.InitializeView();

        // 3. 【關鍵防禦】等待一幀，讓 Unity 處理完所有生成的 3D 物件、UI 佈局與 Collider
        // 這等同於你棋盤中等待 MeshCollider 生成的邏輯
        yield return new WaitForEndOfFrame();

        Debug.Log("[ShopBootstrapper] 商店場景初始化完成，發送就緒訊號");

        // 4. 發射解鎖訊號
        _signalBus.Fire<BoardInitializedSignal>();
        _signalBus.Fire(new SceneUIReadySignal());
        _signalBus.Fire<ShopInitializedSignal>();
    }
}

public class ShopInitializedSignal { }
