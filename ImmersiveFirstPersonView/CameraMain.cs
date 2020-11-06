using System;
using IFPV.States;
using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;

namespace IFPV
{
    internal sealed class CameraMain
    {
        internal CameraMain(IFPVPlugin plugin) { Plugin = plugin ?? throw new ArgumentNullException("plugin"); }

        internal readonly IFPVPlugin Plugin;

        internal CameraStack Stack { get; private set; }

        internal CameraValueMap Values { get; private set; }

        internal CameraStabilize Stabilize { get; private set; }

        internal CameraCull Cull { get; private set; }

        internal CameraHideHelper Hide { get; private set; }

        internal bool IsInitialized { get; private set; }

        internal bool IsEnabled { get; private set; }

        private CameraResult BaseHead { get; set; }

        private CameraResult BaseRoot { get; set; }

        private CameraResult BaseResult { get; set; }

        private CameraResult Offset1Result { get; set; }

        private CameraResult InputResult { get; set; }

        private CameraResult Offset2Result { get; set; }

        private CameraResult FinalResult { get; set; }

        internal CameraResult TempResult { get; set; }

        internal float LastActorTurnX { get; set; }

        internal float LastActorTurnY { get; set; }

        internal int LastActorTurnFrames { get; set; }

        internal bool WasUsingFirstPersonArms { get; private set; }

        private bool                HadKey;
        private bool                LastActorWasPlayer;
        private uint                LastTargetFormId;
        private WantStates          LastWantState = WantStates.None;
        private CameraValueModifier WantMod;

        internal enum WantStates
        {
            None,

            EnabledFromTogglePOV,
            DisabledFromTogglePOV,
            EnabledFromHotkey,
            DisabledFromHotkey,
            EnabledFromZoom,
            DisabledFromZoom
        }

        internal bool AlreadyHasWantState() { return GetWantFromState(LastWantState) > 0; }

        internal void SetWantState(WantStates state)
        {
            var curState = GetWantFromState(state);
            if (curState == 0)
                return;

            if (WantMod != null)
            {
                WantMod.Remove();
                WantMod = null;
            }

            var lastState = GetWantFromState(LastWantState);
            var prev      = LastWantState;
            LastWantState = state;

            WantMod = curState > 0
                ? Values.WantEnabled.AddModifier(null, CameraValueModifier.ModifierTypes.Add, 1.0, false)
                : Values.WantDisabled.AddModifier(null, CameraValueModifier.ModifierTypes.Add, 1.0, false);
        }

        private Setting _min_zoom;
        private Setting _inc_zoom;
        private bool    _init_zoom;

        private void _init_z()
        {
            if (_init_zoom)
                return;
            _init_zoom = true;

            _min_zoom = Setting.FindSettingByName("fMinCurrentZoom:Camera", true, true);
            _inc_zoom = Setting.FindSettingByName("fMouseWheelZoomIncrement:Camera", true, true);
        }

        private int GetWantFromState(WantStates state)
        {
            switch (state)
            {
                case WantStates.None: return 0;

                case WantStates.EnabledFromHotkey:
                    return 1;

                case WantStates.EnabledFromTogglePOV:
                case WantStates.EnabledFromZoom:
                    return Settings.Instance.ReplaceDefaultCamera ? 1 : 0;

                case WantStates.DisabledFromHotkey:
                    return -1;

                case WantStates.DisabledFromTogglePOV:
                case WantStates.DisabledFromZoom:
                    return Settings.Instance.ReplaceDefaultCamera ? -1 : 0;

                default: throw new NotImplementedException();
            }
        }

        internal long _LastTurnIsFromAutoTurn = 0;

        internal void OnMakeTurn()
        {
            if (Stabilize == null)
                return;

            double ftime = Settings.Instance.ActorTurnStabilizeTime;
            if (ftime <= 0.0)
                return;

            var now = Plugin.Time;
            if (now < _LastTurnIsFromAutoTurn)
                ftime *= 1.5;

            var pcam = PlayerCamera.Instance;

            var pstate = pcam?.State;

            if (!(pstate is ThirdPersonState third))
                return;

            double x    = Math.Abs(third.XRotationFromLastResetPoint);
            var    mult = x / (Math.PI * 0.5);
            ftime *= mult;

            var ms = (long) (ftime * 1000.0);
            if (ms < 33)
                return;

            Stabilize.AddTweenFrom(ms, FinalResult.Transform.Position);
        }

