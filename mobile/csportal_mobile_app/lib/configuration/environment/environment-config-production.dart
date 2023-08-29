import 'environment-config-base.dart';

class ProductionEnvironmentConfig implements BaseEnvironmentConfig {
  @override
  String get appAgent => 'Warehouse Receive';

  @override
  String get csportalApi => 'https://csportal-api.cargofe.com/api';

  @override
  String get secureSecret =>
      'bmfm7yinE9ciSJ5DjQ8OT2ac0j9jIpQENeRxNnJQCYiDl8g0q7BZ';
}
