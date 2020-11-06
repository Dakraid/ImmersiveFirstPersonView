using System;
using NetScriptFramework.SkyrimSE;

namespace IFPV
{
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
        internal CameraUpdate(CameraMain     main,
                              CameraResult   result,
                              CameraTarget   target,
                              PlayerCamera   camera,
                              NiAVObject     cameraNode,
                              TESCameraState state,
                              CameraValueMap values)
        {
            CameraMain      = main       ?? throw new ArgumentNullException("main");
            Result          = result     ?? throw new ArgumentNullException("result");
            Target          = target     ?? throw new ArgumentNullException("target");
            GameCamera      = camera     ?? throw new ArgumentNullException("camera");
            GameCameraNode  = cameraNode ?? throw new ArgumentNullException("cameraNode");
            GameCameraState = state      ?? throw new ArgumentNullException("state");
            Values          = values     ?? throw new ArgumentNullException("values");
            CachedMounted   = target.Actor != null && (target.Actor.IsOnMount || target.Actor.IsOnFlyingMount);
        }
    }
}