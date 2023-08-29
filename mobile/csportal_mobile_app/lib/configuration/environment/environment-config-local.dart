import 'environment-config-base.dart';

class LocalEnvironmentConfig implements BaseEnvironmentConfig {
  @override
  String get appAgent => 'Warehouse Receive (Debug)';

  @override
  String get csportalApi => 'http://10.0.2.2:56154/api';

  @override
  String get secureSecret =>
      'Pl4a6TLOLbSel1Z1IP8OHxa4zTh3bEi4aqTR1JUY10XYkNTj3Y8G';
}
