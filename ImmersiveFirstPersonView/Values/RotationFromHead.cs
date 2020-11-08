namespace IFPV.Values
{
    internal sealed class RotationFromHead : CameraValueSimple
    {
        internal RotationFromHead() : base(null, Settings.Instance.RotationFromHead, 1.0) { }
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly RotationFromHead RotationFromHead = new RotationFromHead();
    }
}
