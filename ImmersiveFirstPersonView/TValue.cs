namespace IFPV
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Scriptable value implementation.
    /// </summary>
    internal sealed class TValue
    {
        /// <summary>
        ///     The default amount.
        /// </summary>
        internal readonly double DefaultAmount;

        /// <summary>
        ///     The maximum amount.
        /// </summary>
        internal readonly double MaxAmount;

        /// <summary>
        ///     The minimum amount.
        /// </summary>
        internal readonly double MinAmount;

        /// <summary>
        ///     The tween data.
        /// </summary>
        private readonly List<TweenData> Tween = new List<TweenData>(4);

        /// <summary>
        ///     The value tween is paused.
        /// </summary>
        private long? Paused;

        /// <summary>
        ///     The paused counter.
        /// </summary>
        private int PausedCounter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TValue" /> class.
        /// </summary>
        /// <param name="defaultAmount">The default amount.</param>
        /// <param name="minAmount">The minimum amount.</param>
        /// <param name="maxAmount">The maximum amount.</param>
        internal TValue(double defaultAmount, double minAmount, double maxAmount)
        {
            this.DefaultAmount = defaultAmount;
            this.CurrentAmount = defaultAmount;
            this.MinAmount     = minAmount;
            this.MaxAmount     = maxAmount;
        }

        /// <summary>
        ///     Gets the current amount.
        /// </summary>
        internal double CurrentAmount { get; private set; }

        /// <summary>
        ///     Modifies the value by the specified amount. This will clear all tweens!
        /// </summary>
        /// <param name="amount">The amount.</param>
        internal void Mod(double amount)
        {
            this.Tween.Clear();

            if ( amount == 0.0 )
            {
                return;
            }

            var target = this.CurrentAmount + amount;
            target             = Math.Max(this.MinAmount, Math.Min(this.MaxAmount, target));
            this.CurrentAmount = target;
        }

        /// <summary>
        ///     Pauses the tween updates.
        /// </summary>
        /// <param name="now">The now.</param>
        internal void Pause(long now)
        {
            if ( ++this.PausedCounter == 1 )
            {
                this.Paused = now;
            }
        }

        /// <summary>
        ///     Sets to the specified value. This will clear all tweens!
        /// </summary>
        /// <param name="value">The value.</param>
        internal void SetTo(double value)
        {
            this.Tween.Clear();
            this.CurrentAmount = Math.Max(this.MinAmount, Math.Min(this.MaxAmount, value));
        }

        /// <summary>
        ///     Tweens to the specified value.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="time">The time.</param>
        /// <param name="type">The type.</param>
        /// <param name="replace">if set to <c>true</c> [replace].</param>
        internal void TweenTo(double target, long time, TweenTypes type, bool replace)
        {
            target = Math.Max(this.MinAmount, Math.Min(this.MaxAmount, target));

            var t = TweenData.Setup(target, time, type);

            if ( replace )
            {
                this.Tween.Clear();
            }

            this.Tween.Add(t);
        }

        /// <summary>
        ///     Tweens to the specified value.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="speed">The speed.</param>
        /// <param name="type">The type.</param>
        /// <param name="replace">if set to <c>true</c> [replace].</param>
        internal void TweenTo(double target, double speed, TweenTypes type, bool replace)
        {
            target = Math.Max(this.MinAmount, Math.Min(this.MaxAmount, target));

            var t = TweenData.Setup(target, speed, type);

            if ( replace )
            {
                this.Tween.Clear();
            }

            this.Tween.Add(t);
        }

        /// <summary>
        ///     Unpauses the tween updates.
        /// </summary>
        /// <param name="now">The now.</param>
        internal void Unpause(long now)
        {
            if ( --this.PausedCounter == 0 )
            {
                long diff = 0;

                if ( this.Paused.HasValue )
                {
                    diff        = this.Paused.Value;
                    this.Paused = null;
                    diff        = now - diff;
                }

                if ( diff > 0 )
                {
                    foreach ( var t in this.Tween )
                    {
                        if ( t.BeginTime.HasValue )
                        {
                            t.BeginTime = t.BeginTime.Value + diff;
                        }

                        if ( t.EndTime.HasValue )
                        {
                            t.EndTime = t.EndTime.Value + diff;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Updates the value.
        /// </summary>
        /// <param name="now">The time now.</param>
        internal void Update(long now)
        {
            while ( this.PausedCounter <= 0 && this.Tween.Count != 0 )
            {
                var t = this.Tween[0];

                // Start this tween.
                if ( !t.EndTime.HasValue )
                {
                    t.BeginTime = now;
                    long time = 0;

                    if ( t.Duration.HasValue ) { time = t.Duration.Value; }
                    else if ( t.Speed.HasValue )
                    {
                        var dur = (Math.Abs(t.EndAmount - this.CurrentAmount) / Math.Max(t.Speed.Value, 0.00001)) * 1000.0;

                        if ( dur > 60000.0 )
                        {
                            dur = 60000.0;
                        }

                        time = (long)dur;
                    }

                    t.EndTime     = now + time;
                    t.BeginAmount = this.CurrentAmount;
                }

                // Process this tween.

                // Finished.
                if ( now >= t.EndTime.Value )
                {
                    this.CurrentAmount = t.EndAmount;
                    this.Tween.RemoveAt(0);
                    continue;
                }

                // Updating.
                var ratio = (now - t.BeginTime.Value) / (double)(t.EndTime.Value - t.BeginTime.Value);
                ratio = Math.Min(1.0, Math.Max(0.0, ratio));
                ratio = Utility.ApplyFormula(ratio, t.Type);

                var amount = ((t.EndAmount - t.BeginAmount.Value) * ratio) + t.BeginAmount.Value;
                amount             = Math.Min(this.MaxAmount, Math.Max(this.MinAmount, amount));
                this.CurrentAmount = amount;

                break;
            }
        }

        /// <summary>
        ///     List of formulas for tweening value.
        /// </summary>
        internal enum TweenTypes
        {
            /// <summary>
            ///     Linear formula, no accelerations.
            /// </summary>
            Linear = 0,

            /// <summary>
            ///     Value change starts slow and is accelerating until finished.
            /// </summary>
            Accelerating = 1,

            /// <summary>
            ///     Value change starts fast and is decelerating until finished.
            /// </summary>
            Decelerating = 2,

            /// <summary>
            ///     Value change starts slow and ends slow while picking up speed in the middle.
            /// </summary>
            AccelAndDecel = 3
        }

        private sealed class TweenData
        {
            internal double?    BeginAmount;
            internal long?      BeginTime;
            internal long?      Duration;
            internal double     EndAmount;
            internal long?      EndTime;
            internal double?    Speed;
            internal TweenTypes Type;

            private TweenData() { }

            internal static TweenData Setup(double targetValue, long time, TweenTypes type)
            {
                var t = new TweenData { EndAmount = targetValue, Duration = time, Type = type };
                return t;
            }

            internal static TweenData Setup(double targetValue, double speed, TweenTypes type)
            {
                var t = new TweenData { EndAmount = targetValue, Speed = speed, Type = type };
                return t;
            }
        }
    }
}
