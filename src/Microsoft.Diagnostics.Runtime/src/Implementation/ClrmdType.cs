﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Diagnostics.Runtime.Builders;
using Microsoft.Diagnostics.Runtime.DacInterface;
using Microsoft.Diagnostics.Runtime.Utilities;

namespace Microsoft.Diagnostics.Runtime.Implementation
{
    public class ClrmdType : ClrType
    {
        protected ITypeHelpers Helpers { get; }
        protected IDataReader DataReader => Helpers.DataReader;

        private string? _name;
        private TypeAttributes _attributes;
        private ulong _loaderAllocatorHandle = ulong.MaxValue - 1;

        private IReadOnlyList<ClrMethod>? _methods;
        private IReadOnlyList<ClrInstanceField>? _fields;
        private IReadOnlyList<ClrStaticField>? _statics;

        private EnumData? _enumData;
        private ClrElementType _elementType;
        private GCDesc _gcDesc;

        public override string? Name
        {
            get
            {
                if (_name != null)
                    return _name;

                if (Helpers.GetTypeName(MethodTable, out string? name))
                    return _name = FixGenerics(name);

                return FixGenerics(name);
            }
        }

        public override int StaticSize { get; }
        public override int ComponentSize => 0;
        public override ClrType? ComponentType => null;
        public override ClrModule? Module { get; }
        public override GCDesc GCDesc => GetOrCreateGCDesc();

        public override ClrElementType ElementType => GetElementType();
        public bool Shared { get; }
        public override IClrObjectHelpers ClrObjectHelpers => Helpers.ClrObjectHelpers;

        public override ulong MethodTable { get; }
        public override ClrHeap Heap { get; }

        public override ClrType? BaseType { get; }

        public override bool ContainsPointers { get; }
        public override bool IsShared { get; }

        public ClrmdType(ClrHeap heap, ClrType? baseType, ClrModule? module, ITypeData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            Helpers = data.Helpers;
            MethodTable = data.MethodTable;
            Heap = heap;
            BaseType = baseType;
            Module = module;
            MetadataToken = data.Token;
            Shared = data.IsShared;
            StaticSize = data.BaseSize;
            ContainsPointers = data.ContainsPointers;
            IsShared = data.IsShared;

            // If there are no methods, preempt the expensive work to create methods
            if (data.MethodCount == 0)
                _methods = Array.Empty<ClrMethod>();

            DebugOnlyLoadLazyValues();
        }

        [Conditional("DEBUG")]
        private void DebugOnlyLoadLazyValues()
        {
            _ = Name;
        }

        private GCDesc GetOrCreateGCDesc()
        {
            if (!ContainsPointers || !_gcDesc.IsEmpty)
                return _gcDesc;

            IDataReader reader = Helpers.DataReader;
            if (reader is null)
                return default;

            DebugOnly.Assert(MethodTable != 0, "Attempted to fill GC desc with a constructed (not real) type.");
            if (!reader.Read(MethodTable - (ulong)IntPtr.Size, out int entries))
            {
                _gcDesc = default;
                return default;
            }

            // Get entries in map
            if (entries < 0)
                entries = -entries;

            int slots = 1 + entries * 2;
            byte[] buffer = new byte[slots * IntPtr.Size];
            if (!reader.ReadMemory(MethodTable - (ulong)(slots * IntPtr.Size), buffer, out int read) || read != buffer.Length)
            {
                _gcDesc = default;
                return default;
            }

            // Construct the gc desc
            return _gcDesc = new GCDesc(buffer);
        }

        public override uint MetadataToken { get; }

        public override IEnumerable<ClrInterface> EnumerateInterfaces()
        {
            MetaDataImport? import = Module?.MetadataImport;
            if (import != null)
            {
                foreach (int token in import.EnumerateInterfaceImpls(MetadataToken))
                {
                    if (import.GetInterfaceImplProps(token, out _, out int mdIFace))
                    {
                        ClrInterface? result = GetInterface(import, mdIFace);
                        if (result != null)
                            yield return result;
                    }
                }
            }
        }

        private ClrInterface? GetInterface(MetaDataImport import, int mdIFace)
        {
            ClrInterface? result = null;
            if (!import.GetTypeDefProperties(mdIFace, out string? name, out _, out int extends))
            {
                name = import.GetTypeRefName(mdIFace);
            }

            // TODO:  Handle typespec case.
            if (name != null)
            {
                ClrInterface? type = null;
                if (extends != 0 && extends != 0x01000000)
                    type = GetInterface(import, extends);

                result = new ClrInterface(name, type);
            }

            return result;
        }

