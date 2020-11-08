namespace NetScriptFramework.Tools
{
    using System;
    using System.Threading;

    /// <summary>
    ///     Unique ID generator.
    /// </summary>
    internal sealed class UIDGenerator
    {
        /// <summary>
        ///     The high part for unique IDs. This is generated from timestamp.
        /// </summary>
        private readonly ulong HighPart;

        /// <summary>
        ///     The low part for unique IDs. This is a counter.
        /// </summary>
        private int LowPart;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UIDGenerator" /> class.
        /// </summary>
        internal UIDGenerator()
        {
            var now = DateTime.UtcNow;
            var min = new DateTime(1990, 1, 1);
            ulong high = 0;
            if (now >= min)
            {
                high = unchecked((ulong)(now - min).TotalSeconds);
                if (high > int.MaxValue)
                {
                    high = 0;
                }
            }

            if (high == 0)
            {
                high = (ulong)Randomizer.NextInt(1, int.MaxValue);
            }

            this.HighPart = high << 32;
        }

        /// <summary>
        ///     Generates a unique ID.
        /// </summary>
        /// <returns></returns>
        internal long Generate()
        {
            var result = this.HighPart;
            var low = Interlocked.Increment(ref this.LowPart);
            if (low == 0)
            {
                Main.CriticalException("UIDGenerator encountered an overflow! Too many IDs were generated.", true);
                return 0;
            }

            result |= unchecked((uint)low);
            return unchecked((long)result);
        }
    }
}
