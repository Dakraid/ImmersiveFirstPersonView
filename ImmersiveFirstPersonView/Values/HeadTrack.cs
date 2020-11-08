namespace IFPV.Values
{
    using NetScriptFramework.SkyrimSE;

    internal sealed class HeadTrackEnabled : CameraValueBase
    {
        internal HeadTrackEnabled() => this.Flags |= CameraValueFlags.NoTween | CameraValueFlags.DontUpdateIfDisabled;

        internal override double ChangeSpeed => 1.0;

        internal override double CurrentValue
        {
            get
            {
                var plr = PlayerCharacter.Instance;
                if (plr != null)
                {
                    return plr.IsHeadTrackingEnabled ? 1 : 0;
                }

                return 0;
            }

            set
            {
                var plr = PlayerCharacter.Instance;
                if (plr != null)
                {
                    plr.IsHeadTrackingEnabled = value > 0.0;
                }
            }
        }

        internal override double DefaultValue => 0.0;

        internal override string Name => "head tracking enabled";
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly HeadTrackEnabled HeadTrackEnabled = new HeadTrackEnabled();
    }
}