        private ClrElementType GetElementType()
        {
            if (_elementType != ClrElementType.Unknown)
                return _elementType;

            if (this == Heap.ObjectType)
                return _elementType = ClrElementType.Object;

            if (this == Heap.StringType)
                return _elementType = ClrElementType.String;
            if (ComponentSize > 0)
                return _elementType = ClrElementType.SZArray;

            ClrType? baseType = BaseType;
            if (baseType is null || baseType == Heap.ObjectType)
                return _elementType = ClrElementType.Object;

            if (baseType.Name != "System.ValueType")
            {
                ClrElementType et = baseType.ElementType;
                return _elementType = et;
            }

            switch (Name)
            {
                case "System.Int32":
                    return _elementType = ClrElementType.Int32;
                case "System.Int16":
                    return _elementType = ClrElementType.Int16;
                case "System.Int64":
                    return _elementType = ClrElementType.Int64;
                case "System.IntPtr":
                    return _elementType = ClrElementType.NativeInt;
                case "System.UInt16":
                    return _elementType = ClrElementType.UInt16;
                case "System.UInt32":
                    return _elementType = ClrElementType.UInt32;
                case "System.UInt64":
                    return _elementType = ClrElementType.UInt64;
                case "System.UIntPtr":
                    return _elementType = ClrElementType.NativeUInt;
                case "System.Boolean":
                    return _elementType = ClrElementType.Boolean;
                case "System.Single":
                    return _elementType = ClrElementType.Float;
                case "System.Double":
                    return _elementType = ClrElementType.Double;
                case "System.Byte":
                    return _elementType = ClrElementType.UInt8;
                case "System.Char":
                    return _elementType = ClrElementType.Char;
                case "System.SByte":
                    return _elementType = ClrElementType.Int8;
                case "System.Enum":
                    return _elementType = ClrElementType.Int32;
                default:
                    break;
            }

            return _elementType = ClrElementType.Struct;
        }

        public override bool IsException
        {
            get
            {
                ClrType? type = this;
                while (type != null)
                    if (type == Heap.ExceptionType)
                        return true;
                    else
                        type = type.BaseType;

                return false;
            }
        }

        // TODO:  Add ClrObject GetCcw/GetRcw
        // TODO:  Move out of ClrType.
        public override ComCallWrapper? GetCCWData(ulong obj) => Helpers.Factory.CreateCCWForObject(obj);
        public override RuntimeCallableWrapper? GetRCWData(ulong obj) => Helpers.Factory.CreateRCWForObject(obj);

        private class EnumData
        {
            internal ClrElementType ElementType;
            internal readonly Dictionary<string, object> NameToValue = new Dictionary<string, object>();
            internal readonly Dictionary<object, string> ValueToName = new Dictionary<object, string>();
        }

        public override bool TryGetEnumValue(string name, out int value)
        {
            if (TryGetEnumValue(name, out object val))
            {
                value = (int)val;
                return true;
            }

            value = int.MinValue;
            return false;
        }

        public override bool TryGetEnumValue(string name, out object value)
        {
            if (_enumData is null)
                InitEnumData();

            return _enumData!.NameToValue.TryGetValue(name, out value);
        }

        public override bool IsEnum
        {
            get
            {
                for (ClrType? type = this; type != null; type = type.BaseType)
                    if (type.Name == "System.Enum")
                        return true;

                return false;
            }
        }

        public override string GetEnumName(object value)
        {
            if (_enumData is null)
                InitEnumData();

            _enumData!.ValueToName.TryGetValue(value, out string result);
            return result;
        }

        public override string GetEnumName(int value)
        {
            return GetEnumName((object)value);
        }

        public override IEnumerable<string> GetEnumNames()
        {
            if (_enumData is null)
                InitEnumData();

            return _enumData!.NameToValue.Keys;
        }

        private void InitEnumData()
        {
            if (!IsEnum)
                throw new InvalidOperationException("Type is not an Enum.");

            _enumData = new EnumData();
            MetaDataImport? import = Module?.MetadataImport;
            if (import is null)
                return;

            List<string?> names = new List<string?>();
            foreach (uint token in import.EnumerateFields((int)MetadataToken))
            {
                if (import.GetFieldProps(token, out string? name, out FieldAttributes attr, out IntPtr ppvSigBlob, out int pcbSigBlob, out int pdwCPlusTypeFlag, out IntPtr ppValue))
                {
                    if ((int)attr == 0x606 && name == "value__")
                    {
                        SigParser parser = new SigParser(ppvSigBlob, pcbSigBlob);
                        if (parser.GetCallingConvInfo(out _) && parser.GetElemType(out int elemType))
                            _enumData.ElementType = (ClrElementType)elemType;
                    }

                    // public, static, literal, has default
                    if ((int)attr == 0x8056)
                    {
                        names.Add(name);

                        SigParser parser = new SigParser(ppvSigBlob, pcbSigBlob);
                        parser.GetCallingConvInfo(out _);
                        parser.GetElemType(out _);

                        Type? type = ((ClrElementType)pdwCPlusTypeFlag).GetTypeForElementType();
                        if (type != null)
                        {
                            object o = Marshal.PtrToStructure(ppValue, type);
                            if (name != null)
                            {
                                _enumData.NameToValue[name] = o;
                                _enumData.ValueToName[o] = name;
                            }
                        }
                    }
                }
            }
        }

