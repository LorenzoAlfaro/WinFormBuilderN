from __future__ import annotations

from typing import Any, Iterable, List, Optional

from PyQt5.QtWidgets import QLineEdit, QWidget


def _parse_binding(binding: Optional[str]) -> Optional[tuple[str, str, str, str]]:
    if not binding:
        return None
    parts = binding.split(",")
    if len(parts) < 4:
        return None
    return parts[0], parts[1], parts[2], parts[3]


def _get_control_value(control: QWidget, property_name: str) -> Any:
    if isinstance(control, QLineEdit):
        if property_name == "text":
            return control.text()
    return getattr(control, property_name)


def _set_control_value(control: QWidget, property_name: str, value: Any) -> None:
    if isinstance(control, QLineEdit):
        if property_name == "text":
            control.setText(str(value))
            return
    setattr(control, property_name, value)


def _get_member_value(obj: Any, member_name: str) -> Any:
    if isinstance(obj, dict):
        return obj.get(member_name)
    return getattr(obj, member_name, None)


def _set_member_value(obj: Any, member_name: str, value: Any) -> None:
    if isinstance(obj, dict):
        obj[member_name] = value
        return
    setattr(obj, member_name, value)


def load_fields(obj: Any, controls: Iterable[QWidget]) -> None:
    for control in controls:
        binding = control.property("binding")
        if not binding:
            continue
        parsed = _parse_binding(binding)
        if not parsed:
            continue
        _, control_property, member_name, member_kind = parsed
        value = _get_member_value(obj, member_name)
        _set_control_value(control, control_property, value)


def update_fields(obj: Any, controls: Iterable[QWidget]) -> None:
    for control in controls:
        binding = control.property("binding")
        if not binding:
            continue
        parsed = _parse_binding(binding)
        if not parsed:
            continue
        _, control_property, member_name, member_kind = parsed
        value = _get_control_value(control, control_property)
        _set_member_value(obj, member_name, value)


def create_object(collection: List[Any], item_type: type) -> Any:
    item = item_type()
    collection.append(item)
    return item


def delete_object(collection: List[Any], item: Any) -> None:
    if item in collection:
        collection.remove(item)
