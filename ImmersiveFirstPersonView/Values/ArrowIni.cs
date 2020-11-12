namespace IFPV.Values
{
    internal sealed class ThirdPersonArrowTilt : IniValue
    {
        internal ThirdPersonArrowTilt() : base("f3PArrowTiltUpAngle:Combat") { }
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly ThirdPersonArrowTilt ThirdPersonArrowTilt = new ThirdPersonArrowTilt();
    }
}