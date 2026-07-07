import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "src"))

from pyqt_form_builder.form_functions import (
    create_object,
    delete_object,
    load_fields,
    update_fields,
)
from pyqt_form_builder.sample_app import Item, Order, SampleWindow
from PyQt5.QtWidgets import QApplication, QLineEdit, QWidget, QVBoxLayout

app = QApplication.instance() or QApplication([])


class FormFunctionsTests(unittest.TestCase):
    def test_load_fields_populates_widgets(self):
        class Model:
            def __init__(self):
                self.name = ""
                self.price = ""

        model = Model()
        widget = QWidget()
        layout = QVBoxLayout(widget)
        name_edit = QLineEdit()
        price_edit = QLineEdit()
        name_edit.setProperty("binding", "orders,text,name,property")
        price_edit.setProperty("binding", "orders,text,price,property")
        layout.addWidget(name_edit)
        layout.addWidget(price_edit)

        model.name = "Alpha"
        model.price = "9.99"
        load_fields(model, [name_edit, price_edit])

        self.assertEqual(name_edit.text(), "Alpha")
        self.assertEqual(price_edit.text(), "9.99")

    def test_update_fields_reads_widgets(self):
        class Model:
            def __init__(self):
                self.name = ""
                self.price = ""

        model = Model()
        name_edit = QLineEdit("Beta")
        price_edit = QLineEdit("12.5")
        name_edit.setProperty("binding", "orders,text,name,property")
        price_edit.setProperty("binding", "orders,text,price,property")

        update_fields(model, [name_edit, price_edit])

        self.assertEqual(model.name, "Beta")
        self.assertEqual(model.price, "12.5")

    def test_create_and_delete_object(self):
        class ItemValue:
            def __init__(self):
                self.serial = ""

        items = []
        created = create_object(items, ItemValue)
        self.assertIsInstance(created, ItemValue)
        self.assertEqual(len(items), 1)

        delete_object(items, created)
        self.assertEqual(len(items), 0)

    def test_sample_window_can_create_nested_item(self):
        window = SampleWindow()
        order = Order()

        item = window.add_item_to_order(order)

        self.assertIsInstance(item, Item)
        self.assertEqual(len(order.items), 1)
        self.assertEqual(order.items[0], item)

    def test_sample_window_can_save_and_load_json(self):
        window = SampleWindow()
        order = Order()
        order.name = "Saved Order"
        order.price = "42"
        item = Item()
        item.serial = "SN999"
        item.qty = "7"
        order.items.append(item)
        window.orders = [order]
        window.on_save_json()
        window.orders = []
        window.on_load_json()

        self.assertEqual(len(window.orders), 1)
        self.assertEqual(window.orders[0].name, "Saved Order")
        self.assertEqual(window.orders[0].items[0].serial, "SN999")
        self.assertEqual(window.orders[0].items[0].qty, "7")

    def test_create_item_uses_current_field_values_and_update_order(self):
        window = SampleWindow()
        window.order_list.setCurrentRow(0)
        window.name_edit.setText("Updated Order")
        window.price_edit.setText("88.8")
        window.serial_edit.setText("SN777")
        window.qty_edit.setText("3")

        window.on_update_order()
        window.on_create_item()

        order = window.orders[0]
        self.assertEqual(order.name, "Updated Order")
        self.assertEqual(order.price, "88.8")
        self.assertEqual(order.items[-1].serial, "SN777")
        self.assertEqual(order.items[-1].qty, "3")


if __name__ == "__main__":
    unittest.main()
