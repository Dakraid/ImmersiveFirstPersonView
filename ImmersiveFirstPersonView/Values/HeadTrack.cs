// ReSharper disable InconsistentNaming

namespace IFPV.Values
{
    using NetScriptFramework.SkyrimSE;

    internal sealed class HeadTrackEnabled : CameraValueBase
    {
        private double lastValue;
        internal HeadTrackEnabled() => this.Flags |= CameraValueFlags.NoTween | CameraValueFlags.DontUpdateIfDisabled;

        internal override double DefaultValue => 0.0;

        internal override double ChangeSpeed => 1.0;

        internal override string Name => "head tracking enabled";

        internal override double CurrentValue
        {
            get => this.lastValue;

            set
            {
                if ( this.lastValue.Equals(value) )
                {
                    return;
                }

                this.lastValue = value;

                var plr = PlayerCharacter.Instance;

                if ( plr != null )
                {
                    plr.IsHeadTrackingEnabled = value > 0.0;
                }
            }
        }
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
