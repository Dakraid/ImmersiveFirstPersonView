﻿using NetScriptFramework.SkyrimSE;

namespace IFPV.States
{
    internal class Bleedout : Passenger
    {
        internal override int Priority => (int) Priorities.Bleedout;

        internal override bool Check(CameraUpdate update)
        {
            if (!update.CameraMain.IsEnabled)
                return false;

            return update.GameCameraState.Id == TESCameraStates.Bleedout;
        }
    }
}