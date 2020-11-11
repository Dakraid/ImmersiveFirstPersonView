namespace IFPV.States
{
    using System;
    using NetScriptFramework;
    using NetScriptFramework.SkyrimSE;

    internal sealed class Default : CameraState
    {
        internal static float _look_downoffset_ratio;
        internal static float _look_downoffset_ratio_leftrightmove;

        internal static int CantAutoTurnCounter = 0;

        private CameraValueModifier _autoTurnAngleMod;
        private long                _autoTurnTime;

        private byte                _hadVanityMode;
        private CameraValueModifier _lastCollidedRestrict;
        private float               _lastNearClip;
        private CameraValueModifier _nearClip;

        internal override int Priority => (int)Priorities.Default;

        internal override bool Check(CameraUpdate update) => update.CameraMain.IsEnabled;

        internal override void OnEntering(CameraUpdate update)
        {
            base.OnEntering(update);

            this._hadVanityMode                = update.GameCamera.EnableVanityMode;
            update.GameCamera.EnableVanityMode = 0;

            update.Values.ActorTurnTime.AddModifier(this,
                CameraValueModifier.ModifierTypes.Set,
                Settings.Instance.ActorTurnTime);

            update.Values.BlockPlayerFadeOut.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
            this.UpdateNearClip(update, null);
            if (Settings.Instance.HeadTrackEnable)
            {
                update.Values._HeadTrackEnabled.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
            }

            if (Settings.Instance.AlwaysForceAutoTurn)
            {
                update.Values.FaceCamera.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
            }

            if (Settings.Instance.HideHead || Settings.Instance.HideHelmet)
            {
                update.Values.HideHead.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
            }

            if (Settings.Instance.HideArms)
            {
                update.Values.HideArms.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
            }

            if (Settings.Instance.ExtraResponsiveControls)
            {
                update.Values.ExtraResponsiveControls.AddModifier(this, CameraValueModifier.ModifierTypes.Set, 1.0);
            }
        }

        internal override void OnLeaving(CameraUpdate update)
        {
            base.OnLeaving(update);

            //update.GameCamera.EnableVanityMode = _hadVanityMode;

            if (this._autoTurnAngleMod != null)
            {
                this._autoTurnAngleMod.Remove();
                this._autoTurnAngleMod = null;
            }

            if (this._lastCollidedRestrict != null)
            {
                this._lastCollidedRestrict.Remove();
                this._lastCollidedRestrict = null;
            }

            if (this._nearClip != null)
            {
                this._nearClip.Remove();
                this._nearClip = null;
            }

            _look_downoffset_ratio               = 0.0f;
            _look_downoffset_ratio_leftrightmove = 0.0f;
        }

        internal override void Update(CameraUpdate update)
        {
            base.Update(update);

            if (update.GameCamera.EnableVanityMode != 0)
            {
                this._hadVanityMode                = 1;
                update.GameCamera.EnableVanityMode = 0;
            }

            var hadX = false;
            var hadY = false;
            var x    = update.Values.InputRotationX.CurrentValue;
            var y    = update.Values.InputRotationY.CurrentValue;

            if (update.CameraMain.DidCollideLastUpdate)
            {
                if (this._lastCollidedRestrict == null)
                {
                    this._lastCollidedRestrict = update.Values.RestrictDown.AddModifier(this, CameraValueModifier.ModifierTypes.Set, Settings.Instance.MaximumDownAngleCollided, false);
                }
            }
            else
            {
                if (this._lastCollidedRestrict != null)
                {
                    this._lastCollidedRestrict.Remove();
                    this._lastCollidedRestrict = null;
                }
            }

            double autoTurn = Settings.Instance.ForceAutoTurnOnAngle;
            if (autoTurn < 360.0 && CantAutoTurnCounter == 0)
            {
                if (Utility.RadToDeg(Math.Abs(x)) >= autoTurn)
                {
                    if (this._autoTurnAngleMod == null)
                    {
                        this._autoTurnAngleMod =
                            update.Values.FaceCamera.AddModifier(this,
                                CameraValueModifier.ModifierTypes.Set,
                                1.0,
                                false);
                    }

                    this._autoTurnTime                        = update.CameraMain.Plugin.Time + 500;
                    update.CameraMain._LastTurnIsFromAutoTurn = update.CameraMain.Plugin.Time + 50;
                }
                else if (this._autoTurnAngleMod != null)
                {
                    if (update.CameraMain.Plugin.Time >= this._autoTurnTime)
                    {
                        this._autoTurnAngleMod.Remove();
                        this._autoTurnAngleMod = null;
                    }
                }
            }
            else if (this._autoTurnAngleMod != null)
            {
                this._autoTurnAngleMod.Remove();
                this._autoTurnAngleMod = null;
            }

            var xmod = 1.0;

            if (y < 0.0)
            {
                var angle = update.Values.RestrictDown.CurrentValue;
                if (angle < 360.0)
                {
                    var restrict = -Utility.DegToRad(angle);
                    if (y < restrict)
                    {
                        y    = restrict;
                        xmod = 0.0;
                        hadY = true;
                    }
                    else
                    {
                        var angle2 = update.Values.RestrictSideDown.CurrentValue;
                        if (angle2 > 0.0)
                        {
                            var restrict2 = -Utility.DegToRad(angle - angle2);
                            if (y < restrict2)
                            {
                                var dy = y        - restrict2;
                                var dt = restrict - restrict2;
                                if (dt != 0.0)
                                {
                                    xmod = 1.0 - Utility.ApplyFormula(dy / dt, TValue.TweenTypes.Linear); // Accel?
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                var angle = update.Values.RestrictUp.CurrentValue;
                if (angle < 360.0)
                {
                    var restrict = Utility.DegToRad(angle);
                    if (y > restrict)
                    {
                        y    = restrict;
                        hadY = true;
                    }
                }
            }

            if (x < 0.0)
            {
                var angle = update.Values.RestrictLeft.CurrentValue;
                if (angle < 360.0)
                {
                    if (xmod != 1.0)
                    {
                        var angle2 = update.Values.RestrictLeft2.CurrentValue;
                        angle = ((angle - angle2) * xmod) + angle2;
                    }

                    var restrict = -Utility.DegToRad(angle);
                    if (x < restrict)
                    {
                        x    = restrict;
                        hadX = true;
                    }
                }
            }
            else
            {
                var angle = update.Values.RestrictRight.CurrentValue;
                if (angle < 360.0)
                {
                    if (xmod != 1.0)
                    {
                        var angle2 = update.Values.RestrictRight2.CurrentValue;
                        angle = ((angle - angle2) * xmod) + angle2;
                    }

                    var restrict = Utility.DegToRad(angle);
                    if (x > restrict)
                    {
                        x    = restrict;
                        hadX = true;
                    }
                }
            }

            if (hadX)
            {
                update.Values.InputRotationX.CurrentValue = x;
            }

            if (hadY)
            {
                update.Values.InputRotationY.CurrentValue = y;
            }

            if (hadX || hadY)
            {
                var third = update.GameCameraState as ThirdPersonState;
                if (third != null && Memory.ReadUInt8(third.Address + 0xDC) == 0)
                {
                    update.CameraMain.HandleActorTurnToCamera(update.Target.Actor, third, false);
                }
            }

            this.UpdateNearClip(update, y);

            {
                double downMove = Settings.Instance.DownOffsetBeginAngle;
                if (downMove < 360.0)
                {
                    double downRatio;
                    var    downAngle = -Utility.RadToDeg(y);
                    if (downAngle <= downMove)
                    {
                        downRatio = 0.0;
                    }
                    else if (downAngle >= 90.0)
                    {
                        downRatio = 1.0;
                    }
                    else
                    {
                        downRatio = (downAngle - downMove) / (90.0 - downMove);
                    }

                    _look_downoffset_ratio = (float)downRatio;
                }
                else if (_look_downoffset_ratio != 0.0f) { _look_downoffset_ratio = 0.0f; }

                int moveType;
                if (Settings.Instance.TryFixLeftRightMovementClipping != 0.0f &&
                    !this.Stack.CameraMain.WasUsingFirstPersonArms            &&
                    ((moveType = Moving._move_dir) == 2 || moveType == 6))
                {
                    var swimming = false;
                    if (update.Target.Actor                                             != null &&
                        (Memory.ReadUInt32(update.Target.Actor.Address + 0xC0) & 0x400) != 0)
                    {
                        swimming = true;
                    }

                    if (!swimming)
                    {
                        downMove = 60.0;
                        double downRatio;
                        var    downAngle = -Utility.RadToDeg(y);
                        if (downAngle <= downMove)
                        {
                            downRatio = 0.0;
                        }
                        else if (downAngle >= 90.0)
                        {
                            downRatio = 1.0;
                        }
                        else
                        {
                            downRatio = (downAngle - downMove) / (90.0 - downMove);
                        }

                        var dr = (float)downRatio;
                        if (_look_downoffset_ratio_leftrightmove < dr)
                        {
                            this.IncLeftRightMoveFix(dr);
                        }
                        else if (_look_downoffset_ratio_leftrightmove > dr)
                        {
                            this.DecLeftRightMoveFix(dr);
                        }
                    }
                    else if (_look_downoffset_ratio_leftrightmove != 0.0f)
                    {
                        this.DecLeftRightMoveFix(0.0f);
                    }
                }
                else if (_look_downoffset_ratio_leftrightmove != 0.0f)
                {
                    this.DecLeftRightMoveFix(0.0f);
                }
            }
        }

        private void DecLeftRightMoveFix(float min)
        {
            var diff = IFPVPlugin.Instance._lastDiff2 * 0.001f;
            _look_downoffset_ratio_leftrightmove -= diff * 2.0f;
            if (_look_downoffset_ratio_leftrightmove < min)
            {
                _look_downoffset_ratio_leftrightmove = min;
            }
        }

        private void IncLeftRightMoveFix(float max)
        {
            var diff = IFPVPlugin.Instance._lastDiff2 * 0.001f;
            _look_downoffset_ratio_leftrightmove += diff * 5.0f;
            if (_look_downoffset_ratio_leftrightmove > max)
            {
                _look_downoffset_ratio_leftrightmove = max;
            }
        }

        private void UpdateNearClip(CameraUpdate update, double? y)
        {
            var nc       = 0.0f;
            var interior = false;

            var obj = update.Target.Object;
            if (obj != null)
            {
                var cell = obj.ParentCell;
                if (cell != null && cell.IsInterior)
                {
                    interior = true;
                }
            }

            if (!y.HasValue)
            {
                y = update.Values.InputRotationY.CurrentValue;
            }

            y = Utility.RadToDeg(y.Value);
            var downRatio = 0.0f;
            var downAngle = 60.0;
            if (y.Value < -90.0)
            {
                downRatio = 1.0f;
            }
            else if (y.Value < -downAngle)
            {
                downRatio = (float)(-(y.Value + downAngle) / (90.0 - downAngle));
            }

            var ncNormal = interior
                ? Settings.Instance.NearClipInteriorDefault
                : Settings.Instance.NearClipExteriorDefault;

            var ncDown = interior ? Settings.Instance.NearClipInteriorDown : Settings.Instance.NearClipExteriorDown;

            nc = ((ncDown - ncNormal) * downRatio) + ncNormal;
            nc = Math.Max(1.0f, nc);

            if (this._nearClip == null || this._lastNearClip != nc)
            {
                if (this._nearClip != null)
                {
                    this._nearClip.Remove();
                    this._nearClip = null;
                }

                this._nearClip =
                    update.Values.NearClip.AddModifier(this, CameraValueModifier.ModifierTypes.Set, nc, false);

                this._lastNearClip = nc;
            }
        }
    }
}