        internal void HandleZoom(ThirdPersonState third, bool zoomIn)
        {
            if (third == null)
                return;

            _init_z();

            if (_min_zoom == null || _inc_zoom == null)
                return;

            var min  = _min_zoom.GetFloat();
            var inc  = _inc_zoom.GetFloat();
            var cur  = third.TargetZoomLevel;
            var next = cur + (zoomIn ? -1.0f : 1.0f) * inc;

            if (cur <= min && zoomIn)
            {
                third.TargetZoomLevel = min;
                SetWantState(WantStates.EnabledFromZoom);
                return;
            }

            if (next < min)
                next = min;
            else if (next > 1.0f)
                next = 1.0f;

            third.TargetZoomLevel = next;

            if (!zoomIn && AlreadyHasWantState())
                SetWantState(WantStates.DisabledFromZoom);
        }

        private bool CalculateEnabled(CameraUpdate update)
        {
            var tkeyCode = Settings.Instance.ToggleHotkey;
            if (tkeyCode > 0 && Input.IsPressed((VirtualKeys) tkeyCode))
            {
                if (!HadKey)
                {
                    HadKey = true;
                    SetWantState(IsEnabled ? WantStates.DisabledFromHotkey : WantStates.EnabledFromHotkey);
                }
            }
            else { HadKey = false; }

            var rkeyCode = Settings.Instance.ReloadHotkey;
            if (rkeyCode > 0 && Input.IsPressed((VirtualKeys) rkeyCode))
            {
                Settings.Instance.Load();
                Values = new CameraValueMap(this);
            }

            update.Values.WantEnabled.Update(Plugin.Time, IsEnabled);
            update.Values.WantDisabled.Update(Plugin.Time, IsEnabled);

            if (update.Values.WantDisabled.CurrentValue > 0.0 || update.Values.WantEnabled.CurrentValue <= 0.0)
                return false;

            switch (update.GameCameraState.Id)
            {
                case TESCameraStates.Free:
                case TESCameraStates.FirstPerson:
                case TESCameraStates.TweenMenu:
                case TESCameraStates.AutoVanity:
                    return false;

                case TESCameraStates.VATS:
                {
                    if (Settings.Instance.DisableDuringKillmove)
                        return false;
                }
                    break;
            }

            var menuManager = MenuManager.Instance;
            if (menuManager == null)
                return false;

            return !menuManager.IsMenuOpen("RaceSex Menu");
        }

        internal void UpdateHeadtrack()
        {
            if (!LastActorWasPlayer)
                return;

            if (Values.HeadTrackEnabled.CurrentValue == 0)
                return;

            var plr = PlayerCharacter.Instance;
            if (plr == null)
                return;

            var camera = PlayerCamera.Instance;

            var state = camera?.State;
            if (state == null)
                return;

            if (!(state is ThirdPersonState third))
                return;

            var fullZ = plr.Rotation.Z                    + third.XRotationFromLastResetPoint;
            var fullX = third.YRotationFromLastResetPoint - plr.Rotation.X;

            TempResult.Transform.CopyFrom(FinalResult.Transform);
            var rot = TempResult.Transform.Rotation;
            rot.Identity(1.0f);
            rot.RotateX(fullX, rot);
            rot.RotateZ(-fullZ, rot);

            var pos = TempResult.Transform.Position;
            var amt = Offset1Result.Transform.Position;
            amt.X = 0.0f;
            amt.Y = 1000.0f;
            amt.Z = 0.0f;
            TempResult.Transform.Translate(amt, pos);

            plr.SetLookAtPosition(pos);
        }

        internal void Initialize()
        {
            if (IsInitialized)
                throw new InvalidOperationException();
            IsInitialized = true;

        #if PROFILING
            this._performance_timer.Start();
        #endif

            Values = new CameraValueMap(this);
            Stack  = new CameraStack(this);
            Cull   = new CameraCull(this);
            Hide   = new CameraHideHelper(this);

            BaseHead      = new CameraResult();
            BaseRoot      = new CameraResult();
            BaseResult    = new CameraResult();
            Offset1Result = new CameraResult();
            InputResult   = new CameraResult();
            Offset2Result = new CameraResult();
            FinalResult   = new CameraResult();
            TempResult    = new CameraResult();
        }

        internal bool Update(UpdateCameraEventArgs e)
        {
            if (!(e.Camera is PlayerCamera))
                return false;

            var target = SelectTarget(e);
            if (target == null)
                return false;

            var update = SetupUpdate(e, target);
            if (update != null)
                DoUpdate(update);

            return true;
        }

        internal NiTransform LastResult => FinalResult.Transform;

