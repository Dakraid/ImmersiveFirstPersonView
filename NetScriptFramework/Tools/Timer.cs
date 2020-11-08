using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetScriptFramework.Tools
{
#region Timer class

    /// <summary>
    /// This helps measure time. The timer is not started automatically.
    /// </summary>
    public sealed class Timer
    {
    #region Constructors

        /// <summary>
        /// Create a new timer. The timer is not started automatically.
        /// </summary>
        public Timer() { }

    #endregion

    #region Timer members

        /// <summary>
        /// Get or set time offset. This will be added to time.
        /// </summary>
        public long Offset
        {
            get => Interlocked.Read(ref _offset);
            set => Interlocked.Exchange(ref _offset, value);
        }

        /// <summary>
        /// Get amount of time in milliseconds that have passed since start of timer.
        /// </summary>
        public long Time
        {
            get
            {
                if (!ManualUpdate)
                    Update();

                return Interlocked.Read(ref _value) + Offset;
            }
        }

        /// <summary>
        /// Get or set whether time updates will be performed manually. Default is false, time
        /// is updated every time we ask for it. Manual updates can be useful if timer should only change
        /// at specific locations in code.
        /// </summary>
        public bool ManualUpdate
        {
            get => _manual;
            set => _manual = value;
        }

        /// <summary>
        /// Perform manual time update now.
        /// </summary>
        public void Update() { Interlocked.Exchange(ref _value, _timer.ElapsedTicks * 1000 / Stopwatch.Frequency); }

        /// <summary>
        /// Start timer.
        /// </summary>
        public void Start() { _timer.Start(); }

        /// <summary>
        /// Stop timer.
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
            if (!ManualUpdate)
                Update();
        }

        /// <summary>
        /// Check if timer is started and is currently running.
        /// </summary>
        public bool IsRunning => _timer.IsRunning;

        /// <summary>
        /// Reset and stop timer. Offset is not cleared.
        /// </summary>
        public void Reset() { _timer.Reset(); }

        /// <summary>
        /// Reset and start timer. Offset is not cleared.
        /// </summary>
        public void Restart() { _timer.Restart(); }

    #endregion

    #region Internal members

        private readonly Stopwatch _timer  = new Stopwatch();
        private          long      _offset = 0;
        private          long      _value  = 0;
        private          bool      _manual = false;

    #endregion
    }

#endregion
}