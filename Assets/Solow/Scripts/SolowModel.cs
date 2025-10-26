using System;

namespace Solow
{
    /// <summary>
    /// Pure economic logic for the Solow growth model.
    /// No Unity dependencies - fully testable.
    /// Production function: Y = A * K^α * L^(1-α)
    /// </summary>
    public class SolowModel
    {
        // Parameters
        public float SavingsRate { get; set; } = 0.3f;           // s: fraction of output saved
        public float DepreciationRate { get; set; } = 0.05f;     // δ: capital depreciation rate
        public float PopulationGrowthRate { get; set; } = 0.02f; // n: population growth rate
        public float Technology { get; set; } = 1.0f;            // A: technology level
        public float CapitalShare { get; set; } = 0.33f;         // α: capital's share of output (Cobb-Douglas)

        // State variables
        public float CapitalPerWorker { get; private set; } = 1.0f;  // k: capital per worker
        public float OutputPerWorker { get; private set; } = 1.0f;   // y: output per worker

        // Derived values
        public float InvestmentPerWorker { get; private set; }
        public float CapitalDilution { get; private set; }
        public float GrowthRate { get; private set; }

        /// <summary>
        /// Production function: y = A * k^α
        /// </summary>
        public float ProductionFunction(float k)
        {
            return Technology * MathF.Pow(k, CapitalShare);
        }

        /// <summary>
        /// Investment per worker: s * f(k)
        /// </summary>
        public float GetInvestment(float k)
        {
            return SavingsRate * ProductionFunction(k);
        }

        /// <summary>
        /// Capital dilution: (δ + n) * k
        /// </summary>
        public float GetDepreciation(float k)
        {
            return (DepreciationRate + PopulationGrowthRate) * k;
        }

        /// <summary>
        /// Calculate steady state capital per worker: k* where sf(k*) = (δ+n)k*
        /// Solving: s*A*k^α = (δ+n)*k
        /// k* = (s*A / (δ+n))^(1/(1-α))
        /// </summary>
        public float GetSteadyStateCapital()
        {
            float exponent = 1.0f / (1.0f - CapitalShare);
            return MathF.Pow(SavingsRate * Technology / (DepreciationRate + PopulationGrowthRate), exponent);
        }

        /// <summary>
        /// Steady state output per worker: y* = f(k*)
        /// </summary>
        public float GetSteadyStateOutput()
        {
            return ProductionFunction(GetSteadyStateCapital());
        }

        /// <summary>
        /// Step the model forward in time
        /// Δk = sf(k) - (δ+n)k
        /// </summary>
        public void Step(float deltaTime)
        {
            // Calculate current values
            OutputPerWorker = ProductionFunction(CapitalPerWorker);
            InvestmentPerWorker = GetInvestment(CapitalPerWorker);
            CapitalDilution = GetDepreciation(CapitalPerWorker);

            // Change in capital per worker
            float deltaK = InvestmentPerWorker - CapitalDilution;

            // Growth rate
            GrowthRate = deltaK / CapitalPerWorker;

            // Update capital (with time scaling)
            CapitalPerWorker += deltaK * deltaTime;

            // Prevent negative capital
            if (CapitalPerWorker < 0.01f)
                CapitalPerWorker = 0.01f;
        }

        /// <summary>
        /// Reset to initial conditions
        /// </summary>
        public void Reset(float initialCapital = 1.0f)
        {
            CapitalPerWorker = initialCapital;
            OutputPerWorker = ProductionFunction(CapitalPerWorker);
            InvestmentPerWorker = GetInvestment(CapitalPerWorker);
            CapitalDilution = GetDepreciation(CapitalPerWorker);
            GrowthRate = 0f;
        }

        /// <summary>
        /// Distance from steady state (normalized)
        /// </summary>
        public float GetConvergenceProgress()
        {
            float kStar = GetSteadyStateCapital();
            return CapitalPerWorker / kStar;
        }
    }
}
