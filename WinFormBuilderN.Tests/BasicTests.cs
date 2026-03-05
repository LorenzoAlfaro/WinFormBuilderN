using NUnit.Framework;
using FormBuilder;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

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

        [Test]
        public void GetAll_ReturnsNestedListBoxes()
        {
            var group = new GroupBox();
            var panel = new Panel();
            var lb1 = new ListBox() { Name = "lb1" };
            var lb2 = new ListBox() { Name = "lb2" };
            panel.Controls.Add(lb1);
            group.Controls.Add(panel);
            group.Controls.Add(lb2);

            var result = FormFunctions.GetAll(group, typeof(ListBox)).ToList();

            Assert.That(result.Select(c => c.Name), Is.EquivalentTo(new[] { "lb1", "lb2" }));
        }

        [Test]
        public void GetAll2_ReturnsAllControlsDistinct()
        {
            var group = new GroupBox();
            var panel = new Panel();
            var tb = new TextBox() { Name = "tb" };
            panel.Controls.Add(tb);
            group.Controls.Add(panel);
            group.Controls.Add(new Button() { Name = "btn" });

            var all = FormFunctions.GetAll2(group).ToList();

            Assert.That(all.Any(c => c.Name == "tb"));
            Assert.That(all.Any(c => c.Name == "btn"));
        }

        [Test]
        public void ResetListBox_SetsSelectedIndexToZero_WhenItemsPresent()
        {
            var lb = new ListBox();
            lb.Items.Add("a");
            lb.Items.Add("b");
            lb.SelectedIndex = -1;

            FormFunctions.resetListBox(lb);

            Assert.That(lb.SelectedIndex, Is.EqualTo(0));
        }

        [Test]
        public void ClearSources_ClearsAllListBoxDataSources()
        {
            var group = new GroupBox();
            var lb1 = new ListBox();
            var lb2 = new ListBox();
            lb1.DataSource = new List<string> { "x" };
            lb2.DataSource = new List<string> { "y" };
            group.Controls.Add(lb1);
            group.Controls.Add(lb2);

            FormFunctions.ClearSources(group);

            Assert.That(lb1.DataSource, Is.Null);
            Assert.That(lb2.DataSource, Is.Null);
        }

        [Test]
        public void UpdateFields_And_LoadFields_RoundTrip_FieldAndProperty()
        {
            // model with field and property
            var model = new Order { Name = "initial", Price = 1.0 };

            // controls bound to model members via Tag:
            // Tag format used by FormFunctions: "<ListBoxName>,<ControlProperty>,<ModelMember>,<field|property>"
            var tbName = new TextBox() { Text = "NewOrderName", Tag = "orders,Text,Name,field" };
            var tbPrice = new TextBox() { Text = "12.5", Tag = "orders,Text,Price,property" };

            // Simulate controls returned by getMyControls (we'll pass them directly)
            var controls = new List<Control> { tbName, tbPrice };

            // updateFields should set model members from control properties
            // Need to convert types as code simply assigns control property value to model field/property.
            // For Text -> field string Name is fine. For Price property (double) the code will attempt to SetValue with string,
            // which will fail in strict typing. To keep test aligned with current implementation, make Price a string property here.
            // Adjust: create a model variant with Price as string for this test.
            var model2 = new
            {
                Name = "",
                Price = ""
            };

            // Instead, create a dynamic-like holder using a simple class:
            var container = new SimpleModel();
            controls = new List<Control>
            {
                new TextBox() { Text = "Alpha", Tag = "orders,Text,Name,field" },
                new TextBox() { Text = "9.99", Tag = "orders,Text,Price,property" }
            };

            FormFunctions.updateFields(container, controls);

            Assert.That(container.Name, Is.EqualTo("Alpha"));
            Assert.That(container.Price, Is.EqualTo("9.99"));

            // Now test loadFields: modify container and load back into controls
            container.Name = "LoadedName";
            container.Price = "77.77";
            FormFunctions.loadFields(container, controls.Cast<Control>().ToList());

            Assert.That(((TextBox)controls[0]).Text, Is.EqualTo("LoadedName"));
            Assert.That(((TextBox)controls[1]).Text, Is.EqualTo("77.77"));
        }

        // Helper simple model for text-based field/property tests
        public class SimpleModel
        {
            public string Name;
            public string Price { get; set; }
        }

        [Test]
        public void CreateNewChild_CreateObject_DeleteObject_WorkOnGenericList()
        {
            var orders = new List<Order>();
            // createNewChild should return an instance of Order when called with List<Order>
            var newItem = FormFunctions.createNewChild(orders);
            Assert.That(newItem, Is.TypeOf<Order>());

            // CreateObject should add an item to the list
            var group = new GroupBox(); // no bound controls required for this simple test
            int before = orders.Count;
            FormFunctions.CreateObject(orders, "listBoxOrders", group);
            Assert.That(orders.Count, Is.EqualTo(before + 1));

            // DeleteObject should remove the given item
            var toDelete = orders[0];
            FormFunctions.DeleteObject(orders, toDelete);
            Assert.That(orders.Contains(toDelete), Is.False);
        }

        [Test]
        public void LoadGenericObject_FailsWhenTypeMismatch()
        {
            // red/green TDD: this test will *fail* initially because the library throws
            // when a control property cannot accept the model value. We expect no
            // exception once the code is fixed.

            // Model with a double field
            var model = new ModelWithDouble { Price = 12.34 };

            var group = new GroupBox();
            var tbPrice = new TextBox() { Tag = "orders,Text,Price,field" };
            group.Controls.Add(tbPrice);

            // we assert that no exception is thrown; since the existing implementation
            // throws an ArgumentException, this assertion will fail and highlight the bug.
            Assert.DoesNotThrow(() =>
                FormFunctions.loadGenericObject(model, "orders", group));
        }

        // Helper class for the test
        public class ModelWithDouble
        {
            public double Price;
        }
    }
}