#pragma once
#pragma warning(push)
#pragma warning(disable : 4638)

namespace NetScriptFramework
{
    namespace SkyrimSE
    {
        /// <summary>
        /// Arguments for OnBarterMenu event.
        /// </summary>
        public ref class BarterMenuEventArgs sealed: public HookedEventArgs
        {
        private:
            System::Boolean __v_Entering;
            /// <summary>
            /// Are we entering or exiting the menu?
            /// </summary>
        public:
            property System::Boolean Entering
            {
            public:
                System::Boolean __clrcall get() { return __v_Entering; }

            internal:
                void __clrcall set(System::Boolean value) { __v_Entering = value; }
            }

        private:
            System::UInt32 __v_RefHandle;
            /// <summary>
            /// The ref handle id of the actor the player is trading with.
            /// </summary>
        public:
            property System::UInt32 RefHandle
            {
            public:
                System::UInt32 __clrcall get() { return __v_RefHandle; }

            internal:
                void __clrcall set(System::UInt32 value) { __v_RefHandle = value; }
            }
        };

        /// <summary>
        /// Arguments for OnCalculateDetection event.
        /// </summary>
        public ref class CalculateDetectionEventArgs sealed: public HookedEventArgs
        {
        private:
            Actor ^__v_SourceActor;
            /// <summary>
            /// The source actor who is doing the detecting.
            /// </summary>
        public:
            property Actor ^SourceActor
            {
            public:
                Actor ^ __clrcall get() { return __v_SourceActor; }

            internal:
                void __clrcall set(Actor ^value) { __v_SourceActor = value; }
            }

        private :
            Actor ^__v_TargetActor;
            /// <summary>
            /// The target actor who may get detected by source.
            /// </summary>
        public:
            property Actor ^TargetActor
            {
            public:
                Actor ^ __clrcall get() { return __v_TargetActor; }

            internal:
                void __clrcall set(Actor ^value) { __v_TargetActor = value; }
            }

        private :
            System::Int32 __v_ResultValue;
            /// <summary>
            /// The resulting detection value. Positive means detected. Set -1000 for
            /// definitely not detected at all.
            /// </summary>
        public:
            property System::Int32 ResultValue
            {
            public:
                System::Int32 __clrcall get() { return __v_ResultValue; }

            public:
                void __clrcall set(System::Int32 value) { __v_ResultValue = value; }
            }

        private:
            NiPoint3 ^__v_Position;
            /// <summary>
            /// The position where the detection occurrs.
            /// </summary>
        public:
            property NiPoint3 ^Position
            {
            public:
                NiPoint3 ^ __clrcall get() { return __v_Position; }

            internal:
                void __clrcall set(NiPoint3 ^value) { __v_Position = value; }
            }
        };

        /// <summary>
        /// Arguments for OnCalculateFormGoldValue event.
        /// </summary>
        public ref class CalculateFormGoldValueEventArgs sealed: public HookedEventArgs
        {
        private:
            System::Int32 __v_Value;
            /// <summary>
            /// The calculated gold value.
            /// </summary>
        public:
            property System::Int32 Value
            {
            public:
                System::Int32 __clrcall get() { return __v_Value; }

            public:
                void __clrcall set(System::Int32 value) { __v_Value = value; }
            }

        private:
            System::Int32 __v_OriginalValue;
            /// <summary>
            /// The original value before any events modified it.
            /// </summary>
        public:
            property System::Int32 OriginalValue
            {
            public:
                System::Int32 __clrcall get() { return __v_OriginalValue; }

            internal:
                void __clrcall set(System::Int32 value) { __v_OriginalValue = value; }
            }

        private:
            TESForm ^__v_Form;
            /// <summary>
            /// The form which value we are calculating.
            /// </summary>
        public:
            property TESForm ^Form
            {
            public:
                TESForm ^ __clrcall get() { return __v_Form; }

            internal:
                void __clrcall set(TESForm ^value) { __v_Form = value; }
            }
        };

        /// <summary>
        /// Arguments for OnCameraStateChanging event.
        /// </summary>
        public ref class CameraStateChangingEventArgs sealed: public HookedEventArgs
        {
        private:
            TESCamera ^__v_Camera;
            /// <summary>
            /// The camera that is about to change state.
            /// </summary>
        public:
            property TESCamera ^Camera
            {
            public:
                TESCamera ^ __clrcall get() { return __v_Camera; }

            internal:
                void __clrcall set(TESCamera ^value) { __v_Camera = value; }
            }

        private :
            TESCameraState ^__v_PreviousState;
            /// <summary>
            /// The previous (current) state of the camera.
            /// </summary>
        public:
            property TESCameraState ^PreviousState
            {
            public:
                TESCameraState ^ __clrcall get() { return __v_PreviousState; }

            internal:
                void __clrcall set(TESCameraState ^value) { __v_PreviousState = value; }
            }

        private :
            TESCameraState ^__v_NextState;
            /// <summary>
            /// The next state that will be set on the camera.
            /// </summary>
        public:
            property TESCameraState ^NextState
            {
            public:
                TESCameraState ^ __clrcall get() { return __v_NextState; }

            public:
                void __clrcall set(TESCameraState ^value) { __v_NextState = value; }
            }

        private :
            System::Boolean __v_Skip;
            /// <summary>
            /// Skip changing this state.
            /// </summary>
        public:
            property System::Boolean Skip
            {
            public:
                System::Boolean __clrcall get() { return __v_Skip; }

            public:
                void __clrcall set(System::Boolean value) { __v_Skip = value; }
            }
        };

        /// <summary>
        /// Arguments for OnFrame event.
        /// </summary>
        public ref class FrameEventArgs sealed: public HookedEventArgs { };

        /// <summary>
        /// Arguments for OnGainLevelXP event.
        /// </summary>
        public ref class GainLevelXPEventArgs sealed: public HookedEventArgs
        {
        private:
            System::Single __v_OriginalAmount;
            /// <summary>
            /// The original amount of experience that is to be gained before other events
            /// modified it.
            /// </summary>
        public:
            property System::Single OriginalAmount
            {
            public:
                System::Single __clrcall get() { return __v_OriginalAmount; }

            internal:
                void __clrcall set(System::Single value) { __v_OriginalAmount = value; }
            }

        private:
            System::Single __v_Amount;
            /// <summary>
            /// The amount of experience that is to be gained.
            /// </summary>
        public:
            property System::Single Amount
            {
            public:
                System::Single __clrcall get() { return __v_Amount; }

            public:
                void __clrcall set(System::Single value) { __v_Amount = value; }
            }

        private:
            ActorValueIndices __v_Skill;
            /// <summary>
            /// The skill that is leveling up.
            /// </summary>
        public:
            property ActorValueIndices Skill
            {
            public:
                ActorValueIndices __clrcall get() { return __v_Skill; }

            internal:
                void __clrcall set(ActorValueIndices value) { __v_Skill = value; }
            }

        private:
            System::Boolean __v_IsFromTrainingOrBook;
            /// <summary>
            /// Is this being called from NPC training or book learning or IncPCS command?
            /// Otherwise the skill leveled up normally.
            /// </summary>
        public:
            property System::Boolean IsFromTrainingOrBook
            {
            public:
                System::Boolean __clrcall get() { return __v_IsFromTrainingOrBook; }

            internal:
                void __clrcall set(System::Boolean value) { __v_IsFromTrainingOrBook = value; }
            }
        };

        /// <summary>
        /// Arguments for OnGainSkillXP event.
        /// </summary>
        public ref class GainSkillXPEventArgs sealed: public HookedEventArgs
        {
        private:
            System::Single __v_BaseAmount;
            /// <summary>
            /// The base amount of experience that is to be gained before perks modified
            /// it.
            /// </summary>
        public:
            property System::Single BaseAmount
            {
            public:
                System::Single __clrcall get() { return __v_BaseAmount; }

            internal:
                void __clrcall set(System::Single value) { __v_BaseAmount = value; }
            }

        private:
            System::Single __v_OriginalAmount;
            /// <summary>
            /// The original amount of experience that is to be gained before other events
            /// modified it.
            /// </summary>
        public:
            property System::Single OriginalAmount
            {
            public:
                System::Single __clrcall get() { return __v_OriginalAmount; }

            internal:
                void __clrcall set(System::Single value) { __v_OriginalAmount = value; }
            }

        private:
            System::Single __v_Amount;
            /// <summary>
            /// The amount of experience that is to be gained.
            /// </summary>
        public:
            property System::Single Amount
            {
            public:
                System::Single __clrcall get() { return __v_Amount; }

            public:
                void __clrcall set(System::Single value) { __v_Amount = value; }
            }

        private:
            ActorValueIndices __v_Skill;
            /// <summary>
            /// The skill that is being advanced.
            /// </summary>
        public:
            property ActorValueIndices Skill
            {
            public:
                ActorValueIndices __clrcall get() { return __v_Skill; }

            internal:
                void __clrcall set(ActorValueIndices value) { __v_Skill = value; }
            }

        private:
            System::Boolean __v_IsFromTrainingOrBook;
            /// <summary>
            /// Is this being called from NPC training or book learning or IncPCS command?
            /// If true then we should not perform any modification because it may cause
            /// issues.
            /// </summary>
        public:
            property System::Boolean IsFromTrainingOrBook
            {
            public:
                System::Boolean __clrcall get() { return __v_IsFromTrainingOrBook; }

            internal:
                void __clrcall set(System::Boolean value) { __v_IsFromTrainingOrBook = value; }
            }

        private:
            static System::IntPtr __v___uncapperCompatibility;
        internal :
            static property System::IntPtr __uncapperCompatibility
            {
                System::IntPtr __clrcall get() { return __v___uncapperCompatibility; }

                void __clrcall set(System::IntPtr value) { __v___uncapperCompatibility = value; }
            }

        private:
            static System::Boolean __v___uncapperChecked;
        internal :
            static property System::Boolean __uncapperChecked
            {
                System::Boolean __clrcall get() { return __v___uncapperChecked; }

                void __clrcall set(System::Boolean value) { __v___uncapperChecked = value; }
            }
        };

