namespace IFPV.States
{
    using NetScriptFramework;
    using NetScriptFramework.SkyrimSE;

    internal class Grabbing : CameraState
    {
        internal override int Priority => (int)Priorities.Grabbing;

        internal override bool Check(CameraUpdate update)
        {
            if (!update.CameraMain.IsEnabled)
            {
                return false;
            }

            var plr = PlayerCharacter.Instance;
            if (plr == null)
            {
                return false;
            }

            var refHandle = Memory.ReadUInt32(plr.Address + 0x8C8);
            if (refHandle == 0)
            {
                return false;
            }

            using (var objRef = new ObjectRefHolder(refHandle)) { return objRef.IsValid; }
        }

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            update.Values.FaceCamera.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1);
        }
    }
}
