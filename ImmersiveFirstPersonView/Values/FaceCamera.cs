﻿using IFPV.Values;

namespace IFPV.Values
{
    internal sealed class FaceCamera : CameraValueSimple
    {
        internal FaceCamera() : base(null, 0.0, 1.0) { Flags |= CameraValueFlags.NoTween; }
    }

    internal sealed class ActorTurnTime : CameraValueSimple
    {
        internal ActorTurnTime() : base(null, 0.0, 1.0) { Flags |= CameraValueFlags.NoTween; }
    }
}

namespace IFPV
{
    internal partial class CameraValueMap
    {
        internal readonly ActorTurnTime ActorTurnTime = new ActorTurnTime();
        internal readonly FaceCamera    FaceCamera    = new FaceCamera();
    }
}