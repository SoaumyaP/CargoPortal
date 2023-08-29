class UpdateCheckerMobileModel {
  /// Next action
  late UpdateCheckerResult result;

  /// Message that comments for next action
  late String message;

  /// A new version if available
  String? newVersion;

  /// Package name if new version is available
  String? packageName;

  /// Package url to download if new version is available
  String? packageUrl;

  UpdateCheckerMobileModel.fromJson(Map<String, dynamic> json)
      : result = json['result'] == 1
            ? UpdateCheckerResult.upToDate
            : (json['result'] == 2
                ? UpdateCheckerResult.newUpdate
                : UpdateCheckerResult.forceUpdate),
        newVersion = json['newVersion'],
        message = json['message'],
        packageName = json['packageName'],
        packageUrl = json['packageUrl'];
}

enum UpdateCheckerResult {
  // 1
  upToDate,
  // 2
  newUpdate,
  // 3
  forceUpdate
}
