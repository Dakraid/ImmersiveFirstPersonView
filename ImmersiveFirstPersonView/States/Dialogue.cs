namespace IFPV.States
{
    using NetScriptFramework.SkyrimSE;

    internal class Dialogue : CameraState
    {
        internal override int Priority => (int)Priorities.Dialogue;

        internal override bool Check(CameraUpdate update)
        {
            if (!update.CameraMain.IsEnabled)
            {
                return false;
            }

            var mm = MenuManager.Instance;
            if (mm == null)
            {
                return false;
            }

            return mm.IsMenuOpen("Dialogue Menu");
        }

        internal override void OnEntering(CameraUpdate update) => base.OnEntering(update);

        //update.Values.FaceCamera.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 0);
    }
}