        private CameraUpdate SetupUpdate(UpdateCameraEventArgs e, CameraTarget target)
        {
            if (target == null)
                return null;

            var cameraBase = e.Camera;
            if (!(cameraBase is PlayerCamera playerCamera))
                return null;

            var cameraNode = playerCamera.Node;
            if (cameraNode == null)
                return null;

            var cameraState = playerCamera.State;
            if (cameraState == null)
                return null;

            var update = new CameraUpdate(this, FinalResult, target, playerCamera, cameraNode, cameraState, Values);
            return update;
        }

        private CameraTarget SelectTarget(UpdateCameraEventArgs e)
        {
            var cameraBase = e.Camera;
            if (!(cameraBase is PlayerCamera playerCamera))
                return null;

            TESObjectREFR target    = null;
            var           refHandle = playerCamera.TargetRefHandle;
            using (var objRef = new ObjectRefHolder(refHandle))
            {
                if (objRef.IsValid)
                    target = objRef.Object;
            }

            var t = CameraTarget.Create(target);
            if (t == null)
                return null;

            var actor = t.Actor;
            var obj   = t.Object;
            LastActorWasPlayer = actor != null && actor.IsPlayer;
            LastTargetFormId   = obj?.FormId ?? 0;

            return t;
        }

        internal void FixMouseSensitivity(ref float x, ref float y, float seconds)
        {
            if (seconds <= 0.0f)
            {
                x = 0.0f;
                y = 0.0f;
                return;
            }

            InitMouseSettings();

            var enabled = IsEnabled;
            var sens    = _fMouseHeading?.GetFloat()       ?? 0.0125f;
            var xsens   = _fMouseHeadingXScale?.GetFloat() ?? 0.02f;
            var ysens   = _fMouseHeadingYScale?.GetFloat() ?? 0.85f;
            var sens2   = enabled ? Settings.Instance.LookSensitivity : 1.0f;
            var xsens2  = enabled ? Settings.Instance.LookSensitivityHorizontal : 1.0f;
            var ysens2  = enabled ? Settings.Instance.LookSensitivityVertical : 1.0f;

            var fix = Settings.Instance.FixLookSensitivity;
            if (fix == 2)
                fix = enabled ? 1 : 0;

            if (fix == 1)
            {
                var mult_const = 60.0f; // 42.5f
                x *= sens * xsens * mult_const * sens2 * xsens2;
                y *= sens * ysens * sens2      * ysens2;
            }
            else
            {
                x *= sens * xsens / seconds * sens2 * xsens2;
                y *= sens                   * ysens * sens2 * ysens2;
            }

            if (FixSensitivityMode)
                y *= 2.0f;
        }

        private void InitMouseSettings()
        {
            if (_fMouseSettingInit)
                return;
            _fMouseSettingInit = true;

            _fMouseHeading       = Setting.FindSettingByName("fMouseHeadingSensitivity:Controls", true, true);
            _fMouseHeadingXScale = Setting.FindSettingByName("fMouseHeadingXScale:Controls", true, true);
            _fMouseHeadingYScale = Setting.FindSettingByName("fMouseHeadingYScale:Controls", true, true);
        }

        private bool    _fMouseSettingInit;
        private Setting _fMouseHeading;
        private Setting _fMouseHeadingXScale;
        private Setting _fMouseHeadingYScale;

        internal void OnShadowCulling(int index) { Cull?.OnShadowCulling(index); }

        internal void OnUpdating(int index) { Cull?.OnUpdating(index); }

        internal bool DidCollideLastUpdate { get; private set; }

