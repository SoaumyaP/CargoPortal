import 'environment-config-base.dart';

class QCEnvironmentConfig implements BaseEnvironmentConfig {
  @override
  String get appAgent => 'Warehouse Receive (QC)';

  @override
  String get csportalApi => 'https://g-sp-api.azurewebsites.net/api';

  @override
  String get secureSecret =>
      'SPKuCuhxLYJufF8RZ2XgOX8OalQkTGLBm3iH1vvwNopt1leBaEs1';
}
