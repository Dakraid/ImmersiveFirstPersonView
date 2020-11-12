namespace IFPV
{
    using NetScriptFramework;
    using NetScriptFramework.SkyrimSE;
    using Main = NetScriptFramework.Main;

    internal sealed class CameraResult : TemporaryObject
    {
        private MemoryAllocation Allocation;

        internal CameraResult()
        {
            this.Allocation = Memory.Allocate(0x34);
            this.Transform = MemoryObject.FromAddress<NiTransform>(this.Allocation.Address);
            this.Transform.Position.X = 0.0f;
            this.Transform.Position.Y = 0.0f;
            this.Transform.Position.Z = 0.0f;
            this.Transform.Rotation.Identity(1.0f);
            this.Transform.Scale = 1.0f;
        }

        internal NiTransform Transform { get; private set; }

        protected override void Free()
        {
            if (Main.IsShutdown)
            {
                return;
            }

            this.Transform = null;

            if (this.Allocation != null)
            {
                this.Allocation.Dispose();
                this.Allocation = null;
            }
        }
    }
}