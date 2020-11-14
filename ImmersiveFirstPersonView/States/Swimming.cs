namespace IFPV.States
{
    using NetScriptFramework;

    internal class Swimming : CameraState
    {
        internal override int Priority => (int)Priorities.Swimming;

        internal override bool Check(CameraUpdate update)
        {
            if ( !update.CameraMain.IsEnabled )
            {
                return false;
            }

            var actor = update.Target.Actor;

            if ( actor == null )
            {
                return false;
            }

            var flags = Memory.ReadUInt32(actor.Address + 0xC0) & 0x3FFF;
            return (flags & 0x400) != 0;
        }

        internal override void OnEntering(CameraUpdate update) => base.OnEntering(update);
    }
}
