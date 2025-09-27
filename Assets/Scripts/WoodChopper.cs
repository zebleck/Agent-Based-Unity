using UnityEngine;
using System.Collections.Generic;

public class WoodChopper : MonoBehaviour
{
    public enum State
    {
        Searching = 0,
        Chopping = 1,
        SelectingBuildSpot = 2,
        MovingToBuildSpot = 3,
        Building = 4,
        Idle = 5
    }

    public State currentState = State.Searching;

    // Static references shared by all WoodChopper agents
    public static Transform sharedForestParent;
    public static List<GameObject> allBuildings = new List<GameObject>();
    public static List<Vector3> activeBuildingSpots = new List<Vector3>(); // Spots where building is in progress
    public static List<Transform> claimedTrees = new List<Transform>(); // Trees being chopped by other agents

    public Transform forestParent; // Assign in inspector: the parent GameObject containing trees as children
    public float moveSpeed = 3f;
    public float chopDistance = 1.5f;
    public float chopTime = 5f; // Time to chop a tree

    // Building-related fields
    public GameObject buildingPrefab; // Assign in inspector: the building to spawn
    public GameObject buildingSpotPrefab; // Assign in inspector: red circle indicator
    public float buildTime = 8f; // Time to build
    public float buildDistance = 2f; // Distance to stand from building spot
    public float minDistanceFromObstacles = 5f; // Minimum distance from trees/buildings
    public float buildSpotSearchRadius = 15f; // Radius to search for build spot

    private Transform targetTree;
    private float chopTimer = 0f;

    // Building-related private fields
    private Vector3 selectedBuildPosition;
    private GameObject currentBuildingSpot;
    private GameObject currentBuilding;
    private float buildTimer = 0f;

