﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NextLevelSeven.Codecs;
using NextLevelSeven.Core;
using NextLevelSeven.Cursors.Dividers;
using NextLevelSeven.Diagnostics;
using NextLevelSeven.Native;

namespace NextLevelSeven.Cursors
{
    /// <summary>
    ///     Represents a generic HL7 message element, which may contain other elements.
    /// </summary>
    internal abstract class Element : INativeElement, IEquatable<string>
    {
        private IStringDivider _descendantDivider;
        private bool _descendantDividerInitialized;

        protected Element(string value)
        {
            Index = 0;
            ParentIndex = 0;
            _descendantDivider = GetDescendantDividerRoot(value);
            Ancestor = null;
        }

        protected Element(Element ancestor, int parentIndex, int externalIndex)
        {
            Index = externalIndex;
            ParentIndex = parentIndex;
            Ancestor = ancestor;
        }

        protected Element Ancestor { get; private set; }

        public IStringDivider DescendantDivider
        {
            get
            {
                if (_descendantDivider == null && !_descendantDividerInitialized)
                {
                    _descendantDividerInitialized = true;
                    _descendantDivider = (Ancestor == null)
                        ? GetDescendantDividerRoot(string.Empty)
                        : GetDescendantDivider(Ancestor, ParentIndex);
                }
                return _descendantDivider;
            }
        }

        public abstract EncodingConfiguration EncodingConfiguration { get; }
        protected int ParentIndex { get; set; }
        public event EventHandler ValueChanged;

        public INativeElement this[int index]
        {
            get { return GetDescendant(index); }
        }

        public INativeElement AncestorElement
        {
            get { return Ancestor; }
        }

        public ICodec As
        {
            get { return new Codec(this); }
        }

        public abstract INativeElement CloneDetached();

        public void Delete()
        {
            if (Ancestor == null)
            {
                throw new ElementException(ErrorCode.AncestorDoesNotExist);
            }
            Ancestor.DescendantDivider.Delete(ParentIndex);
        }

        public abstract char Delimiter { get; }

        public virtual int DescendantCount
        {
            get { return DescendantDivider.Count; }
        }

        public IEnumerable<INativeElement> DescendantElements
        {
            get { return new ElementEnumerable(this); }
        }

        public void Erase()
        {
            // TODO: actually mark as nonexistant
            Nullify();
        }

        public bool Exists
        {
            get
            {
                if (Ancestor == null)
                {
                    return true;
                }
                return (Index <= Ancestor.DescendantCount);
            }
        }

        public virtual bool HasSignificantDescendants
        {
            get
            {
                if (!Exists || DescendantCount == 0 || Delimiter == '\0')
                {
                    return false;
                }

                return (DescendantCount > 1) || DescendantElements.Any(d => d.HasSignificantDescendants);
            }
        }

        public int Index { get; set; }

        public virtual string Key
        {
            get
            {
                return (Ancestor != null)
                    ? String.Join(Ancestor.Key, ".", Index.ToString(CultureInfo.InvariantCulture))
                    : Index.ToString(CultureInfo.InvariantCulture);
            }
        }

        public INativeMessage Message
        {
            get
            {
                if (Ancestor is Message)
                {
                    return new NativeMessage(Ancestor as Message);
                }

                return (Ancestor != null)
                    ? Ancestor.Message
                    : null;
            }
        }

        public void Nullify()
        {
            Value = null;
        }

        public virtual string Value
        {
            get
            {
                if (DescendantDivider == null)
                {
                    return null;
                }

                var value = DescendantDivider.Value;
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }

                if (string.Equals("\"\"", value, StringComparison.Ordinal))
                {
                    return null;
                }

                return value;
            }
            set
            {
                DescendantDivider.Value = value;

                if (ValueChanged != null)
                {
                    ValueChanged(this, EventArgs.Empty);
                }
            }
        }

        public string[] Values
        {
            get
            {
                return (DescendantCount > 1)
                    ? DescendantDivider.Value.Split(DescendantDivider.Delimiter)
                    : new[] {DescendantDivider.Value};
            }
            set
            {
                DescendantDivider.Value = (DescendantDivider.Delimiter != '\0')
                    ? string.Join(new string(DescendantDivider.Delimiter, 1), value)
                    : string.Join(string.Empty, value);
            }
        }

        public bool Equals(string other)
        {
            return ToString() == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj.ToString() == ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static implicit operator string(Element element)
        {
            return element.ToString();
        }

        public abstract INativeElement GetDescendant(int index);

        protected virtual IStringDivider GetDescendantDivider(Element ancestor, int index)
        {
            return new StringSubDivider(ancestor.DescendantDivider, Delimiter, index);
        }

        private IStringDivider GetDescendantDividerRoot(string value)
        {
            return new StringDivider(value, Delimiter);
        }

        public override string ToString()
        {
            return DescendantDivider.Value;
        }
    }
}