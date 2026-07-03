using UnityEngine;

namespace ArBlokEvren.Core
{
    /// <summary>
    /// Entry point. Spawns the <see cref="AppBootstrapper"/> which decides — at
    /// runtime — whether the device can run AR (ARCore) and either builds the AR rig
    /// or falls back to a non-AR sandbox. The app therefore runs from an empty scene
    /// with no hand-authored scene graph to keep in sync.
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

            var go = new GameObject("ArBlokEvren");
            go.AddComponent<AppBootstrapper>();
        }
    }
}
