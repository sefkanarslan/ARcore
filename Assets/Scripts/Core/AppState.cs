using System;
using ArSpacePlanner.Measure;

namespace ArSpacePlanner.Core
{
    /// <summary>
    /// Shared, observable application state. UI and the interaction tools read and
    /// write it and subscribe to change events instead of referencing each other.
    /// </summary>
    public sealed class AppState
    {
        private AppMode _mode = AppMode.Measure;
        private MeasureUnit _unit = MeasureUnit.Meters;

        public event Action<AppMode> ModeChanged;
        public event Action<MeasureUnit> UnitChanged;

        /// <summary>Free-form status line surfaced by the UI (hints, results, errors).</summary>
        public event Action<string> StatusChanged;

        public AppMode Mode
        {
            get => _mode;
            set
            {
                if (_mode == value)
                {
                    return;
                }
                _mode = value;
                ModeChanged?.Invoke(_mode);
            }
        }

        public MeasureUnit Unit
        {
            get => _unit;
            set
            {
                if (_unit == value)
                {
                    return;
                }
                _unit = value;
                UnitChanged?.Invoke(_unit);
            }
        }

        public void SetStatus(string message)
        {
            StatusChanged?.Invoke(message);
        }
    }
}
