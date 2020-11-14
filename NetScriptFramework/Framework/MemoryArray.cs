namespace NetScriptFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    ///     Base array of values.
    /// </summary>
    public abstract class MemoryArrayBase
    {
        /// <summary>
        ///     Gets the base address of array entries.
        /// </summary>
        /// <value>
        ///     The address.
        /// </value>
        public IntPtr Address { get; internal set; }

        /// <summary>
        ///     Gets the length of array (count of entries).
        /// </summary>
        /// <value>
        ///     The length.
        /// </value>
        public abstract int Length { get; }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="maxEntries">The maximum entries.</param>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public abstract string ToString(int maxEntries);
    }

    /// <summary>
    ///     Array of values.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="NetScriptFramework.MemoryArrayBase" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{TValue}" />
    public abstract class MemoryArray <TValue> : MemoryArrayBase, IEnumerable<TValue>
    {
        /// <summary>
        ///     Gets the array handler.
        /// </summary>
        /// <value>
        ///     The handler.
        /// </value>
        public abstract MemoryArrayTypeHandler<TValue> Handler { get; }

        /// <summary>
        ///     Gets or sets the value at the specified index.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="System.IndexOutOfRangeException">
        ///     Index must not be negative!
        ///     or
        ///     Index must not exceed the length of array!
        ///     or
        ///     Index must not be negative!
        ///     or
        ///     Index must not exceed the length of array!
        /// </exception>
        public TValue this[int index]
        {
            get
            {
                if ( index < 0 )
                {
                    throw new IndexOutOfRangeException("Index must not be negative!");
                }

                if ( index >= this.Length )
                {
                    throw new IndexOutOfRangeException("Index must not exceed the length of array!");
                }

                return this.Handler.Read(this.Address, index);
            }
            set
            {
                if ( index < 0 )
                {
                    throw new IndexOutOfRangeException("Index must not be negative!");
                }

                if ( index >= this.Length )
                {
                    throw new IndexOutOfRangeException("Index must not exceed the length of array!");
                }

                this.Handler.Write(this.Address, index, value);
            }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TValue> GetEnumerator() => new enumerator(this);

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => this.ToString(4);

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="maxEntries">The maximum entries to show.</param>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString(int maxEntries)
        {
            if ( this.Address == IntPtr.Zero )
            {
                return "null";
            }

            var len = this.Length;
            var str = new StringBuilder(64);
            str.Append('[');
            str.Append(len.ToString());
            str.Append(']');

            if ( len > 0 )
            {
                str.Append(" {");

                if ( maxEntries == 0 )
                {
                    str.Append(" ...");
                }
                else
                {
                    for ( var i = 0; i < len; i++ )
                    {
                        if ( i > 0 )
                        {
                            str.Append(", ");
                        }

                        if ( i >= maxEntries )
                        {
                            str.Append("... ");
                            str.Append((len - maxEntries).ToString());
                            str.Append(" more entr");

                            if ( len - maxEntries == 1 )
                            {
                                str.Append("y");
                            }
                            else
                            {
                                str.Append("ies");
                            }

                            break;
                        }

                        var value = this[i];
                        var sx    = this.Handler.GetText(value);

                        if ( !string.IsNullOrEmpty(sx) )
                        {
                            str.Append(sx);
                        }
                    }
                }

                str.Append(" }");
            }

            return str.ToString();
        }

        /// <summary>
        ///     The enumerator implementation.
        /// </summary>
        /// <seealso cref="NetScriptFramework.MemoryArrayBase" />
        /// <seealso cref="System.Collections.Generic.IEnumerable{TValue}" />
        private sealed class enumerator : IEnumerator<TValue>
        {
            /// <summary>
            ///     The array.
            /// </summary>
            private readonly MemoryArray<TValue> Array;

            /// <summary>
            ///     The index.
            /// </summary>
            private int Index = -1;

            /// <summary>
            ///     Initializes a new instance of the <see cref="enumerator" /> class.
            /// </summary>
            /// <param name="array">The array.</param>
            internal enumerator(MemoryArray<TValue> array) => this.Array = array;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            public TValue Current => this.Array[this.Index];

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            object IEnumerator.Current => this.Current;

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose() { }

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of
            ///     the collection.
            /// </returns>
            public bool MoveNext() => ++this.Index < this.Array.Length;

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset() => this.Index = -1;
        }
    }

    /// <summary>
    ///     Fixed size array of objects.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="NetScriptFramework.MemoryArray{TValue}" />
    public sealed class FixedMemoryArray <TValue> : MemoryArray<TValue>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FixedMemoryArray{TValue}" /> class.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="address">The address.</param>
        /// <param name="length">The length.</param>
        /// <exception cref="System.ArgumentNullException">handler</exception>
        public FixedMemoryArray(MemoryArrayTypeHandler<TValue> handler, IntPtr address, int length)
        {
            if ( handler == null )
            {
                throw new ArgumentNullException("handler");
            }

            this.Address = address;
            this.Handler = handler;
            this.Length  = length;
        }

        /// <summary>
        ///     Gets the array handler.
        /// </summary>
        /// <value>
        ///     The handler.
        /// </value>
        public override MemoryArrayTypeHandler<TValue> Handler { get; }

        /// <summary>
        ///     Gets the length of array (count of entries).
        /// </summary>
        /// <value>
        ///     The length.
        /// </value>
        public override int Length { get; }
    }

    /// <summary>
    ///     Base type handler of array.
    /// </summary>
    public abstract class MemoryArrayTypeHandlerBase
    {
        /// <summary>
        ///     Gets the stride of one entry in the array.
        /// </summary>
        /// <value>
        ///     The stride.
        /// </value>
        public abstract int Stride { get; }
    }

    /// <summary>
    ///     Type handler for array.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="NetScriptFramework.MemoryArrayTypeHandlerBase" />
    public abstract class MemoryArrayTypeHandler <TValue> : MemoryArrayTypeHandlerBase
    {
        /// <summary>
        ///     Reads the specified entry from array.
        /// </summary>
        /// <param name="arrayBase">The array base.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public abstract TValue Read(IntPtr arrayBase, int index);

        /// <summary>
        ///     Writes the specified entry to array.
        /// </summary>
        /// <param name="arrayBase">The array base.</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public abstract void Write(IntPtr arrayBase, int index, TValue value);

        /// <summary>
        ///     Gets the text.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public abstract string GetText(TValue value);
    }
}