        private void DoUpdate(CameraUpdate update)
        {
            {
                var wasEnabled = IsEnabled;
                var isEnabled  = CalculateEnabled(update);

                if (wasEnabled != isEnabled)
                {
                    if (!isEnabled)
                        Stack.DisableAll(update);

                    IsEnabled = isEnabled;
                    if (isEnabled)
                        OnEnabled(update);
                    else
                        OnDisabled(update);
                }
            }

            if (IsEnabled)
                OnUpdating(0);

            if (IsEnabled)
            {
                if (Stabilize == null || Stabilize.ShouldRecreate(update.Target))
                    Stabilize = new CameraStabilize(this, update.Target);

                Stabilize?.Update(update.Target.StabilizeRootNode, update.Target.HeadNode, update);
            }

            Stack.Check(update);
            Stack.Update(update);
            update.Values.Update(Plugin.Time, IsEnabled);
            Hide.Update(update);
            {
                var isFpArms = IsEnabled && update.Values.Show1stPersonArms.CurrentValue >= 0.5;
                if (isFpArms != WasUsingFirstPersonArms)
                    WasUsingFirstPersonArms = isFpArms;
            }

            if (IsEnabled)
            {
                var mode      = update.Values.SkeletonMode.CurrentValue;
                var wantThird = true;
                if (mode <= -0.5)
                    wantThird = !WasUsingFirstPersonArms;
                else if (mode >= 0.5)
                    wantThird = false;
                //else wantThird = true;

                var showFirst = WasUsingFirstPersonArms;
                var showThird = !(DidCollideLastUpdate && Settings.Instance.HidePlayerWhenColliding == 2);

                UpdateSkeleton(showFirst, showThird, wantThird);
            }

            if (IsEnabled)
            {
                if (Stabilize == null || !Stabilize.Get(update.Target.StabilizeRootNode, BaseRoot.Transform))
                    BaseRoot.Transform.CopyFrom(update.Target.HeadNode.WorldTransform);
                BaseHead.Transform.CopyFrom(update.Target.HeadNode.WorldTransform);

                CameraResult cur = null;
                using (cur)
                {
                    {
                        var posRatio = update.Values.PositionFromHead.CurrentValue;
                        switch (posRatio)
                        {
                            case 0.0:
                                BaseResult.Transform.Position.CopyFrom(BaseRoot.Transform.Position);
                                break;
                            case 1.0:
                                BaseResult.Transform.Position.CopyFrom(BaseHead.Transform.Position);
                                break;
                            default:
                            {
                                var pos     = BaseResult.Transform.Position;
                                var rootPos = BaseRoot.Transform.Position;
                                var headPos = BaseHead.Transform.Position;

                                pos.X = (float) ((headPos.X - rootPos.X) * posRatio + rootPos.X);
                                pos.Y = (float) ((headPos.Y - rootPos.Y) * posRatio + rootPos.Y);
                                pos.Z = (float) ((headPos.Z - rootPos.Z) * posRatio + rootPos.Z);
                                break;
                            }
                        }
                    }

                    // Calculate base rotation.
                    {
                        var rotRatio = update.Values.RotationFromHead.CurrentValue;
                        switch (rotRatio)
                        {
                            case 0.0:
                                BaseResult.Transform.Rotation.CopyFrom(BaseRoot.Transform.Rotation);
                                break;
                            case 1.0:
                                BaseResult.Transform.Rotation.CopyFrom(BaseHead.Transform.Rotation);
                                break;
                            default:
                            {
                                var rot     = BaseResult.Transform.Rotation;
                                var rootRot = BaseRoot.Transform.Rotation;
                                var headRot = BaseHead.Transform.Rotation;

                                rootRot.Interpolate(headRot, (float) rotRatio, rot);
                                break;
                            }
                        }
                    }

                    cur = BaseResult;

                    // Calculate offset based on object rotation itself.
                    {
                        var root = update.Target.RootNode;
                        if (root != null)
                        {
                            var x = Values.OffsetObjectPositionX.CurrentValue;
                            var y = Values.OffsetObjectPositionY.CurrentValue;
                            var z = Values.OffsetObjectPositionZ.CurrentValue;

                            if (x != 0.0 || y != 0.0 || z != 0.0)
                            {
                                Offset1Result.Transform.Position.CopyFrom(BaseResult.Transform.Position);
                                Offset1Result.Transform.Rotation.CopyFrom(root.WorldTransform.Rotation);
                                ApplyPositionOffset(Offset1Result.Transform, (float) x, (float) y, (float) z,
                                                    BaseResult.Transform.Position);
                            }
                        }
                    }

                    // Look down offset.
                    if (Default._look_downoffset_ratio > 0.0f || Default._look_downoffset_ratio_leftrightmove > 0.0f)
                    {
                        var ratio = Default._look_downoffset_ratio;
                        var root  = update.Target.RootNode;
                        if (root != null)
                        {
                            var x = Settings.Instance.DownOffsetX * ratio;
                            var y = Settings.Instance.DownOffsetY * ratio;
                            var z = Settings.Instance.DownOffsetZ * ratio;

                            y += Settings.Instance.TryFixLeftRightMovementClipping *
                                 Default._look_downoffset_ratio_leftrightmove;

                            if (x != 0.0f || y != 0.0f || z != 0.0f)
                            {
                                Offset1Result.Transform.Position.CopyFrom(BaseResult.Transform.Position);
                                Offset1Result.Transform.Rotation.CopyFrom(root.WorldTransform.Rotation);
                                ApplyPositionOffset(Offset1Result.Transform, x, y, z, BaseResult.Transform.Position);
                            }
                        }
                    }

                    // Calculate offset #1.
                    {
                        var rx = (float) Values.Offset1RotationX.CurrentValue;
                        var ry = (float) Values.Offset1RotationY.CurrentValue;
                        var px = (float) Values.Offset1PositionX.CurrentValue;
                        var py = (float) Values.Offset1PositionY.CurrentValue;
                        var pz = (float) Values.Offset1PositionZ.CurrentValue;

                        var hasRot = rx != 0.0f || ry != 0.0f;
                        var hasPos = px != 0.0f || py != 0.0f || pz != 0.0f;

                        if (hasRot || hasPos)
                        {
                            Offset1Result.Transform.CopyFrom(cur.Transform);
                            if (hasRot)
                                ApplyRotationOffset(Offset1Result.Transform.Rotation, rx, ry,
                                                    Offset1Result.Transform.Rotation);
                            if (hasPos)
                                ApplyPositionOffset(Offset1Result.Transform, px, py, pz, Offset1Result.Transform.Position);

                            cur = Offset1Result;
                        }
                    }

                    // Calculate input.
                    {
                        var extraX = 0.0f;
                        var extraY = 0.0f;
                        if (LastActorTurnFrames > 0)
                        {
                            LastActorTurnFrames--;
                            extraX = LastActorTurnX;
                            //extraY = this.LastActorTurnY;
                        }

                        var rx = (float) (Values.InputRotationX.CurrentValue + extraX) *
                                 (float) Values.InputRotationXMultiplier.CurrentValue;
                        var ry = (float) (Values.InputRotationY.CurrentValue + extraY) *
                                 (float) Values.InputRotationYMultiplier.CurrentValue;

                        if (rx != 0.0f || ry != 0.0f)
                        {
                            InputResult.Transform.CopyFrom(cur.Transform);
                            ApplyRotationOffset(InputResult.Transform.Rotation, rx, ry, InputResult.Transform.Rotation);

                            cur = InputResult;
                        }
                    }

                    // Calculate offset #2.
                    {
                        var rx = (float) Values.Offset2RotationX.CurrentValue;
                        var ry = (float) Values.Offset2RotationY.CurrentValue;
                        var px = (float) Values.Offset2PositionX.CurrentValue;
                        var py = (float) Values.Offset2PositionY.CurrentValue;
                        var pz = (float) Values.Offset2PositionZ.CurrentValue;

                        var hasRot = rx != 0.0f || ry != 0.0f;
                        var hasPos = px != 0.0f || py != 0.0f || pz != 0.0f;

                        if (hasRot || hasPos)
                        {
                            Offset2Result.Transform.CopyFrom(cur.Transform);
                            if (hasRot)
                                ApplyRotationOffset(Offset2Result.Transform.Rotation, rx, ry,
                                                    Offset2Result.Transform.Rotation);
                            if (hasPos)
                                ApplyPositionOffset(Offset2Result.Transform, px, py, pz, Offset2Result.Transform.Position);

                            cur = Offset2Result;
                        }
                    }

                    // Apply tween from stabilize.
                    {
                        Stabilize?.ApplyTween(cur.Transform.Position, Plugin.Time);
                    }

                    // Apply collision of camera so we don't go inside walls, this can be done within the same transform.
                    {
                        DidCollideLastUpdate = CameraCollision.Apply(update, cur.Transform, cur.Transform.Position);
                    }

                    // Calculate final result.
                    {
                        FinalResult.Transform.CopyFrom(cur.Transform);
                    }
                }

                update.GameCameraNode.LocalTransform.CopyFrom(FinalResult.Transform);
                update.GameCameraNode.Update(0.0f);

                Hide.UpdateFirstPersonSkeletonRotation(update);

                if (update.GameCameraState is ThirdPersonState third)
                    third.Position.CopyFrom(update.GameCameraNode.LocalTransform.Position);

                if (WasUsingFirstPersonArms)
                    UpdateMagicNodePosition(update);
            }
            else { DidCollideLastUpdate = false; }

            if (IsEnabled)
                OnUpdating(1);

            if (!IsEnabled                                               && Settings.Instance.ReplaceDefaultCamera &&
                update.GameCameraState.Id == TESCameraStates.FirstPerson && IsGameCameraSwitchControlsEnabled())
            {
                update.GameCamera.EnterThirdPerson();
                SetWantState(WantStates.EnabledFromTogglePOV);
            }

            FixSensitivityMode = IsEnabled && update.GameCameraState.Id                  == TESCameraStates.ThirdPerson2 &&
                                 Memory.ReadUInt8(update.GameCameraState.Address + 0xDC) != 0;
        }

