class WarehouseBookingModel {
  late String type;
  late String bookingNumber;
  late String shipmentNumber;
  late String scope;
  String? warehouseLocation;
  DateTime? cargoReceivedDate;
  int? stage;

  WarehouseBookingModel(String? qrCode) {
    if (qrCode?.isNotEmpty ?? false) {
      try {
        var qrCodeParts = qrCode?.split(";");
        var part = qrCodeParts?.firstWhere((part) => part.startsWith("type"));
        type = part?.split(":")[1] ?? "";
        part = qrCodeParts?.firstWhere((part) => part.startsWith("booking"));
        bookingNumber = part?.split(":")[1] ?? "";
        part = qrCodeParts?.firstWhere((part) => part.startsWith("so"));
        shipmentNumber = part?.split(":")[1] ?? "";
        part = qrCodeParts?.firstWhere((part) => part.startsWith("scope"));
        scope = part?.split(":")[1] ?? "";
      } on Exception {
        bookingNumber = "";
        shipmentNumber = "";
        scope = "";
      } catch (ex) {
        type = "";
        bookingNumber = "";
        shipmentNumber = "";
        scope = "";
      }
    } else {
      type = "";
      bookingNumber = "";
      shipmentNumber = "";
      scope = "";
    }
  }

  bool isValid() {
    return type == 'warehousebooking' &&
        scope == 'cargoreceived' &&
        bookingNumber.isNotEmpty &&
        shipmentNumber.isNotEmpty;
  }
}
