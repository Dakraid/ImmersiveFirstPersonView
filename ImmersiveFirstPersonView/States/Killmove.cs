namespace IFPV.States
{
    internal class Killmove : Passenger
    {
        internal override int Priority => (int)Priorities.Killmove;

        internal override bool Check(CameraUpdate update)
        {
            if (!update.CameraMain.IsEnabled)
            {
                return false;
            }

            var actor = update.Target.Actor;
            return actor != null && actor.IsInKillmove;
        }

        internal override void OnEntering(CameraUpdate update) => base.OnEntering(update);
    }
}