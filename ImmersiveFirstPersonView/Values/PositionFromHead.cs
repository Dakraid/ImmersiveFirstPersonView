using IFPV.Values;

namespace IFPV.Values
{
    internal sealed class PositionFromHead : CameraValueSimple
    {
        internal PositionFromHead() : base(null, Settings.Instance.PositionFromHead, 2.0) { }
    }
}

namespace IFPV
{
    internal partial class CameraValueMap
    {
        internal readonly PositionFromHead PositionFromHead = new PositionFromHead();
    }
}