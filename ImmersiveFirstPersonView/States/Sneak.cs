namespace IFPV.States
{
    internal class Sneak : CameraState
    {
        internal override int Priority => (int)Priorities.Sneaking;

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
            return actor != null && actor.IsSneaking;
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            update.Values.StabilizeIgnoreOffsetY.AddModifier(
                this, CameraValueModifier.ModifierTypes.SetIfPreviousIsLowerThanThis, 34.0, true, 300);
        }
    }
}
