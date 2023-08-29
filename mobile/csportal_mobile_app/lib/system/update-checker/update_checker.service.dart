import 'dart:convert';
import 'dart:io';

import 'package:csportal_mobile_app/core/constants.dart';
import 'package:flutter/material.dart';
import 'package:package_info_plus/package_info_plus.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../core/http.service.dart';
import 'update_checker.dialog.dart';
import 'update_checker.model.dart';

class UpdateCheckerService {
  /// Call request to server to check for update with current mobile version [currentVersion]
  Future<UpdateCheckerMobileModel> checkApplicationForUpdate(
      String currentVersion) async {
    var currentVersionParam = Uri.encodeComponent(currentVersion);
    var url = 'mobileApplications/updates/$currentVersionParam';
    final response = await HttpService().get(url);

    if (response.statusCode == 200) {
      Map<String, dynamic> modelMap = jsonDecode(response.body);
      var model = UpdateCheckerMobileModel.fromJson(modelMap);
      return model;
    } else {
      throw response.body;
    }
  }

  /// Call update checker function that is displayed as dialog into [context]
  Future<void> fireUpdateCheckerDialog$(BuildContext context) async {
    // In case if just installed new version, update correct checkpoint
    final SharedPreferences sPreferences =
        await SharedPreferences.getInstance();
    final String versionInstalling =
        sPreferences.getString(UpdateCheckerConstant.versionInstalling) ?? '';
    final String version = versionInstalling.split(';')[0];
    final applicationInfo = await PackageInfo.fromPlatform();

    if (applicationInfo.version == version) {
      // Update the last check utc
      await sPreferences.setString(UpdateCheckerConstant.lastCheckDate,
          DateTime.now().toUtc().toIso8601String());

      // Delete unused file
      var file = File(versionInstalling.split(';')[1]);
      file.delete();

      // Delete shared preference value
      await sPreferences.remove(UpdateCheckerConstant.versionInstalling);
    }

    /// Fire request to check for update every 4 hours
    final String lastCheckDate =
        sPreferences.getString(UpdateCheckerConstant.lastCheckDate) ??
            DateTime(1900, 1, 1).toUtc().toIso8601String();

    final checkDate = DateTime.parse(lastCheckDate);
    if (DateTime.now().toUtc().difference(checkDate).inHours >= 4) {
      final info = await PackageInfo.fromPlatform();

      var currentApplicationVersion = info.version;
      var updateResult = await UpdateCheckerService()
          .checkApplicationForUpdate(currentApplicationVersion);

      switch (updateResult.result) {

        // Nothing to do if current version is update to date
        case UpdateCheckerResult.upToDate:
          await sPreferences.setString(UpdateCheckerConstant.lastCheckDate,
              DateTime.now().toUtc().toIso8601String());

          break;

        // There is new version, it is optional update
        case UpdateCheckerResult.newUpdate:
          final String versionSkipped =
              sPreferences.getString(UpdateCheckerConstant.versionSkipped) ??
                  '';
          // No show dialog if user skipped this version before
          if (updateResult.newVersion == versionSkipped) {
            await sPreferences.setString(UpdateCheckerConstant.lastCheckDate,
                DateTime.now().toUtc().toIso8601String());
          } else {
            var updateNotificationDialog =
                UpdateCheckerDialog(model: updateResult);
            showDialog(
                context: context,
                barrierDismissible: false,
                builder: (BuildContext context) {
                  return updateNotificationDialog;
                });
          }

          break;

        // Current version is terminated, required to update
        case UpdateCheckerResult.forceUpdate:
          var updateNotificationDialog =
              UpdateCheckerDialog(model: updateResult);
          showDialog(
              context: context,
              barrierDismissible: false,
              builder: (BuildContext context) {
                return updateNotificationDialog;
              });

          break;
      }
    }
  }
}
