namespace IFPV
{
    using System;
    using NetScriptFramework.SkyrimSE;

    internal sealed class CameraUpdate
    {
        internal readonly bool           CachedMounted;
        internal readonly CameraMain     CameraMain;
        internal readonly PlayerCamera   GameCamera;
        internal readonly NiAVObject     GameCameraNode;
        internal readonly TESCameraState GameCameraState;
        internal readonly CameraResult   Result;
        internal readonly CameraTarget   Target;
        internal readonly CameraValueMap Values;

        internal CameraUpdate(CameraMain main,
            CameraResult result,
            CameraTarget target,
            PlayerCamera camera,
            NiAVObject cameraNode,
            TESCameraState state,
            CameraValueMap values)
        {
            this.CameraMain      = main       ?? throw new ArgumentNullException("main");
            this.Result          = result     ?? throw new ArgumentNullException("result");
            this.Target          = target     ?? throw new ArgumentNullException("target");
            this.GameCamera      = camera     ?? throw new ArgumentNullException("camera");
            this.GameCameraNode  = cameraNode ?? throw new ArgumentNullException("cameraNode");
            this.GameCameraState = state      ?? throw new ArgumentNullException("state");
            this.Values          = values     ?? throw new ArgumentNullException("values");
            this.CachedMounted   = target.Actor != null && (target.Actor.IsOnMount || target.Actor.IsOnFlyingMount);
        }
    }
}
