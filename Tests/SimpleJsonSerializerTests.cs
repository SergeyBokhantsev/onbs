using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class SimpleJsonSerializerTests
    {
        [TestMethod]
        public void SimpleSerializerTest()
        {
            //INIT
            var obj = new { PropString = "text", PropInt = 123, PropDouble = 1.23 };

            //ACT
            var json = Json.SimpleJsonSerializer.Serialize(obj);

            //ASSERT
            Assert.AreEqual("{\"PropString\":\"text\",\"PropInt\":123,\"PropDouble\":1.23}", json);
        }

        [TestMethod]
        public void SimpleSerializerTestNestedObject()
        {
            //INIT
            var obj = new 
            { 
                PropString = "text", 
                PropInt = 123, 
                PropDouble = 1.23, 
                PropObj = new { PropString = "text", PropInt = 123, PropDouble = 1.23 } 
            };

            //ACT
            var json = Json.SimpleJsonSerializer.Serialize(obj);

            //ASSERT
            Assert.AreEqual("{\"PropString\":\"text\",\"PropInt\":123,\"PropDouble\":1.23,\"PropObj\":{\"PropString\":\"text\",\"PropInt\":123,\"PropDouble\":1.23}}", json);
        }

        [TestMethod]
        public void SimpleSerializerTestNestedPrimitiveArray()
        {
            //INIT
            var obj = new
            {
                PropString = "text",
                PropInt = 123,
                PropDouble = 1.23,
                PropObj = new object[] { 1,2,3 }
            };

            //ACT
            var json = Json.SimpleJsonSerializer.Serialize(obj);

            //ASSERT
            Assert.AreEqual("{\"PropString\":\"text\",\"PropInt\":123,\"PropDouble\":1.23,\"PropObj\":[1,2,3]}", json);
        }

        [TestMethod]
        public void SimpleSerializerTestNestedMixedPrimitiveArray()
        {
            //INIT
            var obj = new
            {
                PropString = "text",
                PropInt = 123,
                PropDouble = 1.23,
                PropObj = new object[] { 1, "2", 3.33 }
            };

            //ACT
            var json = Json.SimpleJsonSerializer.Serialize(obj);

            //ASSERT
            Assert.AreEqual("{\"PropString\":\"text\",\"PropInt\":123,\"PropDouble\":1.23,\"PropObj\":[1,\"2\",3.33]}", json);
        }

        [TestMethod]
        public void SimpleSerializerTestNestedObjectArray()
        {
            //INIT
            var obj = new
            {
                PropString = "text",
                PropObj = new object[] { new { PropString = "a" }, new { PropString = "b" } }
            };

            //ACT
            var json = Json.SimpleJsonSerializer.Serialize(obj);

            //ASSERT
            Assert.AreEqual("{\"PropString\":\"text\",\"PropObj\":[{\"PropString\":\"a\"},{\"PropString\":\"b\"}]}", json);
        }
    }
}
