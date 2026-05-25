using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Systems.SceneManagement
{
    #region GameModeStateMachine
    public enum GameMode
    {
        Mode2D,
        Mode3D
    }
    public class GameModeStateMachine
    {
        private static GameModeStateMachine _instance;
        public static GameModeStateMachine Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameModeStateMachine();
                return _instance;
            }
        }
        private GameMode _currentMode = GameMode.Mode3D;

        public GameMode CurrentMode => _currentMode;

        public void TransitionTo(GameMode targetMode)
        {
            if (_currentMode == targetMode) return;
            _currentMode = targetMode;
        }
    }
    #endregion


    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance { get; private set; }

        [SerializeField] private LoadingOverlay loadingOverlay;

        public Dictionary<string, string> sceneSlots = new Dictionary<string, string>(); // ¼Ñ¦ì¦r¨å
        private Scene? currentActiveScene;
        private List<GameObject> activeSceneRootObjects = new List<GameObject>();
        private bool isBusy = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        //API
        public SceneTransitionPlan NewTransition()
        {
            return new SceneTransitionPlan();
        }
        private Coroutine ExecutePlan(SceneTransitionPlan plan)
        {
            if (isBusy) return null;
            isBusy = true;
            return StartCoroutine(ChangeSceneRoutine(plan));
        }

        public class SceneTransitionPlan
        {
            private GameMode? _targetGameMode;
            private List<(string slot, string sceneName, bool setActive)> scenesToLoad = new List<(string, string, bool)>();
            private List<string> slotsToUnload = new List<string>();
            private bool useOverlay = false;
            private bool cleanUnused = false;
            private string mainSceneToHide = null;
            private string mainSceneToShow = null;

            private Action _onCompleteCallback;

            public SceneTransitionPlan Load(string slot, string sceneName, bool setActive = false)
            {
                scenesToLoad.Add((slot, sceneName, setActive));
                return this;
            }

            public SceneTransitionPlan Unload(string slot)
            {
                slotsToUnload.Add(slot);
                return this;
            }

            public SceneTransitionPlan WithGameMode(GameMode mode)
            {
                _targetGameMode = mode;
                return this;
            }

            public SceneTransitionPlan WithOverlay()
            {
                useOverlay = true;
                return this;
            }

            public SceneTransitionPlan CleanUnusedAssets()
            {
                cleanUnused = true;
                return this;
            }

            public SceneTransitionPlan HideMainScene(string sceneName)
            {
                mainSceneToHide = sceneName;
                return this;
            }

            public SceneTransitionPlan ShowMainScene(string sceneName)
            {
                mainSceneToShow = sceneName;
                return this;
            }

            public SceneTransitionPlan OnComplete(Action callback)
            {
                _onCompleteCallback = callback;
                return this;
            }

            public Coroutine Perform()
            {
                if (_targetGameMode.HasValue)
                {
                    GameModeStateMachine.Instance.TransitionTo(_targetGameMode.Value);
                }
                return SceneController.Instance.ExecutePlan(this);
            }

            public List<(string slot, string sceneName, bool setActive)> ScenesToLoad => scenesToLoad;
            public List<string> SlotsToUnload => slotsToUnload;
            public bool UseOverlay => useOverlay;
            public bool CleanUnused => cleanUnused;
            public string MainSceneToHide => mainSceneToHide;
            public string MainSceneToShow => mainSceneToShow;
            public Action OnCompleteCallback => _onCompleteCallback;
        }

        private IEnumerator ChangeSceneRoutine(SceneTransitionPlan plan)
        {
            if (plan.UseOverlay)
            {
                yield return loadingOverlay.FadeIn();
                yield return new WaitForSeconds(1f);
            }

            if (!string.IsNullOrEmpty(plan.MainSceneToHide))
            {
                var mainScene = SceneManager.GetSceneByName(plan.MainSceneToHide);
                if (mainScene.IsValid() && mainScene.isLoaded)
                {
                    foreach (var obj in mainScene.GetRootGameObjects())
                    {
                        if (obj != null) obj.SetActive(false);
                    }
                    Debug.Log($"Hid main scene: {plan.MainSceneToHide}");
                }
            }

            foreach (var slot in plan.SlotsToUnload)
            {
                yield return StartCoroutine(UnloadSceneRoutine(slot));
            }

            if (plan.CleanUnused)
            {
                yield return Resources.UnloadUnusedAssets();
            }

            foreach (var (slot, sceneName, setActive) in plan.ScenesToLoad)
            {
                if (sceneSlots.ContainsKey(slot))
                {
                    yield return StartCoroutine(UnloadSceneRoutine(slot));
                }
                yield return StartCoroutine(LoadAdditiveRoutine(slot, sceneName, setActive));
            }

            if (!string.IsNullOrEmpty(plan.MainSceneToShow))
            {
                var mainScene = SceneManager.GetSceneByName(plan.MainSceneToShow);
                if (mainScene.IsValid() && mainScene.isLoaded)
                {
                    foreach (var obj in mainScene.GetRootGameObjects())
                    {
                        if (obj != null) obj.SetActive(true);
                    }
                    Debug.Log($"Showed main scene: {plan.MainSceneToShow}");
                }
            }

            if (plan.UseOverlay)
            {
                yield return new WaitForSeconds(0.5f);
                yield return loadingOverlay.FadeOut();
            }

            isBusy = false;

            plan.OnCompleteCallback?.Invoke();
        }

        private IEnumerator LoadAdditiveRoutine(string slot, string sceneName, bool setActive)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (operation == null) yield break;
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                yield return null;
            }

            operation.allowSceneActivation = true;
            while (!operation.isDone)
            {
                yield return null;
            }

            var loadedScene = SceneManager.GetSceneByName(sceneName);
            if (setActive)
            {
                SceneManager.SetActiveScene(loadedScene);
                currentActiveScene = loadedScene;
                CacheActiveSceneRootObjects(loadedScene);
            }

            sceneSlots[slot] = sceneName;
        }

        private IEnumerator UnloadSceneRoutine(string slot)
        {
            if (sceneSlots.TryGetValue(slot, out string sceneName))
            {
                var operation = SceneManager.UnloadSceneAsync(sceneName);
                while (!operation.isDone)
                {
                    yield return null;
                }

                sceneSlots.Remove(slot);
            }
        }

        private void CacheActiveSceneRootObjects(Scene scene)
        {
            activeSceneRootObjects.Clear();
            if (scene.IsValid() && scene.isLoaded)
                activeSceneRootObjects.AddRange(scene.GetRootGameObjects());
        }
    }
}
