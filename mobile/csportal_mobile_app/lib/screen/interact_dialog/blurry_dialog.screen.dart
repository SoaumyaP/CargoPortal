import 'dart:ui';

import 'package:flutter/material.dart';

class Blurry2ButtonDialog extends StatelessWidget {
  late String title;
  late String content;
  late String yesButtonText;
  late String noButtonText;
  late Function(Object value) actionCallback;

  Blurry2ButtonDialog(
      {Key? key,
      required this.title,
      required this.content,
      required this.yesButtonText,
      required this.noButtonText,
      required this.actionCallback})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return BackdropFilter(
        filter: ImageFilter.blur(sigmaX: 6, sigmaY: 6),
        child: AlertDialog(
          title: Text(title),
          content: Text(
            content,
          ),
          actions: <Widget>[
            TextButton(
              child: Text(noButtonText),
              onPressed: () {
                Navigator.of(context).pop();
                actionCallback('no');
              },
            ),
            TextButton(
              child: Text(yesButtonText),
              onPressed: () {
                actionCallback('yes');
                Navigator.of(context).pop();
              },
            )
          ],
        ));
  }
}

class Blurry1ButtonDialog extends StatelessWidget {
  late String title;
  late String content;
  late String yesButtonText;
  late Function actionCallback;

  Blurry1ButtonDialog(
      {Key? key,
      required this.title,
      required this.content,
      required this.yesButtonText,
      required this.actionCallback})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return BackdropFilter(
        filter: ImageFilter.blur(sigmaX: 6, sigmaY: 6),
        child: AlertDialog(
          title: Text(title),
          content: Text(
            content,
          ),
          actions: <Widget>[
            TextButton(
              child: Text(yesButtonText),
              onPressed: () {
                actionCallback();
                Navigator.of(context).pop();
              },
            )
          ],
        ));
  }
}
