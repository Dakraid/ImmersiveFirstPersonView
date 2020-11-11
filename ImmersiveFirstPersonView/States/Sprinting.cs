namespace IFPV.States
{
    using NetScriptFramework;

    internal class Sprinting : CameraState
    {
        internal override int Priority => (int)Priorities.Sprinting;

        internal override bool Check(CameraUpdate update)
        {
            if (!update.CameraMain.IsEnabled)
            {
                return false;
            }

            if (update.CachedMounted)
            {
                return false;
            }

            var actor = update.Target.Actor;
            if (actor == null)
            {
                return false;
            }

            if (actor.IsSneaking)
            {
                return false;
            }

            var flags = Memory.ReadUInt32(actor.Address + 0xC0) & 0x3FFF;
            return (flags & 0x100) != 0;
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            update.Values.StabilizeIgnoreOffsetY.AddModifier(this, CameraValueModifier.ModifierTypes.SetIfPreviousIsLowerThanThis, 34.0, true, 200);
            this.AddHeadBobModifier(update, false, true);
        }
    }
}