        /// <summary>
        /// Arguments for OnInterruptCast event.
        /// </summary>
        public ref class InterruptCastEventArgs sealed: public HookedEventArgs
        {
        private:
            MagicCaster ^__v_Caster;
            /// <summary>
            /// The magic caster.
            /// </summary>
        public:
            property MagicCaster ^Caster
            {
            public:
                MagicCaster ^ __clrcall get() { return __v_Caster; }

            internal:
                void __clrcall set(MagicCaster ^value) { __v_Caster = value; }
            }
        };

        /// <summary>
        /// Arguments for OnMagicCasterFire event.
        /// </summary>
        public ref class MagicCasterFireEventArgs sealed: public HookedEventArgs
        {
        private:
            MagicCaster ^__v_Caster;
            /// <summary>
            /// The magic caster.
            /// </summary>
        public:
            property MagicCaster ^Caster
            {
            public:
                MagicCaster ^ __clrcall get() { return __v_Caster; }

            internal:
                void __clrcall set(MagicCaster ^value) { __v_Caster = value; }
            }

        private :
            MagicItem ^__v_Item;
            /// <summary>
            /// The magic that is being cast.
            /// </summary>
        public:
            property MagicItem ^Item
            {
            public:
                MagicItem ^ __clrcall get() { return __v_Item; }

            internal:
                void __clrcall set(MagicItem ^value) { __v_Item = value; }
            }
        };

        /// <summary>
        /// Arguments for OnMainMenu event.
        /// </summary>
        public ref class MainMenuEventArgs sealed: public HookedEventArgs
        {
        private:
            System::Boolean __v_Entering;
            /// <summary>
            /// Are we entering or exiting the main menu?
            /// </summary>
        public:
            property System::Boolean Entering
            {
            public:
                System::Boolean __clrcall get() { return __v_Entering; }

            internal:
                void __clrcall set(System::Boolean value) { __v_Entering = value; }
            }
        };

        /// <summary>
        /// Arguments for OnReduceHUDAmmoCounter event.
        /// </summary>
        public ref class ReduceHUDAmmoCounterEventArgs sealed: public HookedEventArgs
        {
        private:
            System::Int32 __v_HasAmount;
            /// <summary>
            /// The current ammo left (before reducing).
            /// </summary>
        public:
            property System::Int32 HasAmount
            {
            public:
                System::Int32 __clrcall get() { return __v_HasAmount; }

            internal:
                void __clrcall set(System::Int32 value) { __v_HasAmount = value; }
            }

        private:
            ExtraContainerChanges::ItemEntry ^__v_Item;
            /// <summary>
            /// The item entry being spent.
            /// </summary>
        public:
            property ExtraContainerChanges::ItemEntry ^Item
            {
            public:
                ExtraContainerChanges::ItemEntry ^ __clrcall get() { return __v_Item; }

            internal:
                void __clrcall set(ExtraContainerChanges::ItemEntry ^value) { __v_Item = value; }
            }

        private :
            System::Int32 __v_ReduceAmount;
            /// <summary>
            /// The amount of ammo to reduce.
            /// </summary>
        public:
            property System::Int32 ReduceAmount
            {
            public:
                System::Int32 __clrcall get() { return __v_ReduceAmount; }

            public:
                void __clrcall set(System::Int32 value) { __v_ReduceAmount = value; }
            }
        };

        /// <summary>
        /// Arguments for OnRemoveMagicEffectsWithArchetype event.
        /// </summary>
        public ref class RemoveMagicEffectsWithArchetypeEventArgs sealed: public HookedEventArgs
        {
        private:
            Actor ^__v_Owner;
            /// <summary>
            /// The actor who will have the effects removed.
            /// </summary>
        public:
            property Actor ^Owner
            {
            public:
                Actor ^ __clrcall get() { return __v_Owner; }

            internal:
                void __clrcall set(Actor ^value) { __v_Owner = value; }
            }

        private :
            MagicEffectRemovalReasons __v_Reason;
            /// <summary>
            /// The reason for removing.
            /// </summary>
        public:
            property MagicEffectRemovalReasons Reason
            {
            public:
                MagicEffectRemovalReasons __clrcall get() { return __v_Reason; }

            internal:
                void __clrcall set(MagicEffectRemovalReasons value) { __v_Reason = value; }
            }

        private:
            Archetypes __v_Archetype;
            /// <summary>
            /// The effect archetype that is being removed.
            /// </summary>
        public:
            property Archetypes Archetype
            {
            public:
                Archetypes __clrcall get() { return __v_Archetype; }

            internal:
                void __clrcall set(Archetypes value) { __v_Archetype = value; }
            }

        private:
            System::Boolean __v_Skip;
            /// <summary>
            /// Skip removing the effects?
            /// </summary>
        public:
            property System::Boolean Skip
            {
            public:
                System::Boolean __clrcall get() { return __v_Skip; }

            public:
                void __clrcall set(System::Boolean value) { __v_Skip = value; }
            }
        };

        /// <summary>
        /// Arguments for OnShadowCullingBegin event.
        /// </summary>
        public ref class ShadowCullingBeginEventArgs sealed: public HookedEventArgs
        {
        private:
            System::Boolean __v_Separate;
            /// <summary>
            /// Should shadow culling be separated from regular culling? It will be slower
            /// but allows enabling or disabling objects separately in different cull
            /// phases.
            /// </summary>
        public:
            property System::Boolean Separate
            {
            public:
                System::Boolean __clrcall get() { return __v_Separate; }

            public:
                void __clrcall set(System::Boolean value) { __v_Separate = value; }
            }

        private:
            static System::Int32 __v___state;
        internal :
            static property System::Int32 __state
            {
                System::Int32 __clrcall get() { return __v___state; }

                void __clrcall set(System::Int32 value) { __v___state = value; }
            }
        };

        /// <summary>
        /// Arguments for OnShadowCullingEnd event.
        /// </summary>
        public ref class ShadowCullingEndEventArgs sealed: public HookedEventArgs
        {
        private:
            System::Boolean __v_Separate;
            /// <summary>
            /// Were the culling phases separated?
            /// </summary>
        public:
            property System::Boolean Separate
            {
            public:
                System::Boolean __clrcall get() { return __v_Separate; }

            internal:
                void __clrcall set(System::Boolean value) { __v_Separate = value; }
            }
        };

        /// <summary>
        /// Arguments for OnSpendAmmo event.
        /// </summary>
        public ref class SpendAmmoEventArgs sealed: public HookedEventArgs
        {
        private:
            Actor ^__v_Spender;
            /// <summary>
            /// The actor that is spending ammo.
            /// </summary>
        public:
            property Actor ^Spender
            {
            public:
                Actor ^ __clrcall get() { return __v_Spender; }

            internal:
                void __clrcall set(Actor ^value) { __v_Spender = value; }
            }

        private :
            TESObjectWEAP ^__v_Weapon;
            /// <summary>
            /// The weapon that is being used.
            /// </summary>
        public:
            property TESObjectWEAP ^Weapon
            {
            public:
                TESObjectWEAP ^ __clrcall get() { return __v_Weapon; }

            internal:
                void __clrcall set(TESObjectWEAP ^value) { __v_Weapon = value; }
            }

        private :
            System::Int32 __v_HasAmount;
            /// <summary>
            /// Gets the amount of ammo the actor currently has.
            /// </summary>
        public:
            property System::Int32 HasAmount
            {
            public:
                System::Int32 __clrcall get() { return __v_HasAmount; }

            internal:
                void __clrcall set(System::Int32 value) { __v_HasAmount = value; }
            }

        private:
            System::Int32 __v_ReduceAmount;
            /// <summary>
            /// Gets or sets the amount of ammo being used.
            /// </summary>
        public:
            property System::Int32 ReduceAmount
            {
            public:
                System::Int32 __clrcall get() { return __v_ReduceAmount; }

            public:
                void __clrcall set(System::Int32 value) { __v_ReduceAmount = value; }
            }

        private:
            System::Boolean __v_Force;
            /// <summary>
            /// Force ammo to be used? If this is true then ammo will be used even if
            /// actor is not player and weapon does not have the flag to force use ammo.
            /// </summary>
        public:
            property System::Boolean Force
            {
            public:
                System::Boolean __clrcall get() { return __v_Force; }

            public:
                void __clrcall set(System::Boolean value) { __v_Force = value; }
            }

        private:
            System::Boolean __v_Had;
            /// <summary>
            /// Had reduce.
            /// </summary>
        internal :
            property System::Boolean Had
            {
                System::Boolean __clrcall get() { return __v_Had; }

                void __clrcall set(System::Boolean value) { __v_Had = value; }
            }
        };

        /// <summary>
        /// Arguments for OnSpendMagicCost event.
        /// </summary>
        public ref class SpendMagicCostEventArgs sealed: public HookedEventArgs
        {
        private:
            ActorMagicCaster ^__v_Spender;
            /// <summary>
            /// The actor caster that is going to spend cost.
            /// </summary>
        public:
            property ActorMagicCaster ^Spender
            {
            public:
                ActorMagicCaster ^ __clrcall get() { return __v_Spender; }

            internal:
                void __clrcall set(ActorMagicCaster ^value) { __v_Spender = value; }
            }

        private :
            MagicItem ^__v_Item;
            /// <summary>
            /// The spell or thing being cast.
            /// </summary>
        public:
            property MagicItem ^Item
            {
            public:
                MagicItem ^ __clrcall get() { return __v_Item; }

            internal:
                void __clrcall set(MagicItem ^value) { __v_Item = value; }
            }

        private :
            System::Int32 __v_ActorValueIndex;
            /// <summary>
            /// The actor value that is being spent to cast this. Setting this to -1 will
            /// make it not cost anything.
            /// </summary>
        public:
            property System::Int32 ActorValueIndex
            {
            public:
                System::Int32 __clrcall get() { return __v_ActorValueIndex; }

            public:
                void __clrcall set(System::Int32 value) { __v_ActorValueIndex = value; }
            }
        };

