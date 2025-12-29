using UnityEngine;
using UnityEngine.UIElements;

namespace QuestJournal.UI
{
    /// <summary>
    /// Creates the plain SettingsPageController at runtime and passes the correct
    ///
    /// Attach this component to the GameObject that contains the UIDocument which
    /// hosts the Settings page UXML (for example the root UI Document for the scene).
    /// </summary>
    [DisallowMultipleComponent]
    public class SettingsPageComponent : MonoBehaviour
    {
        private SettingsPageController controller;

        void OnEnable()
        {
            var uiDoc = GetComponent<UIDocument>();
            if (uiDoc == null)
            {
                Debug.LogWarning("SettingsPageComponent: UIDocument not found on GameObject. Attach this component to the same GameObject as the UIDocument.");
                return;
            }

            VisualElement pageRoot = uiDoc.rootVisualElement.Q<VisualElement>("SettingsPage");
            if (pageRoot == null) pageRoot = uiDoc.rootVisualElement;

            if (controller == null)
                controller = new SettingsPageController(pageRoot);

        }
    }
}
