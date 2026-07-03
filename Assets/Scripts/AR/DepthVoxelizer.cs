using ArBlokEvren.Blocks;
using ArBlokEvren.Core;
using ArBlokEvren.World;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ArBlokEvren.AR
{
    /// <summary>
    /// Converts the ARCore environment-depth image into voxels each scan tick:
    /// depth pixels are unprojected to world space and written into the voxel world.
    ///
    /// NOTE: depth-image orientation and intrinsics alignment are device dependent;
    /// the unprojection here is a pragmatic approximation and may need per-device
    /// tuning. It is only meaningful on a physical ARCore device with the Depth API.
    /// </summary>
    public sealed class DepthVoxelizer : MonoBehaviour
    {
        [Tooltip("Seconds between scan ticks while scanning is enabled.")]
        public float ScanInterval = 0.25f;

        [Tooltip("Sample every Nth depth pixel (higher = faster, coarser).")]
        public int PixelStride = 8;

        [Tooltip("Maximum voxels written per scan tick.")]
        public int MaxPointsPerTick = 1500;

        public float MinDepth = 0.3f;
        public float MaxDepth = 4.0f;

        private ARCameraManager _cameraManager;
        private AROcclusionManager _occlusionManager;
        private Camera _arCamera;
        private VoxelWorld _world;
        private AppState _state;

        private float _timer;
        private float _floorY = float.MaxValue;

        public void Init(
            ARCameraManager cameraManager,
            AROcclusionManager occlusionManager,
            Camera arCamera,
            VoxelWorld world,
            AppState state)
        {
            _cameraManager = cameraManager;
            _occlusionManager = occlusionManager;
            _arCamera = arCamera;
            _world = world;
            _state = state;
        }

        private void Update()
        {
            if (_state == null || !_state.Scanning)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer < ScanInterval)
            {
                return;
            }

            _timer = 0f;
            ScanOnce();
        }

        private void ScanOnce()
        {
            if (_occlusionManager == null ||
                !_occlusionManager.TryAcquireEnvironmentDepthCpuImage(out XRCpuImage depth))
            {
                return;
            }

            using (depth)
            {
                if (!_cameraManager.TryGetIntrinsics(out XRCameraIntrinsics intr))
                {
                    return;
                }

                WriteVoxels(depth, intr);
            }
        }

        private void WriteVoxels(XRCpuImage depth, XRCameraIntrinsics intr)
        {
            XRCpuImage.Plane plane = depth.GetPlane(0);
            NativeArray<byte> data = plane.data;

            int w = depth.width;
            int h = depth.height;

            // Intrinsics are given for the colour image; scale to the depth image.
            float sx = w / Mathf.Max(1f, intr.resolution.x);
            float sy = h / Mathf.Max(1f, intr.resolution.y);
            float fx = intr.focalLength.x * sx;
            float fy = intr.focalLength.y * sy;
            float cx = intr.principalPoint.x * sx;
            float cy = intr.principalPoint.y * sy;

            bool isFloat = depth.format == XRCpuImage.Format.DepthFloat32;
            Transform cam = _arCamera.transform;

            int written = 0;
            for (int y = 0; y < h && written < MaxPointsPerTick; y += PixelStride)
            {
                for (int x = 0; x < w && written < MaxPointsPerTick; x += PixelStride)
                {
                    float d = ReadDepth(data, plane, x, y, isFloat);
                    if (d < MinDepth || d > MaxDepth)
                    {
                        continue;
                    }

                    float px = (x - cx) * d / fx;
                    float py = -(y - cy) * d / fy;
                    Vector3 world = cam.TransformPoint(new Vector3(px, py, d));

                    if (world.y < _floorY)
                    {
                        _floorY = world.y;
                    }

                    if (_world.SetBlockAtWorld(world, Classify(world.y)))
                    {
                        written++;
                    }
                }
            }
        }

        private BlockType Classify(float worldY)
        {
            if (_floorY == float.MaxValue)
            {
                return BlockType.Stone;
            }

            return worldY <= _floorY + VoxelSettings.VoxelSize * 1.5f ? BlockType.Grass : BlockType.Stone;
        }

        private static float ReadDepth(NativeArray<byte> data, XRCpuImage.Plane plane, int x, int y, bool isFloat)
        {
            int idx = y * plane.rowStride + x * plane.pixelStride;
            if (isFloat)
            {
                if (idx + 3 >= data.Length)
                {
                    return 0f;
                }

                int bits = data[idx] | (data[idx + 1] << 8) | (data[idx + 2] << 16) | (data[idx + 3] << 24);
                return System.BitConverter.Int32BitsToSingle(bits);
            }

            if (idx + 1 >= data.Length)
            {
                return 0f;
            }

            int millimetres = data[idx] | (data[idx + 1] << 8);
            return millimetres / 1000f;
        }
    }
}
