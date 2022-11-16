
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    public class CanvasUI : MonoBehaviour
    {
        public GameObject privacyPrompt;
        public MainController mainController;
        /// <summary>
        /// The key name used in PlayerPrefs which indicates whether the start info has displayed
        /// at least one time.
        /// </summary>
        private const string _hasDisplayedStartInfoKey = "HasDisplayedStartInfo";

        // Start is called before the first frame update
        void Start() 
        {
            if (Application.isEditor) PlayerPrefs.DeleteKey(_hasDisplayedStartInfoKey);
            DisplayPrivacyPromptIfNecessary();
        }

        /// <summary>
        /// Switch to privacy prompt, and disable all other screens.
        /// </summary>
        public void DisplayPrivacyPromptIfNecessary()
        {
            if (PlayerPrefs.HasKey(_hasDisplayedStartInfoKey))
            {
                SwitchToARView();
                return;
            }
            privacyPrompt.SetActive(true);
        }

        // ####################################################
        // Button Actions (Get Started and Learn More)

        /// <summary>
        /// Switch to AR view, and disable all other screens.
        /// </summary>
        public void SwitchToARView()
        {
            PlayerPrefs.SetInt(_hasDisplayedStartInfoKey, 1);
            privacyPrompt.SetActive(false);
            mainController.SetPlatformActive(true);
        }

        /// <summary>
        /// Callback handling "Learn More" Button click event in Privacy Prompt.
        /// </summary>
        public void OnLearnMoreButtonClicked()
        {
            Debug.Log("Learn More clicked. Attempting to open URL...");
            Application.OpenURL(
                "https://developers.google.com/ar/cloud-anchors-privacy");
        }

        // ####################################################
    }