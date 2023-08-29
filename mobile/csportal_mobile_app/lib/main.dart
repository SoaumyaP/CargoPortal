import 'dart:async';

import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_crashlytics/firebase_crashlytics.dart';
import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'configuration/environment/environment-config-base.dart';
import 'system/update-checker/update_checker.service.dart';
import 'warehouse/warehouse_main.screen.dart';

import 'package:package_info_plus/package_info_plus.dart';

Future<void> main() async {
  await runZonedGuarded(() async {
    WidgetsFlutterBinding.ensureInitialized();
    await Firebase.initializeApp();
    FlutterError.onError = FirebaseCrashlytics.instance.recordFlutterError;

    const String environment = String.fromEnvironment(
      'ENVIRONMENT',
      defaultValue: Environment.LOCAL,
    );

    Environment().initConfig(environment);

    runApp(MainApp());
  }, (error, stackTrace) {
    FirebaseCrashlytics.instance.recordError(error, stackTrace);
  });

  // WidgetsFlutterBinding.ensureInitialized();

  // const String environment = String.fromEnvironment(
  //   'ENVIRONMENT',
  //   defaultValue: Environment.DEV,
  // );

  // Environment().initConfig(environment);

  // runApp(MainApp());
}

class MainApp extends StatelessWidget {
  final Future<FirebaseApp> _fbApp = Firebase.initializeApp();

  MainApp({Key? key}) : super(key: key);

  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Warehouse Receive',
      theme: ThemeData(
          // This is the theme of your application.
          //
          // Try running your application with "flutter run". You'll see the
          // application has a blue toolbar. Then, without quitting the app, try
          // changing the primarySwatch below to Colors.green and then invoke
          // "hot reload" (press "r" in the console where you ran "flutter run",
          // or simply save your changes to "hot reload" in a Flutter IDE).
          // Notice that the counter didn't reset back to zero; the application
          // is not restarted.
          primarySwatch: Colors.blue),
      home: FutureBuilder(
        future: _fbApp,
        builder: (context, snapshot) {
          if (snapshot.hasError) {
            return Center(
                child: Text(
              'Something went wrong',
              style: TextStyle(color: Theme.of(context).errorColor),
            ));
          } else if (snapshot.hasData) {
            return const MainAppScreen();
          } else {
            return Container(
              color: Colors.white,
              child: const Center(
                child: CircularProgressIndicator(color: Colors.blue),
              ),
            );
          }
        },
      ),
    );
  }
}

class MainAppScreen extends StatefulWidget {
  const MainAppScreen({Key? key}) : super(key: key);

  // This widget is the home page of your application. It is stateful, meaning
  // that it has a State object (defined below) that contains fields that affect
  // how it looks.

  // This class is the configuration for the state. It holds the values (in this
  // case the title) provided by the parent (in this case the App widget) and
  // used by the build method of the State. Fields in a Widget subclass are
  // always marked "final".

  @override
  State<MainAppScreen> createState() => _MainAppScreenState();
}

class _MainAppScreenState extends State<MainAppScreen> {
  PackageInfo _packageInfo = PackageInfo(
    appName: '--',
    packageName: '--',
    version: '--',
    buildNumber: '--',
  );

  @override
  void initState() {
    super.initState();
    _initPackageInfo();
    _checkForUpdate$();
    // openFile();
  }

  @override
  Widget build(BuildContext context) {
    // This method is rerun every time setState is called, for instance as done
    // by the _incrementCounter method above.
    //
    // The Flutter framework has been optimized to make rerunning build methods
    // fast, so that you can just rebuild anything that needs updating rather
    // than having to individually change instances of widgets.
    return Scaffold(
        appBar: AppBar(
          // prevent from auto adding back button on appbar
          automaticallyImplyLeading: false,
          // Here we take the value from the MyHomePage object that was created by
          // the App.build method, and use it to set our appbar title.
          title: Image.asset(
            'assets/images/main_app.png',
            width: 100,
            fit: BoxFit.fitHeight,
          ),
          centerTitle: true,
        ),
        body: contextBodyBuild());
  }

  Widget contextBodyBuild() {
    return Column(
      children: [
        const WarehouseMainScreen(),
        const Spacer(),
        Text('ver. ${_packageInfo.version}')
      ],
    );
  }

  Future<void> _initPackageInfo() async {
    final info = await PackageInfo.fromPlatform();
    FirebaseCrashlytics.instance.setCustomKey('App name', info.appName);
    FirebaseCrashlytics.instance.setCustomKey('App version', info.version);
    setState(() {
      _packageInfo = info;
    });
  }

  Future<void> _checkForUpdate$() async {
    // Call update checker function
    await UpdateCheckerService().fireUpdateCheckerDialog$(context);
  }
}
