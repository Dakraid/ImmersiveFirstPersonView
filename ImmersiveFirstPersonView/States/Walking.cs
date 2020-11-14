namespace IFPV.States
{
    using NetScriptFramework;

    internal class Walking : CameraState
    {
        internal override int Priority => (int)Priorities.Walking;

        internal override bool Check(CameraUpdate update)
        {
            if ( !update.CameraMain.IsEnabled )
            {
                return false;
            }

            if ( update.CachedMounted )
            {
                return false;
            }

            var actor = update.Target.Actor;

            if ( actor == null )
            {
                return false;
            }

            var flags = Memory.ReadUInt32(actor.Address + 0xC0) & 0x3FFF;
            return (flags & 0x1C0) == 0x40;
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            this.AddHeadBobModifier(update);
        }
    }
}
