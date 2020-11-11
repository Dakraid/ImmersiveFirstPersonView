namespace IFPV
{
    using System;
    using NetScriptFramework;
    using NetScriptFramework.SkyrimSE;

    internal static class Utility
    {
        internal static double ApplyFormula(double ratio, TValue.TweenTypes type)
        {
            if (ratio < 0.0)
            {
                return 0.0;
            }

            if (ratio > 1.0)
            {
                return 1.0;
            }

            switch (type)
            {
                case TValue.TweenTypes.Linear: return ratio;

                case TValue.TweenTypes.Accelerating: return ratio * ratio;

                case TValue.TweenTypes.Decelerating: return Math.Sqrt(ratio);

                case TValue.TweenTypes.AccelAndDecel: return (Math.Sin((ratio * Math.PI) - (Math.PI * 0.5)) * 0.5) + 0.5;

                default: return ratio;
            }
        }

        internal static double ClampToPi(double rad)
        {
            var min = -Math.PI;
            var max = Math.PI;

            //double min = 0.0;
            //double max = Math.PI * 2.0;
            var add = Math.PI * 2.0;

            if (rad < min)
            {
                do { rad += add; } while (rad < min);
            }
            else if (rad > max)
            {
                do { rad -= add; } while (rad > max);
            }

            return rad;
        }

        internal static double DegToRad(double deg) => deg * (Math.PI / 180.0);

        internal static uint GetNiAVFlags(NiAVObject obj) => Memory.ReadUInt32(obj.Address + 0xF4);

        internal static void ModNiAVFlags(NiAVObject obj, uint flags, bool add)
        {
            var ofl = GetNiAVFlags(obj);
            var fl  = ofl;
            if (add)
            {
                fl |= flags;
            }
            else
            {
                fl &= ~flags;
            }

            if (ofl != fl)
            {
                SetNiAVFlags(obj, fl);
            }
        }

        internal static double RadToDeg(double rad) => rad * (180.0 / Math.PI);

        internal static void SetNiAVFlags(NiAVObject obj, uint flags) => Memory.WriteUInt32(obj.Address + 0xF4, flags);
    }

    internal static class GameTypeExtensions
    {
        internal static void CopyFrom(this NiPoint3 pt, NiPoint3 other) => Memory.Copy(other.Address, pt.Address, 0xC);

        internal static void CopyFrom(this NiTransform pt, NiTransform other) => Memory.Copy(other.Address, pt.Address, 0x34);

        internal static void CopyFrom(this NiMatrix33 pt, NiMatrix33 other) => Memory.Copy(other.Address, pt.Address, 0x24);
    }
}
