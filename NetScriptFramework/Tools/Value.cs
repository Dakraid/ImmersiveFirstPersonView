﻿namespace NetScriptFramework.Tools
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    #region Value class

    /// <summary>
    ///     Implements a common value type.
    /// </summary>
    public sealed class Value : IComparable, IConvertible
    {
        #region Constructors

        /// <summary>
        ///     Create a new value from binary data. This must be the same data returned from ToArray.
        /// </summary>
        /// <param name="data">Data to convert.</param>
        /// <param name="index">Index in data when to start reading.</param>
        /// <param name="length">Maximum length of bytes to read.</param>
        /// <param name="size">Size of bytes actually read will be set here.</param>
        /// <exception cref="System.ArgumentNullException">data</exception>
        /// <exception cref="System.ArgumentException">data</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     index
        ///     or
        ///     length
        /// </exception>
        public Value(byte[] data, int index, int length, ref int size)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Length == 0)
            {
                throw new ArgumentException("data", "Empty data given!");
            }

            if (index < 0 || index >= data.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (length <= 0 || length + index > data.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            switch ((TypeCode)data[index])
            {
                case TypeCode.Boolean:
                    size = 2;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = data[index + 1] != 0 ? true : false;
                    this.CurrentType = (TypeCode)data[index];
                    break;

                case TypeCode.Byte:
                    size = 2;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = data[index + 1];
                    this.CurrentType = (TypeCode)data[index];
                    break;

                case TypeCode.SByte:
                    size = 2;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = unchecked((sbyte)data[index + 1]);
                    this.CurrentType = (TypeCode)data[index];
                    break;

                case TypeCode.Char:
                    size = 3;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = BitConverter.ToChar(data, index + 1);
                    this.CurrentType = (TypeCode)data[index];
                    break;

                case TypeCode.Int16:
                    size = 3;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = BitConverter.ToInt16(data, index + 1);
                    this.CurrentType = (TypeCode)data[index];
                    break;

                case TypeCode.UInt16:
                    size = 3;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = BitConverter.ToUInt16(data, index + 1);
                    this.CurrentType = (TypeCode)data[index];
                    break;

                case TypeCode.Int32:
                    size = 5;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = BitConverter.ToInt32(data, index + 1);
                    this.CurrentType = (TypeCode)data[index];
                    break;

                case TypeCode.UInt32:
                    size = 5;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = BitConverter.ToUInt32(data, index + 1);
                    this.CurrentType = (TypeCode)data[index];
                    break;

                case TypeCode.Int64:
                    size = 9;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = BitConverter.ToInt64(data, index + 1);
                    this.CurrentType = (TypeCode)data[index];
                    break;

                case TypeCode.UInt64:
                    size = 9;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = BitConverter.ToUInt64(data, index + 1);
                    this.CurrentType = (TypeCode)data[index];
                    break;

                case TypeCode.Single:
                    size = 5;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = BitConverter.ToSingle(data, index + 1);
                    this.CurrentType = (TypeCode)data[index];
                    break;

                case TypeCode.Double:
                    size = 9;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = BitConverter.ToDouble(data, index + 1);
                    this.CurrentType = (TypeCode)data[index];
                    break;

                case TypeCode.Decimal:
                {
                    size = 17;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    var bits = new int[4];
                    bits[0] = BitConverter.ToInt32(data, index + 1);
                    bits[1] = BitConverter.ToInt32(data, index + 5);
                    bits[2] = BitConverter.ToInt32(data, index + 9);
                    bits[3] = BitConverter.ToInt32(data, index + 13);
                    this.CurrentValue = new decimal(bits);
                    this.CurrentType = (TypeCode)data[index];
                }
                    break;

                case TypeCode.String:
                {
                    size = 5;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    var strByteLen = BitConverter.ToInt32(data, index + 1);
                    if (strByteLen < 0)
                    {
                        throw new ArgumentException("data", "String array length can not be negative!");
                    }

                    size += strByteLen;
                    if (strByteLen > 1024 * 1024)
                    {
                        throw new ArgumentException("data", "String array length is too high!");
                    }

                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = Encoding.Unicode.GetString(data, index + 5, strByteLen) ?? string.Empty;
                    this.CurrentType = (TypeCode)data[index];
                }
                    break;

                case TypeCode.DateTime:
                    size = 9;
                    if (length < size)
                    {
                        throw new ArgumentException("data", "Data is wrong size!");
                    }

                    this.CurrentValue = new DateTime(BitConverter.ToInt64(data, index + 1));
                    this.CurrentType = (TypeCode)data[index];
                    break;

                default:
                    throw new ArgumentException("data",
                        string.Format("Invalid type code {0} found!", (TypeCode)data[index]));
            }
        }

        /// <summary>
        ///     Copy another value.
        /// </summary>
        /// <param name="value">Value to copy.</param>
        public Value(Value value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.CurrentValue = value.CurrentValue;
            this.CurrentType = value.CurrentType;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(IConvertible value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.CurrentValue = value;
            this.CurrentType = value.GetTypeCode();
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(bool value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.Boolean;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(char value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.Char;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(sbyte value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.SByte;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(byte value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.Byte;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(short value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.Int16;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(ushort value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.UInt16;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(int value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.Int32;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(uint value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.UInt32;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(long value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.Int64;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(ulong value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.UInt64;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(float value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.Single;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(double value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.Double;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(decimal value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.Decimal;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set. If null then it will be set as empty string!</param>
        public Value(string value)
        {
            if (value == null)
            {
                value = value ?? string.Empty;
            }

            this.CurrentValue = value;
            this.CurrentType = TypeCode.String;
        }

        /// <summary>
        ///     Create a new value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public Value(DateTime value)
        {
            this.CurrentValue = value;
            this.CurrentType = TypeCode.DateTime;
        }

        #endregion

        #region Value members

        /// <summary>
        ///     Convert value to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => this.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        ///     Convert value. Exception from internal code is not possible.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public string ToString(IFormatProvider provider) => this.CurrentValue.ToString(provider);

        /// <summary>
        ///     Convert value to byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    return new[] {(byte)this.CurrentType, (bool)this.CurrentValue ? (byte)1 : (byte)0};
                case TypeCode.SByte:
                    return new[] {(byte)this.CurrentType, (byte)this.CurrentValue};
                case TypeCode.Byte:
                    return new[] {(byte)this.CurrentType, (byte)this.CurrentValue};
                case TypeCode.Char:
                    return new[] {(byte)this.CurrentType}.Concat(BitConverter.GetBytes((char)this.CurrentValue))
                        .ToArray();
                case TypeCode.Int16:
                    return new[] {(byte)this.CurrentType}.Concat(BitConverter.GetBytes((short)this.CurrentValue))
                        .ToArray();
                case TypeCode.UInt16:
                    return new[] {(byte)this.CurrentType}.Concat(BitConverter.GetBytes((ushort)this.CurrentValue))
                        .ToArray();
                case TypeCode.Int32:
                    return new[] {(byte)this.CurrentType}.Concat(BitConverter.GetBytes((int)this.CurrentValue))
                        .ToArray();
                case TypeCode.UInt32:
                    return new[] {(byte)this.CurrentType}.Concat(BitConverter.GetBytes((uint)this.CurrentValue))
                        .ToArray();
                case TypeCode.Int64:
                    return new[] {(byte)this.CurrentType}.Concat(BitConverter.GetBytes((long)this.CurrentValue))
                        .ToArray();
                case TypeCode.UInt64:
                    return new[] {(byte)this.CurrentType}.Concat(BitConverter.GetBytes((ulong)this.CurrentValue))
                        .ToArray();
                case TypeCode.Single:
                    return new[] {(byte)this.CurrentType}.Concat(BitConverter.GetBytes((float)this.CurrentValue))
                        .ToArray();
                case TypeCode.Double:
                    return new[] {(byte)this.CurrentType}.Concat(BitConverter.GetBytes((double)this.CurrentValue))
                        .ToArray();
                case TypeCode.Decimal:
                {
                    var bits = decimal.GetBits((decimal)this.CurrentValue);
                    return new[] {(byte)this.CurrentType}.Concat(BitConverter.GetBytes(bits[0]))
                        .Concat(BitConverter.GetBytes(bits[1])).Concat(BitConverter.GetBytes(bits[2]))
                        .Concat(BitConverter.GetBytes(bits[3])).ToArray();
                }
                case TypeCode.String:
                {
                    var encoded = Encoding.Unicode.GetBytes((string)this.CurrentValue);
                    return new[] {(byte)this.CurrentType}.Concat(BitConverter.GetBytes(encoded.Length)).Concat(encoded)
                        .ToArray();
                }
                case TypeCode.DateTime:
                    return new[] {(byte)this.CurrentType}
                        .Concat(BitConverter.GetBytes(((DateTime)this.CurrentValue).Ticks)).ToArray();

                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        ///     Write value to stream.
        /// </summary>
        /// <param name="f">Stream to write to.</param>
        public void WriteToStream(BinaryWriter f)
        {
            // Invalid stream.
            if (f == null)
            {
                throw new ArgumentNullException("f");
            }

            // Convert to bytes.
            var data = this.ToArray();
            f.Write(GetBigNumber(data.Length));
            f.Write(data);
        }

        /// <summary>
        ///     Read value from stream.
        /// </summary>
        /// <param name="f">Stream to read from.</param>
        /// <returns></returns>
        public static Value ReadFromStream(BinaryReader f)
        {
            // Invalid stream.
            if (f == null)
            {
                throw new ArgumentNullException("f");
            }

            // Read length.
            var firstByte = f.ReadByte();
            var countByte = 0;
            var prefix = GetBigNumberFirst(firstByte, ref countByte);
            if (countByte != 0)
            {
                var secondBytes = f.ReadBytes(countByte);
                prefix = GetBigNumberSecond(prefix, secondBytes, 0, countByte);
            }

            var data = f.ReadBytes((int)prefix);
            var result = new Value(data, 0, data.Length, ref countByte);
            if (countByte != data.Length)
            {
                throw new InvalidDataException("Failed to read value properly!");
            }

            return result;
        }

        /// <summary>
        ///     Check if two values are equal.
        /// </summary>
        /// <param name="obj">Another value.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            // This value can't be equal.
            if (!(obj is IConvertible))
            {
                return false;
            }

            // Get object to check.
            var checkObject = obj;

            // Special case. We want to use our own string converter.
            if (checkObject is string && this.CurrentType != TypeCode.String)
            {
                checkObject = new Value((string)checkObject);
            }

            // Try to convert types so they can be checked.
            try
            {
                // Get new converted value.
                var converted = Convert.ChangeType(checkObject, this.CurrentType);

                // This can't be equal.
                if (converted == null)
                {
                    return false;
                }

                // Let value decide equality.
                return this.CurrentValue.Equals(converted);
            }
            catch (InvalidCastException) { return false; }
            catch (FormatException) { return false; }
            catch (OverflowException) { return false; }

            // Invalid argument.
        }

        /// <summary>
        ///     Get hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => this.CurrentValue.GetHashCode();

        /// <summary>
        ///     Compare to another value.
        /// </summary>
        /// <param name="obj">Value.</param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            // This value can't be compared.
            if (!(obj is IConvertible))
            {
                return 1;
            }

            // Get object.
            var checkObject = obj;

            // Special case. We want to use our own string converter.
            if (checkObject is string && this.CurrentType != TypeCode.String)
            {
                checkObject = new Value((string)checkObject);
            }

            // Try to convert types so they can be checked.
            try
            {
                // Get new converted value.
                var converted = Convert.ChangeType(checkObject, this.CurrentType);

                // This can't be compared.
                if (converted == null)
                {
                    return 1;
                }

                // Let value decide.
                return ((IComparable)this.CurrentValue).CompareTo(converted);
            }
            catch (InvalidCastException) { return 1; }
            catch (FormatException) { return 1; }
            catch (OverflowException) { return 1; }

            // Invalid argument.
        }

        #endregion

        #region IConvertible members

        /// <summary>
        ///     Get type code of value. This is guaranteed to not be Object.
        /// </summary>
        /// <returns></returns>
        public TypeCode GetTypeCode() => this.CurrentType;

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="conversionType">Type to convert to.</param>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            // Get type code for conversion.
            switch (Type.GetTypeCode(conversionType))
            {
                case TypeCode.Boolean:
                    return this.ToBoolean(provider);
                case TypeCode.SByte:
                    return this.ToSByte(provider);
                case TypeCode.Byte:
                    return this.ToByte(provider);
                case TypeCode.Char:
                    return this.ToChar(provider);
                case TypeCode.Int16:
                    return this.ToInt16(provider);
                case TypeCode.UInt16:
                    return this.ToUInt16(provider);
                case TypeCode.Int32:
                    return this.ToInt32(provider);
                case TypeCode.UInt32:
                    return this.ToUInt32(provider);
                case TypeCode.Int64:
                    return this.ToInt64(provider);
                case TypeCode.UInt64:
                    return this.ToUInt64(provider);
                case TypeCode.Single:
                    return this.ToSingle(provider);
                case TypeCode.Double:
                    return this.ToDouble(provider);
                case TypeCode.Decimal:
                    return this.ToDecimal(provider);
                case TypeCode.String:
                    return this.ToString(provider);
                case TypeCode.DateTime:
                    return this.ToDateTime(provider);

                default:
                    throw new InvalidCastException();
            }
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public bool ToBoolean(IFormatProvider provider = null)
        {
            var result = false;
            var result2 = false;
            switch (this._tryToBool(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to boolean!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(string.Format("Argument '{0}' is not valid for casting to boolean!",
                        this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(
                        string.Format("Argument '{0}' caused an overflow when casting to boolean!", this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToBoolean(out bool result)
        {
            var result1 = false;
            var result2 = false;
            if (this._tryToBool(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = false;
            return false;
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public byte ToByte(IFormatProvider provider = null)
        {
            byte result = 0;
            byte result2 = 0;
            switch (this._tryToByte(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to byte!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(string.Format("Argument '{0}' is not valid for casting to byte!",
                        this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(string.Format("Argument '{0}' caused an overflow when casting to byte!",
                        this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToByte(out byte result)
        {
            byte result1 = 0;
            byte result2 = 0;
            if (this._tryToByte(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public sbyte ToSByte(IFormatProvider provider = null)
        {
            sbyte result = 0;
            sbyte result2 = 0;
            switch (this._tryToSByte(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to signed byte!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(
                        string.Format("Argument '{0}' is not valid for casting to signed byte!", this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(
                        string.Format("Argument '{0}' caused an overflow when casting to signed byte!",
                            this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToSByte(out sbyte result)
        {
            sbyte result1 = 0;
            sbyte result2 = 0;
            if (this._tryToSByte(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public char ToChar(IFormatProvider provider = null)
        {
            var result = (char)0;
            var result2 = (char)0;
            switch (this._tryToChar(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to character!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(
                        string.Format("Argument '{0}' is not valid for casting to character!", this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(
                        string.Format("Argument '{0}' caused an overflow when casting to character!",
                            this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToChar(out char result)
        {
            var result1 = (char)0;
            var result2 = (char)0;
            if (this._tryToChar(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = (char)0;
            return false;
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public short ToInt16(IFormatProvider provider = null)
        {
            short result = 0;
            short result2 = 0;
            switch (this._tryToInt16(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to short!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(string.Format("Argument '{0}' is not valid for casting to short!",
                        this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(
                        string.Format("Argument '{0}' caused an overflow when casting to short!", this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToInt16(out short result)
        {
            short result1 = 0;
            short result2 = 0;
            if (this._tryToInt16(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public ushort ToUInt16(IFormatProvider provider = null)
        {
            ushort result = 0;
            ushort result2 = 0;
            switch (this._tryToUInt16(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to unsigned short!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(
                        string.Format("Argument '{0}' is not valid for casting to unsigned short!", this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(
                        string.Format("Argument '{0}' caused an overflow when casting to unsigned short!",
                            this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToUInt16(out ushort result)
        {
            ushort result1 = 0;
            ushort result2 = 0;
            if (this._tryToUInt16(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public int ToInt32(IFormatProvider provider = null)
        {
            var result = 0;
            var result2 = 0;
            switch (this._tryToInt32(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to integer!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(string.Format("Argument '{0}' is not valid for casting to integer!",
                        this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(
                        string.Format("Argument '{0}' caused an overflow when casting to integer!", this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToInt32(out int result)
        {
            var result1 = 0;
            var result2 = 0;
            if (this._tryToInt32(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public uint ToUInt32(IFormatProvider provider = null)
        {
            uint result = 0;
            uint result2 = 0;
            switch (this._tryToUInt32(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to unsigned integer!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(
                        string.Format("Argument '{0}' is not valid for casting to unsigned integer!",
                            this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(
                        string.Format("Argument '{0}' caused an overflow when casting to unsigned integer!",
                            this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToUInt32(out uint result)
        {
            uint result1 = 0;
            uint result2 = 0;
            if (this._tryToUInt32(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public long ToInt64(IFormatProvider provider = null)
        {
            long result = 0;
            long result2 = 0;
            switch (this._tryToInt64(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to long!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(string.Format("Argument '{0}' is not valid for casting to long!",
                        this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(string.Format("Argument '{0}' caused an overflow when casting to long!",
                        this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToInt64(out long result)
        {
            long result1 = 0;
            long result2 = 0;
            if (this._tryToInt64(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public ulong ToUInt64(IFormatProvider provider = null)
        {
            ulong result = 0;
            ulong result2 = 0;
            switch (this._tryToUInt64(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to unsigned long!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(
                        string.Format("Argument '{0}' is not valid for casting to unsigned long!", this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(
                        string.Format("Argument '{0}' caused an overflow when casting to unsigned long!",
                            this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToUInt64(out ulong result)
        {
            ulong result1 = 0;
            ulong result2 = 0;
            if (this._tryToUInt64(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public float ToSingle(IFormatProvider provider = null)
        {
            var result = 0.0f;
            var result2 = 0.0f;
            switch (this._tryToSingle(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to single!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(string.Format("Argument '{0}' is not valid for casting to single!",
                        this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(
                        string.Format("Argument '{0}' caused an overflow when casting to single!", this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToSingle(out float result)
        {
            var result1 = 0.0f;
            var result2 = 0.0f;
            if (this._tryToSingle(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = 0.0f;
            return false;
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public double ToDouble(IFormatProvider provider = null)
        {
            var result = 0.0;
            var result2 = 0.0;
            switch (this._tryToDouble(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to double!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(string.Format("Argument '{0}' is not valid for casting to double!",
                        this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(
                        string.Format("Argument '{0}' caused an overflow when casting to double!", this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToDouble(out double result)
        {
            var result1 = 0.0;
            var result2 = 0.0;
            if (this._tryToDouble(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = 0.0;
            return false;
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public decimal ToDecimal(IFormatProvider provider = null)
        {
            var result = 0.0m;
            var result2 = 0.0m;
            switch (this._tryToDecimal(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to decimal!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(string.Format("Argument '{0}' is not valid for casting to decimal!",
                        this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(
                        string.Format("Argument '{0}' caused an overflow when casting to decimal!", this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToDecimal(out decimal result)
        {
            var result1 = 0.0m;
            var result2 = 0.0m;
            if (this._tryToDecimal(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = 0.0m;
            return false;
        }

        /// <summary>
        ///     Convert value or throw exception.
        /// </summary>
        /// <param name="provider">Format provider.</param>
        /// <returns></returns>
        public DateTime ToDateTime(IFormatProvider provider = null)
        {
            var result = new DateTime(0);
            var result2 = new DateTime(0);
            switch (this._tryToDateTime(ref result, ref result2))
            {
                case 0: return result; // No errors.
                case 1:
                    throw new FormatException(
                        string.Format("Argument '{0}' is not a valid format for conversion to date time!",
                            this)); // Bad format.
                case 2:
                    throw new InvalidCastException(
                        string.Format("Argument '{0}' is not valid for casting to date time!", this)); // Bad cast.
                case 3: throw new InvalidOperationException();
                case 4:
                    throw new OverflowException(
                        string.Format("Argument '{0}' caused an overflow when casting to date time!",
                            this)); // Overflow.
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Try to convert value and return if we were successful.
        /// </summary>
        /// <param name="result">Result will be set here if we return true.</param>
        /// <returns></returns>
        public bool TryToDateTime(out DateTime result)
        {
            var result1 = new DateTime(0);
            var result2 = new DateTime(0);
            if (this._tryToDateTime(ref result1, ref result2) == 0)
            {
                result = result1;
                return true;
            }

            result = new DateTime(0);
            return false;
        }

        #endregion

        #region Operator conversion

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(bool v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(byte v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(sbyte v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(char v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(short v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(ushort v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(int v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(uint v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(long v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(ulong v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(float v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(double v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(decimal v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(string v) => new Value(v);

        /// <summary>
        ///     Convert to value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static implicit operator Value(DateTime v) => new Value(v);

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator bool(Value v) => v.ToBoolean();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator byte(Value v) => v.ToByte();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator sbyte(Value v) => v.ToSByte();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator char(Value v) => v.ToChar();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator short(Value v) => v.ToInt16();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator ushort(Value v) => v.ToUInt16();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator int(Value v) => v.ToInt32();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator uint(Value v) => v.ToUInt32();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator long(Value v) => v.ToInt64();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator ulong(Value v) => v.ToUInt64();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator float(Value v) => v.ToSingle();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator double(Value v) => v.ToDouble();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator decimal(Value v) => v.ToDecimal();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator string(Value v) => v.ToString();

        /// <summary>
        ///     Convert from value type.
        /// </summary>
        /// <param name="v">Value to convert.</param>
        /// <returns></returns>
        public static explicit operator DateTime(Value v) => v.ToDateTime();

        #endregion

        #region Internal members

        #region Internal conversion

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToBool(ref bool result, ref bool result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result = (sbyte)this.CurrentValue != 0;
                    result2 = result;
                    return 0;

                case TypeCode.Byte:
                    result = (byte)this.CurrentValue != 0;
                    result2 = result;
                    return 0;

                case TypeCode.Char:
                    result2 = (char)this.CurrentValue != (char)0;
                    if ((char)this.CurrentValue == '1' || char.ToLower((char)this.CurrentValue) == 't' ||
                        char.ToLower((char)this.CurrentValue) == 'y')
                    {
                        result = true;
                    }
                    else if ((char)this.CurrentValue == '0' || char.ToLower((char)this.CurrentValue) == 'f' ||
                             char.ToLower((char)this.CurrentValue) == 'n')
                    {
                        result = false;
                    }
                    else
                    {
                        return 1;
                    }

                    return 0;

                case TypeCode.Int16:
                    result = (short)this.CurrentValue != 0;
                    result2 = result;
                    return 0;

                case TypeCode.UInt16:
                    result = (ushort)this.CurrentValue != 0;
                    result2 = result;
                    return 0;

                case TypeCode.Int32:
                    result = (int)this.CurrentValue != 0;
                    result2 = result;
                    return 0;

                case TypeCode.UInt32:
                    result = (uint)this.CurrentValue != 0;
                    result2 = result;
                    return 0;

                case TypeCode.Int64:
                    result = (long)this.CurrentValue != 0;
                    result2 = result;
                    return 0;

                case TypeCode.UInt64:
                    result = (ulong)this.CurrentValue != 0;
                    result2 = result;
                    return 0;

                case TypeCode.Single:
                    result = (float)this.CurrentValue != 0.0f;
                    result2 = result;
                    return 0;

                case TypeCode.Double:
                    result = (double)this.CurrentValue != 0.0;
                    result2 = result;
                    return 0;

                case TypeCode.Decimal:
                    result = (decimal)this.CurrentValue != 0.0m;
                    result2 = result;
                    return 0;

                case TypeCode.String:
                    result2 = ((string)this.CurrentValue).Length != 0;
                    var n = ((string)this.CurrentValue).Trim(' ', '\t', '\r', '\n');
                    if (n.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        n.Equals("t", StringComparison.OrdinalIgnoreCase) ||
                        n.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                        n.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                        n.Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        result = true;
                        return 0;
                    }

                    if (n.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                        n.Equals("f", StringComparison.OrdinalIgnoreCase) ||
                        n.Equals("0", StringComparison.OrdinalIgnoreCase) ||
                        n.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                        n.Equals("n", StringComparison.OrdinalIgnoreCase))
                    {
                        result = false;
                        return 0;
                    }

                    return 1;

                case TypeCode.DateTime:
                    result = ((DateTime)this.CurrentValue).Ticks != 0;
                    result2 = result;
                    return 0;

                default:
                    return 3;
            }
        }

        private double _dec
        {
            get
            {
                var a = (decimal)this.CurrentValue;
                try { return decimal.ToDouble(a); }
                catch (OverflowException)
                {
                    if (a > 0.0m)
                    {
                        return double.MaxValue;
                    }

                    return double.MinValue;
                }
            }
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToSByte(ref sbyte result, ref sbyte result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue ? (sbyte)1 : (sbyte)0;
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result = (sbyte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Byte:
                    result2 = unchecked((sbyte)(byte)this.CurrentValue);
                    if ((byte)this.CurrentValue > (byte)sbyte.MaxValue)
                    {
                        return 4;
                    }

                    result = (sbyte)(byte)this.CurrentValue;
                    return 0;

                case TypeCode.Char:
                    result2 = unchecked((sbyte)(char)this.CurrentValue);
                    if ((char)this.CurrentValue > (char)sbyte.MaxValue)
                    {
                        return 4;
                    }

                    result = (sbyte)(char)this.CurrentValue;
                    return 0;

                case TypeCode.Int16:
                    result2 = unchecked((sbyte)(short)this.CurrentValue);
                    if ((short)this.CurrentValue < sbyte.MinValue)
                    {
                        return 4;
                    }

                    if ((short)this.CurrentValue > sbyte.MaxValue)
                    {
                        return 4;
                    }

                    result = (sbyte)(short)this.CurrentValue;
                    return 0;

                case TypeCode.UInt16:
                    result2 = unchecked((sbyte)(ushort)this.CurrentValue);
                    if ((ushort)this.CurrentValue > (ushort)sbyte.MaxValue)
                    {
                        return 4;
                    }

                    result = (sbyte)(ushort)this.CurrentValue;
                    return 0;

                case TypeCode.Int32:
                    result2 = unchecked((sbyte)(int)this.CurrentValue);
                    if ((int)this.CurrentValue < sbyte.MinValue)
                    {
                        return 4;
                    }

                    if ((int)this.CurrentValue > sbyte.MaxValue)
                    {
                        return 4;
                    }

                    result = (sbyte)(int)this.CurrentValue;
                    return 0;

                case TypeCode.UInt32:
                    result2 = unchecked((sbyte)(uint)this.CurrentValue);
                    if ((uint)this.CurrentValue > (uint)sbyte.MaxValue)
                    {
                        return 4;
                    }

                    result = (sbyte)(uint)this.CurrentValue;
                    return 0;

                case TypeCode.Int64:
                    result2 = unchecked((sbyte)(long)this.CurrentValue);
                    if ((long)this.CurrentValue < sbyte.MinValue)
                    {
                        return 4;
                    }

                    if ((long)this.CurrentValue > sbyte.MaxValue)
                    {
                        return 4;
                    }

                    result = (sbyte)(long)this.CurrentValue;
                    return 0;

                case TypeCode.UInt64:
                    result2 = unchecked((sbyte)(ulong)this.CurrentValue);
                    if ((ulong)this.CurrentValue > (ulong)sbyte.MaxValue)
                    {
                        return 4;
                    }

                    result = (sbyte)(ulong)this.CurrentValue;
                    return 0;

                case TypeCode.Single:
                {
                    result2 = unchecked((sbyte)(float)this.CurrentValue);
                    if ((float)this.CurrentValue < sbyte.MinValue - 2.0f)
                    {
                        return 4;
                    }

                    if ((float)this.CurrentValue > sbyte.MaxValue + 2.0f)
                    {
                        return 4;
                    }

                    var q = (int)(float)this.CurrentValue;
                    if (q < sbyte.MinValue)
                    {
                        return 4;
                    }

                    if (q > sbyte.MaxValue)
                    {
                        return 4;
                    }

                    result = (sbyte)q;
                }
                    return 0;

                case TypeCode.Double:
                {
                    result2 = unchecked((sbyte)(double)this.CurrentValue);
                    if ((double)this.CurrentValue < sbyte.MinValue - 2.0)
                    {
                        return 4;
                    }

                    if ((double)this.CurrentValue > sbyte.MaxValue + 2.0)
                    {
                        return 4;
                    }

                    var q = (int)(double)this.CurrentValue;
                    if (q < sbyte.MinValue)
                    {
                        return 4;
                    }

                    if (q > sbyte.MaxValue)
                    {
                        return 4;
                    }

                    result = (sbyte)q;
                }
                    return 0;

                case TypeCode.Decimal:
                {
                    result2 = unchecked((sbyte)this._dec);
                    if ((decimal)this.CurrentValue < sbyte.MinValue - 2.0m)
                    {
                        return 4;
                    }

                    if ((decimal)this.CurrentValue > sbyte.MaxValue + 2.0m)
                    {
                        return 4;
                    }

                    var q = (int)this._dec;
                    if (q < sbyte.MinValue)
                    {
                        return 4;
                    }

                    if (q > sbyte.MaxValue)
                    {
                        return 4;
                    }

                    result = (sbyte)q;
                }
                    return 0;

                case TypeCode.String:
                {
                    result2 = 0;
                    var n = (string)this.CurrentValue;
                    if (n.Length != 0)
                    {
                        result2 = unchecked((sbyte)(n[0] & 0xFF));
                    }

                    n = n.Trim(' ', '\t', '\r', '\n');
                    var tryDouble = false;
                    object r = null;
                    var error = ParseIntNumber(n, 1, ref tryDouble, ref r);
                    if (tryDouble)
                    {
                        error = ParseUFloatNumber(n, 16, ref r);
                        if (error == 0)
                        {
                            if ((decimal)r < sbyte.MinValue - 2.0m)
                            {
                                return 4;
                            }

                            if ((decimal)r > sbyte.MaxValue + 2.0m)
                            {
                                return 4;
                            }

                            var q = (int)r;
                            if (q < sbyte.MinValue)
                            {
                                return 4;
                            }

                            if (q > sbyte.MaxValue)
                            {
                                return 4;
                            }

                            result = (sbyte)q;
                            return 0;
                        }

                        return error;
                    }

                    if (error == 0)
                    {
                        result = (sbyte)r;
                        return 0;
                    }

                    return error;
                }

                case TypeCode.DateTime:
                    result2 = unchecked((sbyte)((DateTime)this.CurrentValue).Ticks);
                    return 2;

                default:
                    return 3;
            }
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToByte(ref byte result, ref byte result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue ? (byte)1 : (byte)0;
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result2 = unchecked((byte)(sbyte)this.CurrentValue);
                    if ((sbyte)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    result = (byte)(sbyte)this.CurrentValue;
                    return 0;

                case TypeCode.Byte:
                    result = (byte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Char:
                    result2 = unchecked((byte)(char)this.CurrentValue);
                    if ((char)this.CurrentValue > (char)byte.MaxValue)
                    {
                        return 4;
                    }

                    result = (byte)(char)this.CurrentValue;
                    return 0;

                case TypeCode.Int16:
                    result2 = unchecked((byte)(short)this.CurrentValue);
                    if ((short)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    if ((short)this.CurrentValue > byte.MaxValue)
                    {
                        return 4;
                    }

                    result = (byte)(short)this.CurrentValue;
                    return 0;

                case TypeCode.UInt16:
                    result2 = unchecked((byte)(ushort)this.CurrentValue);
                    if ((ushort)this.CurrentValue > byte.MaxValue)
                    {
                        return 4;
                    }

                    result = (byte)(ushort)this.CurrentValue;
                    return 0;

                case TypeCode.Int32:
                    result2 = unchecked((byte)(int)this.CurrentValue);
                    if ((int)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    if ((int)this.CurrentValue > byte.MaxValue)
                    {
                        return 4;
                    }

                    result = (byte)(int)this.CurrentValue;
                    return 0;

                case TypeCode.UInt32:
                    result2 = unchecked((byte)(uint)this.CurrentValue);
                    if ((uint)this.CurrentValue > byte.MaxValue)
                    {
                        return 4;
                    }

                    result = (byte)(uint)this.CurrentValue;
                    return 0;

                case TypeCode.Int64:
                    result2 = unchecked((byte)(long)this.CurrentValue);
                    if ((long)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    if ((long)this.CurrentValue > byte.MaxValue)
                    {
                        return 4;
                    }

                    result = (byte)(long)this.CurrentValue;
                    return 0;

                case TypeCode.UInt64:
                    result2 = unchecked((byte)(ulong)this.CurrentValue);
                    if ((ulong)this.CurrentValue > byte.MaxValue)
                    {
                        return 4;
                    }

                    result = (byte)(ulong)this.CurrentValue;
                    return 0;

                case TypeCode.Single:
                {
                    result2 = unchecked((byte)(float)this.CurrentValue);
                    if ((float)this.CurrentValue < byte.MinValue - 2.0f)
                    {
                        return 4;
                    }

                    if ((float)this.CurrentValue > byte.MaxValue + 2.0f)
                    {
                        return 4;
                    }

                    var q = (int)(float)this.CurrentValue;
                    if (q < 0)
                    {
                        return 4;
                    }

                    if (q > byte.MaxValue)
                    {
                        return 4;
                    }

                    result = (byte)q;
                }
                    return 0;

                case TypeCode.Double:
                {
                    result2 = unchecked((byte)(double)this.CurrentValue);
                    if ((double)this.CurrentValue < byte.MinValue - 2.0)
                    {
                        return 4;
                    }

                    if ((double)this.CurrentValue > byte.MaxValue + 2.0)
                    {
                        return 4;
                    }

                    var q = (int)(double)this.CurrentValue;
                    if (q < 0)
                    {
                        return 4;
                    }

                    if (q > byte.MaxValue)
                    {
                        return 4;
                    }

                    result = (byte)q;
                }
                    return 0;

                case TypeCode.Decimal:
                {
                    result2 = unchecked((byte)this._dec);
                    if ((decimal)this.CurrentValue < byte.MinValue - 2.0m)
                    {
                        return 4;
                    }

                    if ((decimal)this.CurrentValue > byte.MaxValue + 2.0m)
                    {
                        return 4;
                    }

                    var q = (int)this._dec;
                    if (q < 0)
                    {
                        return 4;
                    }

                    if (q > byte.MaxValue)
                    {
                        return 4;
                    }

                    result = (byte)q;
                }
                    return 0;

                case TypeCode.String:
                {
                    result2 = 0;
                    var n = (string)this.CurrentValue;
                    if (n.Length != 0)
                    {
                        result2 = unchecked((byte)(n[0] & 0xFF));
                    }

                    n = n.Trim(' ', '\t', '\r', '\n');
                    var tryDouble = false;
                    object r = null;
                    var error = ParseUIntNumber(n, 1, ref tryDouble, ref r);
                    if (tryDouble)
                    {
                        error = ParseUFloatNumber(n, 16, ref r);
                        if (error == 0)
                        {
                            if ((decimal)r < byte.MinValue - 2.0m)
                            {
                                return 4;
                            }

                            if ((decimal)r > byte.MaxValue + 2.0m)
                            {
                                return 4;
                            }

                            var q = (int)r;
                            if (q < byte.MinValue)
                            {
                                return 4;
                            }

                            if (q > byte.MaxValue)
                            {
                                return 4;
                            }

                            result = (byte)q;
                            return 0;
                        }

                        return error;
                    }

                    if (error == 0)
                    {
                        result = (byte)r;
                        return 0;
                    }

                    return error;
                }

                case TypeCode.DateTime:
                    result2 = unchecked((byte)((DateTime)this.CurrentValue).Ticks);
                    return 2;

                default:
                    return 3;
            }
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToChar(ref char result, ref char result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue ? '1' : '0';
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result2 = unchecked((char)(sbyte)this.CurrentValue);
                    if ((sbyte)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    result = (char)(sbyte)this.CurrentValue;
                    return 0;

                case TypeCode.Byte:
                    result = (char)(byte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Char:
                    result = (char)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int16:
                    result2 = unchecked((char)(short)this.CurrentValue);
                    if ((short)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    result = (char)(short)this.CurrentValue;
                    return 0;

                case TypeCode.UInt16:
                    result = (char)(ushort)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int32:
                    result2 = unchecked((char)(int)this.CurrentValue);
                    if ((int)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    if ((int)this.CurrentValue > char.MaxValue)
                    {
                        return 4;
                    }

                    result = (char)(int)this.CurrentValue;
                    return 0;

                case TypeCode.UInt32:
                    result2 = unchecked((char)(uint)this.CurrentValue);
                    if ((uint)this.CurrentValue > char.MaxValue)
                    {
                        return 4;
                    }

                    result = (char)(uint)this.CurrentValue;
                    return 0;

                case TypeCode.Int64:
                    result2 = unchecked((char)(long)this.CurrentValue);
                    if ((long)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    if ((long)this.CurrentValue > char.MaxValue)
                    {
                        return 4;
                    }

                    result = (char)(long)this.CurrentValue;
                    return 0;

                case TypeCode.UInt64:
                    result2 = unchecked((char)(ulong)this.CurrentValue);
                    if ((ulong)this.CurrentValue > char.MaxValue)
                    {
                        return 4;
                    }

                    result = (char)(ulong)this.CurrentValue;
                    return 0;

                case TypeCode.Single:
                {
                    result2 = unchecked((char)(float)this.CurrentValue);
                    if ((float)this.CurrentValue < char.MinValue - 2.0f)
                    {
                        return 4;
                    }

                    if ((float)this.CurrentValue > char.MaxValue + 2.0f)
                    {
                        return 4;
                    }

                    var q = (int)(float)this.CurrentValue;
                    if (q < 0)
                    {
                        return 4;
                    }

                    if (q > char.MaxValue)
                    {
                        return 4;
                    }

                    result = (char)q;
                }
                    return 0;

                case TypeCode.Double:
                {
                    result2 = unchecked((char)(double)this.CurrentValue);
                    if ((double)this.CurrentValue < char.MinValue - 2.0)
                    {
                        return 4;
                    }

                    if ((double)this.CurrentValue > char.MaxValue + 2.0)
                    {
                        return 4;
                    }

                    var q = (int)(double)this.CurrentValue;
                    if (q < 0)
                    {
                        return 4;
                    }

                    if (q > char.MaxValue)
                    {
                        return 4;
                    }

                    result = (char)q;
                }
                    return 0;

                case TypeCode.Decimal:
                {
                    result2 = unchecked((char)this._dec);
                    if ((decimal)this.CurrentValue < char.MinValue - 2.0m)
                    {
                        return 4;
                    }

                    if ((decimal)this.CurrentValue > char.MaxValue + 2.0m)
                    {
                        return 4;
                    }

                    var q = (int)this._dec;
                    if (q < 0)
                    {
                        return 4;
                    }

                    if (q > char.MaxValue)
                    {
                        return 4;
                    }

                    result = (char)q;
                }
                    return 0;

                case TypeCode.String:
                {
                    result2 = (char)0;
                    var n = (string)this.CurrentValue;
                    if (n.Length != 0)
                    {
                        result2 = n[0];
                    }

                    if (n.Length == 1)
                    {
                        result = n[0];
                        return 0;
                    }

                    n = n.Trim(' ', '\t', '\r', '\n');
                    var tryDouble = false;
                    object r = null;
                    var error = ParseUIntNumber(n, 2, ref tryDouble, ref r);
                    if (tryDouble)
                    {
                        error = ParseUFloatNumber(n, 16, ref r);
                        if (error == 0)
                        {
                            if ((decimal)r < char.MinValue - 2.0m)
                            {
                                return 4;
                            }

                            if ((decimal)r > char.MaxValue + 2.0m)
                            {
                                return 4;
                            }

                            var q = (int)r;
                            if (q < char.MinValue)
                            {
                                return 4;
                            }

                            if (q > char.MaxValue)
                            {
                                return 4;
                            }

                            result = (char)q;
                            return 0;
                        }

                        return error;
                    }

                    if (error == 0)
                    {
                        result = (char)r;
                        return 0;
                    }

                    return error;
                }

                case TypeCode.DateTime:
                    result2 = unchecked((char)((DateTime)this.CurrentValue).Ticks);
                    return 2;

                default:
                    return 3;
            }
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToInt16(ref short result, ref short result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue ? (short)1 : (short)0;
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result = (sbyte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Byte:
                    result = (byte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Char:
                    result2 = unchecked((short)(char)this.CurrentValue);
                    if ((char)this.CurrentValue > (char)short.MaxValue)
                    {
                        return 4;
                    }

                    result = (short)(char)this.CurrentValue;
                    return 0;

                case TypeCode.Int16:
                    result = (short)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt16:
                    result2 = unchecked((short)(ushort)this.CurrentValue);
                    if ((ushort)this.CurrentValue > (ushort)short.MaxValue)
                    {
                        return 4;
                    }

                    result = (short)(ushort)this.CurrentValue;
                    return 0;

                case TypeCode.Int32:
                    result2 = unchecked((short)(int)this.CurrentValue);
                    if ((int)this.CurrentValue < short.MinValue)
                    {
                        return 4;
                    }

                    if ((int)this.CurrentValue > short.MaxValue)
                    {
                        return 4;
                    }

                    result = (short)(int)this.CurrentValue;
                    return 0;

                case TypeCode.UInt32:
                    result2 = unchecked((short)(uint)this.CurrentValue);
                    if ((uint)this.CurrentValue > (uint)short.MaxValue)
                    {
                        return 4;
                    }

                    result = (short)(uint)this.CurrentValue;
                    return 0;

                case TypeCode.Int64:
                    result2 = unchecked((short)(long)this.CurrentValue);
                    if ((long)this.CurrentValue < short.MinValue)
                    {
                        return 4;
                    }

                    if ((long)this.CurrentValue > short.MaxValue)
                    {
                        return 4;
                    }

                    result = (short)(long)this.CurrentValue;
                    return 0;

                case TypeCode.UInt64:
                    result2 = unchecked((short)(ulong)this.CurrentValue);
                    if ((ulong)this.CurrentValue > (ulong)short.MaxValue)
                    {
                        return 4;
                    }

                    result = (short)(ulong)this.CurrentValue;
                    return 0;

                case TypeCode.Single:
                {
                    result2 = unchecked((short)(float)this.CurrentValue);
                    if ((float)this.CurrentValue < short.MinValue - 2.0f)
                    {
                        return 4;
                    }

                    if ((float)this.CurrentValue > short.MaxValue + 2.0f)
                    {
                        return 4;
                    }

                    var q = (int)(float)this.CurrentValue;
                    if (q < short.MinValue)
                    {
                        return 4;
                    }

                    if (q > short.MaxValue)
                    {
                        return 4;
                    }

                    result = (short)q;
                }
                    return 0;

                case TypeCode.Double:
                {
                    result2 = unchecked((short)(double)this.CurrentValue);
                    if ((double)this.CurrentValue < short.MinValue - 2.0)
                    {
                        return 4;
                    }

                    if ((double)this.CurrentValue > short.MaxValue + 2.0)
                    {
                        return 4;
                    }

                    var q = (int)(double)this.CurrentValue;
                    if (q < short.MinValue)
                    {
                        return 4;
                    }

                    if (q > short.MaxValue)
                    {
                        return 4;
                    }

                    result = (short)q;
                }
                    return 0;

                case TypeCode.Decimal:
                {
                    result2 = unchecked((short)this._dec);
                    if ((decimal)this.CurrentValue < short.MinValue - 2.0m)
                    {
                        return 4;
                    }

                    if ((decimal)this.CurrentValue > short.MaxValue + 2.0m)
                    {
                        return 4;
                    }

                    var q = (int)this._dec;
                    if (q < short.MinValue)
                    {
                        return 4;
                    }

                    if (q > short.MaxValue)
                    {
                        return 4;
                    }

                    result = (short)q;
                }
                    return 0;

                case TypeCode.String:
                {
                    result2 = 0;
                    var n = (string)this.CurrentValue;
                    if (n.Length != 0)
                    {
                        result2 = unchecked((short)(n[0] & 0xFFFF));
                    }

                    n = n.Trim(' ', '\t', '\r', '\n');
                    var tryDouble = false;
                    object r = null;
                    var error = ParseIntNumber(n, 2, ref tryDouble, ref r);
                    if (tryDouble)
                    {
                        error = ParseUFloatNumber(n, 16, ref r);
                        if (error == 0)
                        {
                            if ((decimal)r < short.MinValue - 2.0m)
                            {
                                return 4;
                            }

                            if ((decimal)r > short.MaxValue + 2.0m)
                            {
                                return 4;
                            }

                            var q = (int)r;
                            if (q < short.MinValue)
                            {
                                return 4;
                            }

                            if (q > short.MaxValue)
                            {
                                return 4;
                            }

                            result = (short)q;
                            return 0;
                        }

                        return error;
                    }

                    if (error == 0)
                    {
                        result = (short)r;
                        return 0;
                    }

                    return error;
                }

                case TypeCode.DateTime:
                    result2 = unchecked((short)((DateTime)this.CurrentValue).Ticks);
                    return 2;

                default:
                    return 3;
            }
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToUInt16(ref ushort result, ref ushort result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue ? (ushort)1 : (ushort)0;
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result2 = unchecked((ushort)(sbyte)this.CurrentValue);
                    if ((sbyte)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    result = (ushort)(sbyte)this.CurrentValue;
                    return 0;

                case TypeCode.Byte:
                    result = (byte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Char:
                    result = (char)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int16:
                    result2 = unchecked((ushort)(short)this.CurrentValue);
                    if ((short)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    result = (ushort)(short)this.CurrentValue;
                    return 0;

                case TypeCode.UInt16:
                    result = (ushort)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int32:
                    result2 = unchecked((ushort)(int)this.CurrentValue);
                    if ((int)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    if ((int)this.CurrentValue > ushort.MaxValue)
                    {
                        return 4;
                    }

                    result = (ushort)(int)this.CurrentValue;
                    return 0;

                case TypeCode.UInt32:
                    result2 = unchecked((ushort)(uint)this.CurrentValue);
                    if ((uint)this.CurrentValue > ushort.MaxValue)
                    {
                        return 4;
                    }

                    result = (ushort)(uint)this.CurrentValue;
                    return 0;

                case TypeCode.Int64:
                    result2 = unchecked((ushort)(long)this.CurrentValue);
                    if ((long)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    if ((long)this.CurrentValue > ushort.MaxValue)
                    {
                        return 4;
                    }

                    result = (ushort)(long)this.CurrentValue;
                    return 0;

                case TypeCode.UInt64:
                    result2 = unchecked((ushort)(ulong)this.CurrentValue);
                    if ((ulong)this.CurrentValue > ushort.MaxValue)
                    {
                        return 4;
                    }

                    result = (ushort)(ulong)this.CurrentValue;
                    return 0;

                case TypeCode.Single:
                {
                    result2 = unchecked((ushort)(float)this.CurrentValue);
                    if ((float)this.CurrentValue < ushort.MinValue - 2.0f)
                    {
                        return 4;
                    }

                    if ((float)this.CurrentValue > ushort.MaxValue + 2.0f)
                    {
                        return 4;
                    }

                    var q = (int)(float)this.CurrentValue;
                    if (q < 0)
                    {
                        return 4;
                    }

                    if (q > ushort.MaxValue)
                    {
                        return 4;
                    }

                    result = (ushort)q;
                }
                    return 0;

                case TypeCode.Double:
                {
                    result2 = unchecked((ushort)(double)this.CurrentValue);
                    if ((double)this.CurrentValue < ushort.MinValue - 2.0)
                    {
                        return 4;
                    }

                    if ((double)this.CurrentValue > ushort.MaxValue + 2.0)
                    {
                        return 4;
                    }

                    var q = (int)(double)this.CurrentValue;
                    if (q < 0)
                    {
                        return 4;
                    }

                    if (q > ushort.MaxValue)
                    {
                        return 4;
                    }

                    result = (ushort)q;
                }
                    return 0;

                case TypeCode.Decimal:
                {
                    result2 = unchecked((ushort)this._dec);
                    if ((decimal)this.CurrentValue < ushort.MinValue - 2.0m)
                    {
                        return 4;
                    }

                    if ((decimal)this.CurrentValue > ushort.MaxValue + 2.0m)
                    {
                        return 4;
                    }

                    var q = (int)this._dec;
                    if (q < 0)
                    {
                        return 4;
                    }

                    if (q > ushort.MaxValue)
                    {
                        return 4;
                    }

                    result = (ushort)q;
                }
                    return 0;

                case TypeCode.String:
                {
                    result2 = 0;
                    var n = (string)this.CurrentValue;
                    if (n.Length != 0)
                    {
                        result2 = unchecked((ushort)(n[0] & 0xFFFF));
                    }

                    n = n.Trim(' ', '\t', '\r', '\n');
                    var tryDouble = false;
                    object r = null;
                    var error = ParseUIntNumber(n, 2, ref tryDouble, ref r);
                    if (tryDouble)
                    {
                        error = ParseUFloatNumber(n, 16, ref r);
                        if (error == 0)
                        {
                            if ((decimal)r < ushort.MinValue - 2.0m)
                            {
                                return 4;
                            }

                            if ((decimal)r > ushort.MaxValue + 2.0m)
                            {
                                return 4;
                            }

                            var q = (int)r;
                            if (q < ushort.MinValue)
                            {
                                return 4;
                            }

                            if (q > ushort.MaxValue)
                            {
                                return 4;
                            }

                            result = (ushort)q;
                            return 0;
                        }

                        return error;
                    }

                    if (error == 0)
                    {
                        result = (ushort)r;
                        return 0;
                    }

                    return error;
                }

                case TypeCode.DateTime:
                    result2 = unchecked((ushort)((DateTime)this.CurrentValue).Ticks);
                    return 2;

                default:
                    return 3;
            }
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToInt32(ref int result, ref int result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue ? (byte)1 : (byte)0;
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result = (sbyte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Byte:
                    result = (byte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Char:
                    result = (char)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int16:
                    result = (short)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt16:
                    result = (ushort)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int32:
                    result = (int)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt32:
                    result2 = unchecked((int)(uint)this.CurrentValue);
                    if ((uint)this.CurrentValue > int.MaxValue)
                    {
                        return 4;
                    }

                    result = (int)(uint)this.CurrentValue;
                    return 0;

                case TypeCode.Int64:
                    result2 = unchecked((int)(long)this.CurrentValue);
                    if ((long)this.CurrentValue < int.MinValue)
                    {
                        return 4;
                    }

                    if ((long)this.CurrentValue > int.MaxValue)
                    {
                        return 4;
                    }

                    result = (int)(long)this.CurrentValue;
                    return 0;

                case TypeCode.UInt64:
                    result2 = unchecked((int)(ulong)this.CurrentValue);
                    if ((ulong)this.CurrentValue > int.MaxValue)
                    {
                        return 4;
                    }

                    result = (int)(ulong)this.CurrentValue;
                    return 0;

                case TypeCode.Single:
                {
                    result2 = unchecked((int)(float)this.CurrentValue);
                    if ((float)this.CurrentValue < int.MinValue)
                    {
                        return 4;
                    }

                    if ((float)this.CurrentValue > int.MaxValue)
                    {
                        return 4;
                    }

                    result = (int)(float)this.CurrentValue;
                }
                    return 0;

                case TypeCode.Double:
                {
                    result2 = unchecked((int)(double)this.CurrentValue);
                    if ((double)this.CurrentValue < int.MinValue)
                    {
                        return 4;
                    }

                    if ((double)this.CurrentValue > int.MaxValue)
                    {
                        return 4;
                    }

                    result = (int)(double)this.CurrentValue;
                }
                    return 0;

                case TypeCode.Decimal:
                {
                    result2 = unchecked((int)this._dec);
                    if ((decimal)this.CurrentValue < int.MinValue)
                    {
                        return 4;
                    }

                    if ((decimal)this.CurrentValue > int.MaxValue)
                    {
                        return 4;
                    }

                    result = (int)this._dec;
                }
                    return 0;

                case TypeCode.String:
                {
                    result2 = 0;
                    var n = (string)this.CurrentValue;
                    if (n.Length == 1)
                    {
                        result2 = n[0];
                    }
                    else if (n.Length >= 2)
                    {
                        result2 = n[0] | (n[1] << 16);
                    }

                    n = n.Trim(' ', '\t', '\r', '\n');
                    var tryDouble = false;
                    object r = null;
                    var error = ParseIntNumber(n, 4, ref tryDouble, ref r);
                    if (tryDouble)
                    {
                        error = ParseUFloatNumber(n, 16, ref r);
                        if (error == 0)
                        {
                            if ((decimal)r < int.MinValue)
                            {
                                return 4;
                            }

                            if ((decimal)r > int.MaxValue)
                            {
                                return 4;
                            }

                            result = (int)r;
                            return 0;
                        }

                        return error;
                    }

                    if (error == 0)
                    {
                        result = (int)r;
                        return 0;
                    }

                    return error;
                }

                case TypeCode.DateTime:
                    result2 = unchecked((int)((DateTime)this.CurrentValue).Ticks);
                    return 2;

                default:
                    return 3;
            }
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToUInt32(ref uint result, ref uint result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue ? (byte)1 : (byte)0;
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result2 = unchecked((uint)(sbyte)this.CurrentValue);
                    if ((sbyte)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    result = (byte)(sbyte)this.CurrentValue;
                    return 0;

                case TypeCode.Byte:
                    result = (byte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Char:
                    result = (char)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int16:
                    result2 = unchecked((uint)(short)this.CurrentValue);
                    if ((short)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    result = (ushort)(short)this.CurrentValue;
                    return 0;

                case TypeCode.UInt16:
                    result = (ushort)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int32:
                    result2 = unchecked((uint)(int)this.CurrentValue);
                    if ((int)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    result = (uint)(int)this.CurrentValue;
                    return 0;

                case TypeCode.UInt32:
                    result = (uint)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int64:
                    result2 = unchecked((uint)(long)this.CurrentValue);
                    if ((long)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    if ((long)this.CurrentValue > uint.MaxValue)
                    {
                        return 4;
                    }

                    result = (uint)(long)this.CurrentValue;
                    return 0;

                case TypeCode.UInt64:
                    result2 = unchecked((uint)(ulong)this.CurrentValue);
                    if ((ulong)this.CurrentValue > uint.MaxValue)
                    {
                        return 4;
                    }

                    result = (uint)(ulong)this.CurrentValue;
                    return 0;

                case TypeCode.Single:
                {
                    result2 = unchecked((uint)(float)this.CurrentValue);
                    if ((float)this.CurrentValue < 0.0f)
                    {
                        return 4;
                    }

                    if ((float)this.CurrentValue > uint.MaxValue)
                    {
                        return 4;
                    }

                    result = (uint)(float)this.CurrentValue;
                }
                    return 0;

                case TypeCode.Double:
                {
                    result2 = unchecked((uint)(double)this.CurrentValue);
                    if ((double)this.CurrentValue < 0.0)
                    {
                        return 4;
                    }

                    if ((double)this.CurrentValue > uint.MaxValue)
                    {
                        return 4;
                    }

                    result = (uint)(double)this.CurrentValue;
                }
                    return 0;

                case TypeCode.Decimal:
                {
                    result2 = unchecked((uint)this._dec);
                    if ((decimal)this.CurrentValue < 0.0m)
                    {
                        return 4;
                    }

                    if ((decimal)this.CurrentValue > uint.MaxValue)
                    {
                        return 4;
                    }

                    result = (uint)this._dec;
                }
                    return 0;

                case TypeCode.String:
                {
                    result2 = 0;
                    var n = (string)this.CurrentValue;
                    if (n.Length == 1)
                    {
                        result2 = n[0];
                    }
                    else if (n.Length >= 2)
                    {
                        result2 = n[0] | ((uint)n[1] << 16);
                    }

                    n = n.Trim(' ', '\t', '\r', '\n');
                    var tryDouble = false;
                    object r = null;
                    var error = ParseUIntNumber(n, 4, ref tryDouble, ref r);
                    if (tryDouble)
                    {
                        error = ParseUFloatNumber(n, 16, ref r);
                        if (error == 0)
                        {
                            if ((decimal)r < 0.0m)
                            {
                                return 4;
                            }

                            if ((decimal)r > uint.MaxValue)
                            {
                                return 4;
                            }

                            result = (uint)r;
                            return 0;
                        }

                        return error;
                    }

                    if (error == 0)
                    {
                        result = (uint)r;
                        return 0;
                    }

                    return error;
                }

                case TypeCode.DateTime:
                    result2 = unchecked((uint)((DateTime)this.CurrentValue).Ticks);
                    return 2;

                default:
                    return 3;
            }
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToInt64(ref long result, ref long result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue ? (byte)1 : (byte)0;
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result = (sbyte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Byte:
                    result = (byte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Char:
                    result = (char)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int16:
                    result = (short)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt16:
                    result = (ushort)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int32:
                    result = (int)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt32:
                    result = (uint)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int64:
                    result = (long)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt64:
                    result2 = unchecked((long)(ulong)this.CurrentValue);
                    if ((ulong)this.CurrentValue > long.MaxValue)
                    {
                        return 4;
                    }

                    result = (long)(ulong)this.CurrentValue;
                    return 0;

                case TypeCode.Single:
                {
                    result2 = unchecked((long)(float)this.CurrentValue);
                    if ((float)this.CurrentValue < long.MinValue)
                    {
                        return 4;
                    }

                    if ((float)this.CurrentValue > long.MaxValue)
                    {
                        return 4;
                    }

                    result = (long)(float)this.CurrentValue;
                }
                    return 0;

                case TypeCode.Double:
                {
                    result2 = unchecked((long)(double)this.CurrentValue);
                    if ((double)this.CurrentValue < long.MinValue)
                    {
                        return 4;
                    }

                    if ((double)this.CurrentValue > long.MaxValue)
                    {
                        return 4;
                    }

                    result = (long)(double)this.CurrentValue;
                }
                    return 0;

                case TypeCode.Decimal:
                {
                    result2 = unchecked((long)this._dec);
                    if ((decimal)this.CurrentValue < long.MinValue)
                    {
                        return 4;
                    }

                    if ((decimal)this.CurrentValue > long.MaxValue)
                    {
                        return 4;
                    }

                    result = (long)this._dec;
                }
                    return 0;

                case TypeCode.String:
                {
                    result2 = 0;
                    var n = (string)this.CurrentValue;
                    if (n.Length == 1)
                    {
                        result2 = n[0];
                    }
                    else if (n.Length == 2)
                    {
                        result2 = n[0] | (n[1] << 16);
                    }
                    else if (n.Length == 3)
                    {
                        result2 = n[0] | ((long)n[1] << 16) | ((long)n[2] << 32);
                    }
                    else if (n.Length >= 4)
                    {
                        result2 = n[0] | ((long)n[1] << 16) | ((long)n[2] << 32) | ((long)n[3] << 48);
                    }

                    n = n.Trim(' ', '\t', '\r', '\n');
                    var tryDouble = false;
                    object r = null;
                    var error = ParseIntNumber(n, 8, ref tryDouble, ref r);
                    if (tryDouble)
                    {
                        error = ParseUFloatNumber(n, 16, ref r);
                        if (error == 0)
                        {
                            if ((decimal)r < long.MinValue)
                            {
                                return 4;
                            }

                            if ((decimal)r > long.MaxValue)
                            {
                                return 4;
                            }

                            result = (long)r;
                            return 0;
                        }

                        return error;
                    }

                    if (error == 0)
                    {
                        result = (long)r;
                        return 0;
                    }

                    return error;
                }

                case TypeCode.DateTime:
                    result = ((DateTime)this.CurrentValue).Ticks;
                    result2 = result;
                    return 0;

                default:
                    return 3;
            }
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToUInt64(ref ulong result, ref ulong result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue ? (byte)1 : (byte)0;
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result2 = unchecked((uint)(sbyte)this.CurrentValue);
                    if ((sbyte)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    result = (byte)(sbyte)this.CurrentValue;
                    return 0;

                case TypeCode.Byte:
                    result = (byte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Char:
                    result = (char)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int16:
                    result2 = unchecked((uint)(short)this.CurrentValue);
                    if ((short)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    result = (ushort)(short)this.CurrentValue;
                    return 0;

                case TypeCode.UInt16:
                    result = (ushort)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int32:
                    result2 = unchecked((ulong)(int)this.CurrentValue);
                    if ((int)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    result = (uint)(int)this.CurrentValue;
                    return 0;

                case TypeCode.UInt32:
                    result = (uint)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int64:
                    result2 = unchecked((ulong)(long)this.CurrentValue);
                    if ((long)this.CurrentValue < 0)
                    {
                        return 4;
                    }

                    result = (ulong)(long)this.CurrentValue;
                    return 0;

                case TypeCode.UInt64:
                    result = (ulong)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Single:
                {
                    result2 = unchecked((ulong)(float)this.CurrentValue);
                    if ((float)this.CurrentValue < 0.0f)
                    {
                        return 4;
                    }

                    if ((float)this.CurrentValue > ulong.MaxValue)
                    {
                        return 4;
                    }

                    result = (ulong)(float)this.CurrentValue;
                }
                    return 0;

                case TypeCode.Double:
                {
                    result2 = unchecked((ulong)(double)this.CurrentValue);
                    if ((double)this.CurrentValue < 0.0)
                    {
                        return 4;
                    }

                    if ((double)this.CurrentValue > ulong.MaxValue)
                    {
                        return 4;
                    }

                    result = (ulong)(double)this.CurrentValue;
                }
                    return 0;

                case TypeCode.Decimal:
                {
                    result2 = unchecked((ulong)this._dec);
                    if ((decimal)this.CurrentValue < 0.0m)
                    {
                        return 4;
                    }

                    if ((decimal)this.CurrentValue > ulong.MaxValue)
                    {
                        return 4;
                    }

                    result = (ulong)this._dec;
                }
                    return 0;

                case TypeCode.String:
                {
                    result2 = 0;
                    var n = (string)this.CurrentValue;
                    if (n.Length == 1)
                    {
                        result2 = n[0];
                    }
                    else if (n.Length == 2)
                    {
                        result2 = n[0] | ((uint)n[1] << 16);
                    }
                    else if (n.Length == 3)
                    {
                        result2 = n[0] | ((ulong)n[1] << 16) | ((ulong)n[2] << 32);
                    }
                    else if (n.Length >= 4)
                    {
                        result2 = n[0] | ((ulong)n[1] << 16) | ((ulong)n[2] << 32) | ((ulong)n[3] << 48);
                    }

                    n = n.Trim(' ', '\t', '\r', '\n');
                    var tryDouble = false;
                    object r = null;
                    var error = ParseUIntNumber(n, 8, ref tryDouble, ref r);
                    if (tryDouble)
                    {
                        error = ParseUFloatNumber(n, 16, ref r);
                        if (error == 0)
                        {
                            if ((decimal)r < 0.0m)
                            {
                                return 4;
                            }

                            if ((decimal)r > ulong.MaxValue)
                            {
                                return 4;
                            }

                            result = (ulong)r;
                            return 0;
                        }

                        return error;
                    }

                    if (error == 0)
                    {
                        result = (ulong)r;
                        return 0;
                    }

                    return error;
                }

                case TypeCode.DateTime:
                    result2 = unchecked((ulong)((DateTime)this.CurrentValue).Ticks);
                    if (((DateTime)this.CurrentValue).Ticks < 0)
                    {
                        return 4;
                    }

                    result = (ulong)((DateTime)this.CurrentValue).Ticks;
                    return 0;

                default:
                    return 3;
            }
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToSingle(ref float result, ref float result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue ? (byte)1 : (byte)0;
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result = (sbyte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Byte:
                    result = (byte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Char:
                    result = (char)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int16:
                    result = (short)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt16:
                    result = (ushort)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int32:
                    result = (int)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt32:
                    result = (uint)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int64:
                    result = (long)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt64:
                    result = (ulong)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Single:
                    result = (float)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Double:
                {
                    result2 = (float)(double)this.CurrentValue;
                    if ((double)this.CurrentValue < float.MinValue)
                    {
                        return 4;
                    }

                    if ((double)this.CurrentValue > float.MaxValue)
                    {
                        return 4;
                    }

                    result = (float)(double)this.CurrentValue;
                }
                    return 0;

                case TypeCode.Decimal:
                {
                    result2 = (float)this._dec;
                    result = (float)(decimal)this.CurrentValue;
                }
                    return 0;

                case TypeCode.String:
                {
                    result2 = 0;
                    var n = (string)this.CurrentValue;
                    if (n.Length == 1)
                    {
                        result2 = n[0];
                    }
                    else if (n.Length >= 2)
                    {
                        result2 = n[0] | (n[1] << 16);
                    }

                    n = n.Trim(' ', '\t', '\r', '\n');
                    object r = null;
                    var error = ParseUFloatNumber(n, 4, ref r);
                    if (error == 0)
                    {
                        result = (float)r;
                        return 0;
                    }

                    return error;
                }

                case TypeCode.DateTime:
                    result2 = ((DateTime)this.CurrentValue).Ticks;
                    return 2;

                default:
                    return 3;
            }
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToDouble(ref double result, ref double result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue ? (byte)1 : (byte)0;
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result = (sbyte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Byte:
                    result = (byte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Char:
                    result = (char)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int16:
                    result = (short)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt16:
                    result = (ushort)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int32:
                    result = (int)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt32:
                    result = (uint)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int64:
                    result = (long)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt64:
                    result = (ulong)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Single:
                    result = (float)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Double:
                    result = (double)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Decimal:
                {
                    result2 = this._dec;
                    result = this._dec;
                }
                    return 0;

                case TypeCode.String:
                {
                    result2 = 0;
                    var n = (string)this.CurrentValue;
                    if (n.Length == 1)
                    {
                        result2 = n[0];
                    }
                    else if (n.Length == 2)
                    {
                        result2 = n[0] | (n[1] << 16);
                    }
                    else if (n.Length == 3)
                    {
                        result2 = n[0] | ((long)n[1] << 16) | ((long)n[2] << 32);
                    }
                    else if (n.Length >= 4)
                    {
                        result2 = n[0] | ((long)n[1] << 16) | ((long)n[2] << 32) | ((long)n[3] << 48);
                    }

                    n = n.Trim(' ', '\t', '\r', '\n');
                    object r = null;
                    var error = ParseUFloatNumber(n, 8, ref r);
                    if (error == 0)
                    {
                        result = (double)r;
                        return 0;
                    }

                    return error;
                }

                case TypeCode.DateTime:
                    result2 = ((DateTime)this.CurrentValue).Ticks;
                    return 2;

                default:
                    return 3;
            }
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToDecimal(ref decimal result, ref decimal result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue ? (byte)1 : (byte)0;
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result = (sbyte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Byte:
                    result = (byte)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Char:
                    result = (char)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int16:
                    result = (short)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt16:
                    result = (ushort)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int32:
                    result = (int)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt32:
                    result = (uint)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Int64:
                    result = (long)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.UInt64:
                    result = (ulong)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Single:
                    result = (decimal)(float)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Double:
                    result = (decimal)(double)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.Decimal:
                    result = (decimal)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.String:
                {
                    result2 = 0;
                    var n = (string)this.CurrentValue;
                    if (n.Length == 1)
                    {
                        result2 = n[0];
                    }
                    else if (n.Length == 2)
                    {
                        result2 = n[0] | (n[1] << 16);
                    }
                    else if (n.Length == 3)
                    {
                        result2 = n[0] | ((long)n[1] << 16) | ((long)n[2] << 32);
                    }
                    else if (n.Length >= 4)
                    {
                        result2 = n[0] | ((long)n[1] << 16) | ((long)n[2] << 32) | ((long)n[3] << 48);
                    }

                    n = n.Trim(' ', '\t', '\r', '\n');
                    object r = null;
                    var error = ParseUFloatNumber(n, 16, ref r);
                    if (error == 0)
                    {
                        result = (decimal)r;
                        return 0;
                    }

                    return error;
                }

                case TypeCode.DateTime:
                    result2 = ((DateTime)this.CurrentValue).Ticks;
                    return 2;

                default:
                    return 3;
            }
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToString(ref string result, ref string result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    result = (bool)this.CurrentValue ? "True" : "False";
                    result2 = result;
                    return 0;

                case TypeCode.SByte:
                    result = ((sbyte)this.CurrentValue).ToString();
                    result2 = result;
                    return 0;

                case TypeCode.Byte:
                    result = ((byte)this.CurrentValue).ToString();
                    result2 = result;
                    return 0;

                case TypeCode.Char:
                    result = ((char)this.CurrentValue).ToString();
                    result2 = result;
                    return 0;

                case TypeCode.Int16:
                    result = ((short)this.CurrentValue).ToString();
                    result2 = result;
                    return 0;

                case TypeCode.UInt16:
                    result = ((ushort)this.CurrentValue).ToString();
                    result2 = result;
                    return 0;

                case TypeCode.Int32:
                    result = ((int)this.CurrentValue).ToString();
                    result2 = result;
                    return 0;

                case TypeCode.UInt32:
                    result = ((uint)this.CurrentValue).ToString();
                    result2 = result;
                    return 0;

                case TypeCode.Int64:
                    result = ((long)this.CurrentValue).ToString();
                    result2 = result;
                    return 0;

                case TypeCode.UInt64:
                    result = ((ulong)this.CurrentValue).ToString();
                    result2 = result;
                    return 0;

                case TypeCode.Single:
                    result = ((float)this.CurrentValue).ToString(CultureInfo.InvariantCulture);
                    result2 = result;
                    return 0;

                case TypeCode.Double:
                    result = ((double)this.CurrentValue).ToString(CultureInfo.InvariantCulture);
                    result2 = result;
                    return 0;

                case TypeCode.Decimal:
                    result = ((decimal)this.CurrentValue).ToString(CultureInfo.InvariantCulture);
                    result2 = result;
                    return 0;

                case TypeCode.String:
                    result = (string)this.CurrentValue;
                    result2 = result;
                    return 0;

                case TypeCode.DateTime:
                    result = ((DateTime)this.CurrentValue).ToString(CultureInfo.InvariantCulture);
                    result2 = result;
                    return 0;

                default:
                    return 3;
            }
        }

        private int _tryToDateTime(long ticks, ref DateTime result, ref DateTime result2)
        {
            var cv_min = DateTime.MinValue.Ticks;
            var cv_max = DateTime.MaxValue.Ticks;
            var cv = ticks;
            cv = Math.Max(cv_min, Math.Min(cv_max, cv));
            result2 = new DateTime(cv);
            if (cv != ticks)
            {
                return 4;
            }

            result = new DateTime(cv);
            return 0;
        }

        /// <summary>
        ///     Try to convert value internally.
        /// </summary>
        /// <param name="result">Result will be set here if it's valid.</param>
        /// <param name="result2">Unchecked result will be set here always.</param>
        /// <returns></returns>
        private int _tryToDateTime(ref DateTime result, ref DateTime result2)
        {
            // Convert by type.
            switch (this.CurrentType)
            {
                case TypeCode.Boolean:
                    return 2;

                case TypeCode.SByte:
                    return 2;

                case TypeCode.Byte:
                    return 2;

                case TypeCode.Char:
                    return 2;

                case TypeCode.Int16:
                    return 2;

                case TypeCode.UInt16:
                    return 2;

                case TypeCode.Int32:
                    return 2;

                case TypeCode.UInt32:
                    result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) +
                             new TimeSpan(10000000 * (long)(uint)this.CurrentValue);
                    result2 = result;
                    return 0;

                case TypeCode.Int64:
                    return this._tryToDateTime((long)this.CurrentValue, ref result, ref result2);

                case TypeCode.UInt64:
                {
                    var cv = unchecked((long)(ulong)this.CurrentValue);
                    this._tryToDateTime(cv, ref result, ref result2);
                    if ((ulong)this.CurrentValue > long.MaxValue)
                    {
                        return 4;
                    }

                    cv = (long)(ulong)this.CurrentValue;
                    return this._tryToDateTime(cv, ref result, ref result2);
                }

                case TypeCode.Single:
                    return 2;

                case TypeCode.Double:
                    return 2;

                case TypeCode.Decimal:
                    return 2;

                case TypeCode.String:
                {
                    var n = ((string)this.CurrentValue).Trim();
                    DateTime r;
                    if (DateTime.TryParse(n, out r))
                    {
                        result = r;
                        result2 = result;
                        return 0;
                    }

                    long lr = 0;
                    long lr2 = 0;
                    var pr = 0;
                    if ((pr = this._tryToInt64(ref lr, ref lr2)) == 0)
                    {
                        return this._tryToDateTime(lr, ref result, ref result2);
                    }

                    if (pr == 4)
                    {
                        this._tryToDateTime(lr2, ref result, ref result2);
                        return 4;
                    }
                }
                    return 1;

                case TypeCode.DateTime:
                    result = (DateTime)this.CurrentValue;
                    result2 = result;
                    return 0;

                default:
                    return 3;
            }
        }

        #endregion

        /// <summary>
        ///     Get or set base value.
        /// </summary>
        internal readonly IConvertible CurrentValue;

        /// <summary>
        ///     Get or set base type code.
        /// </summary>
        internal readonly TypeCode CurrentType;

        #endregion

        #region Static members

        /// <summary>
        ///     Get value from text and throw if we were not successful.
        /// </summary>
        /// <param name="text">Given text.</param>
        /// <param name="type">Type of value to convert to.</param>
        /// <returns></returns>
        public static Value Parse(string text, TypeCode type)
        {
            // Must have valid text.
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            // Create new value and cast it to a type.
            var v = new Value(text);
            switch (type)
            {
                case TypeCode.Boolean:
                    return new Value(v.ToBoolean());
                case TypeCode.SByte:
                    return new Value(v.ToSByte());
                case TypeCode.Byte:
                    return new Value(v.ToByte());
                case TypeCode.Char:
                    return new Value(v.ToChar());
                case TypeCode.Int16:
                    return new Value(v.ToInt16());
                case TypeCode.UInt16:
                    return new Value(v.ToUInt16());
                case TypeCode.Int32:
                    return new Value(v.ToInt32());
                case TypeCode.UInt32:
                    return new Value(v.ToUInt32());
                case TypeCode.Int64:
                    return new Value(v.ToInt64());
                case TypeCode.UInt64:
                    return new Value(v.ToUInt64());
                case TypeCode.Single:
                    return new Value(v.ToSingle());
                case TypeCode.Double:
                    return new Value(v.ToDouble());
                case TypeCode.Decimal:
                    return new Value(v.ToDecimal());
                case TypeCode.String:
                    return new Value(v.ToString());
                case TypeCode.DateTime:
                    return new Value(v.ToDateTime());

                default:
                    throw new InvalidCastException();
            }
        }

        /// <summary>
        ///     Get value from text and return if we were successful.
        /// </summary>
        /// <param name="text">Given text.</param>
        /// <param name="type">Type of value to convert to.</param>
        /// <param name="result">
        ///     Result will be set here. If false is returned then result may
        ///     or may not have value.
        /// </param>
        /// <returns></returns>
        public static bool TryParse(string text, TypeCode type, out Value result)
        {
            // Must have valid text.
            if (text == null)
            {
                result = null;
                return false;
            }

            // Create new value and cast it to a type.
            var v = new Value(text);
            switch (type)
            {
                case TypeCode.Boolean:
                {
                    var r = false;
                    var q = false;
                    if (v.TryToBoolean(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                case TypeCode.SByte:
                {
                    sbyte r = 0;
                    var q = false;
                    if (v.TryToSByte(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                case TypeCode.Byte:
                {
                    byte r = 0;
                    var q = false;
                    if (v.TryToByte(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                case TypeCode.Char:
                {
                    var r = (char)0;
                    var q = false;
                    if (v.TryToChar(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                case TypeCode.Int16:
                {
                    short r = 0;
                    var q = false;
                    if (v.TryToInt16(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                case TypeCode.UInt16:
                {
                    ushort r = 0;
                    var q = false;
                    if (v.TryToUInt16(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                case TypeCode.Int32:
                {
                    var r = 0;
                    var q = false;
                    if (v.TryToInt32(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                case TypeCode.UInt32:
                {
                    uint r = 0;
                    var q = false;
                    if (v.TryToUInt32(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                case TypeCode.Int64:
                {
                    long r = 0;
                    var q = false;
                    if (v.TryToInt64(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                case TypeCode.UInt64:
                {
                    ulong r = 0;
                    var q = false;
                    if (v.TryToUInt64(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                case TypeCode.Single:
                {
                    var r = 0.0f;
                    var q = false;
                    if (v.TryToSingle(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                case TypeCode.Double:
                {
                    var r = 0.0;
                    var q = false;
                    if (v.TryToDouble(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                case TypeCode.Decimal:
                {
                    var r = 0.0m;
                    var q = false;
                    if (v.TryToDecimal(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                case TypeCode.String:
                {
                    result = v;
                    return true;
                }

                case TypeCode.DateTime:
                {
                    var r = new DateTime(0);
                    var q = false;
                    if (v.TryToDateTime(out r))
                    {
                        q = true;
                    }

                    result = new Value(r);
                    return q;
                }

                default:
                    throw new InvalidCastException();
            }
        }

        /// <summary>
        ///     Parse an integer number.
        /// </summary>
        /// <param name="text">Text to parse from.</param>
        /// <param name="optimalSize">Optimal number of bytes.</param>
        /// <param name="tryDouble">Try to parse as double instead.</param>
        /// <param name="result">Resulting value will be set here.</param>
        /// <returns></returns>
        private static int ParseIntNumber(string text, int optimalSize, ref bool tryDouble, ref object result)
        {
            var orig = text;
            if (text.Length == 0)
            {
                return 1;
            }

            var isNeg = text[0] == '-';
            if (isNeg)
            {
                text = text.Substring(1);
            }

            switch (optimalSize)
            {
                case 1:
                {
                    object r = null;
                    var error = ParseUIntNumber(text, optimalSize, ref tryDouble, ref r);
                    if (error != 0 || tryDouble)
                    {
                        return error;
                    }

                    if (isNeg)
                    {
                        if ((byte)r > -sbyte.MinValue)
                        {
                            return 4;
                        }

                        result = -(sbyte)r;
                        return 0;
                    }

                    if ((byte)r > (byte)sbyte.MaxValue)
                    {
                        return 4;
                    }

                    result = (sbyte)(byte)r;
                    return 0;
                }

                case 2:
                {
                    object r = null;
                    var error = ParseUIntNumber(text, optimalSize, ref tryDouble, ref r);
                    if (error != 0 || tryDouble)
                    {
                        return error;
                    }

                    if (isNeg)
                    {
                        ushort umin = 0;
                        unchecked { umin = -short.MinValue; }

                        if ((ushort)r > umin)
                        {
                            return 4;
                        }

                        if ((ushort)r == umin)
                        {
                            result = short.MinValue;
                        }
                        else
                        {
                            result = -(short)(ushort)r;
                        }

                        return 0;
                    }

                    if ((ushort)r > (ushort)short.MaxValue)
                    {
                        return 4;
                    }

                    result = (short)(ushort)r;
                    return 0;
                }

                case 4:
                {
                    object r = null;
                    var error = ParseUIntNumber(text, optimalSize, ref tryDouble, ref r);
                    if (error != 0 || tryDouble)
                    {
                        return error;
                    }

                    if (isNeg)
                    {
                        uint umin = 0;
                        unchecked { umin = (uint)-int.MinValue; }

                        if ((uint)r > umin)
                        {
                            return 4;
                        }

                        if ((uint)r == umin)
                        {
                            result = int.MinValue;
                        }
                        else
                        {
                            result = -(int)(uint)r;
                        }

                        return 0;
                    }

                    if ((uint)r > int.MaxValue)
                    {
                        return 4;
                    }

                    result = (int)(uint)r;
                    return 0;
                }

                case 8:
                {
                    object r = null;
                    var error = ParseUIntNumber(text, optimalSize, ref tryDouble, ref r);
                    if (error != 0 || tryDouble)
                    {
                        return error;
                    }

                    if (isNeg)
                    {
                        ulong umin = 0;
                        unchecked { umin = (ulong)-long.MinValue; }

                        if ((ulong)r > umin)
                        {
                            return 4;
                        }

                        if ((ulong)r == umin)
                        {
                            result = long.MinValue;
                        }
                        else
                        {
                            result = -(long)(ulong)r;
                        }

                        return 0;
                    }

                    if ((ulong)r > long.MaxValue)
                    {
                        return 4;
                    }

                    result = (long)(ulong)r;
                    return 0;
                }

                default:
                    return 3;
            }
        }

        /// <summary>
        ///     Parse an unsigned integer number.
        /// </summary>
        /// <param name="text">Text to parse from.</param>
        /// <param name="optimalSize">Optimal number of bytes.</param>
        /// <param name="tryDouble">This will be set if we instead should try to parse double.</param>
        /// <param name="result">Result will be set here.</param>
        /// <returns></returns>
        private static int ParseUIntNumber(string text, int optimalSize, ref bool tryDouble, ref object result)
        {
            if (text.Length == 0)
            {
                return 1;
            }

            var isHex = text.StartsWith("0x") || text.StartsWith("0X");
            if (isHex) { text = text.Substring(2); }
            else if (text.EndsWith("h") || text.EndsWith("H"))
            {
                isHex = true;
                text = text.Substring(0, text.Length - 1);
            }

            if (text.Length == 0)
            {
                return 1;
            }

            if (!isHex)
            {
                text = text.Replace(",", "");
            }

            if (!isHex)
            {
                var ptIndex = text.LastIndexOf('.');
                if (ptIndex >= 0)
                {
                    var after = text.Substring(ptIndex + 1);
                    if (after.Length != 0)
                    {
                        var all = after.Distinct().ToArray();
                        if (all.Length > 1 || all[0] != '0')
                        {
                            tryDouble = true;
                            return 0;
                        }
                    }

                    text = text.Substring(0, ptIndex);
                }
            }

            if (text.Length == 0)
            {
                return 1;
            }

            switch (optimalSize)
            {
                case 1:
                {
                    if (isHex)
                    {
                        byte r = 0;
                        if (byte.TryParse(text, NumberStyles.AllowHexSpecifier, null, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }
                    else
                    {
                        byte r = 0;
                        if (byte.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }
                }
                    break;

                case 2:
                {
                    if (isHex)
                    {
                        ushort r = 0;
                        if (ushort.TryParse(text, NumberStyles.AllowHexSpecifier, null, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }
                    else
                    {
                        ushort r = 0;
                        if (ushort.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }
                }
                    break;

                case 4:
                {
                    if (isHex)
                    {
                        uint r = 0;
                        if (uint.TryParse(text, NumberStyles.AllowHexSpecifier, null, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }
                    else
                    {
                        uint r = 0;
                        if (uint.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }
                }
                    break;

                case 8:
                {
                    if (isHex)
                    {
                        ulong r = 0;
                        if (ulong.TryParse(text, NumberStyles.AllowHexSpecifier, null, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }
                    else
                    {
                        ulong r = 0;
                        if (ulong.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }

                    // Return invalid argument here! but only on ulong.
                    return 1;
                }

                default:
                    return 3;
            }

            // Try again as ulong so we can decide if it's overflow.
            if (isHex)
            {
                ulong r = 0;
                if (ulong.TryParse(text, NumberStyles.AllowHexSpecifier, null, out r))
                {
                    return 4;
                }
            }
            else
            {
                ulong r = 0;
                if (ulong.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out r))
                {
                    return 4;
                }
            }

            return 1;
        }

        /// <summary>
        ///     Parse a floating point number.
        /// </summary>
        /// <param name="text">Text to parse from.</param>
        /// <param name="optimalSize">Optimal number of bytes.</param>
        /// <param name="result">Result will be set here.</param>
        /// <returns></returns>
        private static int ParseUFloatNumber(string text, int optimalSize, ref object result)
        {
            if (text.Length == 0)
            {
                return 1;
            }

            var allowHex = true;
            if (text[0] == '-')
            {
                allowHex = false;
            }

            var isHex = allowHex && (text.StartsWith("0x") || text.StartsWith("0X"));
            if (isHex) { text = text.Substring(2); }
            else if (allowHex && (text.EndsWith("h") || text.EndsWith("H")))
            {
                isHex = true;
                text = text.Substring(0, text.Length - 1);
            }

            if (text.Length == 0)
            {
                return 1;
            }

            switch (optimalSize)
            {
                case 4:
                {
                    if (isHex)
                    {
                        var r = 0.0f;
                        if (float.TryParse(text, NumberStyles.AllowHexSpecifier, null, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }
                    else
                    {
                        var r = 0.0f;
                        if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }
                }
                    break;

                case 8:
                {
                    if (isHex)
                    {
                        var r = 0.0;
                        if (double.TryParse(text, NumberStyles.AllowHexSpecifier, null, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }
                    else
                    {
                        var r = 0.0;
                        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }
                }
                    break;

                case 16:
                {
                    if (isHex)
                    {
                        var r = 0.0m;
                        if (decimal.TryParse(text, NumberStyles.AllowHexSpecifier, null, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }
                    else
                    {
                        var r = 0.0m;
                        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out r))
                        {
                            result = r;
                            return 0;
                        }
                    }

                    // Return invalid but only on decimal.
                    return 1;
                }

                default:
                    return 3;
            }

            if (isHex)
            {
                var r = 0.0m;
                if (decimal.TryParse(text, NumberStyles.AllowHexSpecifier, null, out r))
                {
                    return 4;
                }
            }
            else
            {
                var r = 0.0m;
                if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out r))
                {
                    return 4;
                }
            }

            return 1;
        }

        #endregion

        #region CompressedValue members

        /// <summary>
        ///     Convert a big positive integer value into smallest possible amount of bytes. Maximum size
        ///     is 9 bytes.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns></returns>
        private static byte[] GetBigNumber(long value)
        {
            // Negative value is not supported.
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            // Smaller than 128 value can be placed in 1 byte.
            if (value < 128) // 2 ^ (8 -1)
            {
                return new[] {(byte)(128 | value)};
            }

            if (value < 16384) // 2 ^ (16 -2)
            {
                var data = BitConverter.GetBytes((short)value);
#if DEBUG
                if ((data[0] & 192) != 0)
                {
                    throw new InvalidOperationException();
                }
#endif
                data[0] |= 64;
                return data;
            }

            if (value < 2097152) // 2 ^ (24 -3)
            {
                var data = BitConverter.GetBytes((int)value);
#if DEBUG
                if (data[0] != 0 || (data[1] & 224) != 0)
                {
                    throw new InvalidOperationException();
                }
#endif
                data[1] |= 32;
                return new[] {data[1], data[2], data[3]};
            }

            if (value < 268435456) // 2 ^ (32 - 4)
            {
                var data = BitConverter.GetBytes((int)value);
#if DEBUG
                if ((data[0] & 240) != 0)
                {
                    throw new InvalidOperationException();
                }
#endif
                data[0] |= 16;
                return data;
            }

            if (value < 34359738368) // 2 ^ (40 - 5)
            {
                var data = BitConverter.GetBytes(value);
#if DEBUG
                if (data[0] != 0 || data[1] != 0 || data[2] != 0 || (data[3] & 248) != 0)
                {
                    throw new InvalidOperationException();
                }
#endif
                data[3] |= 8;
                return data;
            }

            if (value < 4398046511104) // 2 ^ (48 - 6)
            {
                var data = BitConverter.GetBytes(value);
#if DEBUG
                if (data[0] != 0 || data[1] != 0 || (data[2] & 252) != 0)
                {
                    throw new InvalidOperationException();
                }
#endif
                data[2] |= 4;
                return data;
            }

            if (value < 562949953421312) // 2 ^ (56 - 7)
            {
                var data = BitConverter.GetBytes(value);
#if DEBUG
                if (data[0] != 0 || (data[1] & 254) != 0)
                {
                    throw new InvalidOperationException();
                }
#endif
                data[1] |= 2;
                return data;
            }

            if (value < 72057594037927936) // 2 ^ (64 - 8)
            {
                var data = BitConverter.GetBytes(value);
#if DEBUG
                if ((data[0] & 255) != 0)
                {
                    throw new InvalidOperationException();
                }
#endif
                data[0] |= 1;
                return data;
            }

            return new byte[] {0}.Concat(BitConverter.GetBytes(value)).ToArray();
        }

        /// <summary>
        ///     Get big number, first pass.
        /// </summary>
        /// <param name="first">First byte.</param>
        /// <param name="count">Count of bytes to read after this.</param>
        /// <returns></returns>
        private static long GetBigNumberFirst(byte first, ref int count)
        {
            if ((first & 128) != 0)
            {
                return first & 127;
            }

            if ((first & 64) != 0)
            {
                count = 1;
                return first & 63;
            }

            if ((first & 32) != 0)
            {
                count = 2;
                return first & 31;
            }

            if ((first & 16) != 0)
            {
                count = 3;
                return first & 15;
            }

            if ((first & 8) != 0)
            {
                count = 4;
                return first & 7;
            }

            if ((first & 4) != 0)
            {
                count = 5;
                return first & 3;
            }

            if ((first & 2) != 0)
            {
                count = 6;
                return first & 1;
            }

            if ((first & 1) != 0)
            {
                count = 7;
                return 0;
            }

            count = 8;
            return 0;
        }

        /// <summary>
        ///     Get second part of big number from bytes.
        /// </summary>
        /// <param name="prefix">Prefix (first byte value mask).</param>
        /// <param name="data">Rest of bytes.</param>
        /// <param name="index">Index in buffer.</param>
        /// <param name="length">Length of bytes.</param>
        /// <returns></returns>
        private static long GetBigNumberSecond(long prefix, byte[] data, int index, int length)
        {
            if (length < 8)
            {
                prefix <<= 8 * length;
            }

            if (length == 1)
            {
                prefix |= data[index];
            }
            else if (length == 2)
            {
                prefix |= BitConverter.ToUInt16(data, index);
            }
            else if (length == 3)
            {
                prefix |= BitConverter.ToUInt32(new byte[] {0, data[index], data[index + 1], data[index + 2]}, 0);
            }
            else if (length == 4)
            {
                prefix |= BitConverter.ToUInt32(data, index);
            }
            else if (length == 5)
            {
                prefix |= BitConverter.ToInt64(
                    new byte[]
                    {
                        0, 0, 0, data[index], data[index + 1], data[index + 2], data[index + 3], data[index + 4]
                    }, 0);
            }
            else if (length == 6)
            {
                prefix |= BitConverter.ToInt64(
                    new byte[]
                    {
                        0, 0, data[index], data[index + 1], data[index + 2], data[index + 3], data[index + 4],
                        data[index + 5]
                    }, 0);
            }
            else if (length == 7)
            {
                prefix |= BitConverter.ToInt64(
                    new byte[]
                    {
                        0, data[index], data[index + 1], data[index + 2], data[index + 3], data[index + 4],
                        data[index + 5], data[index + 6]
                    },
                    0);
            }
            else if (length == 8)
            {
                prefix = BitConverter.ToInt64(data, index);
            }
            else
            {
                throw new InvalidOperationException();
            }

            return prefix;
        }

        #endregion
    }

    #endregion
}
