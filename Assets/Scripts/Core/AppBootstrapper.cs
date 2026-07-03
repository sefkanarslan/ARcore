using System.Collections;
using ArBlokEvren.AR;
using ArBlokEvren.Demo;
using ArBlokEvren.Themes;
using ArBlokEvren.UI;
using ArBlokEvren.World;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.ARFoundation;

namespace ArBlokEvren.Core
{
    /// <summary>
    /// Builds the common game systems, then probes for ARCore support and either
    /// wires up the AR depth-scanning rig or a non-AR sandbox so the app is usable
    /// on any Android phone (AR hardware or not).
    /// </summary>
    public sealed class AppBootstrapper : MonoBehaviour
    {
        private ThemeManager _themes;
        private VoxelWorld _world;
        private AppState _state;
        private GameUI _ui;

        private IEnumerator Start()
        {
            _themes = gameObject.AddComponent<ThemeManager>();
            _world = gameObject.AddComponent<VoxelWorld>();
            _world.Init(_themes);

            _state = new AppState();

            _ui = gameObject.AddComponent<GameUI>();
            _ui.Init(_state, _themes, _world);

            yield return ARSession.CheckAvailability();
            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                yield return ARSession.Install();
            }

            bool arSupported =
                ARSession.state == ARSessionState.Ready ||
                ARSession.state == ARSessionState.SessionInitializing ||
                ARSession.state == ARSessionState.SessionTracking;

            if (arSupported)
            {
                SetupAr();
            }
            else
            {
                SetupDemo();
            }
        }

        // ---- AR mode -----------------------------------------------------------

        private void SetupAr()
        {
            ARSession session = BuildSession();
            session.transform.SetParent(transform, false);

            Camera arCamera = BuildOrigin(
                out ARCameraManager cameraManager,
                out AROcclusionManager occlusionManager,
                out ARRaycastManager raycastManager);

            var voxelizer = gameObject.AddComponent<DepthVoxelizer>();
            voxelizer.Init(cameraManager, occlusionManager, arCamera, _world, _state);

            var placer = gameObject.AddComponent<ArBlockPlacer>();
            placer.Init(raycastManager, arCamera, _world, _state);
        }

        private static ARSession BuildSession()
        {
            var go = new GameObject("AR Session");
            go.AddComponent<ARSession>();
            go.AddComponent<ARInputManager>();
            return go.GetComponent<ARSession>();
        }

        private Camera BuildOrigin(
            out ARCameraManager cameraManager,
            out AROcclusionManager occlusionManager,
            out ARRaycastManager raycastManager)
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

            cameraManager = cameraGo.AddComponent<ARCameraManager>();
            cameraGo.AddComponent<ARCameraBackground>();
            occlusionManager = cameraGo.AddComponent<AROcclusionManager>();

            origin.CameraFloorOffsetObject = offsetGo;
            origin.Camera = camera;

            originGo.AddComponent<ARPlaneManager>();
            originGo.AddComponent<ARPointCloudManager>();
            raycastManager = originGo.AddComponent<ARRaycastManager>();

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

        // ---- Non-AR sandbox ----------------------------------------------------

        private void SetupDemo()
        {
            var cameraGo = new GameObject("Demo Camera") { tag = "MainCamera" };
            cameraGo.transform.SetParent(transform, false);

            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.53f, 0.76f, 0.98f);
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 50f;
            cameraGo.AddComponent<AudioListener>();

            DemoWorld.Generate(_world, out Vector3 target);

            var controller = cameraGo.AddComponent<DemoCameraController>();
            controller.Configure(target, 2.4f);

            // Building is the useful default in the sandbox.
            _state.Mode = InteractionMode.Add;

            var placer = gameObject.AddComponent<ArBlockPlacer>();
            placer.InitDemo(camera, _world, _state, 0f);

            _ui.ConfigureDemo(() =>
            {
                _world.Clear();
                DemoWorld.Generate(_world, out _);
            });
        }
    }
}
