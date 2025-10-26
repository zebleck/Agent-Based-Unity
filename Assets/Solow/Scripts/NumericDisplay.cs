using UnityEngine;
using TMPro;

namespace Solow
{
    /// <summary>
    /// Displays current model values as text (k, y, growth rate, steady state, etc.)
    /// </summary>
    public class NumericDisplay : MonoBehaviour
    {
        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI capitalPerWorkerText;
        [SerializeField] private TextMeshProUGUI outputPerWorkerText;
        [SerializeField] private TextMeshProUGUI investmentText;
        [SerializeField] private TextMeshProUGUI depreciationText;
        [SerializeField] private TextMeshProUGUI growthRateText;
        [SerializeField] private TextMeshProUGUI steadyStateCapitalText;
        [SerializeField] private TextMeshProUGUI steadyStateOutputText;
        [SerializeField] private TextMeshProUGUI convergenceText;

        [Header("Combined Display")]
        [SerializeField] private TextMeshProUGUI combinedDisplayText;

        public void UpdateDisplay(SolowModel model)
        {
            if (model == null) return;

            // Individual fields
            if (capitalPerWorkerText != null)
                capitalPerWorkerText.text = $"k = {model.CapitalPerWorker:F3}";

            if (outputPerWorkerText != null)
                outputPerWorkerText.text = $"y = {model.OutputPerWorker:F3}";

            if (investmentText != null)
                investmentText.text = $"Investment = {model.InvestmentPerWorker:F3}";

            if (depreciationText != null)
                depreciationText.text = $"Depreciation = {model.CapitalDilution:F3}";

            if (growthRateText != null)
            {
                float growthPercent = model.GrowthRate * 100f;
                string arrow = model.GrowthRate > 0.001f ? "↑" : model.GrowthRate < -0.001f ? "↓" : "→";
                growthRateText.text = $"Growth {arrow} {growthPercent:F2}%";

                // Color code growth rate
                if (Mathf.Abs(model.GrowthRate) < 0.001f)
                    growthRateText.color = Color.yellow;
                else if (model.GrowthRate > 0)
                    growthRateText.color = Color.green;
                else
                    growthRateText.color = Color.red;
            }

            if (steadyStateCapitalText != null)
                steadyStateCapitalText.text = $"k* = {model.GetSteadyStateCapital():F3}";

            if (steadyStateOutputText != null)
                steadyStateOutputText.text = $"y* = {model.GetSteadyStateOutput():F3}";

            if (convergenceText != null)
            {
                float progress = model.GetConvergenceProgress();
                convergenceText.text = $"k/k* = {progress:F2}";

                if (progress >= 0.95f && progress <= 1.05f)
                    convergenceText.color = Color.cyan;
                else if (progress < 1f)
                    convergenceText.color = Color.green;
                else
                    convergenceText.color = Color.red;
            }

            // Combined display (if using single text field instead)
            if (combinedDisplayText != null)
            {
                combinedDisplayText.text = FormatCombinedDisplay(model);
            }
        }

        private string FormatCombinedDisplay(SolowModel model)
        {
            float growthPercent = model.GrowthRate * 100f;
            string arrow = model.GrowthRate > 0.001f ? "↑" : model.GrowthRate < -0.001f ? "↓" : "→";

            return $@"<b>SOLOW MODEL</b>

<b>Current State:</b>
Capital per worker (k):    {model.CapitalPerWorker:F3}
Output per worker (y):     {model.OutputPerWorker:F3}

<b>Flows:</b>
Investment:                {model.InvestmentPerWorker:F3}
Depreciation:              {model.CapitalDilution:F3}
Growth Rate {arrow}:            {growthPercent:F2}%

<b>Steady State:</b>
k*:                        {model.GetSteadyStateCapital():F3}
y*:                        {model.GetSteadyStateOutput():F3}

<b>Convergence:</b>
k/k*:                      {model.GetConvergenceProgress():F2}";
        }
    }
}
