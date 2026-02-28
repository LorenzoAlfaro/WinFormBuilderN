using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FormBuilder.FormFunctions;
using System.IO;
using Newtonsoft.Json;

namespace WinFormBuilderN.TestApp
{

    public partial class Form1 : Form
    {
        // Expose to MainHub loader
        public static bool includeMe = true;
        public static int accessLevel = 3;
        // simple model types
        public class Item
        {
            public string Serial { get; set; }
            public int Qty { get; set; }
        }

        public class Order
        {
            public string Name { get; set; }
            public double Price { get; set; }
            public List<Item> items = new List<Item>();
            public override string ToString() => Name ?? "(unnamed)";
        }

        public class OrdersCollection
        {
            public string Name = "MyOrders";
            public List<Order> orders = new List<Order>();
        }

        private OrdersCollection data = new OrdersCollection();

        // Controls
        private GroupBox groupBox1;
        private ListBox listBoxOrders;
        private ListBox listBoxItems;
        private TextBox textBoxOrderName;
        private TextBox textBoxOrderPrice;
        private TextBox textBoxItemSerial;
        private TextBox textBoxItemQty;
        private Button btnCreateOrder, btnUpdateOrder, btnDeleteOrder;
        private Button btnCreateItem, btnUpdateItem, btnDeleteItem;
        private Button btnLoadJson, btnSaveJson;
        private Label labelStatus;

        public Form1()
        {
            Text = "OrderTester - FormFunctions example";
            Width = 800; Height = 480;
            InitializeControls();
            BindMetadata();
            LoadSampleData();
            reloadMainObject();
        }

        // Constructor used by the dynamic loader (CreateInstance2)
        public Form1(dynamic mySettings) : this()
        {
            // mySettings ignored for this example, but accepted so MainHub can create the form
        }

        private void InitializeControls()
        {
            groupBox1 = new GroupBox() { Text = "Orders", Dock = DockStyle.Fill };
            Controls.Add(groupBox1);

            listBoxOrders = new ListBox() { Name = "listBoxOrders", Left = 8, Top = 24, Width = 240, Height = 300 };
            listBoxItems = new ListBox() { Name = "listBoxItems", Left = 260, Top = 24, Width = 240, Height = 300 };

            // Order fields
            textBoxOrderName = new TextBox() { Left = 8, Top = 340, Width = 200 };
            textBoxOrderPrice = new TextBox() { Left = 8, Top = 370, Width = 200 };

            // Item fields
            textBoxItemSerial = new TextBox() { Left = 260, Top = 340, Width = 200 };
            textBoxItemQty = new TextBox() { Left = 260, Top = 370, Width = 200 };

            // Buttons
            btnCreateOrder = new Button() { Left = 8, Top = 400, Width = 70, Text = "Create" };
            btnUpdateOrder = new Button() { Left = 84, Top = 400, Width = 70, Text = "Update" };
            btnDeleteOrder = new Button() { Left = 160, Top = 400, Width = 70, Text = "Delete" };

            btnCreateItem = new Button() { Left = 260, Top = 400, Width = 70, Text = "Create" };
            btnUpdateItem = new Button() { Left = 336, Top = 400, Width = 70, Text = "Update" };
            btnDeleteItem = new Button() { Left = 412, Top = 400, Width = 70, Text = "Delete" };

            btnLoadJson = new Button() { Left = 520, Top = 24, Width = 120, Text = "Load JSON" };
            btnSaveJson = new Button() { Left = 520, Top = 56, Width = 120, Text = "Save JSON" };

            labelStatus = new Label() { Left = 520, Top = 100, Width = 240, Height = 80, Text = "Status" };

            groupBox1.Controls.AddRange(new Control[] {
            listBoxOrders, listBoxItems,
            textBoxOrderName, textBoxOrderPrice,
            textBoxItemSerial, textBoxItemQty,
            btnCreateOrder, btnUpdateOrder, btnDeleteOrder,
            btnCreateItem, btnUpdateItem, btnDeleteItem,
            btnLoadJson, btnSaveJson, labelStatus
        });

            // events
            listBoxOrders.SelectedIndexChanged += ListBoxOrders_SelectedIndexChanged;
            listBoxItems.SelectedIndexChanged += ListBoxItems_SelectedIndexChanged;

            btnCreateOrder.Click += (s, e) => { CRUDButton(btnCreateOrder, EventArgs.Empty, groupBox1, data, labelStatus); reloadMainObject(); };
            btnDeleteOrder.Click += (s, e) => { CRUDButton(btnDeleteOrder, EventArgs.Empty, groupBox1, data, labelStatus); reloadMainObject(); };
            btnUpdateOrder.Click += (s, e) => { CRUDButton(btnUpdateOrder, EventArgs.Empty, groupBox1, data, labelStatus); reloadMainObject(); };

            btnCreateItem.Click += (s, e) => { CRUDButton(btnCreateItem, EventArgs.Empty, groupBox1, data, labelStatus); reloadMainObject(); };
            btnDeleteItem.Click += (s, e) => { CRUDButton(btnDeleteItem, EventArgs.Empty, groupBox1, data, labelStatus); reloadMainObject(); };
            btnUpdateItem.Click += (s, e) => { CRUDButton(btnUpdateItem, EventArgs.Empty, groupBox1, data, labelStatus); reloadMainObject(); };

            btnLoadJson.Click += BtnLoadJson_Click;
            btnSaveJson.Click += BtnSaveJson_Click;
        }

