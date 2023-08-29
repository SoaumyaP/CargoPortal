import 'dart:async';

import 'package:csportal_mobile_app/configuration/environment/environment-config-base.dart';
import 'package:dio/dio.dart';

import 'package:http/http.dart' as http;
import 'package:retry/retry.dart';

class HttpService {
  final String baseCsPortalApi = Environment().config.csportalApi;
  final String appAgent = Environment().config.appAgent;
  final String secureSecret = Environment().config.secureSecret;

  Future<http.Response> get(String url) {
    var coreHeaders = {"App-Agent": appAgent, "Secure-Secret": secureSecret};
    if (url.isEmpty) {
      throw 'Request url $url is not valid';
    }
    var response =
        http.get(Uri.parse('$baseCsPortalApi/$url'), headers: coreHeaders);
    return response;
  }

  Future<http.Response> post(String url) {
    var coreHeaders = {"App-Agent": appAgent, "Secure-Secret": secureSecret};
    if (url.isEmpty) {
      throw 'Request url $url is not valid';
    }
    var response =
        http.post(Uri.parse('$baseCsPortalApi/$url'), headers: coreHeaders);
    return response;
  }

  /// Download file with re-try.
  /// @url: link to download file
  /// @filePath: location to store file
  /// onReceiveProgress: callback as receiving file context
  Future<String?> downloadFile(String url, String filePath,
      Function(int, int)? onReceiveProgress) async {
    final result = await retry(() async {
      await Dio().download(url, filePath, onReceiveProgress: onReceiveProgress);
      return filePath;
    });
    return result;
  }
}
