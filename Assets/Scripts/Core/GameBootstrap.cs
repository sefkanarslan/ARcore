using UnityEngine;

namespace ArSpacePlanner.Core
{
    /// <summary>
    /// Entry point. Runs from an empty scene and spawns the <see cref="AppBootstrapper"/>
    /// which builds the AR rig and all systems from code, so there is no hand-authored
    /// scene graph to keep in sync.
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

            var go = new GameObject("ArSpacePlanner");
            go.AddComponent<AppBootstrapper>();
        }
    }
}
