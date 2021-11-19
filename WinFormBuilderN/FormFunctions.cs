using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace FormBuilder
{
    public static class FormFunctions
    {
        //use the listbox1..AccessibleDescription  to store the name of the other children listbox

        public delegate void updateUI(string realFunctionName, ListBox list);

        //public void updateUI(string functionName, ListBox ListBox)
        //{            
        //    List<Object> myParams = new List<object>();
        //    myParams.Add(ListBox.SelectedItem);          
        //    this.GetType().GetMethod(functionName).Invoke(this, myParams.ToArray());//loadTestList is run from here
        //}
        public static void updateListbox(ListBox myList, Control groupBox, updateUI updateUIFunction)
        {
            updateUIFunction(myList.AccessibleName, myList);
            if (myList.AccessibleDescription != null)
            {
                string[] childrenListBoxes = myList.AccessibleDescription.Split(',');
                var c = GetAll(groupBox, typeof(ListBox));

                foreach (string listboxName in childrenListBoxes)
                {
                    foreach (ListBox list in c)
                    {
                        //update their data source                       
                        if (list.Name == listboxName)
                        {
                            list.Visible = false;//my hack flag to stop event bubbling
                            list.DataSource = null;

                            if (myList.SelectedItem != null)
                            {
                                list.DataSource = myList.SelectedItem
                                .GetType()
                                .GetField(list.AccessibleDefaultActionDescription)
                                .GetValue(myList.SelectedItem);
                                updateListbox(list, groupBox, updateUIFunction);
                            }
                            list.Visible = true;//my hack flag to stop event bubbling
                        }
                    }
                }
            }
        }
        public static List<Control> getMyControls(Control myControl, string ListBoxName)
        {
            var c = GetAll2(myControl);

            List<Control> myControls = new List<Control>();
            foreach (Control thisControls in c)
            {
                if (thisControls.Tag != null)
                {
                    string tag = (string)thisControls.Tag;
                    string[] tags = tag.Split(',');
                    if (tags[0] == ListBoxName)
                    {
                        myControls.Add(thisControls);
                    }
                }
            }
            return myControls;
        }
        public static void resetListboxes(Control myControl)
        {
            var c = GetAll(myControl, typeof(ListBox));
            foreach (ListBox list in c)
            {
                resetListBox(list);
            }
        }
        public static void resetListBox(ListBox list)
        {
            if (list.Items.Count != 0)
            {
                //list.SelectedIndex = list.Items.Count - 1;
                list.SelectedIndex = 0;
            }
        }
        public static void ClearSources(Control myControl)
        {
            var c = GetAll(myControl, typeof(ListBox));
            foreach (ListBox list in c)
            {
                list.DataSource = null;
            }
        }
        public static IEnumerable<Control> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }
        public static List<Control> getControlsType(List<Control> controls, Type type)
        {
            List<Control> filteredControls = (List<Control>)controls.Where(c => c.GetType() == type);
            return filteredControls;
        }
        public static IEnumerable<Control> GetAll2(Control control)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll2(ctrl).Concat(controls))
                .Distinct();

        }
        public static void updateFields(object obj, List<Control> myControls)
        {
            if (obj != null)
            {
                foreach (var control in myControls)
                {
                    string tag = (string)control.Tag;
                    string[] tags = tag.Split(',');
                    if (tags.Length > 2)
                    {
                        if (tags[3] == "field")
                        {
                            obj.GetType().
                            GetField(tags[2]).SetValue(obj,
                            control.GetType().
                            GetProperty(tags[1]).GetValue(control));
                        }
                        else //property
                        {
                            obj.GetType().
                               GetProperty(tags[2]).SetValue(obj,
                               control.GetType().
                               GetProperty(tags[1]).GetValue(control));
                        }
                    }
                }
            }
        }
        public static void clearTextFields(List<Control> myControls)
        {
            foreach (TextBox item in myControls.Where(c => c.GetType() == typeof(TextBox)))
            {
                if ((string)item.Tag != "")
                {
                    item.Text = "";
                }
            }
            foreach (CheckBox item in myControls.Where(c => c.GetType() == typeof(CheckBox)))
            {
                if ((string)item.Tag != "")
                {
                    item.Checked = false;
                }
            }
        }
        public static void loadFields(object obj, List<Control> myControls)
        {
            if (obj != null)
            {
                foreach (var control in myControls)
                {
                    if (control.Tag != null)
                    {
                        string tag = (string)control.Tag;
                        string[] tags = tag.Split(',');
                        if (tags.Length > 2)
                        {
                            if (tags[3] == "field")
                            {
                                control.GetType()
                                    .GetProperty(tags[1])
                                    .SetValue(control, obj.GetType()
                                    .GetField(tags[2]).GetValue(obj));
                            }
                            else
                            {
                                control.GetType()
                                    .GetProperty(tags[1])
                                    .SetValue(control, obj.GetType()
                                    .GetProperty(tags[2]).GetValue(obj));
                            }
                        }
                    }
                }
            }
        }
        public static void loadGenericObject(Object genericObj, string ListBoxName, GroupBox groupBox1)
        {
            List<Control> myControls = getMyControls(groupBox1, ListBoxName);
            clearTextFields(myControls);
            loadFields(genericObj, myControls);
        }
        public static void CRUDButton(object sender, EventArgs e, GroupBox groupBox1, object testList, Label label9)
        {
            Button myButton = (Button)sender;
            string tag = (string)myButton.Tag;
            string[] tags = tag.Split(',');
            ListBox myListBox = (ListBox)groupBox1.Controls.Find(tags[0], true)[0];
            if (tags[1] == "create")
            {
                ModifiedObject(tags[0], true, myListBox, groupBox1, testList);
            }
            else if (tags[1] == "delete")
            {
                ModifiedObject(tags[0], false, myListBox, groupBox1, testList);
            }
            else if (tags[1] == "update")
            {
                if (myListBox.SelectedItem != null)
                {
                    List<Control> myControls = getMyControls(groupBox1, tags[0]);
                    updateFields(myListBox.SelectedItem, myControls);
                }
                else
                {
                    label9.Text = "No Item selected";
                }
            }
            else
            {

            }
        }
        public static void ModifiedObject(string tag, bool createDelete, ListBox myListBox, GroupBox groupBox1, object testList)
        {
            object parentObject = getParentObject(myListBox, groupBox1, testList);
            object realList = getParentList(parentObject, myListBox.AccessibleDefaultActionDescription);
            if (createDelete)
            {
                CreateObject(realList, tag, groupBox1);
            }
            else
            {
                DeleteObject(realList, myListBox.SelectedItem);
            }
        }
        public static object getParentList(object parentObject, string collectionName)
        {
            object realList;
            FieldInfo fieldInfo = parentObject
                .GetType()
                .GetField(collectionName);//finde the collection of the parent Object to add the new item
            TypedReference reference = __makeref(parentObject);
            realList = fieldInfo.GetValueDirect(reference);//some crazy stuff
            return realList;
        }
        public static object getParentObject(ListBox myListBox, GroupBox groupBox1, object testList)
        {
            object parentObject;
            string tag2 = (string)myListBox.Tag;
            if (tag2 == "Top")
            {
                parentObject = testList;
            }
            else
            {
                ListBox parentListBox = (ListBox)groupBox1.Controls.Find(tag2, true)[0];
                if (parentListBox.SelectedItem != null)
                {
                    parentObject = parentListBox.SelectedItem;
                }
                else
                {
                    parentObject = null;
                }
            }
            return parentObject;
        }
        public static void CreateObject(object realList, string tag, GroupBox groupBox1)
        {
            object newItem = createNewChild(realList);
            List<Control> myControls = getMyControls(groupBox1, tag);//get the binded controls of that object
            updateFields(newItem, myControls);
            List<Object> myParams = new List<object>();//add logic to create new parameter            
            myParams.Add(newItem);//this is a work around to call List<generic> myList; myList.Add(newItem), because everything is generic!
            realList
                .GetType()
                .GetMethod("Add")
                .Invoke(realList, myParams.ToArray());
        }
        public static void DeleteObject(object realList, object itemToBeDeleted)
        {
            List<Object> myParams = new List<object>();
            myParams.Add(itemToBeDeleted);
            realList
                .GetType()
                .GetMethod("Remove")
                .Invoke(realList, myParams.ToArray());
        }
        public static object createNewChild(object realList)
        {
            Type myType = realList
                .GetType()
                .GetProperty("Item")
                .PropertyType; //returns the object type of the parent from the ListBox
            object newItem = Activator.CreateInstance(myType);
            return newItem;
        }
    }
}
