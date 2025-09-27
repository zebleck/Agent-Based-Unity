using UnityEngine;
using System.Collections.Generic;

public class WoodChopper : MonoBehaviour
{
    public enum State
    {
        Searching = 0,
        Chopping = 1
    }

    public State currentState = State.Searching;

    public Transform forestParent; // Assign in inspector: the parent GameObject containing trees as children
    public float moveSpeed = 3f;
    public float chopDistance = 1.5f;
    public float chopTime = 5f; // Time to chop a tree

    private Transform targetTree;
    private float chopTimer = 0f;

    private Animator animator;
    public int wood = 0;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("state", (int)currentState);
        }
    }

    void Update()
    {
        // Update the animator state parameter every frame, explicitly showing the int value
        if (animator != null)
        {
            animator.SetInteger("state", (int)currentState); // 0 = Searching, 1 = Chopping
        }

        switch (currentState)
        {
            case State.Searching: // 0
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
                    Destroy(targetTree.gameObject);
                    targetTree = null;
                    currentState = State.Searching; // 0
                }
                break;
        }
    }

    void FindNearestTree()
    {
        if (forestParent == null || forestParent.childCount == 0)
        {
            targetTree = null;
            return;
        }

        float minDist = float.MaxValue;
        Transform nearest = null;
        foreach (Transform tree in forestParent)
        {
            if (tree == null) continue;
            float dist = Vector3.Distance(transform.position, tree.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = tree;
            }
        }
        targetTree = nearest;
    }
}
