using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class Scene3EndingUIHandler : MonoBehaviour
    {
        public GameObject panelRoot;
        public TMP_Text titleText;
        public TMP_Text descriptionText;
        public Button returnButton;
        public InGameManager inGameManager;
        public string startSceneName = "Scene1Home";
        public string winTitle = "结局：真相揭露";
        public string loseTitle = "结局：迷失病区";

        [TextArea(2, 8)]
        public string winDescription = "你战胜了院长，病区的幻象开始崩塌。";

        [TextArea(2, 8)]
        public string loseDescription = "你没能逃离院长的追击，意识被病区吞没。";

        private void Awake()
        {
            ResolveReferences();
            Hide();

            if (returnButton != null)
            {
                returnButton.onClick.AddListener(ReturnToStart);
            }
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            if (returnButton != null)
            {
                returnButton.onClick.RemoveListener(ReturnToStart);
            }
        }

        private void ResolveReferences()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            if (inGameManager == null)
            {
                inGameManager = InGameManager.Instance != null ? InGameManager.Instance : FindObjectOfType<InGameManager>();
            }
        }

        private void Subscribe()
        {
            if (inGameManager != null)
            {
                inGameManager.OnGameEnded -= ShowEnding;
                inGameManager.OnGameEnded += ShowEnding;
            }
        }

        private void Unsubscribe()
        {
            if (inGameManager != null)
            {
                inGameManager.OnGameEnded -= ShowEnding;
            }
        }

        private void ShowEnding(bool isWin)
        {
            SetText(titleText, isWin ? winTitle : loseTitle);
            SetText(descriptionText, isWin ? winDescription : loseDescription);

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        private void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void ReturnToStart()
        {
            if (!string.IsNullOrEmpty(startSceneName))
            {
                SceneManager.LoadScene(startSceneName);
            }
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
