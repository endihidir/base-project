using UnityBase.Tag;
using UnityEngine;

namespace UnityBase.Managers.SO
{
    [CreateAssetMenu(menuName = "Game/ManagerData/GameManagerData")]
    public class GameManagerSO : ScriptableObject
    {
        [Header("Splash Screen")] public CanvasGroup splashScreen;

        [Header("Settings")] public int targetFrameRate = 60;
        public bool isMultiTouchEnabled = false;
        public bool passSplashScreen;

        public void Initialize()
        {
            splashScreen = FindObjectOfType<Tag_SplashScreenCanvas>(true)?.canvasGroup;
        }
    }
}