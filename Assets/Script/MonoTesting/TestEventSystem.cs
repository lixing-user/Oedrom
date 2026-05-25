using UnityEngine;
using UnityEngine.EventSystems;

public class TestEventSystem : MonoBehaviour
{
    void Awake()
    {
        // 只有在 Unity 編輯器底下，且是直接按下 Play 單獨測試該場景時才保留它
        // 如果是正式打包(Build)的遊戲，或是從其他場景載入過來的，就直接把它砍了
#if !UNITY_EDITOR
        Destroy(gameObject);
#else
        // 簡單判斷：如果場景裡已經有別人了，我就自殺 (跟前面教的邏輯一樣)
        if (FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
        }
#endif
    }
}
