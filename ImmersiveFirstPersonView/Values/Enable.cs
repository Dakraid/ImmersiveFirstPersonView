namespace IFPV.Values
{
    internal sealed class WantEnabled : CameraValueSimple
    {
        internal WantEnabled() : base(null, 0.0, 1.0) => this.Flags |= CameraValueFlags.NoTween;
    }

    internal sealed class WantDisabled : CameraValueSimple
    {
        internal WantDisabled() : base(null, 0.0, 1.0) => this.Flags |= CameraValueFlags.NoTween;
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly WantDisabled WantDisabled = new WantDisabled();
        internal readonly WantEnabled  WantEnabled  = new WantEnabled();
    }
}
