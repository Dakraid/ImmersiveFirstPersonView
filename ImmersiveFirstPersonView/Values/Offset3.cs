namespace IFPV.Values
{
    internal sealed class OffsetObjectPositionX : CameraValueSimple
    {
        internal OffsetObjectPositionX() : base(null, 0.0, 20.0) { }
    }

    internal sealed class OffsetObjectPositionY : CameraValueSimple
    {
        internal OffsetObjectPositionY() : base(null, 0.0, 20.0) { }
    }

    internal sealed class OffsetObjectPositionZ : CameraValueSimple
    {
        internal OffsetObjectPositionZ() : base(null, 0.0, 20.0) { }
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly OffsetObjectPositionX OffsetObjectPositionX = new OffsetObjectPositionX();
        internal readonly OffsetObjectPositionY OffsetObjectPositionY = new OffsetObjectPositionY();
        internal readonly OffsetObjectPositionZ OffsetObjectPositionZ = new OffsetObjectPositionZ();
    }
}