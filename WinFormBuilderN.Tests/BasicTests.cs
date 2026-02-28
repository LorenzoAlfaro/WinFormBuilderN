using NUnit.Framework;
using FormBuilder;
using System.Windows.Forms;
using System.Collections.Generic;

namespace WinFormBuilderN.Tests
{
    [TestFixture]

    public class BasicTests
    {
        // Simple test model types used by the tests
        public class Item
        {
            public string Serial;
            public int Qty;
            public override string ToString() => Serial ?? "(no serial)";
        }

        public class Order
        {
            public string Name;
            public double Price;
            public List<Item> items = new List<Item>();
            public override string ToString() => Name ?? "(unnamed)";
        }

        [Test]
        public void GetAll_ReturnsEmpty_WhenNoControls()
        {
            var dummy = new GroupBox();
            var result = FormFunctions.GetAll(dummy, typeof(Button));
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }
    }
}