namespace NetScriptFramework.ArrayHandlers
{
    using System;
    using System.Globalization;

    public sealed class MemoryArrayTypeHandler_Bool : MemoryArrayTypeHandler<bool>
    {
        public override int Stride => 1;

        public override bool Read(IntPtr arrayBase, int index) => Memory.ReadUInt8(arrayBase + (index * this.Stride)) != 0;

        public override void Write(IntPtr arrayBase, int index, bool value) => Memory.WriteUInt8(arrayBase + (index * this.Stride), value ? (byte)1 : (byte)0);

        public override string GetText(bool value) => value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class MemoryArrayTypeHandler_UInt8 : MemoryArrayTypeHandler<byte>
    {
        public override int Stride => 1;

        public override byte Read(IntPtr arrayBase, int index) => Memory.ReadUInt8(arrayBase + (index * this.Stride));

        public override void Write(IntPtr arrayBase, int index, byte value) => Memory.WriteUInt8(arrayBase + (index * this.Stride), value);

        public override string GetText(byte value) => value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class MemoryArrayTypeHandler_Int8 : MemoryArrayTypeHandler<sbyte>
    {
        public override int Stride => 1;

        public override sbyte Read(IntPtr arrayBase, int index) => Memory.ReadInt8(arrayBase + (index * this.Stride));

        public override void Write(IntPtr arrayBase, int index, sbyte value) => Memory.WriteInt8(arrayBase + (index * this.Stride), value);

        public override string GetText(sbyte value) => value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class MemoryArrayTypeHandler_UInt16 : MemoryArrayTypeHandler<ushort>
    {
        public override int Stride => 2;

        public override ushort Read(IntPtr arrayBase, int index) => Memory.ReadUInt16(arrayBase + (index * this.Stride));

        public override void Write(IntPtr arrayBase, int index, ushort value) => Memory.WriteUInt16(arrayBase + (index * this.Stride), value);

        public override string GetText(ushort value) => value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class MemoryArrayTypeHandler_Int16 : MemoryArrayTypeHandler<short>
    {
        public override int Stride => 2;

        public override short Read(IntPtr arrayBase, int index) => Memory.ReadInt16(arrayBase + (index * this.Stride));

        public override void Write(IntPtr arrayBase, int index, short value) => Memory.WriteInt16(arrayBase + (index * this.Stride), value);

        public override string GetText(short value) => value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class MemoryArrayTypeHandler_UInt32 : MemoryArrayTypeHandler<uint>
    {
        public override int Stride => 4;

        public override uint Read(IntPtr arrayBase, int index) => Memory.ReadUInt32(arrayBase + (index * this.Stride));

        public override void Write(IntPtr arrayBase, int index, uint value) => Memory.WriteUInt32(arrayBase + (index * this.Stride), value);

        public override string GetText(uint value) => value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class MemoryArrayTypeHandler_Int32 : MemoryArrayTypeHandler<int>
    {
        public override int Stride => 4;

        public override int Read(IntPtr arrayBase, int index) => Memory.ReadInt32(arrayBase + (index * this.Stride));

        public override void Write(IntPtr arrayBase, int index, int value) => Memory.WriteInt32(arrayBase + (index * this.Stride), value);

        public override string GetText(int value) => value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class MemoryArrayTypeHandler_UInt64 : MemoryArrayTypeHandler<ulong>
    {
        public override int Stride => 8;

        public override ulong Read(IntPtr arrayBase, int index) => Memory.ReadUInt64(arrayBase + (index * this.Stride));

        public override void Write(IntPtr arrayBase, int index, ulong value) => Memory.WriteUInt64(arrayBase + (index * this.Stride), value);

        public override string GetText(ulong value) => value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class MemoryArrayTypeHandler_Int64 : MemoryArrayTypeHandler<long>
    {
        public override int Stride => 8;

        public override long Read(IntPtr arrayBase, int index) => Memory.ReadInt64(arrayBase + (index * this.Stride));

        public override void Write(IntPtr arrayBase, int index, long value) => Memory.WriteInt64(arrayBase + (index * this.Stride), value);

        public override string GetText(long value) => value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class MemoryArrayTypeHandler_Float : MemoryArrayTypeHandler<float>
    {
        public override int Stride => 4;

        public override float Read(IntPtr arrayBase, int index) => Memory.ReadFloat(arrayBase + (index * this.Stride));

        public override void Write(IntPtr arrayBase, int index, float value) => Memory.WriteFloat(arrayBase + (index * this.Stride), value);

        public override string GetText(float value) => value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class MemoryArrayTypeHandler_Double : MemoryArrayTypeHandler<double>
    {
        public override int Stride => 8;

        public override double Read(IntPtr arrayBase, int index) => Memory.ReadDouble(arrayBase + (index * this.Stride));

        public override void Write(IntPtr arrayBase, int index, double value) => Memory.WriteDouble(arrayBase + (index * this.Stride), value);

        public override string GetText(double value) => value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class MemoryArrayTypeHandler_Pointer : MemoryArrayTypeHandler<IntPtr>
    {
        public override int Stride => Main.Is64Bit ? 8 : 4;

        public override IntPtr Read(IntPtr arrayBase, int index) => Memory.ReadPointer(arrayBase + (index * this.Stride));

        public override void Write(IntPtr arrayBase, int index, IntPtr value) => Memory.WritePointer(arrayBase + (index * this.Stride), value);

        public override string GetText(IntPtr value) => value.ToHexString();
    }

    public sealed class MemoryArrayTypeHandler_StringPointer : MemoryArrayTypeHandler<string>
    {
        public override int Stride => Main.Is64Bit ? 8 : 4;

        public override string Read(IntPtr arrayBase, int index)
        {
            var ptr = Memory.ReadPointer(arrayBase + (index * this.Stride));

            if ( ptr == IntPtr.Zero )
            {
                return null;
            }

            return Memory.ReadString(ptr, false);
        }

        public override void Write(IntPtr arrayBase, int index, string value) => throw new InvalidOperationException("Writing string is not allowed!");

        public override string GetText(string value)
        {
            if ( value == null )
            {
                return "null";
            }

            return "\"" + value + "\"";
        }
    }

    public sealed class MemoryArrayTypeHandler_WStringPointer : MemoryArrayTypeHandler<string>
    {
        public override int Stride => Main.Is64Bit ? 8 : 4;

        public override string Read(IntPtr arrayBase, int index)
        {
            var ptr = Memory.ReadPointer(arrayBase + (index * this.Stride));

            if ( ptr == IntPtr.Zero )
            {
                return null;
            }

            return Memory.ReadString(ptr, true);
        }

        public override void Write(IntPtr arrayBase, int index, string value) => throw new InvalidOperationException("Writing string is not allowed!");

        public override string GetText(string value)
        {
            if ( value == null )
            {
                return "null";
            }

            return "\"" + value + "\"";
        }
    }

    public sealed class MemoryArrayTypeHandler_Struct <TValue> : MemoryArrayTypeHandler<TValue> where TValue : IMemoryObject
    {
        public MemoryArrayTypeHandler_Struct(int stride)
        {
            if ( stride < 0 )
            {
                throw new ArgumentOutOfRangeException("Unknown stride for type `" + typeof(TValue).Name + "`!");
            }

            this.Stride = stride;
        }

        public override int Stride { get; }

        public override TValue Read(IntPtr arrayBase, int index) => MemoryObject.FromAddress<TValue>(arrayBase + (index * this.Stride));

        public override void Write(IntPtr arrayBase, int index, TValue value)
        {
            var data = Memory.ReadBytes(value.Cast<TValue>(), this.Stride);
            Memory.WriteBytes(arrayBase + (index * this.Stride), data);
        }

        public override string GetText(TValue value) => value.ToString();
    }

    public sealed class MemoryArrayTypeHandler_Reference <TValue> : MemoryArrayTypeHandler<TValue> where TValue : IMemoryObject
    {
        public override int Stride => Main.Is64Bit ? 8 : 4;

        public override TValue Read(IntPtr arrayBase, int index) => MemoryObject.FromAddress<TValue>(Memory.ReadPointer(arrayBase + (index * this.Stride)));

        public override void Write(IntPtr arrayBase, int index, TValue value) => Memory.WritePointer(arrayBase + (index * this.Stride), value == null ? IntPtr.Zero : value.Cast<TValue>());

        public override string GetText(TValue value)
        {
            if ( value == null )
            {
                return "null";
            }

            return value.ToString();
        }
    }
}
