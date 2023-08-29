class WarehouseCargoReceiveModel {
  final String location;
  final int stage;
  DateTime? cargoReceivedDate;

  WarehouseCargoReceiveModel.fromJson(Map<String, dynamic> json)
      : location = json['location'],
        stage = json['stage'],
        cargoReceivedDate = json['cargoReceivedDate'] != null
            ? DateTime.parse(json['cargoReceivedDate'])
            : null;

  Map<String, dynamic> toJson() => {
        'location': location,
        'stage': stage,
        'cargoReceivedDate': cargoReceivedDate,
      };
}
