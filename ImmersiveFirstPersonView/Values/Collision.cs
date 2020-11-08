namespace IFPV.Values
{
    internal sealed class CollisionEnabled : CameraValueSimple
    {
        internal CollisionEnabled() : base(null, 1.0, 1.0) => this.Flags |= CameraValueFlags.NoTween;
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly CollisionEnabled CollisionEnabled = new CollisionEnabled();
    }
}
