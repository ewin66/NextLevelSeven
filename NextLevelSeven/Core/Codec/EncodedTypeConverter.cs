﻿using System;
using System.Collections.Generic;
using NextLevelSeven.Conversion;

namespace NextLevelSeven.Core.Codec
{
    /// <summary>Provides HL7 value conversion.</summary>
    internal sealed class EncodedTypeConverter : IEncodedTypeConverter
    {
        /// <summary>Create a codec that references the specified element's data.</summary>
        /// <param name="baseElement">Element to reference.</param>
        public EncodedTypeConverter(IElement baseElement)
        {
            BaseElement = baseElement;
        }

        /// <summary>Referenced element.</summary>
        private IElement BaseElement { get; set; }

        /// <summary>Get or set the element's value as a date.</summary>
        public DateTime? AsDate
        {
            get { return DateTimeConverter.ConvertToDate(BaseElement.Value); }
            set { BaseElement.Value = DateTimeConverter.ConvertFromDate(value); }
        }

        /// <summary>Get the element's values as dates.</summary>
        public IIndexedEncodedTypeConverter<DateTime?> AsDates
        {
            get
            {
                return new IndexedEncodedTypeConverter<DateTime?>(BaseElement, DateTimeConverter.ConvertToDate,
                    DateTimeConverter.ConvertFromDate);
            }
        }

        /// <summary>Get or set the element's value as a date/time.</summary>
        public DateTimeOffset? AsDateTime
        {
            get { return DateTimeConverter.ConvertToDateTime(BaseElement.Value); }
            set { BaseElement.Value = DateTimeConverter.ConvertFromDateTime(value); }
        }

        /// <summary>Get the element's values as date/times.</summary>
        public IIndexedEncodedTypeConverter<DateTimeOffset?> AsDateTimes
        {
            get
            {
                return new IndexedEncodedTypeConverter<DateTimeOffset?>(BaseElement, DateTimeConverter.ConvertToDateTime,
                    DateTimeConverter.ConvertFromDateTime);
            }
        }

        /// <summary>Get or set the element's value as a decimal.</summary>
        public decimal? AsDecimal
        {
            get { return NumberConverter.ConvertToDecimal(BaseElement.Value); }
            set { BaseElement.Value = NumberConverter.ConvertFromDecimal(value); }
        }

        /// <summary>Get the element's values as decimals.</summary>
        public IIndexedEncodedTypeConverter<decimal?> AsDecimals
        {
            get
            {
                return new IndexedEncodedTypeConverter<decimal?>(BaseElement, NumberConverter.ConvertToDecimal,
                    NumberConverter.ConvertFromDecimal);
            }
        }

        /// <summary>Get or set the element's value as formatted text.</summary>
        public IEnumerable<string> AsFormattedText
        {
            get
            {
                return TextConverter.ConvertToFormattedText(BaseElement.Value, BaseElement.Delimiter,
                    BaseElement.Encoding);
            }
            set
            {
                BaseElement.Value = TextConverter.ConvertFromFormattedText(value, BaseElement.Delimiter,
                    BaseElement.Encoding);
            }
        }

        /// <summary>Get or set the element's value as an integer.</summary>
        public int? AsInt
        {
            get { return NumberConverter.ConvertToInt(BaseElement.Value); }
            set { BaseElement.Value = NumberConverter.ConvertFromInt(value); }
        }

        /// <summary>Get the element's values as integers.</summary>
        public IIndexedEncodedTypeConverter<int?> AsInts
        {
            get
            {
                return new IndexedEncodedTypeConverter<int?>(BaseElement, NumberConverter.ConvertToInt,
                    NumberConverter.ConvertFromInt);
            }
        }

        /// <summary>Get or set the element's value as a string.</summary>
        public string AsString
        {
            get { return TextConverter.ConvertToString(BaseElement.Value); }
            set { BaseElement.Value = TextConverter.ConvertFromString(value); }
        }

        /// <summary>Get the element's values as strings.</summary>
        public IIndexedEncodedTypeConverter<string> AsStrings
        {
            get
            {
                return new IndexedEncodedTypeConverter<string>(BaseElement, TextConverter.ConvertToString,
                    TextConverter.ConvertFromString);
            }
        }

        /// <summary>Get or set the element's value as a text field.</summary>
        public string AsTextField
        {
            get
            {
                return string.Join(Environment.NewLine,
                    TextConverter.ConvertToFormattedText(BaseElement.Value, BaseElement.Delimiter, BaseElement.Encoding));
            }
            set
            {
                BaseElement.Value =
                    TextConverter.ConvertFromFormattedText(value.Replace(Environment.NewLine, "\xD").Split('\xD'),
                        BaseElement.Delimiter, BaseElement.Encoding);
            }
        }

        /// <summary>Get or set the element's value as a time.</summary>
        public TimeSpan? AsTime
        {
            get { return DateTimeConverter.ConvertToTime(BaseElement.Value); }
            set { BaseElement.Value = DateTimeConverter.ConvertFromTime(value); }
        }

        /// <summary>Get the element's values as times.</summary>
        public IIndexedEncodedTypeConverter<TimeSpan?> AsTimes
        {
            get
            {
                return new IndexedEncodedTypeConverter<TimeSpan?>(BaseElement, DateTimeConverter.ConvertToTime,
                    DateTimeConverter.ConvertFromTime);
            }
        }
    }
}