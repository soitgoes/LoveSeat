using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace LoveSeat.IntegrationTest
{
    [TestFixture]
    public class NewtonSoftTests
    {
        [Test]
        public void JArray_Should_Support_Complex_Types()
        {
            var arry = new JArray();
            arry.Add(1);
            arry.Add("abc");
            var result = arry.ToString(Formatting.None);
            Assert.AreEqual("[1,\"abc\"]", result);
        }
        [Test]
        public void KeyOptions_Should_Produce_Single_Value_For_A_Single_Array()
        {
            var arry = new KeyOptions();
            arry.Add(1);
            var result = arry.ToString();
            Assert.AreEqual("1", result);
            
        }
        [Test]
        public void KeyOptions_Should_Produce_A_Complex_Array_For_Multiple_Values()
        {
            var arry = new KeyOptions();
            arry.Add(1);
            arry.Add(new DateTime(2011,1,1));
            var result = arry.ToString();
            Assert.AreEqual("[1,%222011-01-01T00%3a00%3a00%22]", result);
        }

        [Test]
        public void KeyOptions_Should_Produce_Squirley_Brackets_for_CouchValueMax()
        {
            var arry = new KeyOptions();
            arry.Add(CouchValue.MaxValue);
            arry.Add(1);
            var result = arry.ToString();
            Assert.AreEqual("[{},1]", result);
        }
        [Test]
       public void KeyOptions_Should_Produce_Squirley_Brackets_for_CouchValueMax2()
       {
           var arry = new KeyOptions(CouchValue.MaxValue, 1);
           var result = arry.ToString();
           Assert.AreEqual("[{},1]", result);
       }


        [Test]
        public void KeyOptions_Should_Produce_IsoTime()
        {
            var arry = new KeyOptions();
            arry.Add(CouchValue.MinValue);
            arry.Add(new DateTime(2011,1,1));
            var result = arry.ToString();
            Assert.AreEqual("[null,%222011-01-01T00%3a00%3a00%22]", result);
    
        }

        [Test]
        public void KeyOptions_Constructor_Fails()
        {
            var arry = new KeyOptions(CouchValue.MinValue);
            var result = arry.ToString();
        }
    }
}