        private void BindMetadata()
        {
            // ListBox meta: children, collection field, loader name, parent tag
            listBoxOrders.AccessibleDescription = "listBoxItems"; // child listboxes
            listBoxOrders.AccessibleDefaultActionDescription = "orders"; // collection field name on root
            listBoxOrders.AccessibleName = "loadGenericObject"; // loader function name
            listBoxOrders.Tag = "Top"; // parent is root

            listBoxItems.AccessibleDescription = "";
            listBoxItems.AccessibleDefaultActionDescription = "items"; // collection field name on selected Order
            listBoxItems.AccessibleName = "loadGenericObject";
            listBoxItems.Tag = "listBoxOrders"; // parent listbox name

            // Controls binding: format "ListBoxName,ControlProperty,MemberName,field|property"
            textBoxOrderName.Tag = "listBoxOrders,Text,Name,property";
            textBoxOrderPrice.Tag = "listBoxOrders,Text,Price,property";

            textBoxItemSerial.Tag = "listBoxItems,Text,Serial,property";
            textBoxItemQty.Tag = "listBoxItems,Text,Qty,property";

            // Buttons tags for CRUD: "ListBoxName,create|update|delete"
            btnCreateOrder.Tag = "listBoxOrders,create";
            btnUpdateOrder.Tag = "listBoxOrders,update";
            btnDeleteOrder.Tag = "listBoxOrders,delete";

            btnCreateItem.Tag = "listBoxItems,create";
            btnUpdateItem.Tag = "listBoxItems,update";
            btnDeleteItem.Tag = "listBoxItems,delete";
        }

        private void LoadSampleData()
        {
            // sample data with two orders
            var o1 = new Order() { Name = "Order A", Price = 12.5 };
            o1.items.Add(new Item() { Serial = "SN100", Qty = 2 });
            o1.items.Add(new Item() { Serial = "SN101", Qty = 1 });

            var o2 = new Order() { Name = "Order B", Price = 99.99 };
            o2.items.Add(new Item() { Serial = "SN200", Qty = 5 });

            data.orders.Add(o1);
            data.orders.Add(o2);
        }

        private void ListBoxOrders_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lb = (ListBox)sender;
            if (lb.Visible && lb.DataSource != null && lb.SelectedItem != null)
            {
                // call into FormFunctions to populate children and fields
                updateListbox(lb, groupBox1, updateUI);
            }
        }

