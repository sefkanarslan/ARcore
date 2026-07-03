using ArBlokEvren.Blocks;

namespace ArBlokEvren.Core
{
    public enum InteractionMode
    {
        /// <summary>Tapping places a block on the face you tapped.</summary>
        Add,

        /// <summary>Tapping removes the block you tapped.</summary>
        Break
    }

    /// <summary>Mutable, UI-facing application state shared across systems.</summary>
    public sealed class AppState
    {
        public InteractionMode Mode = InteractionMode.Break;
        public bool Scanning;
        public BlockType SelectedBlock = BlockType.Stone;
    }
}
