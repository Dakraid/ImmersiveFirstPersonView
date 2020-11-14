namespace NetScriptFramework.Tools
{
    using System.Diagnostics;
    using System.Threading;

#region Timer class

    /// <summary>
    ///     This helps measure time. The timer is not started automatically.
    /// </summary>
    public sealed class Timer
    {
    #region Constructors

    #endregion

    #region Timer members

        /// <summary>
        ///     Get or set time offset. This will be added to time.
        /// </summary>
        public long Offset
        {
            get => Interlocked.Read(ref this._offset);
            set => Interlocked.Exchange(ref this._offset, value);
        }

        /// <summary>
        ///     Get amount of time in milliseconds that have passed since start of timer.
        /// </summary>
        public long Time
        {
            get
            {
                if ( !this.ManualUpdate )
                {
                    this.Update();
                }

                return Interlocked.Read(ref this._value) + this.Offset;
            }
        }

        /// <summary>
        ///     Get or set whether time updates will be performed manually. Default is false, time
        ///     is updated every time we ask for it. Manual updates can be useful if timer should only change
        ///     at specific locations in code.
        /// </summary>
        public bool ManualUpdate { get; set; } = false;

        /// <summary>
        ///     Perform manual time update now.
        /// </summary>
        public void Update() => Interlocked.Exchange(ref this._value, (this._timer.ElapsedTicks * 1000) / Stopwatch.Frequency);

        /// <summary>
        ///     Start timer.
        /// </summary>
        public void Start() => this._timer.Start();

        /// <summary>
        ///     Stop timer.
        /// </summary>
        public void Stop()
        {
            this._timer.Stop();

            if ( !this.ManualUpdate )
            {
                this.Update();
            }
        }

        /// <summary>
        ///     Check if timer is started and is currently running.
        /// </summary>
        public bool IsRunning => this._timer.IsRunning;

        /// <summary>
        ///     Reset and stop timer. Offset is not cleared.
        /// </summary>
        public void Reset() => this._timer.Reset();

        /// <summary>
        ///     Reset and start timer. Offset is not cleared.
        /// </summary>
        public void Restart() => this._timer.Restart();

    #endregion

    #region Internal members

        private readonly Stopwatch _timer = new Stopwatch();
        private          long      _offset;
        private          long      _value;

    #endregion
    }

#endregion
}
