namespace IFPV.States
{
    using NetScriptFramework.SkyrimSE;

    internal class Jumping : CameraState
    {
        internal override int Priority => (int)Priorities.Jumping;

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

            var state = actor.MovementState;
            return state == bhkCharacterStateTypes.Jumping || state == bhkCharacterStateTypes.InAir;
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            update.Values.StabilizeIgnoreOffsetX.AddModifier(this, CameraValueModifier.ModifierTypes.SetIfPreviousIsLowerThanThis, 25.0, true, 500);
            update.Values.StabilizeIgnoreOffsetY.AddModifier(this, CameraValueModifier.ModifierTypes.SetIfPreviousIsLowerThanThis, 37.0, true, 700);
        }
    }
}
