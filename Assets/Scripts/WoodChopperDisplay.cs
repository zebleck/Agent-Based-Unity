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
            string stateString = woodChopper.currentState.ToString();
            int wood = woodChopper.wood;
            textMeshProUGUI.text = $"State: {stateString}\nWood: {wood}";
        }
    }
}