        /// <summary>
        /// Arguments for OnSpendPoison event.
        /// </summary>
        public ref class SpendPoisonEventArgs sealed: public HookedEventArgs
        {
        private:
            Actor ^__v_Spender;
            /// <summary>
            /// The actor that is spending the charge.
            /// </summary>
        public:
            property Actor ^Spender
            {
            public:
                Actor ^ __clrcall get() { return __v_Spender; }

            internal:
                void __clrcall set(Actor ^value) { __v_Spender = value; }
            }

        private :
            ExtraContainerChanges::ItemEntry ^__v_Item;
            /// <summary>
            /// The item that is being used in attack. This is the item that has the
            /// poison charge on it.
            /// </summary>
        public:
            property ExtraContainerChanges::ItemEntry ^Item
            {
            public:
                ExtraContainerChanges::ItemEntry ^ __clrcall get() { return __v_Item; }

            internal:
                void __clrcall set(ExtraContainerChanges::ItemEntry ^value) { __v_Item = value; }
            }

        private :
            System::Boolean __v_Skip;
            /// <summary>
            /// Skip reducing the poison charge?
            /// </summary>
        public:
            property System::Boolean Skip
            {
            public:
                System::Boolean __clrcall get() { return __v_Skip; }

            public:
                void __clrcall set(System::Boolean value) { __v_Skip = value; }
            }
        };

        /// <summary>
        /// Arguments for OnUpdateCamera event.
        /// </summary>
        public ref class UpdateCameraEventArgs sealed: public HookedEventArgs
        {
        private:
            TESCamera ^__v_Camera;
            /// <summary>
            /// The camera that is being updated.
            /// </summary>
        public:
            property TESCamera ^Camera
            {
            public:
                TESCamera ^ __clrcall get() { return __v_Camera; }

            internal:
                void __clrcall set(TESCamera ^value) { __v_Camera = value; }
            }
        };

        /// <summary>
        /// Arguments for OnUpdatedPlayerHeadtrack event.
        /// </summary>
        public ref class UpdatedPlayerHeadtrackEventArgs sealed: public HookedEventArgs {};

        /// <summary>
        /// Arguments for OnUpdatePlayerControls event.
        /// </summary>
        public ref class UpdatePlayerControlsEventArgs sealed: public HookedEventArgs
        {
        private:
            PlayerControls ^__v_Controls;
            /// <summary>
            /// The player controls instance.
            /// </summary>
        public:
            property PlayerControls ^Controls
            {
            public:
                PlayerControls ^ __clrcall get() { return __v_Controls; }

            internal:
                void __clrcall set(PlayerControls ^value) { __v_Controls = value; }
            }
        };

        /// <summary>
        /// Arguments for OnUpdatePlayerTurnToCamera event.
        /// </summary>
        public ref class UpdatePlayerTurnToCameraEventArgs sealed: public HookedEventArgs
        {
        private:
            Actor ^__v_Target;
            /// <summary>
            /// The actor that is being controlled by camera and will be rotated or not.
            /// </summary>
        public:
            property Actor ^Target
            {
            public:
                Actor ^ __clrcall get() { return __v_Target; }

            internal:
                void __clrcall set(Actor ^value) { __v_Target = value; }
            }

        private :
            System::Boolean __v_FreeLook;
            /// <summary>
            /// Should actor have free look or should they face the camera.
            /// </summary>
        public:
            property System::Boolean FreeLook
            {
            public:
                System::Boolean __clrcall get() { return __v_FreeLook; }

            public:
                void __clrcall set(System::Boolean value) { __v_FreeLook = value; }
            }

        private:
            System::Boolean __v_Moving;
            /// <summary>
            /// Is the actor moving?
            /// </summary>
        public:
            property System::Boolean Moving
            {
            public:
                System::Boolean __clrcall get() { return __v_Moving; }

            internal:
                void __clrcall set(System::Boolean value) { __v_Moving = value; }
            }

        private:
            System::Boolean __v_Weapon;
            /// <summary>
            /// Does the actor have weapon out?
            /// </summary>
        public:
            property System::Boolean Weapon
            {
            public:
                System::Boolean __clrcall get() { return __v_Weapon; }

            internal:
                void __clrcall set(System::Boolean value) { __v_Weapon = value; }
            }

        private:
            System::Boolean __v_Had;
            /// <summary>
            /// Internal value.
            /// </summary>
        internal :
            property System::Boolean Had
            {
                System::Boolean __clrcall get() { return __v_Had; }

                void __clrcall set(System::Boolean value) { __v_Had = value; }
            }
        };

        /// <summary>
        /// Arguments for OnWeaponFireProjectilePosition event.
        /// </summary>
        public ref class WeaponFireProjectilePositionEventArgs sealed: public HookedEventArgs
        {
        private:
            TESObjectREFR ^__v_Attacker;
            /// <summary>
            /// The object or actor who is firing this weapon.
            /// </summary>
        public:
            property TESObjectREFR ^Attacker
            {
            public:
                TESObjectREFR ^ __clrcall get() { return __v_Attacker; }

            internal:
                void __clrcall set(TESObjectREFR ^value) { __v_Attacker = value; }
            }

        private :
            NiAVObject ^__v_Node;
            /// <summary>
            /// The node where the position would be copied from. Set this to null to use
            /// the current point position instead.
            /// </summary>
        public:
            property NiAVObject ^Node
            {
            public:
                NiAVObject ^ __clrcall get() { return __v_Node; }

            public:
                void __clrcall set(NiAVObject ^value) { __v_Node = value; }
            }

        private :
            NiPoint3 ^__v_Position;
            /// <summary>
            /// The position that will be used if node is null.
            /// </summary>
        public:
            property NiPoint3 ^Position
            {
            public:
                NiPoint3 ^ __clrcall get() { return __v_Position; }

            internal:
                void __clrcall set(NiPoint3 ^value) { __v_Position = value; }
            }

        private :
            TESObjectWEAP ^__v_Weapon;
            /// <summary>
            /// The weapon that is being fired.
            /// </summary>
        public:
            property TESObjectWEAP ^Weapon
            {
            public:
                TESObjectWEAP ^ __clrcall get() { return __v_Weapon; }

            internal:
                void __clrcall set(TESObjectWEAP ^value) { __v_Weapon = value; }
            }
        };

