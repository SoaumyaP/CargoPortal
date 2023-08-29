import 'environment-config-base.dart';

class DevEnvironmentConfig implements BaseEnvironmentConfig {
  @override
  String get appAgent => 'Warehouse Receive (Debug)';

  @override
  String get csportalApi => 'https://51a2-113-161-78-65.ap.ngrok.io/api';

  @override
  String get secureSecret =>
      'Pl4a6TLOLbSel1Z1IP8OHxa4zTh3bEi4aqTR1JUY10XYkNTj3Y8G';
}