        private void ListBoxItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lb = (ListBox)sender;
            if (lb.Visible && lb.DataSource != null && lb.SelectedItem != null)
            {
                updateListbox(lb, groupBox1, updateUI);
            }
        }

        // instance loader that mirrors the library behavior but performs safe conversions
        public void loadGenericObject(object genericObj, string ListBoxName, GroupBox groupBox1)
        {
            if (genericObj == null || string.IsNullOrEmpty(ListBoxName) || groupBox1 == null) return;

            foreach (Control c in groupBox1.Controls)
            {
                try
                {
                    if (c.Tag == null) continue;
                    string tag = c.Tag.ToString();
                    var parts = tag.Split(',');
                    if (parts.Length < 4) continue; // expect ListBoxName,ControlProperty,MemberName,field|property
                    if (!string.Equals(parts[0], ListBoxName, StringComparison.OrdinalIgnoreCase)) continue;

                    string controlProp = parts[1];
                    string memberName = parts[2];
                    string memberKind = parts[3];

                    // get value from object (try property then field)
                    object memberValue = null;
                    var t = genericObj.GetType();
                    var prop = t.GetProperty(memberName);
                    if (prop != null) memberValue = prop.GetValue(genericObj);
                    else
                    {
                        var field = t.GetField(memberName);
                        if (field != null) memberValue = field.GetValue(genericObj);
                    }

                    if (memberValue == null) memberValue = "";

                    // get control property info
                    var cp = c.GetType().GetProperty(controlProp);
                    if (cp == null) continue;

                    // If control property is string, convert numerics to string
                    if (cp.PropertyType == typeof(string))
                    {
                        string s = memberValue is string ? (string)memberValue : Convert.ToString(memberValue);
                        cp.SetValue(c, s);
                    }
                    else if (cp.PropertyType == typeof(bool))
                    {
                        bool b = false;
                        if (memberValue is bool) b = (bool)memberValue;
                        else bool.TryParse(Convert.ToString(memberValue), out b);
                        cp.SetValue(c, b);
                    }
                    else
                    {
                        // attempt to convert
                        try
                        {
                            var converted = Convert.ChangeType(memberValue, cp.PropertyType);
                            cp.SetValue(c, converted);
                        }
                        catch
                        {
                            // fallback: if property is object, set directly; otherwise skip
                            if (cp.PropertyType == typeof(object)) cp.SetValue(c, memberValue);
                        }
                    }
                }
                catch { /* swallow individual control errors to avoid breaking the UI update */ }
            }
        }

        // delegate used by FormFunctions.updateListbox to call our instance loader
        public void updateUI(string functionName, ListBox ListBox)
        {
            List<object> myParams = new List<object>();
            myParams.Add(ListBox.SelectedItem);
            myParams.Add(ListBox.Name);
            myParams.Add(groupBox1);
            this.GetType().GetMethod(functionName).Invoke(this, myParams.ToArray());
        }

        private void reloadMainObject()
        {
            listBoxOrders.Visible = false;
            listBoxOrders.DataSource = null;
            listBoxOrders.DisplayMember = "Name";
            listBoxOrders.DataSource = data.orders;
            listBoxOrders.Visible = true;
            // populate children for selected
            if (listBoxOrders.SelectedItem != null)
            {
                updateListbox(listBoxOrders, groupBox1, updateUI);
            }
        }

        private void BtnLoadJson_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "JSON|*.json" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var txt = File.ReadAllText(ofd.FileName);
                    try
                    {
                        data = JsonConvert.DeserializeObject<OrdersCollection>(txt);
                        if (data == null) data = new OrdersCollection();
                        reloadMainObject();
                        labelStatus.Text = "Loaded " + ofd.FileName;
                    }
                    catch (Exception ex)
                    {
                        labelStatus.Text = "Error: " + ex.Message;
                    }
                }
            }
        }

        private void BtnSaveJson_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "JSON|*.json", FileName = "orders.json" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var txt = JsonConvert.SerializeObject(data, Formatting.Indented);
                    File.WriteAllText(sfd.FileName, txt);
                    labelStatus.Text = "Saved " + sfd.FileName;
                }
            }
        }
    }


}
