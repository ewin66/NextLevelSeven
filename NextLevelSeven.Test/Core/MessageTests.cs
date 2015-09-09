﻿using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextLevelSeven.Core;
using NextLevelSeven.Native;

namespace NextLevelSeven.Test.Core
{
    [TestClass]
    public class MessageTests
    {
        [TestInitialize]
        public void Message_Initialization()
        {
            var message = new NativeMessage(ExampleMessages.Standard);
            Assert.IsNotNull(message, "Message was not initialized.");
            Debug.WriteLine(message.Type);
        }

        [TestMethod]
        public void Message_ConvertsMshCorrectly()
        {
            var message = new NativeMessage(ExampleMessages.MshOnly);
            Assert.AreEqual(ExampleMessages.MshOnly, message.ToString(), "MSH conversion back to string did not match.");
        }

        [TestMethod]
        public void Message_ReturnsBasicMessage()
        {
            var message = new NativeMessage();
            Assert.AreEqual(1, message.DescendantCount, @"Default message should not contain multiple segments.");
            Assert.AreEqual("MSH", message[1].Type, @"Default message should create an MSH segment.");
            Assert.AreEqual(@"^~\&", message[1][2].Value, @"Default message should use standard HL7 encoding characters.");
            Assert.AreEqual("|", message[1][1].Value, @"Default message should use standard HL7 field delimiter character.");
        }

        [TestMethod]
        public void Message_EmptyConstructorMatchesStaticEmptyConstructor()
        {
            Assert.AreEqual(new NativeMessage().ToString(), NativeMessage.Create().ToString());
        }

        [TestMethod]
        public void Message_StringConstructorMatchesStaticStringConstructor()
        {
            Assert.AreEqual(new NativeMessage(ExampleMessages.Standard).ToString(), NativeMessage.Create(ExampleMessages.Standard).ToString());
        }

        [TestMethod]
        public void Message_ThrowsOnNullData()
        {
            It.Throws<MessageException>(() => NativeMessage.Create(null));
        }

        [TestMethod]
        public void Message_ThrowsOnEmptyData()
        {
            It.Throws<MessageException>(() => NativeMessage.Create(string.Empty));
        }

        [TestMethod]
        public void Message_ThrowsOnShortData()
        {
            It.Throws<MessageException>(() => NativeMessage.Create("MSH|123"));
        }

        [TestMethod]
        public void Message_CanRetrieveMessageTypeAndTriggerEvent()
        {
            var message = new NativeMessage(ExampleMessages.Standard);
            Assert.AreEqual("ADT", message.Type, "Message type is incorrect.");
            Assert.AreEqual("A17", message.TriggerEvent, "Message trigger event is incorrect.");
        }

        [TestMethod]
        public void Message_CanParseMessageDate()
        {
            var message = new NativeMessage(ExampleMessages.Standard);
            Assert.IsTrue(message.Time.HasValue, "Parsed message date is incorrect.");
            Assert.AreEqual(new DateTime(2013, 05, 28, 07, 38, 29), message.Time.Value.DateTime);
        }

        [TestMethod]
        public void Message_CanRetrieveMessageVersion()
        {
            var message = new NativeMessage(ExampleMessages.Standard);
            Assert.AreEqual("2.3", message.Version, "Message version is incorrect.");
        }

        [TestMethod]
        public void Message_CanRetrievePatientId()
        {
            var message = new NativeMessage(ExampleMessages.Standard);
            var pid = message["PID"].First();
            Assert.AreEqual("Colon", pid[5][0][1].Value, "Patient name is incorrect.");
        }

        [TestMethod]
        public void Message_CanRetrieveMultipleSegments()
        {
            var message = new NativeMessage(ExampleMessages.Standard);
            Assert.AreEqual(3, message["OBX"].Count(), "Incorrect number of segments were found.");
        }

        [TestMethod]
        public void Message_CanRetrieveRepetitions()
        {
            var message = new NativeMessage(ExampleMessages.RepeatingName);
            var pid = message["PID"].First();
            Assert.AreEqual("Lincoln^Abe~Bro~Dude", pid[5][0].Value, "Retrieving full field data using index 0 returned incorrect data.");
            Assert.AreEqual("Lincoln^Abe", pid[5][1].Value, "Retrieving first repetition returned incorrect data.");
            Assert.AreEqual("Bro", pid[5][2].Value, "Retrieving second repetition returned incorrect data.");
            Assert.AreEqual("Dude", pid[5][3].Value, "Retrieving third repetition returned incorrect data.");
        }

