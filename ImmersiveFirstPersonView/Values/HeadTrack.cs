// ReSharper disable InconsistentNaming

namespace IFPV.Values
{
    internal sealed class HeadTrackEnabled : CameraValueBase
    {
        internal HeadTrackEnabled() => this.Flags |= CameraValueFlags.NoTween | CameraValueFlags.DontUpdateIfDisabled;

        internal override double DefaultValue => 0.0;

        internal override double ChangeSpeed => 1.0;

        internal override string Name => "head tracking enabled";

        internal override double CurrentValue
        {
            get => this.lastValue;

            set
            {
                if (this.lastValue.Equals(value))
                {
                    return;
                }

                this.lastValue = value;

                var plr = NetScriptFramework.SkyrimSE.PlayerCharacter.Instance;
                if (plr != null)
                {
                    plr.IsHeadTrackingEnabled = value > 0.0;
                }
            }
        }

        private double lastValue;
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly HeadTrackEnabled _HeadTrackEnabled = new HeadTrackEnabled();
    }
}