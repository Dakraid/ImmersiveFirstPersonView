using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using Main = NetScriptFramework.Main;

namespace IFPV
{
    internal sealed class CameraResult : TemporaryObject
    {
        private MemoryAllocation Allocation;

        internal CameraResult()
        {
            Allocation           = Memory.Allocate(0x34);
            Transform            = MemoryObject.FromAddress<NiTransform>(Allocation.Address);
            Transform.Position.X = 0.0f;
            Transform.Position.Y = 0.0f;
            Transform.Position.Z = 0.0f;
            Transform.Rotation.Identity(1.0f);
            Transform.Scale = 1.0f;
        }

        internal NiTransform Transform { get; private set; }

        protected override void Free()
        {
            if (Main.IsShutdown)
                return;

            Transform = null;

            if (Allocation != null)
            {
                Allocation.Dispose();
                Allocation = null;
            }
        }
    }
}