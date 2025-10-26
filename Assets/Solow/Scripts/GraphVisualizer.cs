using UnityEngine;

namespace Solow
{
    /// <summary>
    /// Visualizes the Solow model as a 2D graph with curves.
    /// Shows production function f(k), investment sf(k), and depreciation (δ+n)k
    /// </summary>
    public class GraphVisualizer : MonoBehaviour
    {
        [Header("Graph Settings")]
        [SerializeField] private float graphWidth = 10f;
        [SerializeField] private float graphHeight = 5f;
        [SerializeField] private int resolution = 100;
        [SerializeField] private float maxK = 10f;

        [Header("Line Renderers")]
        [SerializeField] private LineRenderer productionLine;
        [SerializeField] private LineRenderer investmentLine;
        [SerializeField] private LineRenderer depreciationLine;
        [SerializeField] private Transform currentPointMarker;
        [SerializeField] private Transform steadyStateMarker;

        [Header("Colors")]
        [SerializeField] private Color productionColor = Color.blue;
        [SerializeField] private Color investmentColor = Color.green;
        [SerializeField] private Color depreciationColor = Color.red;

        private SolowModel cachedModel;

        private void Awake()
        {
            // Create line renderers if not assigned
            if (productionLine == null)
                productionLine = CreateLineRenderer("ProductionLine", productionColor);

            if (investmentLine == null)
                investmentLine = CreateLineRenderer("InvestmentLine", investmentColor);

            if (depreciationLine == null)
                depreciationLine = CreateLineRenderer("DepreciationLine", depreciationColor);

            // Create markers if not assigned
            if (currentPointMarker == null)
                currentPointMarker = CreateMarker("CurrentPoint", Color.yellow, 0.2f);

            if (steadyStateMarker == null)
                steadyStateMarker = CreateMarker("SteadyState", Color.cyan, 0.15f);
        }

        public void Initialize(SolowModel model)
        {
            cachedModel = model;
            DrawStaticCurves(model);
            UpdateMarkers(model);
        }

        public void UpdateGraph(SolowModel model)
        {
            // Only redraw curves if parameters changed significantly
            if (ShouldRedrawCurves(model))
            {
                DrawStaticCurves(model);
                cachedModel = model;
            }

            UpdateMarkers(model);
        }

        private void DrawStaticCurves(SolowModel model)
        {
            float maxY = model.ProductionFunction(maxK);

            // Production function
            DrawCurve(productionLine, (k) => model.ProductionFunction(k), maxY);

            // Investment function
            DrawCurve(investmentLine, (k) => model.GetInvestment(k), maxY);

            // Depreciation line
            DrawCurve(depreciationLine, (k) => model.GetDepreciation(k), maxY);
        }

        private void DrawCurve(LineRenderer line, System.Func<float, float> function, float maxY)
        {
            line.positionCount = resolution;

            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)(resolution - 1);
                float k = t * maxK;
                float y = function(k);

                // Map to local space
                Vector3 pos = new Vector3(
                    (k / maxK) * graphWidth - graphWidth / 2f,
                    (y / maxY) * graphHeight,
                    0
                );

                line.SetPosition(i, pos);
            }
        }

        private void UpdateMarkers(SolowModel model)
        {
            float maxY = model.ProductionFunction(maxK);

            // Current point
            if (currentPointMarker != null)
            {
                float k = model.CapitalPerWorker;
                float y = model.OutputPerWorker;

                currentPointMarker.localPosition = new Vector3(
                    (k / maxK) * graphWidth - graphWidth / 2f,
                    (y / maxY) * graphHeight,
                    -0.1f
                );
            }

            // Steady state point
            if (steadyStateMarker != null)
            {
                float kStar = model.GetSteadyStateCapital();
                float yStar = model.GetSteadyStateOutput();

                steadyStateMarker.localPosition = new Vector3(
                    (kStar / maxK) * graphWidth - graphWidth / 2f,
                    (yStar / maxY) * graphHeight,
                    -0.1f
                );
            }
        }

        private bool ShouldRedrawCurves(SolowModel model)
        {
            if (cachedModel == null) return true;

            return Mathf.Abs(cachedModel.SavingsRate - model.SavingsRate) > 0.001f ||
                   Mathf.Abs(cachedModel.DepreciationRate - model.DepreciationRate) > 0.001f ||
                   Mathf.Abs(cachedModel.PopulationGrowthRate - model.PopulationGrowthRate) > 0.001f ||
                   Mathf.Abs(cachedModel.Technology - model.Technology) > 0.001f ||
                   Mathf.Abs(cachedModel.CapitalShare - model.CapitalShare) > 0.001f;
        }

        private LineRenderer CreateLineRenderer(string name, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;

            LineRenderer lr = obj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.useWorldSpace = false;

            return lr;
        }

        private Transform CreateMarker(string name, Color color, float size)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = name;
            obj.transform.SetParent(transform);
            obj.transform.localScale = Vector3.one * size;

            Renderer rend = obj.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Standard"));
            rend.material.color = color;

            return obj.transform;
        }
    }
}
