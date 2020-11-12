namespace IFPV.Values
{
    internal sealed class PositionFromHead : CameraValueSimple
    {
        internal PositionFromHead() : base(null, Settings.Instance.PositionFromHead, 2.0) { }
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly PositionFromHead PositionFromHead = new PositionFromHead();
    }
}