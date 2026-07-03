using System.IO;
using UnityEngine;

namespace ArSpacePlanner.Planner
{
    /// <summary>
    /// Persists furniture arrangements to a JSON file under
    /// <see cref="Application.persistentDataPath"/> so a layout survives app restarts.
    /// Positions are stored in AR session space; they line up again only within the
    /// same tracking session, which is the expected use for a quick "save my plan".
    /// </summary>
    public static class LayoutStore
    {
        private static string FilePath => Path.Combine(Application.persistentDataPath, "layout.json");

        public static bool Exists => File.Exists(FilePath);

        public static void Save(LayoutData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(FilePath, json);
        }

        public static LayoutData Load()
        {
            if (!Exists)
            {
                return new LayoutData();
            }

            string json = File.ReadAllText(FilePath);
            LayoutData data = JsonUtility.FromJson<LayoutData>(json);
            return data ?? new LayoutData();
        }
    }
}
