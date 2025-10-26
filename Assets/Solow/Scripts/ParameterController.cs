using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Solow
{
    /// <summary>
    /// UI controller for adjusting Solow model parameters via sliders.
    /// </summary>
    public class ParameterController : MonoBehaviour
    {
        [Header("Simulator Reference")]
        [SerializeField] private SolowSimulator simulator;

        [Header("Sliders")]
        [SerializeField] private Slider savingsRateSlider;
        [SerializeField] private Slider depreciationRateSlider;
        [SerializeField] private Slider populationGrowthSlider;
        [SerializeField] private Slider technologySlider;
        [SerializeField] private Slider capitalShareSlider;
        [SerializeField] private Slider initialCapitalSlider;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI savingsRateLabel;
        [SerializeField] private TextMeshProUGUI depreciationRateLabel;
        [SerializeField] private TextMeshProUGUI populationGrowthLabel;
        [SerializeField] private TextMeshProUGUI technologyLabel;
        [SerializeField] private TextMeshProUGUI capitalShareLabel;
        [SerializeField] private TextMeshProUGUI initialCapitalLabel;

        [Header("Buttons")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private TextMeshProUGUI pauseButtonText;

        private void Start()
        {
            SetupSliders();
            SetupButtons();
        }

        private void SetupSliders()
        {
            // Savings rate (s)
            if (savingsRateSlider != null)
            {
                savingsRateSlider.minValue = 0.01f;
                savingsRateSlider.maxValue = 0.9f;
                savingsRateSlider.value = 0.3f;
                savingsRateSlider.onValueChanged.AddListener(OnSavingsRateChanged);
            }

            // Depreciation rate (δ)
            if (depreciationRateSlider != null)
            {
                depreciationRateSlider.minValue = 0.01f;
                depreciationRateSlider.maxValue = 0.2f;
                depreciationRateSlider.value = 0.05f;
                depreciationRateSlider.onValueChanged.AddListener(OnDepreciationRateChanged);
            }

            // Population growth (n)
            if (populationGrowthSlider != null)
            {
                populationGrowthSlider.minValue = 0.0f;
                populationGrowthSlider.maxValue = 0.1f;
                populationGrowthSlider.value = 0.02f;
                populationGrowthSlider.onValueChanged.AddListener(OnPopulationGrowthChanged);
            }

            // Technology (A)
            if (technologySlider != null)
            {
                technologySlider.minValue = 0.1f;
                technologySlider.maxValue = 3.0f;
                technologySlider.value = 1.0f;
                technologySlider.onValueChanged.AddListener(OnTechnologyChanged);
            }

            // Capital share (α)
            if (capitalShareSlider != null)
            {
                capitalShareSlider.minValue = 0.1f;
                capitalShareSlider.maxValue = 0.9f;
                capitalShareSlider.value = 0.33f;
                capitalShareSlider.onValueChanged.AddListener(OnCapitalShareChanged);
            }

            // Initial capital
            if (initialCapitalSlider != null)
            {
                initialCapitalSlider.minValue = 0.1f;
                initialCapitalSlider.maxValue = 10f;
                initialCapitalSlider.value = 1.0f;
                initialCapitalSlider.onValueChanged.AddListener(OnInitialCapitalChanged);
            }
        }

        private void SetupButtons()
        {
            if (resetButton != null)
            {
                resetButton.onClick.AddListener(OnResetClicked);
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseClicked);
                UpdatePauseButtonText();
            }
        }

        private void OnSavingsRateChanged(float value)
        {
            if (savingsRateLabel != null)
                savingsRateLabel.text = $"s = {value:F2}";

            simulator?.UpdateParameters(s: value);
        }

        private void OnDepreciationRateChanged(float value)
        {
            if (depreciationRateLabel != null)
                depreciationRateLabel.text = $"δ = {value:F2}";

            simulator?.UpdateParameters(delta: value);
        }

        private void OnPopulationGrowthChanged(float value)
        {
            if (populationGrowthLabel != null)
                populationGrowthLabel.text = $"n = {value:F2}";

            simulator?.UpdateParameters(n: value);
        }

        private void OnTechnologyChanged(float value)
        {
            if (technologyLabel != null)
                technologyLabel.text = $"A = {value:F2}";

            simulator?.UpdateParameters(a: value);
        }

        private void OnCapitalShareChanged(float value)
        {
            if (capitalShareLabel != null)
                capitalShareLabel.text = $"α = {value:F2}";

            simulator?.UpdateParameters(alpha: value);
        }

        private void OnInitialCapitalChanged(float value)
        {
            if (initialCapitalLabel != null)
                initialCapitalLabel.text = $"k₀ = {value:F2}";
        }

        private void OnResetClicked()
        {
            float initialK = initialCapitalSlider != null ? initialCapitalSlider.value : 1.0f;
            simulator?.ResetSimulation(initialK);
        }

        private void OnPauseClicked()
        {
            simulator?.TogglePause();
            UpdatePauseButtonText();
        }

        private void UpdatePauseButtonText()
        {
            if (pauseButtonText != null && simulator != null)
            {
                // This would need to check simulator's autoRun state
                pauseButtonText.text = "Pause/Play";
            }
        }
    }
}
