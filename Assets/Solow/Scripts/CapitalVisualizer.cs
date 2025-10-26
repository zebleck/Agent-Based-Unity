using UnityEngine;

namespace Solow
{
    /// <summary>
    /// Physical visualization of capital stock with particle effects for flows.
    /// Shows capital as a growing/shrinking stack with investment and depreciation particles.
    /// </summary>
    public class CapitalVisualizer : MonoBehaviour
    {
        [Header("Capital Stack")]
        [SerializeField] private Transform capitalStack;
        [SerializeField] private float maxHeight = 10f;
        [SerializeField] private float heightScale = 2f;
        [SerializeField] private Color capitalColor = new Color(0.8f, 0.8f, 0.2f);

        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem investmentParticles;
        [SerializeField] private ParticleSystem depreciationParticles;
        [SerializeField] private float particleEmissionScale = 10f;

        [Header("Workers")]
        [SerializeField] private Transform workersContainer;
        [SerializeField] private int numberOfWorkers = 10;
        [SerializeField] private float workerRadius = 3f;

        private Material capitalMaterial;
        private Transform[] workers;

        private void Awake()
        {
            SetupCapitalStack();
            SetupParticles();
            SetupWorkers();
        }

        public void Initialize(SolowModel model)
        {
            UpdateVisualization(model);
        }

        public void UpdateVisualization(SolowModel model)
        {
            UpdateCapitalHeight(model.CapitalPerWorker);
            UpdateParticleFlows(model);
            UpdateCapitalColor(model);
        }

        private void SetupCapitalStack()
        {
            if (capitalStack == null)
            {
                GameObject stack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stack.name = "CapitalStack";
                stack.transform.SetParent(transform);
                stack.transform.localPosition = Vector3.zero;
                capitalStack = stack.transform;

                capitalMaterial = new Material(Shader.Find("Standard"));
                capitalMaterial.color = capitalColor;
                stack.GetComponent<Renderer>().material = capitalMaterial;
            }
        }

        private void SetupParticles()
        {
            // Investment particles (flowing in, green)
            if (investmentParticles == null)
            {
                GameObject invObj = new GameObject("InvestmentParticles");
                invObj.transform.SetParent(transform);
                invObj.transform.localPosition = Vector3.up * 5f;

                investmentParticles = invObj.AddComponent<ParticleSystem>();
                var invMain = investmentParticles.main;
                invMain.startColor = Color.green;
                invMain.startSpeed = 2f;
                invMain.startSize = 0.2f;
                invMain.simulationSpace = ParticleSystemSimulationSpace.World;

                var invEmission = investmentParticles.emission;
                invEmission.rateOverTime = 0;
            }

            // Depreciation particles (flowing out, red)
            if (depreciationParticles == null)
            {
                GameObject depObj = new GameObject("DepreciationParticles");
                depObj.transform.SetParent(transform);
                depObj.transform.localPosition = Vector3.zero;

                depreciationParticles = depObj.AddComponent<ParticleSystem>();
                var depMain = depreciationParticles.main;
                depMain.startColor = Color.red;
                depMain.startSpeed = 2f;
                depMain.startSize = 0.2f;
                depMain.simulationSpace = ParticleSystemSimulationSpace.World;

                var depEmission = depreciationParticles.emission;
                depEmission.rateOverTime = 0;
            }
        }

        private void SetupWorkers()
        {
            if (workersContainer == null)
            {
                GameObject container = new GameObject("Workers");
                container.transform.SetParent(transform);
                container.transform.localPosition = Vector3.zero;
                workersContainer = container.transform;
            }

            workers = new Transform[numberOfWorkers];
            for (int i = 0; i < numberOfWorkers; i++)
            {
                GameObject worker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                worker.name = $"Worker_{i}";
                worker.transform.SetParent(workersContainer);
                worker.transform.localScale = Vector3.one * 0.3f;

                // Position in circle around capital
                float angle = (i / (float)numberOfWorkers) * 2f * Mathf.PI;
                worker.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * workerRadius,
                    0.5f,
                    Mathf.Sin(angle) * workerRadius
                );

                Material workerMat = new Material(Shader.Find("Standard"));
                workerMat.color = new Color(0.3f, 0.3f, 0.8f);
                worker.GetComponent<Renderer>().material = workerMat;

                workers[i] = worker.transform;
            }
        }

        private void UpdateCapitalHeight(float k)
        {
            if (capitalStack == null) return;

            float targetHeight = Mathf.Clamp(k * heightScale, 0.1f, maxHeight);

            // Smooth transition
            Vector3 currentScale = capitalStack.localScale;
            currentScale.y = Mathf.Lerp(currentScale.y, targetHeight, Time.deltaTime * 2f);
            capitalStack.localScale = currentScale;

            // Position so base stays at y=0
            Vector3 pos = capitalStack.localPosition;
            pos.y = currentScale.y / 2f;
            capitalStack.localPosition = pos;
        }

        private void UpdateParticleFlows(SolowModel model)
        {
            if (investmentParticles != null)
            {
                var invEmission = investmentParticles.emission;
                invEmission.rateOverTime = model.InvestmentPerWorker * particleEmissionScale;

                // Position above capital
                investmentParticles.transform.position =
                    capitalStack.position + Vector3.up * (capitalStack.localScale.y + 1f);
            }

            if (depreciationParticles != null)
            {
                var depEmission = depreciationParticles.emission;
                depEmission.rateOverTime = model.CapitalDilution * particleEmissionScale;

                // Position at capital
                depreciationParticles.transform.position = capitalStack.position;
            }
        }

        private void UpdateCapitalColor(SolowModel model)
        {
            if (capitalMaterial == null) return;

            // Color based on growth rate
            // Green when growing, red when shrinking, yellow at steady state
            float convergence = model.GetConvergenceProgress();

            Color targetColor;
            if (convergence < 0.95f)
                targetColor = Color.Lerp(Color.blue, Color.yellow, convergence);
            else if (convergence > 1.05f)
                targetColor = Color.Lerp(Color.yellow, Color.red, (convergence - 1f) / 0.5f);
            else
                targetColor = Color.yellow; // Near steady state

            capitalMaterial.color = Color.Lerp(capitalMaterial.color, targetColor, Time.deltaTime);
        }
    }
}
