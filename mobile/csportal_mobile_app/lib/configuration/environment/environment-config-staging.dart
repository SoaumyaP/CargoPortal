import 'environment-config-base.dart';

class StagingEnvironmentConfig implements BaseEnvironmentConfig {
  @override
  String get appAgent => 'Warehouse Receive (Staging)';

  @override
  String get csportalApi => 'https://newuatcsfeportalapi.azurewebsites.net/api';

  @override
  String get secureSecret =>
      'UBZmsZ3fMTJxd5WTGSo0grJUG8lvF5d7S5JL7wOSBPLsd3nGu9tz';
}
