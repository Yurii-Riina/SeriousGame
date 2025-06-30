using System.Collections.Generic;
using UnityEngine;

public class GrillIndependentRotator : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform pivotPoint;
    [SerializeField] private float openAngle = 75f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float range = 3f;

    private class GrillState
    {
        public bool isOpen = false;
        public float currentAngle = 0f;
        public bool rotating = false;
        public float targetAngle = 0f;
    }

    private Dictionary<Transform, GrillState> grillStates = new Dictionary<Transform, GrillState>();
    private List<Transform> activeGrills = new List<Transform>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Transform grill = hit.collider.transform;

                if (grill.parent != null && grill.parent.name.StartsWith("Grill"))
                    grill = grill.parent;

                if (grill != null && grill.name.StartsWith("Grill"))
                {
                    if (!grillStates.ContainsKey(grill))
                        grillStates[grill] = new GrillState();

                    GrillState state = grillStates[grill];

                    if (!state.rotating)
                    {
                        state.rotating = true;
                        state.targetAngle = state.isOpen ? 0f : openAngle;

                        if (!activeGrills.Contains(grill))
                            activeGrills.Add(grill);
                    }
                }
            }
        }

        List<Transform> completed = new List<Transform>();

        foreach (Transform grill in activeGrills)
        {
            GrillState state = grillStates[grill];

            if (state.rotating)
            {
                float step = rotationSpeed * Time.deltaTime;
                float direction = Mathf.Sign(state.targetAngle - state.currentAngle);

                if (Mathf.Abs(state.targetAngle - state.currentAngle) <= step)
                {
                    step = Mathf.Abs(state.targetAngle - state.currentAngle);
                    state.rotating = false;
                    state.isOpen = !state.isOpen;
                    state.currentAngle = state.targetAngle;

                    GrillCookingManager cookingManager = grill.GetComponent<GrillCookingManager>();
                    if (cookingManager != null)
                    {
                        if (!state.isOpen)
                        {
                            cookingManager.StartCooking();
                        }
                        else
                        {
                            cookingManager.PlayOpenSound();
                        }
                    }

                    completed.Add(grill);
                }
                else
                {
                    state.currentAngle += step * direction;
                }

                grill.RotateAround(pivotPoint.position, pivotPoint.right, step * direction);
            }
            else
            {
                completed.Add(grill);
            }
        }

        foreach (Transform grill in completed)
        {
            activeGrills.Remove(grill);
        }
    }
}
