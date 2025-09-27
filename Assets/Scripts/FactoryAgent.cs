using UnityEngine;

public class FactoryAgent : MonoBehaviour
{
    public int resource = 100;
    public int capital = 0;

    public CarFollower car; // reference to the car object

    private bool busy = false;

    void Update()
    {
        if (!busy && resource >= 10)
        {
            StartCoroutine(ProductionCycle());
        }
    }

    private System.Collections.IEnumerator ProductionCycle()
    {
        busy = true;

        // Spend resources
        resource -= 10;

        // Car goes to market
        yield return StartCoroutine(car.MoveForwardCoroutine());

        // Car drives back
        yield return StartCoroutine(car.MoveBackwardCoroutine());

        // Gain capital
        capital += 20;

        busy = false;
    }
}