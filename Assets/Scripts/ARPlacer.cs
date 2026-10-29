using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacer : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private GameObject hintPanel;      // optional
    [SerializeField] private GameObject controlsPanel;  // optional
    [SerializeField] private bool hidePlanesAfterPlacing = true;

    private static readonly List<ARRaycastHit> hits = new();
    private bool placedOnce;

    private void Update()
    {
        var pointer = UnityEngine.InputSystem.Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;

        // Ignore taps that land on UI buttons
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector2 pos = pointer.position.ReadValue();
        if (!raycastManager.Raycast(pos, hits, TrackableType.PlaneWithinPolygon)) return;

        Pose pose = hits[0].pose;
        contentRoot.transform.SetPositionAndRotation(pose.position, Quaternion.identity);
        placedOnce = true;

        if (!contentRoot.activeSelf)
        {
            contentRoot.SetActive(true);
            if (hintPanel != null) hintPanel.SetActive(false);
            if (controlsPanel != null) controlsPanel.SetActive(true);
        }
    }

    private void LateUpdate()
    {
        if (hidePlanesAfterPlacing && placedOnce && planeManager != null)
            planeManager.SetTrackablesActive(false);
    }
}