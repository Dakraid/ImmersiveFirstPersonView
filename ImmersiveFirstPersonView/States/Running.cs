using NetScriptFramework;

namespace IFPV.States
{
    internal class Running : CameraState
    {
        internal override int Priority => (int) Priorities.Running;

        internal override bool Check(CameraUpdate update)
        {
            if (!update.CameraMain.IsEnabled)
                return false;

            if (update.CachedMounted)
                return false;

            var actor = update.Target.Actor;
            if (actor == null)
                return false;

            var flags = Memory.ReadUInt32(actor.Address + 0xC0) & 0x3FFF;
            return (flags & 0x180) == 0x80;
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            update.Values.StabilizeIgnoreOffsetY.AddModifier(
                this, CameraValueModifier.ModifierTypes.SetIfPreviousIsLowerThanThis, 23.0, true, 200);
            AddHeadBobModifier(update);
        }
    }
}