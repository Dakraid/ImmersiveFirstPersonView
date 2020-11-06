using IFPV.Values;

namespace IFPV.Values
{
    internal sealed class ThirdPersonArrowTilt : IniValue
    {
        internal ThirdPersonArrowTilt() : base("f3PArrowTiltUpAngle:Combat") { }
    }
}

namespace IFPV
{
    internal partial class CameraValueMap
    {
        internal readonly ThirdPersonArrowTilt ThirdPersonArrowTilt = new ThirdPersonArrowTilt();
    }
}