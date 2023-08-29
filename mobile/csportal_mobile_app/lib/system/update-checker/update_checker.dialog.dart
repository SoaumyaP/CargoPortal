import 'dart:io';
import 'dart:ui';

import 'package:flutter/material.dart';
import 'package:open_file/open_file.dart';
import 'package:path_provider/path_provider.dart';
import 'package:percent_indicator/circular_percent_indicator.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../core/constants.dart';
import '../../core/http.service.dart';
import 'update_checker.model.dart';

class UpdateCheckerDialog extends StatefulWidget {
  final String title = 'Cargo Receive Update!';
  final String yesButtonText = 'UPDATE';
  final String noButtonText = 'NO, THANKS';
  late UpdateCheckerMobileModel model;

  UpdateCheckerDialog({Key? key, required this.model}) : super(key: key);

  @override
  State<UpdateCheckerDialog> createState() => _UpdateCheckerDialogState();
}

class _UpdateCheckerDialogState extends State<UpdateCheckerDialog> {
  UpdateCheckerDialogStage stage = UpdateCheckerDialogStage.initialize;
  double downloadPercentage = 0;
  late SharedPreferences _sPreferences;

  @override
  void initState() {
    SharedPreferences.getInstance().then((value) => {_sPreferences = value});
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return WillPopScope(
      onWillPop: () {
        return Future.value(false);
      },
      child: BackdropFilter(
          filter: ImageFilter.blur(sigmaX: 6, sigmaY: 6),
          child: AlertDialog(
              title: Text(widget.title),
              content: _contentBuilder(),
              actions: _actionsBuilder())),
    );
  }

  /// To build dialog context
  Widget? _contentBuilder() {
    switch (stage) {
      case UpdateCheckerDialogStage.initialize:
        return Text(widget.model.message);
      case UpdateCheckerDialogStage.downloading:
        var percentLiteral =
            '${(downloadPercentage * 100).toStringAsFixed(0)}%';
        return CircularPercentIndicator(
          radius: 46.0,
          lineWidth: 8.0,
          animation: false,
          percent: downloadPercentage,
          center: Text(
            percentLiteral,
            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16.0),
          ),
          footer: const Padding(
            padding: EdgeInsetsDirectional.only(top: 20),
            child: Text(
              'Downloading...',
              style: TextStyle(fontWeight: FontWeight.bold, fontSize: 12.0),
            ),
          ),
          circularStrokeCap: CircularStrokeCap.round,
          progressColor: Theme.of(context).primaryColor,
        );
      default:
        return null;
    }
  }

  /// To build action buttons
  List<Widget>? _actionsBuilder() {
    if (stage == UpdateCheckerDialogStage.downloading) {
      return [];
    }
    return [
      if (widget.model.result == UpdateCheckerResult.newUpdate)
        TextButton(
          onPressed: noButtonPressed,
          child: Text(widget.noButtonText),
        ),
      TextButton(
        onPressed: yesButtonPressed,
        child: Text(widget.yesButtonText),
      )
    ];
  }

  /// To handle as pressed Yes button
  Future<void> yesButtonPressed() async {
    setState(() {
      stage = UpdateCheckerDialogStage.downloading;
    });

    var packageName = widget.model.packageName!;
    var packageUrl = widget.model.packageUrl!;

    final path = await _localApplicationDirectoryPath;
    final filePath = '$path/updates/$packageName';

    await _sPreferences.setString(UpdateCheckerConstant.versionInstalling,
        '${widget.model.newVersion!};$filePath');

    _proceedApplicationUpdate$(packageUrl, filePath);
  }

  /// To handle as pressed No button
  Future<void> noButtonPressed() async {
    await _sPreferences.setString(UpdateCheckerConstant.lastCheckDate,
        DateTime.now().toUtc().toIso8601String());

    await _sPreferences
        .setString(
            UpdateCheckerConstant.versionSkipped, widget.model.newVersion!)
        .then((value) => {Navigator.of(context).pop()});
  }

  /// Get folder of application
  Future<String> get _localApplicationDirectoryPath async {
    final directory = await getApplicationDocumentsDirectory();
    return directory.path;
  }

  /// Proceed application update progress
  Future<void> _proceedApplicationUpdate$(
      String packageUrl, String filePathToStore) async {
    // download new version package
    final fileToStore = File(filePathToStore);
    var storedFilePath = await HttpService().downloadFile(
        packageUrl, fileToStore.path, updateDownloadingProgressCallback);

    // open .apk file to install
    if (storedFilePath?.isNotEmpty ?? false) {
      await OpenFile.open(fileToStore.path);
    }
  }

  /// downloading progress callback to update GUI
  void updateDownloadingProgressCallback(int currentValue, int totalValue) {
    double percentage = currentValue / totalValue;
    setState(() {
      downloadPercentage = percentage;
    });
  }
}

enum UpdateCheckerDialogStage { initialize, downloading, installing, failed }
