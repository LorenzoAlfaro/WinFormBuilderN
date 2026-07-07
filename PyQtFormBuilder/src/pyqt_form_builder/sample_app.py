from __future__ import annotations

from PyQt5.QtWidgets import (
    QApplication,
    QFormLayout,
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


class Order:
    def __init__(self):
        self.name = ""
        self.price = ""


class SampleWindow(QWidget):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("PyQtFormBuilder sample")
        self.orders = []

        self.list_widget = QListWidget()
        self.name_edit = QLineEdit()
        self.price_edit = QLineEdit()
        self.create_button = QPushButton("Create")
        self.delete_button = QPushButton("Delete")

        self.name_edit.setProperty("binding", "orders,text,name,property")
        self.price_edit.setProperty("binding", "orders,text,price,property")

        form_layout = QFormLayout()
        form_layout.addRow("Name", self.name_edit)
        form_layout.addRow("Price", self.price_edit)

        button_layout = QVBoxLayout()
        button_layout.addWidget(self.create_button)
        button_layout.addWidget(self.delete_button)

        main_layout = QVBoxLayout(self)
        main_layout.addWidget(self.list_widget)
        main_layout.addLayout(form_layout)
        main_layout.addLayout(button_layout)

        self.create_button.clicked.connect(self.on_create)
        self.delete_button.clicked.connect(self.on_delete)
        self.list_widget.currentItemChanged.connect(self.on_selection_changed)

        self._seed_data()

    def _seed_data(self):
        order = create_object(self.orders, Order)
        order.name = "Demo Order"
        order.price = "19.99"
        self._refresh_list()
        self.list_widget.setCurrentRow(0)

    def _refresh_list(self):
        self.list_widget.clear()
        for order in self.orders:
            self.list_widget.addItem(order.name or "(unnamed)")

    def on_create(self):
        order = create_object(self.orders, Order)
        self._refresh_list()
        self.list_widget.setCurrentRow(self.list_widget.count() - 1)

    def on_delete(self):
        current_row = self.list_widget.currentRow()
        if current_row >= 0 and current_row < len(self.orders):
            delete_object(self.orders, self.orders[current_row])
            self._refresh_list()

    def on_selection_changed(self, current_item):
        if not current_item:
            return
        row = self.list_widget.row(current_item)
        if row < 0 or row >= len(self.orders):
            return
        order = self.orders[row]
        load_fields(order, [self.name_edit, self.price_edit])


if __name__ == "__main__":
    app = QApplication([])
    window = SampleWindow()
    window.resize(400, 300)
    window.show()
    app.exec_()
