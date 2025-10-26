using UnityEngine;
using UnityEngine.Events;

namespace Solow
{
    /// <summary>
    /// Controller for the Solow model simulation.
    /// Manages the model state and coordinates all visualizers.
    /// </summary>
    public class SolowSimulator : MonoBehaviour
    {
        [Header("Simulation Settings")]
        [SerializeField] private float timeScale = 1.0f;
        [SerializeField] private bool autoRun = true;

        [Header("Initial Conditions")]
        [SerializeField] private float initialCapital = 1.0f;

        [Header("Model Parameters")]
        [SerializeField, Range(0.01f, 0.9f)] private float savingsRate = 0.3f;
        [SerializeField, Range(0.01f, 0.2f)] private float depreciationRate = 0.05f;
        [SerializeField, Range(0.0f, 0.1f)] private float populationGrowthRate = 0.02f;
        [SerializeField, Range(0.1f, 3.0f)] private float technology = 1.0f;
        [SerializeField, Range(0.1f, 0.9f)] private float capitalShare = 0.33f;

        [Header("Visualizers")]
        [SerializeField] private GraphVisualizer graphVisualizer;
        [SerializeField] private CapitalVisualizer capitalVisualizer;
        [SerializeField] private NumericDisplay numericDisplay;

        // Events
        public UnityEvent<SolowModel> OnModelUpdated = new UnityEvent<SolowModel>();
        public UnityEvent<SolowModel> OnParametersChanged = new UnityEvent<SolowModel>();

        // The model
        private SolowModel model;
        public SolowModel Model => model;

        private void Awake()
        {
            // Initialize model
            model = new SolowModel
            {
                SavingsRate = savingsRate,
                DepreciationRate = depreciationRate,
                PopulationGrowthRate = populationGrowthRate,
                Technology = technology,
                CapitalShare = capitalShare
            };

            model.Reset(initialCapital);
        }

        private void Start()
        {
            // Initial update of all visualizers
            UpdateVisualizersImmediate();
            OnParametersChanged?.Invoke(model);
        }

        private void Update()
        {
            if (autoRun)
            {
                // Step the model
                model.Step(Time.deltaTime * timeScale);

                // Update visualizers
                UpdateVisualizers();

                // Notify listeners
                OnModelUpdated?.Invoke(model);
            }
        }

        /// <summary>
        /// Update model parameters from UI or external source
        /// </summary>
        public void UpdateParameters(float? s = null, float? delta = null, float? n = null,
                                     float? a = null, float? alpha = null)
        {
            if (s.HasValue) model.SavingsRate = s.Value;
            if (delta.HasValue) model.DepreciationRate = delta.Value;
            if (n.HasValue) model.PopulationGrowthRate = n.Value;
            if (a.HasValue) model.Technology = a.Value;
            if (alpha.HasValue) model.CapitalShare = alpha.Value;

            // Update visualizers immediately when parameters change
            UpdateVisualizersImmediate();
            OnParametersChanged?.Invoke(model);
        }

        /// <summary>
        /// Reset simulation
        /// </summary>
        public void ResetSimulation(float? newInitialCapital = null)
        {
            if (newInitialCapital.HasValue)
                initialCapital = newInitialCapital.Value;

            model.Reset(initialCapital);
            UpdateVisualizersImmediate();
            OnParametersChanged?.Invoke(model);
        }

        /// <summary>
        /// Toggle simulation running
        /// </summary>
        public void TogglePause()
        {
            autoRun = !autoRun;
        }

        /// <summary>
        /// Set time scale
        /// </summary>
        public void SetTimeScale(float scale)
        {
            timeScale = Mathf.Max(0.1f, scale);
        }

        private void UpdateVisualizers()
        {
            if (graphVisualizer != null)
                graphVisualizer.UpdateGraph(model);

            if (capitalVisualizer != null)
                capitalVisualizer.UpdateVisualization(model);

            if (numericDisplay != null)
                numericDisplay.UpdateDisplay(model);
        }

        private void UpdateVisualizersImmediate()
        {
            if (graphVisualizer != null)
                graphVisualizer.Initialize(model);

            if (capitalVisualizer != null)
                capitalVisualizer.Initialize(model);

            if (numericDisplay != null)
                numericDisplay.UpdateDisplay(model);
        }

        // Allow inspector changes to update model in real-time
        private void OnValidate()
        {
            if (model != null)
            {
                model.SavingsRate = savingsRate;
                model.DepreciationRate = depreciationRate;
                model.PopulationGrowthRate = populationGrowthRate;
                model.Technology = technology;
                model.CapitalShare = capitalShare;
            }
        }
    }
}