        private bool FixSensitivityMode;

        private int? LastSkeletonParameter;

        internal void UpdateSkeletonWithLastParameters()
        {
            if (!LastSkeletonParameter.HasValue)
                return;

            var v = LastSkeletonParameter.Value;
            UpdateSkeleton((v & 1) != 0, (v & 2) != 0, (v & 4) != 0);
        }

        private void UpdateSkeleton(bool showFirst, bool showThird, bool wantThirdPersonMode)
        {
            var plr = PlayerCharacter.Instance;
            if (plr == null)
            {
                LastSkeletonParameter = null;
                return;
            }

            var fpSkeleton = plr.GetSkeletonNode(true);
            var tpSkeleton = plr.GetSkeletonNode(false);

            if (fpSkeleton == null || tpSkeleton == null)
            {
                LastSkeletonParameter = null;
                return;
            }

            LastSkeletonParameter = (showFirst ? 1 : 0) + (showThird ? 2 : 0) + (wantThirdPersonMode ? 4 : 0);

            var isThirdPersonMode = (Memory.ReadUInt8(plr.Address + 0xBDB) & 1) != 0;
            var isFirst           = (Utility.GetNiAVFlags(fpSkeleton)      & 1) == 0;
            var isThird           = (Utility.GetNiAVFlags(tpSkeleton)      & 1) == 0;

            if (isFirst == showFirst && isThird == showThird && wantThirdPersonMode == isThirdPersonMode)
                return;

            if (wantThirdPersonMode != isThirdPersonMode)
            {
                Utility.ModNiAVFlags(fpSkeleton, 1, !wantThirdPersonMode);
                Utility.ModNiAVFlags(tpSkeleton, 1, wantThirdPersonMode);
                CustomSwitchSkeletonCall++;
                Memory.InvokeCdecl(Plugin.SwitchSkeleton, plr.Address, wantThirdPersonMode ? 0 : 1);
                CustomSwitchSkeletonCall--;
                Utility.ModNiAVFlags(fpSkeleton, 1, !showFirst);
                Utility.ModNiAVFlags(tpSkeleton, 1, !showThird);
                return;
            }

            if (isFirst != showFirst)
                Utility.ModNiAVFlags(fpSkeleton, 1, !showFirst);

            if (isThird != showThird)
                Utility.ModNiAVFlags(tpSkeleton, 1, !showThird);
        }

