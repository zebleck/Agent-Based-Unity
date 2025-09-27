using UnityEngine;
using TMPro;

public class WoodChopperDisplay : MonoBehaviour
{
    public WoodChopper woodChopper; // Assign in inspector
    private TextMeshProUGUI textMeshProUGUI;

    void Awake()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        if (textMeshProUGUI == null)
        {
            Debug.LogWarning("TextMeshProUGUI component not found on this GameObject.");
        }
    }

    void Update()
    {
        if (woodChopper != null && textMeshProUGUI != null)
        {
            int wood = woodChopper.wood;

            // Add more descriptive text for building states
            string displayText = "";
            switch(woodChopper.currentState)
            {
                case WoodChopper.State.Searching:
                    displayText = "Searching for Trees";
                    break;
                case WoodChopper.State.Chopping:
                    displayText = "Chopping Trees";
                    break;
                case WoodChopper.State.SelectingBuildSpot:
                    displayText = "Selecting Build Spot";
                    break;
                case WoodChopper.State.MovingToBuildSpot:
                    displayText = "Moving to Build";
                    break;
                case WoodChopper.State.Building:
                    displayText = "Building";
                    break;
                case WoodChopper.State.Idle:
                    displayText = "Idle (No trees)";
                    break;
            }

            textMeshProUGUI.text = $"State: {displayText}\nWood: {wood}";
        }
    }
}
