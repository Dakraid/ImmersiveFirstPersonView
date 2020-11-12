namespace IFPV.Values
{
    internal sealed class Offset1PositionX : CameraValueSimple
    {
        internal Offset1PositionX() : base(null, Settings.Instance.BaseOffsetX, 10.0) { }
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly Offset1PositionX Offset1PositionX = new Offset1PositionX();
    }
}

namespace IFPV.Values
{
    internal sealed class Offset1PositionY : CameraValueSimple
    {
        internal Offset1PositionY() : base(null, Settings.Instance.BaseOffsetY, 10.0) { }
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly Offset1PositionY Offset1PositionY = new Offset1PositionY();
    }
}

namespace IFPV.Values
{
    internal sealed class Offset1PositionZ : CameraValueSimple
    {
        internal Offset1PositionZ() : base(null, Settings.Instance.BaseOffsetZ, 10.0) { }
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly Offset1PositionZ Offset1PositionZ = new Offset1PositionZ();
    }
}

namespace IFPV.Values
{
    using System;

    internal sealed class Offset1RotationX : CameraValueSimple
    {
        internal Offset1RotationX() : base(null, 0.0, Math.PI * 0.5) { }
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly Offset1RotationX Offset1RotationX = new Offset1RotationX();
    }
}

namespace IFPV.Values
{
    using System;

    internal sealed class Offset1RotationY : CameraValueSimple
    {
        internal Offset1RotationY() : base(null, 0.0, Math.PI * 0.5) { }
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly Offset1RotationY Offset1RotationY = new Offset1RotationY();
    }
}