        internal void FixSpineTwist(IntPtr twistModifier)
        {
            if (!IsEnabled || !WasUsingFirstPersonArms)
                return;

            var namePtr = Memory.ReadPointer(twistModifier + 0x38);
            if (namePtr == IntPtr.Zero)
                return;

            var ux = Memory.ReadUInt64(namePtr);

            switch (ux)
            {
                case 0x6361747441776F42: // BowAttac -> BowAttackSpineTwistModifier
                case 0x697053636967614D: // MagicSpi -> MagicSpineTwistModifier
                case 0x70536C6175746952
                    :                    // RitualSp -> RitualSpellSpineTwistModifier + RitualSpell_AimedConcentrationLoop + RitualSpell_AimedConLoop_MG
                case 0x69705374756F6853: // ShoutSpi -> ShoutSpineTwistModifier
                {
                    Memory.WriteInt16(twistModifier + 0x64, -1);
                }
                    break;

                /*case 0x65646F4E6B6F6F4C: // LookNode -> LookNodeRotateModifierFixed
                case 0x54746F5250747331: // 1stPRotT -> 1stPRotTwistModifier
                    break;*/
            }
        }

        internal bool IsGameCameraSwitchControlsEnabled()
        {
            var controls = PlayerControls.Instance;
            return controls != null && Memory.InvokeCdecl(Plugin.PlayerControls_IsCamSwitchControlsEnabled, controls.Address).ToBool();
        }

        private void OnEnabled(CameraUpdate update) { LastActorTurnFrames = 0; }

        private void OnDisabled(CameraUpdate update) { UpdateSkeleton(GameWantsSkeletonMode > 0, GameWantsSkeletonMode < 0, GameWantsSkeletonMode < 0); }

        internal bool HookSwitchSkeleton(Actor actor, bool firstPerson)
        {
            if (actor == null || !actor.IsPlayer || CustomSwitchSkeletonCall > 0)
                return false;

            GameWantsSkeletonMode = firstPerson ? 1 : -1;

            return IsEnabled;
        }

