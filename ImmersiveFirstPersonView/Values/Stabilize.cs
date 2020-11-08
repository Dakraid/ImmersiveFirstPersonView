namespace IFPV.Values
{
    internal sealed class StabilizeHistoryDuration : CameraValueSimple
    {
        internal StabilizeHistoryDuration(double value) : base(null, value, 5000.0) { }
    }

    internal sealed class StabilizeIgnorePositionX : CameraValueSimple
    {
        internal StabilizeIgnorePositionX(double value) : base(null, value, 5.0) { }
    }

    internal sealed class StabilizeIgnorePositionY : CameraValueSimple
    {
        internal StabilizeIgnorePositionY(double value) : base(null, value, 5.0) { }
    }

    internal sealed class StabilizeIgnorePositionZ : CameraValueSimple
    {
        internal StabilizeIgnorePositionZ(double value) : base(null, value, 5.0) { }
    }

    internal sealed class StabilizeIgnoreRotationX : CameraValueSimple
    {
        internal StabilizeIgnoreRotationX(double value) : base(null, value, 30.0) { }
    }

    internal sealed class StabilizeIgnoreRotationY : CameraValueSimple
    {
        internal StabilizeIgnoreRotationY(double value) : base(null, value, 30.0) { }
    }

    internal sealed class StabilizeIgnoreOffsetX : CameraValueSimple
    {
        internal StabilizeIgnoreOffsetX(double value) : base(null, value, 720.0) =>
            this.Formula = TValue.TweenTypes.Decelerating;
    }

    internal sealed class StabilizeIgnoreOffsetY : CameraValueSimple
    {
        internal StabilizeIgnoreOffsetY(double value) : base(null, value, 720.0) =>
            this.Formula = TValue.TweenTypes.Decelerating;
    }
}

namespace IFPV
{
    using Values;

    internal partial class CameraValueMap
    {
        internal readonly StabilizeHistoryDuration StabilizeHistoryDuration =
            new StabilizeHistoryDuration(Settings.Instance.StabilizeHistoryDuration * 1000.0f);

        internal readonly StabilizeIgnoreOffsetX StabilizeIgnoreOffsetX =
            new StabilizeIgnoreOffsetX(Settings.Instance.StabilizeIgnoreOffsetX);

        internal readonly StabilizeIgnoreOffsetY StabilizeIgnoreOffsetY =
            new StabilizeIgnoreOffsetY(Settings.Instance.StabilizeIgnoreOffsetY);

        internal readonly StabilizeIgnorePositionX StabilizeIgnorePositionX =
            new StabilizeIgnorePositionX(Settings.Instance.StabilizeIgnorePositionX);

        internal readonly StabilizeIgnorePositionY StabilizeIgnorePositionY =
            new StabilizeIgnorePositionY(Settings.Instance.StabilizeIgnorePositionY);

        internal readonly StabilizeIgnorePositionZ StabilizeIgnorePositionZ =
            new StabilizeIgnorePositionZ(Settings.Instance.StabilizeIgnorePositionZ);

        internal readonly StabilizeIgnoreRotationX StabilizeIgnoreRotationX =
            new StabilizeIgnoreRotationX(Settings.Instance.StabilizeIgnoreRotationX);

        internal readonly StabilizeIgnoreRotationY StabilizeIgnoreRotationY =
            new StabilizeIgnoreRotationY(Settings.Instance.StabilizeIgnoreRotationY);
    }
}
