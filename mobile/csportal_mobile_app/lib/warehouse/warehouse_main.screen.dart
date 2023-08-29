import 'package:firebase_crashlytics/firebase_crashlytics.dart';
import 'package:flutter/material.dart';
import 'warehouse_qr_scan.screen.dart';

class WarehouseMainScreen extends StatefulWidget {
  const WarehouseMainScreen({Key? key}) : super(key: key);

  @override
  State<WarehouseMainScreen> createState() => _WarehouseMainScreenState();
}

class _WarehouseMainScreenState extends State<WarehouseMainScreen> {
  @override
  Widget build(BuildContext context) {
    return Center(
      // Center is a layout widget. It takes a single child and positions it
      // in the middle of the parent.
      child: Column(
        // Column is also a layout widget. It takes a list of children and
        // arranges them vertically. By default, it sizes itself to fit its
        // children horizontally, and tries to be as tall as its parent.
        //
        // Invoke "debug painting" (press "p" in the console, choose the
        // "Toggle Debug Paint" action from the Flutter Inspector in Android
        // Studio, or the "Toggle Debug Paint" command in Visual Studio Code)
        // to see the wireframe for each widget.
        //
        // Column has various properties to control how it sizes itself and
        // how it positions its children. Here we use mainAxisAlignment to
        // center the children vertically; the main axis here is the vertical
        // axis because Columns are vertical (the cross axis would be
        // horizontal).
        mainAxisAlignment: MainAxisAlignment.start,
        children: <Widget>[
          Container(
              margin: EdgeInsets.fromLTRB(8, 50, 8, 8),
              child: TextButton(
                style: ButtonStyle(
                    foregroundColor: MaterialStateProperty.all<Color>(
                        Theme.of(context).colorScheme.primary)),
                onPressed: () {},
                child: const Text(
                  'Welcome!',
                  style: TextStyle(fontSize: 20),
                ),
              )),
          Icon(Icons.qr_code, size: 200),
          Container(
              margin: EdgeInsets.fromLTRB(8, 50, 8, 8),
              child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                      primary: Theme.of(context).primaryColor,
                      onSurface: Colors.white,
                      minimumSize: Size(150.0, 50.0)),
                  onPressed: () => {_openWarehouseQRScanScreen()},
                  child: const Text('Scan to confirm cargo received')))
        ],
      ),
    );
  }

  void _openWarehouseQRScanScreen() {
    Navigator.of(context)
        .push(MaterialPageRoute(builder: (context) => WarehouseQRScanScreen()));
  }
}
