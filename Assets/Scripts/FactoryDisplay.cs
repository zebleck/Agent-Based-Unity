using UnityEngine;
using TMPro;

public class FactoryDisplay : MonoBehaviour
{
    public FactoryAgent agent; // Reference to the FactoryAgent to display
    public TextMeshProUGUI resourceText;  // Reference to a TextMeshProUGUI component

    void Update()
    {
        if (agent != null && resourceText != null)
        {
            resourceText.text = "Factory 1\nResources: " + agent.resource + "\nCapital: " + agent.capital;
        }
    }
}

