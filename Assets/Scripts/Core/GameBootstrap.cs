using ArBlokEvren.AR;
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
    /// Entry point. Builds the AR rig (session, XR origin, camera + managers) and
    /// all game systems entirely from code, so the app runs from an empty scene and
    /// there is no hand-authored scene graph to keep in sync.
    /// </summary>
    public static class GameBootstrap
    {
        private static bool _started;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (_started)
            {
                return;
            }

            _started = true;

            ARSession session = BuildSession();
            Camera arCamera = BuildOrigin(
                out ARCameraManager cameraManager,
                out AROcclusionManager occlusionManager,
                out ARRaycastManager raycastManager);

            Object.DontDestroyOnLoad(session.transform.root.gameObject);
            Object.DontDestroyOnLoad(arCamera.transform.root.gameObject);

            var gameGo = new GameObject("Game");
            Object.DontDestroyOnLoad(gameGo);

            var themes = gameGo.AddComponent<ThemeManager>();
            var world = gameGo.AddComponent<VoxelWorld>();
            world.Init(themes);

            var state = new AppState();

            var voxelizer = gameGo.AddComponent<DepthVoxelizer>();
            voxelizer.Init(cameraManager, occlusionManager, arCamera, world, state);

            var placer = gameGo.AddComponent<ArBlockPlacer>();
            placer.Init(raycastManager, arCamera, world, state);

            var ui = gameGo.AddComponent<GameUI>();
            ui.Init(state, themes, world);
        }

        private static ARSession BuildSession()
        {
            var go = new GameObject("AR Session");
            go.AddComponent<ARSession>();
            go.AddComponent<ARInputManager>();
            return go.GetComponent<ARSession>();
        }

        private static Camera BuildOrigin(
            out ARCameraManager cameraManager,
            out AROcclusionManager occlusionManager,
            out ARRaycastManager raycastManager)
        {
            var originGo = new GameObject("XR Origin");
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
    }
}