        /// <summary>
        /// Contains events for game.
        /// </summary>
        public ref class Events sealed abstract
        {
        private:
            static Event<BarterMenuEventArgs ^> ^__handler_BarterMenu;

        public:
            /// <summary>
            /// Event is raised when opening or closing the trade/barter menu.
            /// </summary>
            /// <value>
            /// OnBarterMenu event handler.
            /// </value>
            static property Event<BarterMenuEventArgs ^> ^OnBarterMenu
            {
                Event<BarterMenuEventArgs ^> ^get() { return __handler_BarterMenu; }
            }
        private :
            static BarterMenuEventArgs ^__before_BarterMenu_1(CPURegisters ^ctx)
            {
                BarterMenuEventArgs ^args = gcnew BarterMenuEventArgs();
                args->Entering            = true;
                args->RefHandle           = Memory::ReadUInt32(__VIDS::VID519283.Value, false);
                return args;
            }

        private :
            static BarterMenuEventArgs ^__before_BarterMenu_2(CPURegisters ^ctx)
            {
                BarterMenuEventArgs ^args = gcnew BarterMenuEventArgs();
                args->Entering            = false;
                args->RefHandle           = Memory::ReadUInt32(__VIDS::VID519283.Value, false);
                return args;
            }

        private :
            static Event<CalculateDetectionEventArgs ^> ^__handler_CalculateDetection;

        public:
            /// <summary>
            /// Event is raised when calculating whether actor detects another.
            /// </summary>
            /// <value>
            /// OnCalculateDetection event handler.
            /// </value>
            static property Event<CalculateDetectionEventArgs ^> ^OnCalculateDetection
            {
                Event<CalculateDetectionEventArgs ^> ^get() { return __handler_CalculateDetection; }
            }
        private :
            static CalculateDetectionEventArgs ^__before_CalculateDetection(CPURegisters ^ctx)
            {
                CalculateDetectionEventArgs ^args = gcnew CalculateDetectionEventArgs();
                args->SourceActor                 = MemoryObject::FromAddress<Actor ^>(Memory::ReadPointer(ctx->SP + 0x70, false));
                args->TargetActor                 = MemoryObject::FromAddress<Actor ^>(ctx->BX);
                args->ResultValue                 = Memory::ReadInt32(ctx->BP + 0xD0, false);
                args->Position                    = MemoryObject::FromAddress<NiPoint3 ^>(Memory::ReadPointer(ctx->BP - 0x68, false));
                return args;
            }

        private :
            static void __after_CalculateDetection(CPURegisters ^ctx, CalculateDetectionEventArgs ^args)
            {
                int val = args->ResultValue;
                if(val < -1000)
                    val = -1000;
                Memory::WriteInt32(ctx->BP + 0xD0, val, false);
            }

        private:
            static Event<CalculateFormGoldValueEventArgs ^> ^__handler_CalculateFormGoldValue;

        public:
            /// <summary>
            /// Raised when calculating the gold value of a form.
            /// </summary>
            /// <value>
            /// OnCalculateFormGoldValue event handler.
            /// </value>
            static property Event<CalculateFormGoldValueEventArgs ^> ^OnCalculateFormGoldValue
            {
                Event<CalculateFormGoldValueEventArgs ^> ^get() { return __handler_CalculateFormGoldValue; }
            }
        private :
            static CalculateFormGoldValueEventArgs ^__before_CalculateFormGoldValue_1(CPURegisters ^ctx)
            {
                CalculateFormGoldValueEventArgs ^args = gcnew CalculateFormGoldValueEventArgs();
                args->Value                           = static_cast<int>(ctx->AX.ToInt64());
                args->OriginalValue                   = args->Value;
                args->Form                            = MemoryObject::FromAddress<TESForm ^>(ctx->BX);
                return args;
            }

        private :
            static void __after_CalculateFormGoldValue_1(CPURegisters ^ctx, CalculateFormGoldValueEventArgs ^args)
            {
                if(args->OriginalValue != args->Value)
                    ctx->AX = System::IntPtr(args->Value);
            }

        private:
            static CalculateFormGoldValueEventArgs ^__before_CalculateFormGoldValue_2(CPURegisters ^ctx)
            {
                CalculateFormGoldValueEventArgs ^args = gcnew CalculateFormGoldValueEventArgs();
                args->Value                           = static_cast<int>(ctx->AX.ToInt64());
                args->OriginalValue                   = args->Value;
                args->Form                            = MemoryObject::FromAddress<TESForm ^>(ctx->BX);
                return args;
            }

        private :
            static void __after_CalculateFormGoldValue_2(CPURegisters ^ctx, CalculateFormGoldValueEventArgs ^args)
            {
                if(args->OriginalValue != args->Value)
                    ctx->AX = System::IntPtr(args->Value);
            }

        private:
            static CalculateFormGoldValueEventArgs ^__before_CalculateFormGoldValue_3(CPURegisters ^ctx)
            {
                CalculateFormGoldValueEventArgs ^args = gcnew CalculateFormGoldValueEventArgs();
                args->Value                           = static_cast<int>(ctx->AX.ToInt64());
                args->OriginalValue                   = args->Value;
                args->Form                            = MemoryObject::FromAddress<TESForm ^>(ctx->BX);
                return args;
            }

        private :
            static void __after_CalculateFormGoldValue_3(CPURegisters ^ctx, CalculateFormGoldValueEventArgs ^args)
            {
                if(args->OriginalValue != args->Value)
                    ctx->AX = System::IntPtr(args->Value);
            }

        private:
            static Event<CameraStateChangingEventArgs ^> ^__handler_CameraStateChanging;

        public:
            /// <summary>
            /// This is raised when the camera state is about to change. This is called
            /// for all cameras not only player camera!
            /// </summary>
            /// <value>
            /// OnCameraStateChanging event handler.
            /// </value>
            static property Event<CameraStateChangingEventArgs ^> ^OnCameraStateChanging
            {
                Event<CameraStateChangingEventArgs ^> ^get() { return __handler_CameraStateChanging; }
            }
        private :
            static CameraStateChangingEventArgs ^__before_CameraStateChanging(CPURegisters ^ctx)
            {
                CameraStateChangingEventArgs ^args = gcnew CameraStateChangingEventArgs();
                args->Camera                       = MemoryObject::FromAddress<TESCamera ^>(ctx->CX);
                if(args->Camera != nullptr)
                    args->PreviousState = args->Camera->State;
                args->NextState = MemoryObject::FromAddress<TESCameraState ^>(ctx->DX);
                args->Skip      = false;
                return args;
            }

        private :
            static void __after_CameraStateChanging(CPURegisters ^ctx, CameraStateChangingEventArgs ^args)
            {
                if(args->Skip)
                    ctx->IP = __VIDS::VID32290.Value + 0x6A;
                else
                    ctx->DX = args->NextState != nullptr ? args->NextState->Cast<TESCameraState ^>() : System::IntPtr::Zero;
            }

        private:
            static Event<FrameEventArgs ^> ^__handler_Frame;

        public:
            /// <summary>
            /// Event is raised every frame.
            /// </summary>
            /// <value>
            /// OnFrame event handler.
            /// </value>
            static property Event<FrameEventArgs ^> ^OnFrame
            {
                Event<FrameEventArgs ^> ^get() { return __handler_Frame; }
            }
        private :
            static FrameEventArgs ^__before_Frame(CPURegisters ^ctx)
            {
                FrameEventArgs ^args = gcnew FrameEventArgs();
                return args;
            }

        private :
            static Event<GainLevelXPEventArgs ^> ^__handler_GainLevelXP;

        public:
            /// <summary>
            /// Raised when gaining character level XP from increasing a skill.
            /// </summary>
            /// <value>
            /// OnGainLevelXP event handler.
            /// </value>
            static property Event<GainLevelXPEventArgs ^> ^OnGainLevelXP
            {
                Event<GainLevelXPEventArgs ^> ^get() { return __handler_GainLevelXP; }
            }
        private :
            static GainLevelXPEventArgs ^__before_GainLevelXP(CPURegisters ^ctx)
            {
                GainLevelXPEventArgs ^args = gcnew GainLevelXPEventArgs();
                args->Skill                = static_cast<ActorValueIndices>(static_cast<int>(ctx->SI.ToInt64()));
                auto fromPtr               = Memory::ReadPointer(ctx->SP + 0x178, false);
                args->IsFromTrainingOrBook = (fromPtr == __VIDS::VID40555.Value + 0x9E) || (fromPtr == GainSkillXPEventArgs::__uncapperCompatibility);
                args->OriginalAmount       = ctx->XMM0f;
                args->Amount               = args->OriginalAmount;
                return args;
            }

        private :
            static void __after_GainLevelXP(CPURegisters ^ctx, GainLevelXPEventArgs ^args)
            {
                if(args->Amount != args->OriginalAmount)
                    ctx->XMM0f = args->Amount;
            }

        private:
            static Event<GainSkillXPEventArgs ^> ^__handler_GainSkillXP;

        public:
            /// <summary>
            /// Raised when player is gaining skill experience from any means.
            /// </summary>
            /// <value>
            /// OnGainSkillXP event handler.
            /// </value>
            static property Event<GainSkillXPEventArgs ^> ^OnGainSkillXP
            {
                Event<GainSkillXPEventArgs ^> ^get() { return __handler_GainSkillXP; }
            }
        private :
            static GainSkillXPEventArgs ^__before_GainSkillXP(CPURegisters ^ctx)
            {
                GainSkillXPEventArgs ^args = gcnew GainSkillXPEventArgs();
                if(!GainSkillXPEventArgs::__uncapperChecked)
                {
                    GainSkillXPEventArgs::__uncapperChecked = true;
                    auto           proc                     = System::Diagnostics::Process::GetCurrentProcess();
                    System::IntPtr uncapperPtr              = System::IntPtr::Zero;
                    if(proc != nullptr)
                    {
                        auto modules = proc->Modules;
                        for each(System::Diagnostics::ProcessModule ^m in modules)
                        {
                            if(m->ModuleName != nullptr && m->ModuleName->Equals("SkyrimUncapper.dll", System::StringComparison::OrdinalIgnoreCase))
                            {
                                uncapperPtr = m->BaseAddress + 0x4CE4;
                                break;
                            }
                        }
                    }
                    GainSkillXPEventArgs::__uncapperCompatibility = uncapperPtr;
                }

                args->BaseAmount           = Memory::ReadFloat(ctx->SP + 0x188, false);
                args->Skill                = static_cast<ActorValueIndices>(static_cast<int>(ctx->SI.ToInt64()));
                auto fromPtr               = Memory::ReadPointer(ctx->SP + 0x178, false);
                args->IsFromTrainingOrBook = (fromPtr == __VIDS::VID40555.Value + 0x9E) || (fromPtr == GainSkillXPEventArgs::__uncapperCompatibility);
                Memory::InvokeCdecl(__VIDS::VID23073.Value, ctx->CX, ctx->DX, ctx->R8, ctx->R9);
                args->OriginalAmount = Memory::ReadFloat(ctx->SP + 0x188, false);
                args->Amount         = args->OriginalAmount;
                return args;
            }

        private :
            static void __after_GainSkillXP(CPURegisters ^ctx, GainSkillXPEventArgs ^args)
            {
                if(args->Amount != args->OriginalAmount)
                    Memory::WriteFloat(ctx->SP + 0x188, args->Amount, false);
            }

        private:
            static Event<InterruptCastEventArgs ^> ^__handler_InterruptCast;

        public:
            /// <summary>
            /// Raised when casting is being interrupted.
            /// </summary>
            /// <value>
            /// OnInterruptCast event handler.
            /// </value>
            static property Event<InterruptCastEventArgs ^> ^OnInterruptCast
            {
                Event<InterruptCastEventArgs ^> ^get() { return __handler_InterruptCast; }
            }
        private :
            static InterruptCastEventArgs ^__before_InterruptCast(CPURegisters ^ctx)
            {
                InterruptCastEventArgs ^args = gcnew InterruptCastEventArgs();
                args->Caster                 = MemoryObject::FromAddress<MagicCaster ^>(ctx->BX);
                return args;
            }

        private :
            static Event<MagicCasterFireEventArgs ^> ^__handler_MagicCasterFire;

        public:
            /// <summary>
            /// Raised when magic caster fires the spell.
            /// </summary>
            /// <value>
            /// OnMagicCasterFire event handler.
            /// </value>
            static property Event<MagicCasterFireEventArgs ^> ^OnMagicCasterFire
            {
                Event<MagicCasterFireEventArgs ^> ^get() { return __handler_MagicCasterFire; }
            }
        private :
            static MagicCasterFireEventArgs ^__before_MagicCasterFire(CPURegisters ^ctx)
            {
                MagicCasterFireEventArgs ^args = gcnew MagicCasterFireEventArgs();
                args->Caster                   = MemoryObject::FromAddress<MagicCaster ^>(ctx->DI);
                args->Item                     = MemoryObject::FromAddress<MagicItem ^>(ctx->SI);
                return args;
            }

        private :
            static Event<MainMenuEventArgs ^> ^__handler_MainMenu;

        public:
            /// <summary>
            /// Event is raised when entering or exiting the Main Menu. This is called
            /// after the initial loading icon is finished when entering for the first
            /// time. This event is raised again if player returns to main menu from game
            /// later.
            /// </summary>
            /// <value>
            /// OnMainMenu event handler.
            /// </value>
            static property Event<MainMenuEventArgs ^> ^OnMainMenu
            {
                Event<MainMenuEventArgs ^> ^get() { return __handler_MainMenu; }
            }
        private :
            static MainMenuEventArgs ^__before_MainMenu_1(CPURegisters ^ctx)
            {
                MainMenuEventArgs ^args = gcnew MainMenuEventArgs();
                args->Entering          = false;
                return args;
            }

        private :
            static MainMenuEventArgs ^__before_MainMenu_2(CPURegisters ^ctx)
            {
                MainMenuEventArgs ^args = gcnew MainMenuEventArgs();
                args->Entering          = true;
                return args;
            }

        private :
            static Event<ReduceHUDAmmoCounterEventArgs ^> ^__handler_ReduceHUDAmmoCounter;

        public:
            /// <summary>
            /// This is raised when player uses ammo and we are about to reduce the
            /// counter in HUD.
            /// </summary>
            /// <value>
            /// OnReduceHUDAmmoCounter event handler.
            /// </value>
            static property Event<ReduceHUDAmmoCounterEventArgs ^> ^OnReduceHUDAmmoCounter
            {
                Event<ReduceHUDAmmoCounterEventArgs ^> ^get() { return __handler_ReduceHUDAmmoCounter; }
            }
        private :
            static ReduceHUDAmmoCounterEventArgs ^__before_ReduceHUDAmmoCounter(CPURegisters ^ctx)
            {
                ReduceHUDAmmoCounterEventArgs ^args = gcnew ReduceHUDAmmoCounterEventArgs();
                args->HasAmount                     = static_cast<int>(MCH::u(ctx->AX) & 0xFFFFFFFF);
                args->Item                          = MemoryObject::FromAddress<ExtraContainerChanges::ItemEntry ^>(ctx->R14);
                args->ReduceAmount                  = 1;
                return args;
            }

        private :
            static void __after_ReduceHUDAmmoCounter(CPURegisters ^ctx, ReduceHUDAmmoCounterEventArgs ^args)
            {
                int newCount = args->HasAmount - args->ReduceAmount;
                ctx->AX      = System::IntPtr(static_cast<long long>(static_cast<unsigned>(newCount)));
            }

        private:
            static Event<RemoveMagicEffectsWithArchetypeEventArgs ^> ^__handler_RemoveMagicEffectsWithArchetype;

        public:
            /// <summary>
            /// Raised when going to remove magic effects with specific archetype due to
            /// actions taken.
            /// </summary>
            /// <value>
            /// OnRemoveMagicEffectsWithArchetype event handler.
            /// </value>
            static property Event<RemoveMagicEffectsWithArchetypeEventArgs ^> ^OnRemoveMagicEffectsWithArchetype
            {
                Event<RemoveMagicEffectsWithArchetypeEventArgs ^> ^get() { return __handler_RemoveMagicEffectsWithArchetype; }
            }
        private :
            static RemoveMagicEffectsWithArchetypeEventArgs ^__before_RemoveMagicEffectsWithArchetype_1(CPURegisters ^ctx)
            {
                RemoveMagicEffectsWithArchetypeEventArgs ^args = gcnew RemoveMagicEffectsWithArchetypeEventArgs();
                args->Owner                                    = MemoryObject::FromAddress<Actor ^>(ctx->BX);
                args->Reason                                   = MagicEffectRemovalReasons::Unknown;
                args->Archetype                                = static_cast<Archetypes>(MCH::u(ctx->DX) & 0xFFFFFFFF);
                args->Skip                                     = false;
                auto debug                                     = NetScriptFramework::Main::GameInfo;
                if(debug != nullptr)
                {
                    auto fromPtr = Memory::ReadPointer(ctx->SP + 0x28, false);
                    auto fn      = debug->GetFunctionInfo(fromPtr, true);
                    if(fn != nullptr)
                    {
                        switch(fn->Id)
                        {
                        case 19369: args->Reason = MagicEffectRemovalReasons::Activation;
                            break;
                        case 33363: args->Reason = MagicEffectRemovalReasons::CastSpellOrUseMagicItem;
                            break;
                        case 36220: args->Reason = MagicEffectRemovalReasons::Dialogue;
                            break;
                        case 37650: args->Reason = MagicEffectRemovalReasons::Attacking;
                            break;
                        case 41778: args->Reason = MagicEffectRemovalReasons::Attacking;
                            break;
                        }
                    }
                }
                return args;
            }

        private :
            static void __after_RemoveMagicEffectsWithArchetype_1(CPURegisters ^ctx, RemoveMagicEffectsWithArchetypeEventArgs ^args)
            {
                if(args->Skip)
                    ctx->Skip();
            }

        private:
            static RemoveMagicEffectsWithArchetypeEventArgs ^__before_RemoveMagicEffectsWithArchetype_2(CPURegisters ^ctx)
            {
                RemoveMagicEffectsWithArchetypeEventArgs ^args = gcnew RemoveMagicEffectsWithArchetypeEventArgs();
                args->Owner                                    = MemoryObject::FromAddress<Actor ^>(ctx->BX);
                args->Reason                                   = MagicEffectRemovalReasons::Unknown;
                args->Archetype                                = static_cast<Archetypes>(MCH::u(ctx->DX) & 0xFFFFFFFF);
                args->Skip                                     = false;
                auto debug                                     = NetScriptFramework::Main::GameInfo;
                if(debug != nullptr)
                {
                    auto fromPtr = Memory::ReadPointer(ctx->SP + 0x28, false);
                    auto fn      = debug->GetFunctionInfo(fromPtr, true);
                    if(fn != nullptr)
                    {
                        switch(fn->Id)
                        {
                        case 19369: args->Reason = MagicEffectRemovalReasons::Activation;
                            break;
                        case 33363: args->Reason = MagicEffectRemovalReasons::CastSpellOrUseMagicItem;
                            break;
                        case 36220: args->Reason = MagicEffectRemovalReasons::Dialogue;
                            break;
                        case 37650: args->Reason = MagicEffectRemovalReasons::Attacking;
                            break;
                        case 41778: args->Reason = MagicEffectRemovalReasons::Attacking;
                            break;
                        }
                    }
                }
                return args;
            }

        private :
            static void __after_RemoveMagicEffectsWithArchetype_2(CPURegisters ^ctx, RemoveMagicEffectsWithArchetypeEventArgs ^args)
            {
                if(args->Skip)
                    ctx->Skip();
            }

        private:
            static RemoveMagicEffectsWithArchetypeEventArgs ^__before_RemoveMagicEffectsWithArchetype_3(CPURegisters ^ctx)
            {
                RemoveMagicEffectsWithArchetypeEventArgs ^args = gcnew RemoveMagicEffectsWithArchetypeEventArgs();
                args->Owner                                    = MemoryObject::FromAddress<Actor ^>(ctx->DI);
                args->Reason                                   = MagicEffectRemovalReasons::Attacked;
                args->Archetype                                = static_cast<Archetypes>(MCH::u(ctx->DX) & 0xFFFFFFFF);
                args->Skip                                     = false;
                return args;
            }

        private :
            static void __after_RemoveMagicEffectsWithArchetype_3(CPURegisters ^ctx, RemoveMagicEffectsWithArchetypeEventArgs ^args)
            {
                if(args->Skip)
                    ctx->Skip();
            }

        private:
            static Event<ShadowCullingBeginEventArgs ^> ^__handler_ShadowCullingBegin;

        public:
            /// <summary>
            /// Event is raised when shadow culling is about to begin.
            /// </summary>
            /// <value>
            /// OnShadowCullingBegin event handler.
            /// </value>
            static property Event<ShadowCullingBeginEventArgs ^> ^OnShadowCullingBegin
            {
                Event<ShadowCullingBeginEventArgs ^> ^get() { return __handler_ShadowCullingBegin; }
            }
        private :
            static ShadowCullingBeginEventArgs ^__before_ShadowCullingBegin(CPURegisters ^ctx)
            {
                ShadowCullingBeginEventArgs ^args = gcnew ShadowCullingBeginEventArgs();
                return args;
            }

        private :
            static void __after_ShadowCullingBegin(CPURegisters ^ctx, ShadowCullingBeginEventArgs ^args)
            {
                if(args->Separate)
                    ShadowCullingBeginEventArgs::__state = 1;
                else
                {
                    ShadowCullingBeginEventArgs::__state = 0;
                    auto ptr                             = Memory::ReadPointer(__VIDS::VID527999.Value, false);
                    Memory::InvokeCdecl(__VIDS::VID67991.Value, ptr, 0, 0);
                }
                Memory::InvokeCdecl(__VIDS::VID100419.Value);
            }

        private:
            static Event<ShadowCullingEndEventArgs ^> ^__handler_ShadowCullingEnd;

        public:
            /// <summary>
            /// Event is raised when shadow culling is finishing.
            /// </summary>
            /// <value>
            /// OnShadowCullingEnd event handler.
            /// </value>
            static property Event<ShadowCullingEndEventArgs ^> ^OnShadowCullingEnd
            {
                Event<ShadowCullingEndEventArgs ^> ^get() { return __handler_ShadowCullingEnd; }
            }
        private :
            static ShadowCullingEndEventArgs ^__before_ShadowCullingEnd(CPURegisters ^ctx)
            {
                ShadowCullingEndEventArgs ^args = gcnew ShadowCullingEndEventArgs();
                args->Separate                  = ShadowCullingBeginEventArgs::__state != 0;
                return args;
            }

        private :
            static void __after_ShadowCullingEnd(CPURegisters ^ctx, ShadowCullingEndEventArgs ^args)
            {
                auto ptr = Memory::ReadPointer(__VIDS::VID527999.Value, false);
                if(args->Separate)
                    Memory::InvokeCdecl(__VIDS::VID67991.Value, ptr, 0, 0);
                Memory::InvokeCdecl(__VIDS::VID67992.Value, ptr);
            }

        private:
            static Event<SpendAmmoEventArgs ^> ^__handler_SpendAmmo;

        public:
            /// <summary>
            /// Event is raised just before actor is about to spend ammo.
            /// </summary>
            /// <value>
            /// OnSpendAmmo event handler.
            /// </value>
            static property Event<SpendAmmoEventArgs ^> ^OnSpendAmmo
            {
                Event<SpendAmmoEventArgs ^> ^get() { return __handler_SpendAmmo; }
            }
        private :
            static SpendAmmoEventArgs ^__before_SpendAmmo(CPURegisters ^ctx)
            {
                SpendAmmoEventArgs ^args = gcnew SpendAmmoEventArgs();
                args->Spender            = MemoryObject::FromAddress<Actor ^>(ctx->BX);
                args->Weapon             = MemoryObject::FromAddress<TESObjectWEAP ^>(ctx->BP);
                args->HasAmount          = static_cast<int>(MCH::u(ctx->DI) & 0xFFFFFFFF);
                args->ReduceAmount       = static_cast<int>(MCH::u(ctx->SI) & 0xFFFFFFFF);
                args->Force              = false;
                args->Had                = args->ReduceAmount > 0;
                return args;
            }

        private :
            static void __after_SpendAmmo(CPURegisters ^ctx, SpendAmmoEventArgs ^args)
            {
                if(args->Had && args->ReduceAmount <= 0)
                {
                    ctx->R14 = Memory::ReadPointer(ctx->AX, false);
                    ctx->IP  = __VIDS::VID37596.Value + 0x20A;
                    return;
                }
                ctx->SI = System::IntPtr(static_cast<long long>(static_cast<unsigned>(args->ReduceAmount)));
                if(args->Force)
                {
                    ctx->R14 = Memory::ReadPointer(ctx->AX, false);
                    ctx->IP  = __VIDS::VID37596.Value + 0xD0;
                }
            }

        private:
            static Event<SpendMagicCostEventArgs ^> ^__handler_SpendMagicCost;

        public:
            /// <summary>
            /// Event is raised when we are about to calculate the casting cost for a
            /// magic thing.
            /// </summary>
            /// <value>
            /// OnSpendMagicCost event handler.
            /// </value>
            static property Event<SpendMagicCostEventArgs ^> ^OnSpendMagicCost
            {
                Event<SpendMagicCostEventArgs ^> ^get() { return __handler_SpendMagicCost; }
            }
        private :
            static SpendMagicCostEventArgs ^__before_SpendMagicCost(CPURegisters ^ctx)
            {
                SpendMagicCostEventArgs ^args = gcnew SpendMagicCostEventArgs();
                args->Spender                 = MemoryObject::FromAddress<ActorMagicCaster ^>(ctx->BX);
                args->Item                    = MemoryObject::FromAddress<MagicItem ^>(ctx->DI);
                args->ActorValueIndex         = static_cast<int>(MCH::u(ctx->AX) & 0xFFFFFFFF);
                return args;
            }

        private :
            static void __after_SpendMagicCost(CPURegisters ^ctx, SpendMagicCostEventArgs ^args) { ctx->AX = System::IntPtr(static_cast<long long>(static_cast<unsigned>(args->ActorValueIndex))); }

        private:
            static Event<SpendPoisonEventArgs ^> ^__handler_SpendPoison;

        public:
            /// <summary>
            /// Event is raised when poison charge from weapon is about to be spent.
            /// </summary>
            /// <value>
            /// OnSpendPoison event handler.
            /// </value>
            static property Event<SpendPoisonEventArgs ^> ^OnSpendPoison
            {
                Event<SpendPoisonEventArgs ^> ^get() { return __handler_SpendPoison; }
            }
        private :
            static SpendPoisonEventArgs ^__before_SpendPoison_1(CPURegisters ^ctx)
            {
                SpendPoisonEventArgs ^args = gcnew SpendPoisonEventArgs();
                args->Spender              = MemoryObject::FromAddress<Actor ^>(ctx->BX);
                args->Item                 = MemoryObject::FromAddress<ExtraContainerChanges::ItemEntry ^>(ctx->DI);
                args->Skip                 = false;
                return args;
            }

        private :
            static void __after_SpendPoison_1(CPURegisters ^ctx, SpendPoisonEventArgs ^args)
            {
                if(args->Skip)
                    ctx->Skip();
            }

        private:
            static SpendPoisonEventArgs ^__before_SpendPoison_2(CPURegisters ^ctx)
            {
                SpendPoisonEventArgs ^args = gcnew SpendPoisonEventArgs();
                args->Spender              = MemoryObject::FromAddress<Actor ^>(ctx->SI);
                args->Item                 = MemoryObject::FromAddress<ExtraContainerChanges::ItemEntry ^>(ctx->DI);
                args->Skip                 = false;
                return args;
            }

        private :
            static void __after_SpendPoison_2(CPURegisters ^ctx, SpendPoisonEventArgs ^args)
            {
                if(args->Skip)
                    ctx->Skip();
            }

        private:
            static Event<UpdateCameraEventArgs ^> ^__handler_UpdateCamera;

        public:
            /// <summary>
            /// Event is raised when camera is updated.
            /// </summary>
            /// <value>
            /// OnUpdateCamera event handler.
            /// </value>
            static property Event<UpdateCameraEventArgs ^> ^OnUpdateCamera
            {
                Event<UpdateCameraEventArgs ^> ^get() { return __handler_UpdateCamera; }
            }
        private :
            static UpdateCameraEventArgs ^__before_UpdateCamera(CPURegisters ^ctx)
            {
                UpdateCameraEventArgs ^args = gcnew UpdateCameraEventArgs();
                args->Camera                = MemoryObject::FromAddress<TESCamera ^>(ctx->DI);
                return args;
            }

        private :
            static Event<UpdatedPlayerHeadtrackEventArgs ^> ^__handler_UpdatedPlayerHeadtrack;

        public:
            /// <summary>
            /// This event is raised just after calculating player head track. This is
            /// here so you can overwrite it if you wish.
            /// </summary>
            /// <value>
            /// OnUpdatedPlayerHeadtrack event handler.
            /// </value>
            static property Event<UpdatedPlayerHeadtrackEventArgs ^> ^OnUpdatedPlayerHeadtrack
            {
                Event<UpdatedPlayerHeadtrackEventArgs ^> ^get() { return __handler_UpdatedPlayerHeadtrack; }
            }
        private :
            static UpdatedPlayerHeadtrackEventArgs ^__before_UpdatedPlayerHeadtrack(CPURegisters ^ctx)
            {
                UpdatedPlayerHeadtrackEventArgs ^args = gcnew UpdatedPlayerHeadtrackEventArgs();
                return args;
            }

        private :
            static Event<UpdatePlayerControlsEventArgs ^> ^__handler_UpdatePlayerControls;

        public:
            /// <summary>
            /// This is raised every frame to update the player controls. It is not raised
            /// in menus or if the controls should not reach the player character for
            /// other reasons.
            /// </summary>
            /// <value>
            /// OnUpdatePlayerControls event handler.
            /// </value>
            static property Event<UpdatePlayerControlsEventArgs ^> ^OnUpdatePlayerControls
            {
                Event<UpdatePlayerControlsEventArgs ^> ^get() { return __handler_UpdatePlayerControls; }
            }
        private :
            static UpdatePlayerControlsEventArgs ^__before_UpdatePlayerControls(CPURegisters ^ctx)
            {
                UpdatePlayerControlsEventArgs ^args = gcnew UpdatePlayerControlsEventArgs();
                args->Controls                      = MemoryObject::FromAddress<PlayerControls ^>(ctx->BX);
                return args;
            }

        private :
            static void __after_UpdatePlayerControls(CPURegisters ^ctx, UpdatePlayerControlsEventArgs ^args)
            {
                if(Memory::ReadUInt8(ctx->BX + 0x4F, false) == 0)
                    ctx->IP = __VIDS::VID41259.Value + 0x9A;
            }

        private:
            static Event<UpdatePlayerTurnToCameraEventArgs ^> ^__handler_UpdatePlayerTurnToCamera;

        public:
            /// <summary>
            /// Event is raised when calculating whether player should turn to where
            /// camera is facing.
            /// </summary>
            /// <value>
            /// OnUpdatePlayerTurnToCamera event handler.
            /// </value>
            static property Event<UpdatePlayerTurnToCameraEventArgs ^> ^OnUpdatePlayerTurnToCamera
            {
                Event<UpdatePlayerTurnToCameraEventArgs ^> ^get() { return __handler_UpdatePlayerTurnToCamera; }
            }
        private :
            static UpdatePlayerTurnToCameraEventArgs ^__before_UpdatePlayerTurnToCamera(CPURegisters ^ctx)
            {
                UpdatePlayerTurnToCameraEventArgs ^args = gcnew UpdatePlayerTurnToCameraEventArgs();
                args->Target                            = MemoryObject::FromAddress<Actor ^>(ctx->AX);
                args->FreeLook                          = MCH::b(ctx->DI);
                args->Moving                            = MCH::b(ctx->DX);
                args->Weapon                            = !MCH::b(ctx->SI);
                args->Had                               = args->FreeLook;
                return args;
            }

        private :
            static void __after_UpdatePlayerTurnToCamera(CPURegisters ^ctx, UpdatePlayerTurnToCameraEventArgs ^args)
            {
                if(args->Had != args->FreeLook)
                {
                    ctx->DI = System::IntPtr(static_cast<long long>(args->FreeLook ? 1 : 0));
                    if(args->FreeLook)
                    {
                        ctx->DX = System::IntPtr::Zero;
                        ctx->SI = System::IntPtr(static_cast<long long>(1));
                    }
                    else
                    {
                        ctx->SI = System::IntPtr::Zero;
                        ctx->DX = System::IntPtr(static_cast<long long>(1));
                    }
                }
            }

        private:
            static Event<WeaponFireProjectilePositionEventArgs ^> ^__handler_WeaponFireProjectilePosition;

        public:
            /// <summary>
            /// Event is raised when creating a projectile from firing a weapon. Here you
            /// can overwrite the position where the projectile begins from.
            /// </summary>
            /// <value>
            /// OnWeaponFireProjectilePosition event handler.
            /// </value>
            static property Event<WeaponFireProjectilePositionEventArgs ^> ^OnWeaponFireProjectilePosition
            {
                Event<WeaponFireProjectilePositionEventArgs ^> ^get() { return __handler_WeaponFireProjectilePosition; }
            }
        private :
            static WeaponFireProjectilePositionEventArgs ^__before_WeaponFireProjectilePosition(CPURegisters ^ctx)
            {
                WeaponFireProjectilePositionEventArgs ^args = gcnew WeaponFireProjectilePositionEventArgs();
                args->Attacker                              = MemoryObject::FromAddress<TESObjectREFR ^>(ctx->R13);
                args->Node                                  = MemoryObject::FromAddress<NiAVObject ^>(ctx->AX);
                args->Position                              = MemoryObject::FromAddress<NiPoint3 ^>(ctx->SP + 0x48);
                args->Weapon                                = MemoryObject::FromAddress<TESObjectWEAP ^>(ctx->R12);
                return args;
            }

        private :
            static void __after_WeaponFireProjectilePosition(CPURegisters ^ctx, WeaponFireProjectilePositionEventArgs ^args)
            {
                auto ptr = args->Node != nullptr ? args->Node->Cast<NiAVObject ^>() : System::IntPtr::Zero;
                ctx->AX  = ptr;
                ctx->BX  = ptr;
            }

        internal :
            static void InitializeEvents()
            {
                __handler_BarterMenu = gcnew EventHook<BarterMenuEventArgs ^>(
                    EventHookFlags::None,
                    "BarterMenu",
                    gcnew EventHookParameters<BarterMenuEventArgs ^>(__VIDS::VID49999.Value, 5, 5, "48 89 4C 24 08", gcnew System::Func<CPURegisters ^, BarterMenuEventArgs ^>(__before_BarterMenu_1), nullptr),
                    gcnew EventHookParameters<BarterMenuEventArgs ^>(__VIDS::VID50066.Value + 0x19, 5, 5, "E8", gcnew System::Func<CPURegisters ^, BarterMenuEventArgs ^>(__before_BarterMenu_2), nullptr)
                    );
                __handler_CalculateDetection = gcnew EventHook<CalculateDetectionEventArgs ^>(
                    EventHookFlags::None,
                    "CalculateDetection",
                    gcnew EventHookParameters<CalculateDetectionEventArgs ^>(
                        __VIDS::VID41659.Value + 0x52B,
                        6,
                        6,
                        "8B 95 D0 00 00 00",
                        gcnew System::Func<CPURegisters ^, CalculateDetectionEventArgs ^>(__before_CalculateDetection),
                        gcnew System::Action<CPURegisters ^, CalculateDetectionEventArgs ^>(__after_CalculateDetection)
                        )
                    );
                __handler_CalculateFormGoldValue = gcnew EventHook<CalculateFormGoldValueEventArgs ^>(
                    EventHookFlags::None,
                    "CalculateFormGoldValue",
                    gcnew EventHookParameters<CalculateFormGoldValueEventArgs ^>(
                        __VIDS::VID14799.Value + 0x2E,
                        5,
                        5,
                        "48 83 C4 30 5B",
                        gcnew System::Func<CPURegisters ^, CalculateFormGoldValueEventArgs ^>(__before_CalculateFormGoldValue_1),
                        gcnew System::Action<CPURegisters ^, CalculateFormGoldValueEventArgs ^>(__after_CalculateFormGoldValue_1)
                        ),
                    gcnew EventHookParameters<CalculateFormGoldValueEventArgs ^>(
                        __VIDS::VID14799.Value + 0x67,
                        5,
                        5,
                        "48 83 C4 30 5B",
                        gcnew System::Func<CPURegisters ^, CalculateFormGoldValueEventArgs ^>(__before_CalculateFormGoldValue_2),
                        gcnew System::Action<CPURegisters ^, CalculateFormGoldValueEventArgs ^>(__after_CalculateFormGoldValue_2)
                        ),
                    gcnew EventHookParameters<CalculateFormGoldValueEventArgs ^>(
                        __VIDS::VID14799.Value + 0x95,
                        5,
                        5,
                        "48 83 C4 30 5B",
                        gcnew System::Func<CPURegisters ^, CalculateFormGoldValueEventArgs ^>(__before_CalculateFormGoldValue_3),
                        gcnew System::Action<CPURegisters ^, CalculateFormGoldValueEventArgs ^>(__after_CalculateFormGoldValue_3)
                        )
                    );
                __handler_CameraStateChanging = gcnew EventHook<CameraStateChangingEventArgs ^>(
                    EventHookFlags::None,
                    "CameraStateChanging",
                    gcnew EventHookParameters<CameraStateChangingEventArgs ^>(
                        __VIDS::VID32290.Value,
                        5,
                        5,
                        "48 89 5C 24 08",
                        gcnew System::Func<CPURegisters ^, CameraStateChangingEventArgs ^>(__before_CameraStateChanging),
                        gcnew System::Action<CPURegisters ^, CameraStateChangingEventArgs ^>(__after_CameraStateChanging)
                        )
                    );
                __handler_Frame = gcnew EventHook<FrameEventArgs ^>(
                    EventHookFlags::None,
                    "Frame",
                    gcnew EventHookParameters<FrameEventArgs ^>(__VIDS::VID35565.Value + 0x1E, 5, 5, "E8", gcnew System::Func<CPURegisters ^, FrameEventArgs ^>(__before_Frame), nullptr)
                    );
                __handler_GainLevelXP = gcnew EventHook<GainLevelXPEventArgs ^>(
                    EventHookFlags::None,
                    "GainLevelXP",
                    gcnew EventHookParameters<GainLevelXPEventArgs ^>(
                        __VIDS::VID40554.Value + 0x230,
                        6,
                        6,
                        "F3 0F 58 01 8B D6",
                        gcnew System::Func<CPURegisters ^, GainLevelXPEventArgs ^>(__before_GainLevelXP),
                        gcnew System::Action<CPURegisters ^, GainLevelXPEventArgs ^>(__after_GainLevelXP)
                        )
                    );
                __handler_GainSkillXP = gcnew EventHook<GainSkillXPEventArgs ^>(
                    EventHookFlags::AlwaysRun,
                    "GainSkillXP",
                    gcnew EventHookParameters<GainSkillXPEventArgs ^>(
                        __VIDS::VID40554.Value + 0x143,
                        5,
                        0,
                        "E8 ? ? ? ? 48 8B 05",
                        gcnew System::Func<CPURegisters ^, GainSkillXPEventArgs ^>(__before_GainSkillXP),
                        gcnew System::Action<CPURegisters ^, GainSkillXPEventArgs ^>(__after_GainSkillXP)
                        )
                    );
                __handler_InterruptCast = gcnew EventHook<InterruptCastEventArgs ^>(
                    EventHookFlags::None,
                    "InterruptCast",
                    gcnew EventHookParameters<InterruptCastEventArgs ^>(__VIDS::VID33630.Value + 0x10, 6, 6, "48 8B 01 FF 50 40", gcnew System::Func<CPURegisters ^, InterruptCastEventArgs ^>(__before_InterruptCast), nullptr)
                    );
                __handler_MagicCasterFire = gcnew EventHook<MagicCasterFireEventArgs ^>(
                    EventHookFlags::None,
                    "MagicCasterFire",
                    gcnew EventHookParameters<MagicCasterFireEventArgs ^>(__VIDS::VID33629.Value + 0xF7, 6, 6, "4C 8B 17 4C 8B CE", gcnew System::Func<CPURegisters ^, MagicCasterFireEventArgs ^>(__before_MagicCasterFire), nullptr)
                    );
                __handler_MainMenu = gcnew EventHook<MainMenuEventArgs ^>(
                    EventHookFlags::None,
                    "MainMenu",
                    gcnew EventHookParameters<MainMenuEventArgs ^>(__VIDS::VID51324.Value + 0x19, 5, 5, "E8", gcnew System::Func<CPURegisters ^, MainMenuEventArgs ^>(__before_MainMenu_1), nullptr),
                    gcnew EventHookParameters<MainMenuEventArgs ^>(__VIDS::VID51236.Value, 5, 5, "48 89 4C 24 08", gcnew System::Func<CPURegisters ^, MainMenuEventArgs ^>(__before_MainMenu_2), nullptr)
                    );
                __handler_ReduceHUDAmmoCounter = gcnew EventHook<ReduceHUDAmmoCounterEventArgs ^>(
                    EventHookFlags::AlwaysRun,
                    "ReduceHUDAmmoCounter",
                    gcnew EventHookParameters<ReduceHUDAmmoCounterEventArgs ^>(
                        __VIDS::VID42928.Value + 0x63A,
                        7,
                        -5,
                        "E8 ?? ?? ?? ?? FF C8",
                        gcnew System::Func<CPURegisters ^, ReduceHUDAmmoCounterEventArgs ^>(__before_ReduceHUDAmmoCounter),
                        gcnew System::Action<CPURegisters ^, ReduceHUDAmmoCounterEventArgs ^>(__after_ReduceHUDAmmoCounter)
                        )
                    );
                __handler_RemoveMagicEffectsWithArchetype = gcnew EventHook<RemoveMagicEffectsWithArchetypeEventArgs ^>(
                    EventHookFlags::None,
                    "RemoveMagicEffectsWithArchetype",
                    gcnew EventHookParameters<RemoveMagicEffectsWithArchetypeEventArgs ^>(
                        __VIDS::VID37864.Value + 0x2B,
                        5,
                        5,
                        "E8",
                        gcnew System::Func<CPURegisters ^, RemoveMagicEffectsWithArchetypeEventArgs ^>(__before_RemoveMagicEffectsWithArchetype_1),
                        gcnew System::Action<CPURegisters ^, RemoveMagicEffectsWithArchetypeEventArgs ^>(__after_RemoveMagicEffectsWithArchetype_1)
                        ),
                    gcnew EventHookParameters<RemoveMagicEffectsWithArchetypeEventArgs ^>(
                        __VIDS::VID37864.Value + 0x4F,
                        5,
                        5,
                        "E8",
                        gcnew System::Func<CPURegisters ^, RemoveMagicEffectsWithArchetypeEventArgs ^>(__before_RemoveMagicEffectsWithArchetype_2),
                        gcnew System::Action<CPURegisters ^, RemoveMagicEffectsWithArchetypeEventArgs ^>(__after_RemoveMagicEffectsWithArchetype_2)
                        ),
                    gcnew EventHookParameters<RemoveMagicEffectsWithArchetypeEventArgs ^>(
                        __VIDS::VID37672.Value + 0xB9,
                        5,
                        5,
                        "E8",
                        gcnew System::Func<CPURegisters ^, RemoveMagicEffectsWithArchetypeEventArgs ^>(__before_RemoveMagicEffectsWithArchetype_3),
                        gcnew System::Action<CPURegisters ^, RemoveMagicEffectsWithArchetypeEventArgs ^>(__after_RemoveMagicEffectsWithArchetype_3)
                        )
                    );
                __handler_ShadowCullingBegin = gcnew EventHook<ShadowCullingBeginEventArgs ^>(
                    EventHookFlags::AlwaysRun,
                    "ShadowCullingBegin",
                    gcnew EventHookParameters<ShadowCullingBeginEventArgs ^>(
                        __VIDS::VID100414.Value + 0x2CC,
                        5,
                        0,
                        "E8",
                        gcnew System::Func<CPURegisters ^, ShadowCullingBeginEventArgs ^>(__before_ShadowCullingBegin),
                        gcnew System::Action<CPURegisters ^, ShadowCullingBeginEventArgs ^>(__after_ShadowCullingBegin)
                        )
                    );
                __handler_ShadowCullingEnd = gcnew EventHook<ShadowCullingEndEventArgs ^>(
                    EventHookFlags::AlwaysRun,
                    "ShadowCullingEnd",
                    gcnew EventHookParameters<ShadowCullingEndEventArgs ^>(
                        __VIDS::VID100414.Value + 0x2D1,
                        0x12,
                        0,
                        "E8",
                        gcnew System::Func<CPURegisters ^, ShadowCullingEndEventArgs ^>(__before_ShadowCullingEnd),
                        gcnew System::Action<CPURegisters ^, ShadowCullingEndEventArgs ^>(__after_ShadowCullingEnd)
                        )
                    );
                __handler_SpendAmmo = gcnew EventHook<SpendAmmoEventArgs ^>(
                    EventHookFlags::None,
                    "SpendAmmo",
                    gcnew EventHookParameters<SpendAmmoEventArgs ^>(
                        __VIDS::VID37596.Value + 0x84,
                        5,
                        -5,
                        "E8",
                        gcnew System::Func<CPURegisters ^, SpendAmmoEventArgs ^>(__before_SpendAmmo),
                        gcnew System::Action<CPURegisters ^, SpendAmmoEventArgs ^>(__after_SpendAmmo)
                        )
                    );
                __handler_SpendMagicCost = gcnew EventHook<SpendMagicCostEventArgs ^>(
                    EventHookFlags::None,
                    "SpendMagicCost",
                    gcnew EventHookParameters<SpendMagicCostEventArgs ^>(
                        __VIDS::VID33362.Value + 0x156,
                        6,
                        6,
                        "44 8B F8 83 F8 FF",
                        gcnew System::Func<CPURegisters ^, SpendMagicCostEventArgs ^>(__before_SpendMagicCost),
                        gcnew System::Action<CPURegisters ^, SpendMagicCostEventArgs ^>(__after_SpendMagicCost)
                        )
                    );
                __handler_SpendPoison = gcnew EventHook<SpendPoisonEventArgs ^>(
                    EventHookFlags::None,
                    "SpendPoison",
                    gcnew EventHookParameters<SpendPoisonEventArgs ^>(
                        __VIDS::VID37799.Value + 0x188,
                        5,
                        5,
                        "E8",
                        gcnew System::Func<CPURegisters ^, SpendPoisonEventArgs ^>(__before_SpendPoison_1),
                        gcnew System::Action<CPURegisters ^, SpendPoisonEventArgs ^>(__after_SpendPoison_1)
                        ),
                    gcnew EventHookParameters<SpendPoisonEventArgs ^>(
                        __VIDS::VID41778.Value + 0x169,
                        5,
                        5,
                        "E8",
                        gcnew System::Func<CPURegisters ^, SpendPoisonEventArgs ^>(__before_SpendPoison_2),
                        gcnew System::Action<CPURegisters ^, SpendPoisonEventArgs ^>(__after_SpendPoison_2)
                        )
                    );
                __handler_UpdateCamera = gcnew EventHook<UpdateCameraEventArgs ^>(
                    EventHookFlags::None,
                    "UpdateCamera",
                    gcnew EventHookParameters<UpdateCameraEventArgs ^>(__VIDS::VID32289.Value + 0x4A, 5, 5, "48 8B 5C 24 40", gcnew System::Func<CPURegisters ^, UpdateCameraEventArgs ^>(__before_UpdateCamera), nullptr)
                    );
                __handler_UpdatedPlayerHeadtrack = gcnew EventHook<UpdatedPlayerHeadtrackEventArgs ^>(
                    EventHookFlags::None,
                    "UpdatedPlayerHeadtrack",
                    gcnew EventHookParameters<UpdatedPlayerHeadtrackEventArgs ^>(
                        __VIDS::VID39375.Value + 0xC26,
                        8,
                        8,
                        "F3 0F 10 87 10 0A 00 00",
                        gcnew System::Func<CPURegisters ^, UpdatedPlayerHeadtrackEventArgs ^>(__before_UpdatedPlayerHeadtrack),
                        nullptr
                        )
                    );
                __handler_UpdatePlayerControls = gcnew EventHook<UpdatePlayerControlsEventArgs ^>(
                    EventHookFlags::AlwaysRun,
                    "UpdatePlayerControls",
                    gcnew EventHookParameters<UpdatePlayerControlsEventArgs ^>(
                        __VIDS::VID41259.Value + 0x88,
                        6,
                        0,
                        "80 7B 4F 00 74 0C",
                        gcnew System::Func<CPURegisters ^, UpdatePlayerControlsEventArgs ^>(__before_UpdatePlayerControls),
                        gcnew System::Action<CPURegisters ^, UpdatePlayerControlsEventArgs ^>(__after_UpdatePlayerControls)
                        )
                    );
                __handler_UpdatePlayerTurnToCamera = gcnew EventHook<UpdatePlayerTurnToCameraEventArgs ^>(
                    EventHookFlags::None,
                    "UpdatePlayerTurnToCamera",
                    gcnew EventHookParameters<UpdatePlayerTurnToCameraEventArgs ^>(
                        __VIDS::VID49968.Value + 0x93,
                        7,
                        7,
                        "40 38 BB DC 00 00 00",
                        gcnew System::Func<CPURegisters ^, UpdatePlayerTurnToCameraEventArgs ^>(__before_UpdatePlayerTurnToCamera),
                        gcnew System::Action<CPURegisters ^, UpdatePlayerTurnToCameraEventArgs ^>(__after_UpdatePlayerTurnToCamera)
                        )
                    );
                __handler_WeaponFireProjectilePosition = gcnew EventHook<WeaponFireProjectilePositionEventArgs ^>(
                    EventHookFlags::None,
                    "WeaponFireProjectilePosition",
                    gcnew EventHookParameters<WeaponFireProjectilePositionEventArgs ^>(
                        __VIDS::VID17693.Value + 0x2A9,
                        7,
                        7,
                        "45 0F 57 DB 48 85 C0",
                        gcnew System::Func<CPURegisters ^, WeaponFireProjectilePositionEventArgs ^>(__before_WeaponFireProjectilePosition),
                        gcnew System::Action<CPURegisters ^, WeaponFireProjectilePositionEventArgs ^>(__after_WeaponFireProjectilePosition)
                        )
                    );
            }
        };
    }
}

#pragma warning(pop)