        [TestMethod]
        public void Message_RetrievalMethodsAreIdentical()
        {
            var message = new NativeMessage(ExampleMessages.Standard);
            Assert.AreEqual(message.GetField(1).Value, message[1].Value, "Retrieval methods differ at the segment level.");
            Assert.AreEqual(message.GetField(1, 2).Value, message[1][2].Value, "Retrieval methods differ at the field level.");
            Assert.AreEqual(message.GetField(1, 2, 0).Value, message[1][2][0].Value, "Retrieval methods differ at the repetition level.");
            Assert.AreEqual(message.GetField(1, 2, 0, 1).Value, message[1][2][0][1].Value, "Retrieval methods differ at the component level.");
        }

        [TestMethod]
        public void Message_HasUniqueDescendantKeys()
        {
            var message = new NativeMessage(ExampleMessages.Standard);
            var keys = message.Segments.Select(s => s.Key).ToList();
            var distinctKeys = keys.Distinct();

            foreach (var key in keys)
            {
                Debug.WriteLine(key);
            }
            Assert.AreEqual(distinctKeys.Count(), message.Segments.Count(), "Segments don't have entirely unique keys.");
        }

        [TestMethod]
        public void Message_HasUniqueKeys()
        {
            var message1 = new NativeMessage(ExampleMessages.Standard);
            var message2 = new NativeMessage(ExampleMessages.Standard);
            Assert.AreNotEqual(message1.Key, message2.Key);
        }

        [TestMethod]
        public void Message_CanBeCloned()
        {
            var message = new NativeMessage(ExampleMessages.Standard);
            var clone = message.Clone();
            Assert.AreNotSame(message, clone, "Cloned message is the same referenced object.");
            Assert.AreEqual(message.Value, clone.Value, "Cloned message has different contents.");
        }

        [TestMethod]
        public void Message_WithIdentivalValueToAnotherMessage_IsEquivalent()
        {
            var message1 = new NativeMessage(ExampleMessages.Standard);
            var message2 = new NativeMessage(ExampleMessages.Standard);
            Assert.AreEqual(message1, message2);
        }

        [TestMethod]
        public void Message_WhenCreatedUsingString_IsEquivalentToTheString()
        {
            var message = new NativeMessage(ExampleMessages.Standard);
            Assert.AreEqual(message, ExampleMessages.Standard);
        }

        [TestMethod]
        public void Message_WithOnlyOneSegment_WillClaimToHaveSignificantDescendants()
        {
            var message = new NativeMessage();
            Assert.IsTrue(message.HasSignificantDescendants,
                @"Message should claim to have significant descendants if any segments do.");
        }

        [TestMethod]
        public void Message_UsesReasonableMemory_WhenParsingLargeMessages()
        {
            var before = GC.GetTotalMemory(true);
            var message = new NativeMessage();
            message[1000000][1000000].Value = Randomized.String();
            var messageString = message.ToString();
            var usage = GC.GetTotalMemory(false) - before;
            var overhead = usage - (messageString.Length << 1);
            var usePerCharacter = (overhead / (messageString.Length << 1));
            Assert.IsTrue(usePerCharacter < 20);
        }

        [TestMethod]
        public void Message_CanMapSegments()
        {
            var id = Randomized.String();
            IMessage tree = new NativeMessage(string.Format("MSH|^~\\&|{0}", id));
            Assert.AreEqual(string.Format("MSH|^~\\&|{0}", id), tree.GetValue(1));
        }

        [TestMethod]
        public void Message_CanMapFields()
        {
            var id = Randomized.String();
            IMessage tree = new NativeMessage(string.Format("MSH|^~\\&|{0}", id));
            Assert.AreEqual(id, tree.GetValue(1, 3));
        }

        [TestMethod]
        public void Message_CanMapRepetitions()
        {
            var id1 = Randomized.String();
            var id2 = Randomized.String();
            IMessage tree = new NativeMessage(string.Format("MSH|^~\\&|{0}~{1}", id1, id2));
            Assert.AreEqual(id1, tree.GetValue(1, 3, 1));
            Assert.AreEqual(id2, tree.GetValue(1, 3, 2));
        }

        [TestMethod]
        public void Message_CanMapComponents()
        {
            var id1 = Randomized.String();
            var id2 = Randomized.String();
            IMessage tree = new NativeMessage(string.Format("MSH|^~\\&|{0}^{1}", id1, id2));
            Assert.AreEqual(id1, tree.GetValue(1, 3, 1, 1));
            Assert.AreEqual(id2, tree.GetValue(1, 3, 1, 2));
        }

        [TestMethod]
        public void Message_CanMapSubcomponents()
        {
            var id1 = Randomized.String();
            var id2 = Randomized.String();
            IMessage tree = new NativeMessage(string.Format("MSH|^~\\&|{0}&{1}", id1, id2));
            Assert.AreEqual(id1, tree.GetValue(1, 3, 1, 1, 1));
            Assert.AreEqual(id2, tree.GetValue(1, 3, 1, 1, 2));
        }
    }
}
