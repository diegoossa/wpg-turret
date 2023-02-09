using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace WPG.Turret.Gameplay
{
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance;

        private Label _scoreLabel;
        private Button _restartButton;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _scoreLabel = root.Q<Label>("score-label");
            _restartButton = root.Q<Button>("restart-button");
            _restartButton.clicked += OnRestartButtonClicked;
        }

        private void OnRestartButtonClicked()
        {
            // TODO: Properly reset the game state
            SceneManager.LoadScene("ECSGame");
        }

        public void IncreaseScore()
        {
            if (int.TryParse(_scoreLabel.text, out var score))
            {
                _scoreLabel.text = (++score).ToString();
            }
        }

        public void GameOver()
        {
            _restartButton.style.display = DisplayStyle.Flex;
        }
    }
}