    private Animator animator;
    public int wood = 0;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("state", (int)currentState);
        }

        // Initialize static references if not already set
        if (sharedForestParent == null && forestParent != null)
        {
            sharedForestParent = forestParent;
        }

        // Initialize buildings list if null
        if (allBuildings == null)
        {
            allBuildings = new List<GameObject>();
        }
    }

    void Update()
    {
        // Update the animator state parameter every frame, explicitly showing the int value
        if (animator != null)
        {
            animator.SetInteger("state", (int)currentState); // 0 = Searching, 1 = Chopping, 2 = SelectingBuildSpot, 3 = MovingToBuildSpot, 4 = Building
        }

        switch (currentState)
        {
            case State.Searching: // 0
                // Check if we have enough wood to build (also check here in case no trees are available)
                if (wood >= 3)
                {
                    currentState = State.SelectingBuildSpot;
                    break;
                }

                FindNearestTree();
                if (targetTree != null)
                {
                    float dist = Vector3.Distance(transform.position, targetTree.position);
                    if (dist > chopDistance)
                    {
                        // Move towards the tree
                        Vector3 dir = (targetTree.position - transform.position).normalized;
                        transform.position += dir * moveSpeed * Time.deltaTime;

                        // Rotate towards the tree smoothly (lerp)
                        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
                    }
                    else
                    {
                        currentState = State.Chopping; // 1
                        chopTimer = 0f;

                        // Claim this tree so other agents don't target it
                        if (!claimedTrees.Contains(targetTree))
                        {
                            claimedTrees.Add(targetTree);
                        }
                    }
                }
                else
                {
                    // No trees available and not enough wood to build - go idle
                    if (wood < 3)
                    {
                        currentState = State.Idle; // 5
                    }
                }
                break;

            case State.Chopping: // 1
                if (targetTree == null)
                {
                    currentState = State.Searching; // 0
                    break;
                }

                // Already in range, so just chop
                chopTimer += Time.deltaTime;
                if (chopTimer >= chopTime)
                {
                    wood++;

                    // Unclaim the tree before destroying it
                    if (claimedTrees.Contains(targetTree))
                    {
                        claimedTrees.Remove(targetTree);
                    }

                    Destroy(targetTree.gameObject);
                    targetTree = null;

                    // Check if we have enough wood to build
                    if (wood >= 3)
                    {
                        currentState = State.SelectingBuildSpot; // 2
                    }
                    else
                    {
                        currentState = State.Searching; // 0
                    }
                }
                break;

            case State.SelectingBuildSpot: // 2
                if (FindSuitableBuildSpot())
                {
                    // Add this spot to active building spots so other agents avoid it
                    activeBuildingSpots.Add(selectedBuildPosition);

                    // Create building spot indicator
                    if (buildingSpotPrefab != null)
                    {
                        currentBuildingSpot = Instantiate(buildingSpotPrefab, selectedBuildPosition, Quaternion.identity);
                    }
                    currentState = State.MovingToBuildSpot; // 3
                }
                else
                {
                    // Couldn't find a spot, go back to searching for trees
                    currentState = State.Searching; // 0
                }
                break;

            case State.MovingToBuildSpot: // 3
                if (currentBuildingSpot != null)
                {
                    float dist = Vector3.Distance(transform.position, selectedBuildPosition);
                    if (dist > buildDistance)
                    {
                        // Move towards the build spot
                        Vector3 dir = (selectedBuildPosition - transform.position).normalized;
                        transform.position += dir * moveSpeed * Time.deltaTime;

                        // Rotate towards the build spot
                        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
                    }
                    else
                    {
                        // Face the building spot before starting to build
                        Vector3 lookDir = (selectedBuildPosition - transform.position).normalized;
                        if (lookDir != Vector3.zero)
                        {
                            transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
                        }

                        // Start building
                        currentState = State.Building; // 4
                        buildTimer = 0f;

                        // Spawn the building at scale 0
                        if (buildingPrefab != null)
                        {
                            currentBuilding = Instantiate(buildingPrefab, selectedBuildPosition, Quaternion.identity);
                            currentBuilding.transform.localScale = Vector3.zero;
                            // Tag the building so future buildings avoid it
                            currentBuilding.tag = "Building";
                        }
                    }
                }
                else
                {
                    // Lost the building spot somehow
                    currentState = State.Searching; // 0
                }
                break;

            case State.Building: // 4
                buildTimer += Time.deltaTime;

                // Scale the building gradually
                if (currentBuilding != null)
                {
                    float progress = buildTimer / buildTime;
                    currentBuilding.transform.localScale = Vector3.one * Mathf.Clamp01(progress);
                }

                if (buildTimer >= buildTime)
                {
                    // Building complete
                    wood -= 3; // Deduct wood cost

                    // Add completed building to static list
                    if (currentBuilding != null)
                    {
                        allBuildings.Add(currentBuilding);
                    }

                    // Remove this spot from active building spots
                    if (activeBuildingSpots.Contains(selectedBuildPosition))
                    {
                        activeBuildingSpots.Remove(selectedBuildPosition);
                    }

                    // Clean up building spot indicator
                    if (currentBuildingSpot != null)
                    {
                        Destroy(currentBuildingSpot);
                        currentBuildingSpot = null;
                    }

                    currentBuilding = null;
                    currentState = State.Searching; // 0
                }
                break;

            case State.Idle: // 5
                // Periodically check if trees are available or if we can build
                if (wood >= 3)
                {
                    currentState = State.SelectingBuildSpot; // Try to build
                }
                else
                {
                    // Check if trees have become available
                    Transform activeForest = sharedForestParent != null ? sharedForestParent : forestParent;
                    if (activeForest != null && activeForest.childCount > 0)
                    {
                        // Check if there are any unclaimed trees
                        bool hasAvailableTrees = false;
                        foreach (Transform tree in activeForest)
                        {
                            if (tree != null && !claimedTrees.Contains(tree))
                            {
                                hasAvailableTrees = true;
                                break;
                            }
                        }

                        if (hasAvailableTrees)
                        {
                            currentState = State.Searching; // Trees available, start searching
                        }
                    }
                }
                break;
        }
    }

    void FindNearestTree()
    {
        // Use shared forest parent if available, otherwise use instance forest parent
        Transform activeForest = sharedForestParent != null ? sharedForestParent : forestParent;

        if (activeForest == null || activeForest.childCount == 0)
        {
            targetTree = null;
            return;
        }

        float minDist = float.MaxValue;
        Transform nearest = null;
        foreach (Transform tree in activeForest)
        {
            if (tree == null) continue;

            // Skip trees that are already being chopped by other agents
            if (claimedTrees.Contains(tree)) continue;

            float dist = Vector3.Distance(transform.position, tree.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = tree;
            }
        }
        targetTree = nearest;
    }

    bool FindSuitableBuildSpot()
    {
        Vector3 bestPosition = Vector3.zero;
        bool foundSpot = false;

        // Start from close and expand outward
        float searchDistance = minDistanceFromObstacles + 1f;
        float maxSearchDistance = buildSpotSearchRadius;
        int angleSteps = 8; // Check 8 directions initially

        while (searchDistance <= maxSearchDistance && !foundSpot)
        {
            // Check positions in a circle at current distance
            for (int i = 0; i < angleSteps; i++)
            {
                float angle = (360f / angleSteps) * i * Mathf.Deg2Rad;

                Vector3 offset = new Vector3(
                    Mathf.Sin(angle) * searchDistance,
                    0,
                    Mathf.Cos(angle) * searchDistance
                );

                Vector3 candidatePosition = transform.position + offset;
                candidatePosition.y = transform.position.y;

                // Check if this position is clear
                if (IsPositionClear(candidatePosition))
                {
                    selectedBuildPosition = candidatePosition;
                    foundSpot = true;
                    break;
                }
            }

            // Expand search radius and increase angle resolution
            searchDistance += 2f;
            if (searchDistance > 10f)
            {
                angleSteps = 16; // More angles for larger distances
            }
        }

        return foundSpot;
    }

    bool IsPositionClear(Vector3 position)
    {
        // Use shared forest parent if available, otherwise use instance forest parent
        Transform activeForest = sharedForestParent != null ? sharedForestParent : forestParent;

        // Check against trees specifically
        if (activeForest != null)
        {
            foreach (Transform tree in activeForest)
            {
                if (tree == null) continue;
                float dist = Vector3.Distance(position, tree.position);
                if (dist < minDistanceFromObstacles)
                {
                    return false;
                }
            }
        }

        // Check against existing buildings using static list
        foreach (GameObject building in allBuildings)
        {
            if (building == null || building == currentBuilding) continue;
            float dist = Vector3.Distance(position, building.transform.position);
            if (dist < minDistanceFromObstacles)
            {
                return false;
            }
        }

        // Check against current building spot to avoid overlapping spots
        if (currentBuildingSpot != null)
        {
            float dist = Vector3.Distance(position, currentBuildingSpot.transform.position);
            if (dist < minDistanceFromObstacles)
            {
                return false;
            }
        }

        // Check against other agents' active building spots
        foreach (Vector3 buildSpot in activeBuildingSpots)
        {
            float dist = Vector3.Distance(position, buildSpot);
            if (dist < minDistanceFromObstacles)
            {
                return false;
            }
        }

        return true;
    }

    void OnDestroy()
    {
        // Clean up static references when agent is destroyed

        // Remove any buildings created by this agent from the static list
        if (currentBuilding != null && allBuildings.Contains(currentBuilding))
        {
            allBuildings.Remove(currentBuilding);
        }

        // Remove active building spot if this agent was building
        if (activeBuildingSpots.Contains(selectedBuildPosition))
        {
            activeBuildingSpots.Remove(selectedBuildPosition);
        }

        // Unclaim any tree this agent was chopping
        if (targetTree != null && claimedTrees.Contains(targetTree))
        {
            claimedTrees.Remove(targetTree);
        }

        // Clean up building spot if it exists
        if (currentBuildingSpot != null)
        {
            Destroy(currentBuildingSpot);
        }

        // If this agent set the shared forest parent and no other agents exist, clear it
        if (sharedForestParent == forestParent)
        {
            WoodChopper[] remainingAgents = FindObjectsOfType<WoodChopper>();
            if (remainingAgents.Length <= 1) // Only this agent or no agents left
            {
                sharedForestParent = null;
            }
        }
    }
}
