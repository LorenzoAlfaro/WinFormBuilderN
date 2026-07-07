from __future__ import annotations

import json
from pathlib import Path

from PyQt5.QtWidgets import (
    QApplication,
    QFormLayout,
    QHBoxLayout,
    QLabel,
    QLineEdit,
    QListWidget,
    QPushButton,
    QVBoxLayout,
    QWidget,
)

from pyqt_form_builder.form_functions import (
    create_object,
    delete_object,
    load_fields,
    update_fields,
)


class SampleWindowDict(QWidget):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("PyQtFormBuilder sample (dict)")
        self.orders = []

        self.order_list = QListWidget()
        self.item_list = QListWidget()
        self.name_edit = QLineEdit()
        self.price_edit = QLineEdit()
        self.serial_edit = QLineEdit()
        self.qty_edit = QLineEdit()
        self.create_order_button = QPushButton("Create Order")
        self.update_order_button = QPushButton("Update Order")
        self.delete_order_button = QPushButton("Delete Order")
        self.create_item_button = QPushButton("Add Item")
        self.update_item_button = QPushButton("Update Item")
        self.delete_item_button = QPushButton("Delete Item")
        self.load_button = QPushButton("Load JSON")
        self.save_button = QPushButton("Save JSON")

        self.name_edit.setProperty("binding", "orders,text,name,property")
        self.price_edit.setProperty("binding", "orders,text,price,property")
        self.serial_edit.setProperty("binding", "items,text,serial,property")
        self.qty_edit.setProperty("binding", "items,text,qty,property")

        order_layout = QVBoxLayout()
        order_layout.addWidget(QLabel("Orders"))
        order_layout.addWidget(self.order_list)
        order_layout.addWidget(self.create_order_button)
        order_layout.addWidget(self.update_order_button)
        order_layout.addWidget(self.delete_order_button)

        item_layout = QVBoxLayout()
        item_layout.addWidget(QLabel("Items"))
        item_layout.addWidget(self.item_list)
        item_layout.addWidget(self.create_item_button)
        item_layout.addWidget(self.update_item_button)
        item_layout.addWidget(self.delete_item_button)

        form_layout = QFormLayout()
        form_layout.addRow("Name", self.name_edit)
        form_layout.addRow("Price", self.price_edit)
        form_layout.addRow("Item Serial", self.serial_edit)
        form_layout.addRow("Item Qty", self.qty_edit)

        button_row = QHBoxLayout()
        button_row.addWidget(self.load_button)
        button_row.addWidget(self.save_button)

        main_layout = QVBoxLayout(self)
        lists_layout = QHBoxLayout()
        lists_layout.addLayout(order_layout)
        lists_layout.addLayout(item_layout)
        main_layout.addLayout(lists_layout)
        main_layout.addLayout(form_layout)
        main_layout.addLayout(button_row)

        self.create_order_button.clicked.connect(self.on_create_order)
        self.update_order_button.clicked.connect(self.on_update_order)
        self.delete_order_button.clicked.connect(self.on_delete_order)
        self.create_item_button.clicked.connect(self.on_create_item)
        self.update_item_button.clicked.connect(self.on_update_item)
        self.delete_item_button.clicked.connect(self.on_delete_item)
        self.load_button.clicked.connect(self.on_load_json)
        self.save_button.clicked.connect(self.on_save_json)
        self.order_list.currentItemChanged.connect(self.on_order_selection_changed)
        self.item_list.currentItemChanged.connect(self.on_item_selection_changed)

        self._seed_data()

    def _seed_data(self):
        order = self._create_order_dict()
        order["name"] = "Demo Order"
        order["price"] = "19.99"
        item = self.add_item_to_order(order)
        item["serial"] = "SN100"
        item["qty"] = "2"
        self._refresh_orders()
        self.order_list.setCurrentRow(0)

    def _create_order_dict(self) -> dict:
        order = {"name": "", "price": "", "items": []}
        self.orders.append(order)
        return order

    def _refresh_orders(self):
        self.order_list.clear()
        for order in self.orders:
            self.order_list.addItem(order.get("name") or "(unnamed)")

    def _refresh_items(self, order: dict | None):
        self.item_list.clear()
        if not order:
            return
        for item in order.get("items", []):
            self.item_list.addItem(
                f"{item.get('serial') or '(no serial)'} / {item.get('qty') or '(no qty)'}"
            )

    def add_item_to_order(self, order: dict) -> dict:
        item = {"serial": "", "qty": ""}
        order.setdefault("items", []).append(item)
        self._refresh_items(order)
        return item

    def on_create_order(self):
        order = self._create_order_dict()
        update_fields(order, [self.name_edit, self.price_edit])
        self._refresh_orders()
        self.order_list.setCurrentRow(self.order_list.count() - 1)

    def on_update_order(self):
        current_row = self.order_list.currentRow()
        if current_row < 0 or current_row >= len(self.orders):
            return
        order = self.orders[current_row]
        update_fields(order, [self.name_edit, self.price_edit])
        self._refresh_orders()
        self.order_list.setCurrentRow(current_row)

    def on_delete_order(self):
        current_row = self.order_list.currentRow()
        if current_row >= 0 and current_row < len(self.orders):
            del self.orders[current_row]
            self._refresh_orders()

    def on_create_item(self):
        current_row = self.order_list.currentRow()
        if current_row < 0 or current_row >= len(self.orders):
            return
        order = self.orders[current_row]
        item = self.add_item_to_order(order)
        update_fields(item, [self.serial_edit, self.qty_edit])
        self._refresh_items(order)
        self.item_list.setCurrentRow(self.item_list.count() - 1)

    def on_update_item(self):
        current_row = self.order_list.currentRow()
        if current_row < 0 or current_row >= len(self.orders):
            return
        order = self.orders[current_row]
        item_row = self.item_list.currentRow()
        if item_row < 0 or item_row >= len(order.get("items", [])):
            return
        item = order["items"][item_row]
        update_fields(item, [self.serial_edit, self.qty_edit])
        self._refresh_items(order)
        self.item_list.setCurrentRow(item_row)

    def on_delete_item(self):
        current_row = self.order_list.currentRow()
        if current_row < 0 or current_row >= len(self.orders):
            return
        order = self.orders[current_row]
        item_row = self.item_list.currentRow()
        if item_row < 0 or item_row >= len(order.get("items", [])):
            return
        del order["items"][item_row]
        self._refresh_items(order)

    def on_order_selection_changed(self, current_item):
        if not current_item:
            self._refresh_items(None)
            return
        row = self.order_list.row(current_item)
        if row < 0 or row >= len(self.orders):
            return
        order = self.orders[row]
        load_fields(order, [self.name_edit, self.price_edit])
        self._refresh_items(order)

    def on_item_selection_changed(self, current_item):
        if not current_item:
            return
        order_row = self.order_list.currentRow()
        if order_row < 0 or order_row >= len(self.orders):
            return
        order = self.orders[order_row]
        item_row = self.item_list.row(current_item)
        if item_row < 0 or item_row >= len(order.get("items", [])):
            return
        item = order["items"][item_row]
        load_fields(item, [self.serial_edit, self.qty_edit])

    def on_load_json(self):
        path = Path("sample_data_dict.json")
        if not path.exists():
            return
        with path.open("r", encoding="utf-8") as handle:
            data = json.load(handle)
        self.orders = []
        for order_data in data.get("orders", []):
            order = {
                "name": order_data.get("name", ""),
                "price": order_data.get("price", ""),
                "items": [],
            }
            for item_data in order_data.get("items", []):
                order["items"].append(
                    {
                        "serial": item_data.get("serial", ""),
                        "qty": item_data.get("qty", ""),
                    }
                )
            self.orders.append(order)
        self._refresh_orders()
        if self.orders:
            self.order_list.setCurrentRow(0)

    def on_save_json(self):
        path = Path("sample_data_dict.json")
        payload = {
            "orders": [
                {
                    "name": order.get("name", ""),
                    "price": order.get("price", ""),
                    "items": [
                        {"serial": item.get("serial", ""), "qty": item.get("qty", "")}
                        for item in order.get("items", [])
                    ],
                }
                for order in self.orders
            ]
        }
        with path.open("w", encoding="utf-8") as handle:
            json.dump(payload, handle, indent=2)


if __name__ == "__main__":
    app = QApplication([])
    window = SampleWindowDict()
    window.resize(400, 300)
    window.show()
    app.exec_()
