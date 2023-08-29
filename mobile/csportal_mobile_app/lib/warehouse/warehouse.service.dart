import 'dart:convert';

import '../core/http.service.dart';
import 'models/warehouse_cargoreceive.model.dart';

class WarehouseService {
  Future<WarehouseCargoReceiveModel> getWarehouseCargoReceive(
      String warehouseBookingNumber) async {
    var url = 'warehouseBookings/$warehouseBookingNumber/cargoReceive';
    final response = await HttpService().get(url);

    if (response.statusCode == 200) {
      Map<String, dynamic> modelMap = jsonDecode(response.body);
      var model = WarehouseCargoReceiveModel.fromJson(modelMap);
      return model;
    } else {
      throw response.body;
    }
  }

  Future<String> putWarehouseCargoReceive(String warehouseBookingNumber) async {
    var url = 'warehouseBookings/$warehouseBookingNumber/fullCargoReceive';
    final response = await HttpService().post(url);
    if (response.statusCode == 200) {
      Map<String, dynamic> modelMap = jsonDecode(response.body);
      return modelMap['cargoReceivedDate'];
    } else {
      throw response.body;
    }
  }
}
