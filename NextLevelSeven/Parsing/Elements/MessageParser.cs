﻿using System;
using System.Collections.Generic;
using System.Linq;
using NextLevelSeven.Core;
using NextLevelSeven.Core.Encoding;
using NextLevelSeven.Core.Properties;
using NextLevelSeven.Diagnostics;
using NextLevelSeven.Utility;

namespace NextLevelSeven.Parsing.Elements
{
    /// <summary>Represents a textual HL7v2 message.</summary>
    internal sealed class MessageParser : Parser, IMessageParser
    {
        /// <summary>Segment cache.</summary>
        private readonly IndexedCache<int, SegmentParser> _segments;

        /// <summary>Create a message with a default MSH segment.</summary>
        public MessageParser()
        {
            _segments = new IndexedCache<int, SegmentParser>(CreateSegment);
            Value = @"MSH|^~\&|";
        }

        /// <summary>Get the segment delimiter.</summary>
        public override char Delimiter
        {
            get { return '\xD'; }
        }

        /// <summary>Get the root message for this element.</summary>
        ISegmentParser IMessageParser.this[int index]
        {
            get { return _segments[index]; }
        }

        /// <summary>Check for validity of the message. Returns true if the message can reasonably be parsed.</summary>
        /// <returns>True if the message can be parsed, false otherwise.</returns>
        public bool Validate()
        {
            var value = Value;
            return value != null && value.StartsWith("MSH");
        }

        /// <summary>Get data from a specific place in the message. Depth is determined by how many indices are specified.</summary>
        /// <param name="segment">Segment index.</param>
        /// <param name="field">Field index.</param>
        /// <param name="repetition">Repetition number.</param>
        /// <param name="component">Component index.</param>
        /// <param name="subcomponent">Subcomponent index.</param>
        /// <returns>The occurrences of the specified element.</returns>
        public IEnumerable<string> GetValues(int segment = -1, int field = -1, int repetition = -1, int component = -1,
            int subcomponent = -1)
        {
            return segment < 0 ? Values : _segments[segment].GetValues(field, repetition, component, subcomponent);
        }

        /// <summary>Get data from a specific place in the message. Depth is determined by how many indices are specified.</summary>
        /// <param name="segment">Segment index.</param>
        /// <param name="field">Field index.</param>
        /// <param name="repetition">Repetition number.</param>
        /// <param name="component">Component index.</param>
        /// <param name="subcomponent">Subcomponent index.</param>
        /// <returns>The first occurrence of the specified element.</returns>
        public string GetValue(int segment = -1, int field = -1, int repetition = -1, int component = -1,
            int subcomponent = -1)
        {
            return segment < 0 ? Value : _segments[segment].GetValue(field, repetition, component, subcomponent);
        }

        /// <summary>Deep clone this message.</summary>
        /// <returns>Clone of the message.</returns>
        public override IElement Clone()
        {
            return CloneInternal();
        }

        /// <summary>Deep clone this message.</summary>
        /// <returns>Clone of the message.</returns>
        IMessage IMessage.Clone()
        {
            return CloneInternal();
        }

        /// <summary>Access message details as a property set.</summary>
        public IMessageDetails Details
        {
            get { return new MessageDetails(this); }
        }

        /// <summary>Get all segments.</summary>
        public IEnumerable<ISegmentParser> Segments
        {
            get
            {
                var count = ValueCount;
                for (var i = 1; i <= count; i++)
                {
                    yield return _segments[i];
                }
            }
        }

        /// <summary>Get all segments.</summary>
        IEnumerable<ISegment> IMessage.Segments
        {
            get { return Segments; }
        }

        /// <summary>Get or set the value of this message.</summary>
        public override string Value
        {
            get { return base.Value; }
            set
            {
                if (value == null)
                {
                    throw new ParserException(ErrorCode.MessageDataMustNotBeNull);
                }
                if (value.Length < 8)
                {
                    throw new ParserException(ErrorCode.MessageDataIsTooShort);
                }
                if (!value.StartsWith("MSH"))
                {
                    throw new ParserException(ErrorCode.MessageDataMustStartWithMsh);
                }
                base.Value = SanitizeLineEndings(value);
            }
        }

        /// <summary>Get descendant element.</summary>
        /// <param name="index">Index of the element.</param>
        /// <returns></returns>
        protected override IElementParser GetDescendant(int index)
        {
            return _segments[index];
        }

        /// <summary>Create a segment object.</summary>
        /// <param name="index">Desired index.</param>
        /// <returns>Segment object.</returns>
        private SegmentParser CreateSegment(int index)
        {
            if (index < 1)
            {
                throw new ParserException(ErrorCode.SegmentIndexMustBeGreaterThanZero);
            }

            var result = new SegmentParser(this, index - 1, index);
            return result;
        }

        /// <summary>Determines whether this object is equivalent to another object.</summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>True, if objects are considered to be equivalent.</returns>
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
            return SanitizeLineEndings(obj.ToString()) == ToString();
        }

        /// <summary>Get the hash code for this element.</summary>
        /// <returns>Hash code of the value's string.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>Change all system line endings to HL7 line endings.</summary>
        /// <param name="message">String to transform.</param>
        /// <returns>Sanitized string.</returns>
        private static string SanitizeLineEndings(string message)
        {
            return message == null
                ? null
                : message.Replace(Environment.NewLine, EncodingConfiguration.SegmentDelimiterString);
        }

        /// <summary>Deep clone this message.</summary>
        /// <returns>Clone of the message.</returns>
        private MessageParser CloneInternal()
        {
            return new MessageParser
            {
                Value = Value,
                Index = Index
            };
        }
    }
}