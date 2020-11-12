namespace IFPV.States
{
    internal class Weapon : CameraState
    {
        internal override int Priority => (int)Priorities.Weapon;

        internal override bool Check(CameraUpdate update)
        {
            if (!update.CameraMain.IsEnabled)
            {
                return false;
            }

            var actor = update.Target.Actor;
            return actor != null && actor.IsWeaponDrawn;
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            if (Settings.Instance.ShowNormalFirstPersonArms)
            {
                update.Values.HideArms.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
                update.Values.Show1stPersonArms.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
            }

            update.Values.ThirdPersonArrowTilt.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.5);
        }
    }
}