        private int GameWantsSkeletonMode;
        private int CustomSwitchSkeletonCall;

        internal bool GetOverwriteWeaponNode(TESForm obj, NiPoint3 pt)
        {
            if (!IsEnabled)
                return false;

            if (obj == null || obj.FormId != LastTargetFormId)
                return false;

            var pcam = PlayerCamera.Instance;

            var pn = pcam?.Node;
            if (pn == null)
                return false;

            var pos = pn.WorldTransform.Position;
            pt.X = pos.X;
            pt.Y = pos.Y;
            pt.Z = pos.Z - 10.0f;
            return true;
        }

        internal NiAVObject GetOverwriteMagicNode(MagicCaster caster)
        {
            if (!IsEnabled)
                return null;

            if (!(caster is ActorMagicCaster actorCaster))
                return null;

            var actor = actorCaster.Owner;
            if (actor == null || actor.FormId != LastTargetFormId)
                return null;

            InitMagicNode();

            if (MagicNodes == null)
                return null;

            switch (actorCaster.ActorCasterType)
            {
                case EquippedSpellSlots.LeftHand:  return MagicNodes[0];
                case EquippedSpellSlots.RightHand: return MagicNodes[1];
                case EquippedSpellSlots.Other:     return MagicNodes[2];
            }

            return null;
        }

        internal void UpdateMagicNodePosition(CameraUpdate update)
        {
            InitMagicNode();

            if (MagicNodes == null)
                return;

            for (var i = 0; i < MagicNodes.Length; i++)
            {
                var node = MagicNodes[i];
                var wt   = node.WorldTransform;

                {
                    wt.CopyFrom(FinalResult.Transform);
                    wt.Translate(MagicTranslates[i], wt.Position);
                }
            }
        }

        private void InitMagicNode()
        {
            if (MagicNodeAllocation != null)
                return;

            const int count = 3;
            const int size  = 0x130;
            const int size2 = 0x10;
            MagicNodeAllocation = Memory.Allocate(size * count + size2 * count);
            MagicNodes          = new NiNode[count];
            MagicTranslates     = new NiPoint3[count];
            var s = Settings.Instance;
            for (var i = 0; i < count; i++)
            {
                var addrOfThis = MagicNodeAllocation.Address + size * i;
                Memory.InvokeCdecl(Plugin.NiNode_ctor, addrOfThis, 0);
                MagicNodes[i] = MemoryObject.FromAddress<NiNode>(addrOfThis);
                MagicNodes[i].IncRef();

                MagicTranslates[i] =
                    MemoryObject.FromAddress<NiPoint3>(MagicNodeAllocation.Address + size * count + size2 * i);
                switch (i)
                {
                    case 0:
                        MagicTranslates[i].X = s.MagicLeftOffsetX;
                        MagicTranslates[i].Y = s.MagicLeftOffsetY;
                        MagicTranslates[i].Z = s.MagicLeftOffsetZ;
                        break;

                    case 1:
                        MagicTranslates[i].X = s.MagicRightOffsetX;
                        MagicTranslates[i].Y = s.MagicRightOffsetY;
                        MagicTranslates[i].Z = s.MagicRightOffsetZ;
                        break;

                    case 2:
                        MagicTranslates[i].X = s.MagicVoiceOffsetX;
                        MagicTranslates[i].Y = s.MagicVoiceOffsetY;
                        MagicTranslates[i].Z = s.MagicVoiceOffsetZ;
                        break;

                    default:
                        MagicTranslates[i].X = 0.0f;
                        MagicTranslates[i].Y = 0.0f;
                        MagicTranslates[i].Z = 0.0f;
                        break;
                }
            }
        }

        private MemoryAllocation MagicNodeAllocation;
        private NiNode[]         MagicNodes;
        private NiPoint3[]       MagicTranslates;

        private void ApplyRotationOffset(NiMatrix33 matrix, float x, float y, NiMatrix33 result)
        {
            if (x == 0.0f && y == 0.0f)
            {
                if (!result.Equals(matrix))
                    result.CopyFrom(matrix);
                return;
            }

            var rot = TempResult.Transform.Rotation;
            rot.Identity(1.0f);

            if (y != 0.0f)
                rot.RotateX(y, rot);

            if (x != 0.0f)
                rot.RotateZ(-x, rot);

            matrix.Multiply(rot, result);
        }

