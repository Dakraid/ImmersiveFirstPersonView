namespace IFPV.States
{
    using NetScriptFramework;

    internal class SneakRoll : CameraState
    {
        internal override int Priority => (int)Priorities.SneakRoll;

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

            if (!actor.IsSneaking)
            {
                return false;
            }

            var flags = Memory.ReadUInt32(actor.Address + 0xC0) & 0x3FFF;
            return (flags & 0x100) != 0;
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            //update.Values.StabilizeHistoryDuration.AddModifier(this, CameraValueModifier.ModifierTypes.SetIfPreviousIsHigherThanThis, 100.0);
            update.Values.RotationFromHead.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
            update.Values.PositionFromHead.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
        }
    }
}