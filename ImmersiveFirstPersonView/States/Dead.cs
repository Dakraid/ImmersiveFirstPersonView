namespace IFPV.States
{
    internal class Dead : Passenger
    {
        internal override int Priority => (int) Priorities.Dead;

        internal override bool Check(CameraUpdate update)
        {
            if (!update.CameraMain.IsEnabled)
                return false;

            var actor = update.Target.Actor;
            if (actor == null)
                return false;

            return actor.IsDead;
        }
    }
}