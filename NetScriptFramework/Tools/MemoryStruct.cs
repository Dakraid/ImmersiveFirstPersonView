using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScriptFramework.Tools
{
    /// <summary>
    /// This class can be used to allocate a temporary struct. By default when allocated all data is zeroed out, fill out the data using the index accessor where
    /// index is the offset in struct.
    /// </summary>
    /// <seealso cref="NetScriptFramework.TemporaryObject" />
    public sealed class MemoryStruct : TemporaryObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStruct"/> class.
        /// </summary>
        internal MemoryStruct() { }

        /// <summary>
        /// The allocation of struct.
        /// </summary>
        internal MemoryAllocation Allocation = null;

        /// <summary>
        /// The field types.
        /// </summary>
        internal ulong[] Fields = null;

        /// <summary>
        /// Gets the base address (pointer to struct).
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public IntPtr Address => Allocation.Address;

        /// <summary>
        /// Gets the size of struct in bytes.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int Size { get; internal set; }

        /// <summary>
        /// Sets the value in this struct but replaces any old values that may have conflicted with this offset and value size.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset;Offset can not be negative!
        /// or
        /// offset;Offset can not exceed the size of struct!
        /// or
        /// offset;Offset + value.Size can not exceed the size of struct!
        /// </exception>
        /// <exception cref="System.ArgumentNullException">value</exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public void SetValue(int offset, MemoryStructField value)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Offset can not be negative!");

            if (offset > Size)
                throw new ArgumentOutOfRangeException("offset", "Offset can not exceed the size of struct!");

            if (offset + value.Size > Size)
                throw new ArgumentOutOfRangeException("offset", "Offset + value.Size can not exceed the size of struct!");

            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Data == null)
                throw new InvalidOperationException();

            value.Offset = offset;

            var p     = value.Packed;
            var begin = offset;
            var end   = offset + value.Size;
            for (var i = begin; i < end; i++)
            {
                if (Fields[i] != 0)
                    ClearValue(i);

                Fields[i] = p;
            }

            Memory.WriteBytes(Allocation.Address + offset, value.Data);
        }

        /// <summary>
        /// Sets the value in this struct but doesn't set if it would conflict with any other value offsets that have been set so far in which case it returns false instead. If there was exact same type of
        /// value at the same offset it can be replaced with this function.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset;Offset can not be negative!
        /// or
        /// offset;Offset can not exceed the size of struct!
        /// or
        /// offset;Offset + value.Size can not exceed the size of struct!
        /// </exception>
        /// <exception cref="System.ArgumentNullException">value</exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public bool SetValueSafe(int offset, MemoryStructField value)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Offset can not be negative!");

            if (offset > Size)
                throw new ArgumentOutOfRangeException("offset", "Offset can not exceed the size of struct!");

            if (offset + value.Size > Size)
                throw new ArgumentOutOfRangeException("offset", "Offset + value.Size can not exceed the size of struct!");

            if (value == null)
                throw new ArgumentNullException("value");

            value.Offset = offset;

            var   begin = offset;
            var   end   = offset + value.Size;
            ulong p2    = 0;
            var   p     = value.Packed;
            for (var i = begin; i < end; i++)
            {
                p2 = Fields[i];
                if (p2 == 0)
                    continue;

                // Exact same type and offset and size of value.
                if (p2 == p)
                    break;
            }

            if (value.Data == null)
                throw new InvalidOperationException();

            for (var i = begin; i < end; i++)
                Fields[i] = p;

            Memory.WriteBytes(Allocation.Address + offset, value.Data);
            return true;
        }

        /// <summary>
        /// Gets the value at offset. The offset does not have to be exact beginning offset of value as long as the value would conflict with this offset it returns the result. Check result's offset
        /// for actual offset of value.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset;Offset can not be negative!
        /// or
        /// offset;Offset can not exceed the size of struct!
        /// </exception>
        public MemoryStructField GetValue(int offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Offset can not be negative!");

            if (offset > Size)
                throw new ArgumentOutOfRangeException("offset", "Offset can not exceed the size of struct!");

            ulong p = 0;
            if (offset == Size || (p = Fields[offset]) == 0)
                return null;

            return MemoryStructField.FromPacked(p, this);
        }

        /// <summary>
        /// Clears the value at offset. This will set it back to zero and remove any traces of it.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset;Offset can not be negative!
        /// or
        /// offset;Offset can not exceed the size of struct!
        /// </exception>
        public bool ClearValue(int offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Offset can not be negative!");

            if (offset > Size)
                throw new ArgumentOutOfRangeException("offset", "Offset can not exceed the size of struct!");

            if (offset == Size)
                return false;

            var p = Fields[offset];
            if (p == 0)
                return false;

            var  realOffset = 0;
            var  realSize   = 0;
            byte realType   = 0;
            MemoryStructField.ReadPacked(p, ref realOffset, ref realSize, ref realType);

            var end = realOffset + realSize;
            for (var i = realOffset; i < end; i++)
                Fields[i] = 0;

            Memory.WriteZero(Allocation.Address + realOffset, realSize);
            return true;
        }

        /// <summary>
        /// Gets or sets the <see cref="MemoryStructField"/> with the specified offset.
        /// </summary>
        /// <value>
        /// The <see cref="MemoryStructField"/>.
        /// </value>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public MemoryStructField this[int offset]
        {
            get => GetValue(offset);
            set => SetValue(offset, value);
        }

        /// <summary>
        /// Frees resources.
        /// </summary>
        protected override void Free()
        {
            if (Allocation != null)
            {
                Allocation.Dispose();
                Allocation = null;
            }
        }
    }

    /// <summary>
    /// A value in memory struct.
    /// </summary>
    public sealed class MemoryStructField
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="MemoryStructField"/> class from being created.
        /// </summary>
        private MemoryStructField() { }

        /// <summary>
        /// Gets the size of value.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int Size { get; internal set; }

        /// <summary>
        /// Gets the begin offset of value in struct.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        public int Offset { get; internal set; }

        /// <summary>
        /// Gets the type of value.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public FieldTypes Type { get; private set; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        internal byte[] Data { get; private set; }

        /// <summary>
        /// Gets the packed value.
        /// </summary>
        /// <value>
        /// The packed.
        /// </value>
        internal ulong Packed
        {
            get
            {
                ulong v = 0;
                v =   (byte) Type;
                v <<= 24;
                v |=  (uint) (Size & 0x00FFFFFF);
                v <<= 24;
                v |=  (uint) (Offset & 0x00FFFFFF);
                return v;
            }
        }

        /// <summary>
        /// Reads the packed value.
        /// </summary>
        /// <param name="packed">The packed.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="size">The size.</param>
        /// <param name="type">The type.</param>
        internal static void ReadPacked(ulong packed, ref int offset, ref int size, ref byte type)
        {
            offset = (int) (packed          & 0x00FFFFFF);
            size   = (int) ((packed  >> 24) & 0x00FFFFFF);
            type   = (byte) ((packed >> 48) & 0xFF);
        }

        /// <summary>
        /// Get field data from packed value.
        /// </summary>
        /// <param name="packed">The packed.</param>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        internal static MemoryStructField FromPacked(ulong packed, MemoryStruct obj)
        {
            if (packed == 0)
                return null;

            var  offset = 0;
            var  size   = 0;
            byte type   = 0;
            ReadPacked(packed, ref offset, ref size, ref type);

            var f = new MemoryStructField();
            f.Offset = offset;
            f.Size   = size;
            f.Type   = (FieldTypes) type;
            f.Data   = Memory.ReadBytes(obj.Address + offset, size);
            return f;
        }

        /// <summary>
        /// Memory struct field types.
        /// </summary>
        public enum FieldTypes : byte
        {
            None = 0,

            Pointer   = 1,
            Float     = 2,
            Double    = 3,
            UInt8     = 4,
            Int8      = 5,
            UInt16    = 6,
            Int16     = 7,
            UInt32    = 8,
            Int32     = 9,
            UInt64    = 10,
            Int64     = 11,
            ByteArray = 12
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="IntPtr"/> to <see cref="MemoryStructField"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator MemoryStructField(IntPtr value)
        {
            var f = new MemoryStructField();
            f.Type = FieldTypes.Pointer;
            if (Main.Is64Bit)
                f.Data = BitConverter.GetBytes(value.ToInt64());
            else
                f.Data = BitConverter.GetBytes(value.ToInt32());
            f.Size = f.Data.Length;
            return f;
        }

        /// <summary>
        /// Tries to convert value.
        /// </summary>
        /// <param name="value">The value if successful.</param>
        /// <returns></returns>
        public bool TryToPointer(ref IntPtr value)
        {
            if (Type != FieldTypes.Pointer)
                return false;
            if (Main.Is64Bit)
            {
                var v = BitConverter.ToInt64(Data, 0);
                value = new IntPtr(v);
            }
            else
            {
                var v = BitConverter.ToInt32(Data, 0);
                value = new IntPtr(v);
            }

            return true;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Single"/> to <see cref="MemoryStructField"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator MemoryStructField(float value)
        {
            var f = new MemoryStructField();
            f.Type = FieldTypes.Float;
            f.Data = BitConverter.GetBytes(value);
            f.Size = f.Data.Length;
            return f;
        }

        /// <summary>
        /// Tries to convert value.
        /// </summary>
        /// <param name="value">The value if successful.</param>
        /// <returns></returns>
        public bool TryToFloat(ref float value)
        {
            if (Type != FieldTypes.Float)
                return false;
            value = BitConverter.ToSingle(Data, 0);
            return true;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Double"/> to <see cref="MemoryStructField"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator MemoryStructField(double value)
        {
            var f = new MemoryStructField();
            f.Type = FieldTypes.Float;
            f.Data = BitConverter.GetBytes(value);
            f.Size = f.Data.Length;
            return f;
        }

        /// <summary>
        /// Tries to convert value.
        /// </summary>
        /// <param name="value">The value if successful.</param>
        /// <returns></returns>
        public bool TryToDouble(ref double value)
        {
            if (Type != FieldTypes.Double)
                return false;
            value = BitConverter.ToDouble(Data, 0);
            return true;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Byte"/> to <see cref="MemoryStructField"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator MemoryStructField(byte value)
        {
            var f = new MemoryStructField();
            f.Type = FieldTypes.UInt8;
            f.Data = new byte[] {value};
            f.Size = f.Data.Length;
            return f;
        }

        /// <summary>
        /// Tries to convert value.
        /// </summary>
        /// <param name="value">The value if successful.</param>
        /// <returns></returns>
        public bool TryToUInt8(ref byte value)
        {
            if (Type != FieldTypes.UInt8)
                return false;
            value = Data[0];
            return true;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.SByte"/> to <see cref="MemoryStructField"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator MemoryStructField(sbyte value)
        {
            var f = new MemoryStructField();
            f.Type = FieldTypes.Int8;
            f.Data = new byte[] {unchecked((byte) value)};
            f.Size = f.Data.Length;
            return f;
        }

        /// <summary>
        /// Tries to convert value.
        /// </summary>
        /// <param name="value">The value if successful.</param>
        /// <returns></returns>
        public bool TryToInt8(ref sbyte value)
        {
            if (Type != FieldTypes.Int8)
                return false;
            value = unchecked((sbyte) Data[0]);
            return true;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt16"/> to <see cref="MemoryStructField"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator MemoryStructField(ushort value)
        {
            var f = new MemoryStructField();
            f.Type = FieldTypes.UInt16;
            f.Data = BitConverter.GetBytes(value);
            f.Size = f.Data.Length;
            return f;
        }

        /// <summary>
        /// Tries to convert value.
        /// </summary>
        /// <param name="value">The value if successful.</param>
        /// <returns></returns>
        public bool TryToUInt16(ref ushort value)
        {
            if (Type != FieldTypes.UInt16)
                return false;
            value = BitConverter.ToUInt16(Data, 0);
            return true;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int16"/> to <see cref="MemoryStructField"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator MemoryStructField(short value)
        {
            var f = new MemoryStructField();
            f.Type = FieldTypes.Int16;
            f.Data = BitConverter.GetBytes(value);
            f.Size = f.Data.Length;
            return f;
        }

        /// <summary>
        /// Tries to convert value.
        /// </summary>
        /// <param name="value">The value if successful.</param>
        /// <returns></returns>
        public bool TryToInt16(ref short value)
        {
            if (Type != FieldTypes.Int16)
                return false;
            value = BitConverter.ToInt16(Data, 0);
            return true;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt32"/> to <see cref="MemoryStructField"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator MemoryStructField(uint value)
        {
            var f = new MemoryStructField();
            f.Type = FieldTypes.UInt32;
            f.Data = BitConverter.GetBytes(value);
            f.Size = f.Data.Length;
            return f;
        }

        /// <summary>
        /// Tries to convert value.
        /// </summary>
        /// <param name="value">The value if successful.</param>
        /// <returns></returns>
        public bool TryToUInt32(ref uint value)
        {
            if (Type != FieldTypes.UInt32)
                return false;
            value = BitConverter.ToUInt32(Data, 0);
            return true;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="MemoryStructField"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator MemoryStructField(int value)
        {
            var f = new MemoryStructField();
            f.Type = FieldTypes.Int32;
            f.Data = BitConverter.GetBytes(value);
            f.Size = f.Data.Length;
            return f;
        }

        /// <summary>
        /// Tries to convert value.
        /// </summary>
        /// <param name="value">The value if successful.</param>
        /// <returns></returns>
        public bool TryToInt32(ref int value)
        {
            if (Type != FieldTypes.Int32)
                return false;
            value = BitConverter.ToInt32(Data, 0);
            return true;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt64"/> to <see cref="MemoryStructField"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator MemoryStructField(ulong value)
        {
            var f = new MemoryStructField();
            f.Type = FieldTypes.UInt64;
            f.Data = BitConverter.GetBytes(value);
            f.Size = f.Data.Length;
            return f;
        }

        /// <summary>
        /// Tries to convert value.
        /// </summary>
        /// <param name="value">The value if successful.</param>
        /// <returns></returns>
        public bool TryToUInt64(ref ulong value)
        {
            if (Type != FieldTypes.UInt64)
                return false;
            value = BitConverter.ToUInt64(Data, 0);
            return true;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int64"/> to <see cref="MemoryStructField"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator MemoryStructField(long value)
        {
            var f = new MemoryStructField();
            f.Type = FieldTypes.Int64;
            f.Data = BitConverter.GetBytes(value);
            f.Size = f.Data.Length;
            return f;
        }

        /// <summary>
        /// Tries to convert value.
        /// </summary>
        /// <param name="value">The value if successful.</param>
        /// <returns></returns>
        public bool TryToInt64(ref long value)
        {
            if (Type != FieldTypes.Int64)
                return false;
            value = BitConverter.ToInt64(Data, 0);
            return true;
        }

        /// <summary>
        /// Performs an implicit conversion from byte array to <see cref="MemoryStructField"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">value</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">value.Length</exception>
        public static implicit operator MemoryStructField(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (value.Length == 0)
                throw new ArgumentOutOfRangeException("value.Length");

            var f = new MemoryStructField();
            f.Type = FieldTypes.ByteArray;
            f.Data = value;
            f.Size = f.Data.Length;
            return f;
        }

        /// <summary>
        /// Tries to convert value.
        /// </summary>
        /// <param name="value">The value if successful.</param>
        /// <returns></returns>
        public bool TryToBytes(ref byte[] value)
        {
            if (Type != FieldTypes.ByteArray)
                return false;
            value = Data.ToArray();
            return true;
        }
    }
}