        private void ApplyPositionOffset(NiTransform transform, float x, float y, float z, NiPoint3 result)
        {
            if (x == 0.0f && y == 0.0f && z == 0.0f)
            {
                var tpos = transform.Position;
                if (!result.Equals(tpos))
                    result.CopyFrom(tpos);
                return;
            }

            var pos = TempResult.Transform.Position;
            pos.X = x;
            pos.Y = y;
            pos.Z = z;
            transform.Translate(pos, result);
        }

        internal void HandleActorTurnToCamera(Actor actor, ThirdPersonState third, bool fromFreeLookChanged)
        {
            if (third == null || actor == null)
                return;

            double x    = third.XRotationFromLastResetPoint;
            double y    = third.YRotationFromLastResetPoint;
            var    didx = 0.0;
            var    didy = 0.0;

            if (x == 0.0 && y == 0.0)
                return;

            var    max  = 0.0;
            double diff = Plugin._lastDiff2;
            var    time = Values.ActorTurnTime.CurrentValue;

            if (time <= 0.0)
                max = Math.PI * 2.0;
            else if (diff >= 1.0)
                max = diff * 0.001 / time * Math.PI * 2.0;

            // Turn left or right.
            if (x != 0.0)
            {
                var actual = x;
                if (Math.Abs(actual) > max)
                {
                    if (actual < 0.0)
                        actual = -max;
                    else
                        actual = max;
                }

                Memory.InvokeCdecl(Plugin.ActorTurnZ, actor.Address, (float) actual);
                if (actual == x)
                    third.XRotationFromLastResetPoint = 0.0f;
                else
                    third.XRotationFromLastResetPoint -= (float) actual;

                didx = actual;
            }

            // Turn up or down.
            if (y != 0.0)
            {
                var actual = y;
                Memory.InvokeCdecl(Plugin.ActorTurnX, actor.Address, -(float) actual);
                third.YRotationFromLastResetPoint = 0.0f;

                didy = actual;
            }

            // Fix visual error with turning.
            if (!fromFreeLookChanged) return;

            const int duration = 0;
            if (!IsEnabled || !(LastActorTurnFrames < duration)) return;

            LastActorTurnX      = (float) didx;
            LastActorTurnY      = (float) didy;
            LastActorTurnFrames = duration;
        }

    #if PROFILING
        internal enum _performance_track : int
        {
            Frame,
            CameraUpdate,

            Max
        }

        private long[] _performance_ticks = new long[(int)_performance_track.Max];
        private long[] _performance_begin = new long[(int)_performance_track.Max];
        private int[] _performance_times = new int[(int)_performance_track.Max];
        private int[] _performance_counters = new int[(int)_performance_track.Max];
        private System.Diagnostics.Stopwatch _performance_timer = new System.Diagnostics.Stopwatch();
        internal int _prof_state = 0;

        internal void begin_track(_performance_track type)
        {
            int i = (int)type;
            if (++this._performance_counters[i] != 1)
                return;

            this._performance_begin[i] = this._performance_timer.ElapsedTicks;
        }

        internal void end_track(_performance_track type)
        {
            int i = (int)type;
            if (--this._performance_counters[i] != 0)
                return;

            long now = this._performance_timer.ElapsedTicks;
            this._performance_ticks[i] += now - this._performance_begin[i];
            this._performance_times[i]++;
        }

        internal void _end_profiling()
        {
            using (var f = new System.IO.StreamWriter("SkyrimSE_IFPV_Profiling.txt", false))
            {
                f.Write(string.Format("{0,-20}", "Type"));
                long msdiv = System.Diagnostics.Stopwatch.Frequency / 1000;
                f.Write(string.Format("{0,10}", "Total"));
                f.Write(string.Format("{0,10}", "Times"));
                f.Write(string.Format("{0,10}", "Average"));
                f.WriteLine();

                for (int i = 0; i < (int)_performance_track.Max; i++)
                {
                    f.Write(string.Format("{0,-20}", ((_performance_track)i).ToString()));
                    f.Write(string.Format("{0,10}", this._performance_ticks[i] / msdiv));
                    f.Write(string.Format("{0,10}", this._performance_times[i]));
                    double avg = 0.0;
                    if (_performance_times[i] > 0)
                        avg = (double)(_performance_ticks[i] / msdiv) / (double)_performance_times[i];
                    f.Write(string.Format("{0,10}", avg.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture)));
                    f.WriteLine();
                }
            }

            NetScriptFramework.Skyrim.MenuManager.ShowHUDMessage("Stopped profiling IFPV.", null, true);
        }
    #endif
    }
}