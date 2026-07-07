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
        class Item:
            def __init__(self):
                self.serial = ""

        items = []
        created = create_object(items, Item)
        self.assertIsInstance(created, Item)
        self.assertEqual(len(items), 1)

        delete_object(items, created)
        self.assertEqual(len(items), 0)


if __name__ == "__main__":
    unittest.main()
