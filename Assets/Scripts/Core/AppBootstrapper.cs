using System.Collections;
using ArSpacePlanner.Input;
using ArSpacePlanner.Measure;
using ArSpacePlanner.Planner;
using ArSpacePlanner.UI;
using ArSpacePlanner.Util;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ArSpacePlanner.Core
{
    /// <summary>
    /// Builds the AR rig (session, origin, camera, plane/raycast/anchor managers) and
    /// wires up the measurement tool, the furniture planner, and the UI. Everything is
    /// created in code so the project ships with an empty scene.
    /// </summary>
    public sealed class AppBootstrapper : MonoBehaviour
    {
        private AppState _state;
        private MeasurementManager _measure;
        private FurniturePlacer _planner;
        private AppUI _ui;

        private IEnumerator Start()
        {
            InputRouter.Enable();
            _state = new AppState();

            yield return ARSession.CheckAvailability();
            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                yield return ARSession.Install();
            }

            BuildSession();

            Camera arCamera = BuildOrigin(
                out ARRaycastManager raycastManager,
                out ARPlaneManager planeManager);

            planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;

            _measure = gameObject.AddComponent<MeasurementManager>();
            _measure.Init(raycastManager, arCamera, _state);

            _planner = gameObject.AddComponent<FurniturePlacer>();
            _planner.Init(raycastManager, arCamera, _state);
            _planner.SelectCatalog(0);

            var screenshots = gameObject.AddComponent<ScreenshotService>();

            _ui = gameObject.AddComponent<AppUI>();
            _ui.Init(_state, _measure, _planner, screenshots);

            _state.Mode = AppMode.Measure;
        }

        private void BuildSession()
        {
            var go = new GameObject("AR Session");
            go.transform.SetParent(transform, false);
            go.AddComponent<ARSession>();
            go.AddComponent<ARInputManager>();
        }

        private Camera BuildOrigin(
            out ARRaycastManager raycastManager,
            out ARPlaneManager planeManager)
        {
            var originGo = new GameObject("XR Origin");
            originGo.transform.SetParent(transform, false);
            var origin = originGo.AddComponent<XROrigin>();

            var offsetGo = new GameObject("Camera Offset");
            offsetGo.transform.SetParent(originGo.transform, false);

            var cameraGo = new GameObject("AR Camera") { tag = "MainCamera" };
            cameraGo.transform.SetParent(offsetGo.transform, false);

            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 20f;

            AddTrackedPoseDriver(cameraGo);

            cameraGo.AddComponent<ARCameraManager>();
            cameraGo.AddComponent<ARCameraBackground>();

            origin.CameraFloorOffsetObject = offsetGo;
            origin.Camera = camera;

            planeManager = originGo.AddComponent<ARPlaneManager>();
            planeManager.planePrefab = BuildPlanePrefab();

            raycastManager = originGo.AddComponent<ARRaycastManager>();
            // Enables the anchor subsystem so ARAnchor components created by the
            // measurement tool are tracked by ARCore.
            originGo.AddComponent<ARAnchorManager>();

            return camera;
        }

        private static void AddTrackedPoseDriver(GameObject cameraGo)
        {
            var driver = cameraGo.AddComponent<TrackedPoseDriver>();
            driver.positionInput = new InputActionProperty(new InputAction(
                "Position", InputActionType.Value, "<XRHMD>/centerEyePosition", expectedControlType: "Vector3"));
            driver.rotationInput = new InputActionProperty(new InputAction(
                "Rotation", InputActionType.Value, "<XRHMD>/centerEyeRotation", expectedControlType: "Quaternion"));
            driver.trackingStateInput = new InputActionProperty(new InputAction(
                "Tracking State", InputActionType.Value, "<XRHMD>/trackingState", expectedControlType: "Integer"));
        }

        /// <summary>
        /// A minimal plane template so detected surfaces get a faint visualization,
        /// making it obvious where measurement/placement will land.
        /// </summary>
        private static GameObject BuildPlanePrefab()
        {
            var go = new GameObject("ARPlaneVisualizer",
                typeof(ARPlane),
                typeof(MeshFilter),
                typeof(MeshRenderer),
                typeof(ARPlaneMeshVisualizer));

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = MaterialFactory.Transparent(new Color(0.15f, 0.55f, 0.95f, 0.25f));
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            go.SetActive(false);
            return go;
        }
    }
}