        public override bool IsFree => this == Heap.FreeType;

        private const uint FinalizationSuppressedFlag = 0x40000000;

        public override bool IsFinalizeSuppressed(ulong obj)
        {
            // TODO move to ClrObject?
            uint value = Helpers.DataReader.ReadUnsafe<uint>(obj - 4);

            return (value & FinalizationSuppressedFlag) == FinalizationSuppressedFlag;
        }

        public override bool IsFinalizable => Methods.Any(method => method.IsVirtual && method.Name == "Finalize");

        public override bool IsArray => ComponentSize != 0 && !IsString && !IsFree;
        public override bool IsCollectible => LoaderAllocatorHandle != 0;

        public override ulong LoaderAllocatorHandle
        {
            get
            {
                if (_loaderAllocatorHandle != ulong.MaxValue - 1)
                    return _loaderAllocatorHandle;

                ulong handle = Helpers.GetLoaderAllocatorHandle(MethodTable);
                _loaderAllocatorHandle = handle;
                return handle;
            }
        }

        public override bool IsString => this == Heap.StringType;

        public override IReadOnlyList<ClrInstanceField> Fields
        {
            get
            {
                if (_fields != null)
                    return _fields;

                if (Helpers.Factory.CreateFieldsForType(this, out IReadOnlyList<ClrInstanceField> fields, out IReadOnlyList<ClrStaticField> statics))
                {
                    _fields = fields;
                    _statics = statics;
                }

                return fields;
            }
        }

        public override IReadOnlyList<ClrStaticField> StaticFields
        {
            get
            {
                if (_statics != null)
                    return _statics;

                if (Helpers.Factory.CreateFieldsForType(this, out IReadOnlyList<ClrInstanceField> fields, out IReadOnlyList<ClrStaticField> statics))
                {
                    _fields = fields;
                    _statics = statics;
                }

                return statics;
            }
        }

        public override IReadOnlyList<ClrMethod> Methods
        {
            get
            {
                if (_methods != null)
                    return _methods;

                // Returns whether or not we should cache methods or not
                if (Helpers.Factory.CreateMethodsForType(this, out IReadOnlyList<ClrMethod> methods))
                    _methods = methods;

                return methods;
            }

        }

        //TODO: remove
        public override ClrStaticField? GetStaticFieldByName(string name) => StaticFields.FirstOrDefault(f => f.Name == name);

        //TODO: remove
        public override ClrInstanceField? GetFieldByName(string name) => Fields.FirstOrDefault(f => f.Name == name);

        public override ulong GetArrayElementAddress(ulong objRef, int index) => throw new InvalidOperationException($"{Name} is not an array.");
        public override object? GetArrayElementValue(ulong objRef, int index) => throw new InvalidOperationException($"{Name} is not an array.");

        // convenience function for testing
        public static string? FixGenerics(string? name) => RuntimeBuilder.FixGenerics(name);

        private void InitFlags()
        {
            if (_attributes != 0 || Module is null)
                return;

            MetaDataImport? import = Module?.MetadataImport;
            if (import is null)
            {
                _attributes = (TypeAttributes)0x70000000;
                return;
            }

            if (!import.GetTypeDefAttributes((int)MetadataToken, out _attributes) || _attributes == 0)
                _attributes = (TypeAttributes)0x70000000;
        }

        public override bool IsInternal
        {
            get
            {
                if (_attributes == 0)
                    InitFlags();

                TypeAttributes visibility = _attributes & TypeAttributes.VisibilityMask;
                return visibility == TypeAttributes.NestedAssembly || visibility == TypeAttributes.NotPublic;
            }
        }

        public override bool IsPublic
        {
            get
            {
                if (_attributes == 0)
                    InitFlags();

                TypeAttributes visibility = _attributes & TypeAttributes.VisibilityMask;
                return visibility == TypeAttributes.Public || visibility == TypeAttributes.NestedPublic;
            }
        }

        public override bool IsPrivate
        {
            get
            {
                if (_attributes == 0)
                    InitFlags();

                TypeAttributes visibility = _attributes & TypeAttributes.VisibilityMask;
                return visibility == TypeAttributes.NestedPrivate;
            }
        }

        public override bool IsProtected
        {
            get
            {
                if (_attributes == 0)
                    InitFlags();

                TypeAttributes visibility = _attributes & TypeAttributes.VisibilityMask;
                return visibility == TypeAttributes.NestedFamily;
            }
        }

        public override bool IsAbstract
        {
            get
            {
                if (_attributes == 0)
                    InitFlags();

                return (_attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract;
            }
        }

        public override bool IsSealed
        {
            get
            {
                if (_attributes == 0)
                    InitFlags();

                return (_attributes & TypeAttributes.Sealed) == TypeAttributes.Sealed;
            }
        }

        public override bool IsInterface
        {
            get
            {
                if (_attributes == 0)
                    InitFlags();
                return (_attributes & TypeAttributes.Interface) == TypeAttributes.Interface;
            }
        }
    }
}