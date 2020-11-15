namespace IFPV.States
{
    internal class Mounted : CameraState
    {
        internal override int Priority => (int)Priorities.Mounted;

        internal override bool Check(CameraUpdate update)
        {
            if ( !update.CameraMain.IsEnabled )
            {
                return false;
            }

            return update.CachedMounted;
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            update.Values.HideArms.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 0.0);
            update.Values.Show1stPersonArms.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 0.0);
            update.Values.StabilizeHistoryDuration.AddModifier(this, CameraValueModifier.ModifierTypes.SetIfPreviousIsHigherThanThis, 200.0);
            update.Values.StabilizeIgnorePositionZ.AddModifier(this, CameraValueModifier.ModifierTypes.Add, 1.0);
            update.Values.StabilizeIgnorePositionY.AddModifier(this, CameraValueModifier.ModifierTypes.Add, -0.5);
        }
    }
}
