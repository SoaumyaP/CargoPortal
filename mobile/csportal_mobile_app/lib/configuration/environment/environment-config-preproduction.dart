import 'environment-config-base.dart';

class PreproductionEnvironmentConfig implements BaseEnvironmentConfig {
  @override
  String get appAgent => 'Warehouse Receive (Preproduction)';

  @override
  String get csportalApi =>
      'https://csportal-api-cargofe-preprd.azurewebsites.net/api';

  @override
  String get secureSecret =>
      'dcBg4NmBkaDxzIiMI8GPVUpWWBuWjeKMI1Lp1Jpf3stpnXnFTuRt';
}
