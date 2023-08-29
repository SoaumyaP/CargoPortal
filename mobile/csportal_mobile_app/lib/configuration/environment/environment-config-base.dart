import 'package:csportal_mobile_app/configuration/environment/environment-config-dev.dart';
import 'package:csportal_mobile_app/configuration/environment/environment-config-local.dart';
import 'package:csportal_mobile_app/configuration/environment/environment-config-preproduction.dart';
import 'package:csportal_mobile_app/configuration/environment/environment-config-production.dart';
import 'package:csportal_mobile_app/configuration/environment/environment-config-qc.dart';
import 'package:csportal_mobile_app/configuration/environment/environment-config-staging.dart';

abstract class BaseEnvironmentConfig {
  String get csportalApi;
  String get appAgent;
  String get secureSecret;
}

class Environment {
  static final Environment _singleton = Environment._internal();

  factory Environment() {
    return _singleton;
  }

  Environment._internal();

  static const String LOCAL = 'local';
  static const String DEV = 'dev';
  static const String QC = 'qc';
  static const String STAGING = 'staging';
  static const String PREPRODUCTION = 'preproduction';
  static const String PRODUCTION = 'production';

  late BaseEnvironmentConfig config;

  initConfig(String environment) {
    var lowerCaseEnvironment = environment.toLowerCase();
    config = _getConfig(lowerCaseEnvironment);
  }

  BaseEnvironmentConfig _getConfig(String environment) {
    switch (environment) {
      case Environment.DEV:
        return DevEnvironmentConfig();
      case Environment.QC:
        return QCEnvironmentConfig();
      case Environment.STAGING:
        return StagingEnvironmentConfig();
      case Environment.PREPRODUCTION:
        return PreproductionEnvironmentConfig();
      case Environment.PRODUCTION:
        return ProductionEnvironmentConfig();
      default:
        return LocalEnvironmentConfig();
    }
  }
}
