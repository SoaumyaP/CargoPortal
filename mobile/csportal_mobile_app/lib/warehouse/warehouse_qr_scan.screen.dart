import 'dart:developer';
import 'dart:io';

import 'package:csportal_mobile_app/main.dart';
import 'package:csportal_mobile_app/warehouse/warehouse_cargoreceive.screen.dart';
import 'package:flutter/material.dart';
import 'package:qr_code_scanner/qr_code_scanner.dart';

class WarehouseQRScanScreen extends StatefulWidget {
  WarehouseQRScanScreen({Key? key}) : super(key: key) {}

  @override
  State<StatefulWidget> createState() => _WarehouseQRScanScreenState();
}

class _WarehouseQRScanScreenState extends State<WarehouseQRScanScreen> {
  Barcode? result;
  QRViewController? controller;
  final GlobalKey qrKey = GlobalKey(debugLabel: 'QR');

  // In order to get hot reload to work we need to pause the camera if the platform
  // is android, or resume the camera if the platform is iOS.
  @override
  void reassemble() {
    super.reassemble();
    if (Platform.isAndroid) {
      controller!.pauseCamera();
    }
    controller!.resumeCamera();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: null,
      body: Column(
        children: <Widget>[
          Expanded(flex: 4, child: _buildQrView(context)),
          Expanded(
            flex: 1,
            child: FittedBox(
              fit: BoxFit.contain,
              child: Column(
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                children: <Widget>[
                  if (result != null)
                    Container(
                      margin: EdgeInsets.all(8),
                      child: Text('QR code found',
                          style: TextStyle(
                              fontSize: 13,
                              color: Theme.of(context).primaryColor)),
                    )
                  else
                    Container(
                      margin: EdgeInsets.all(8),
                      child: Text('Position QR code in this frame',
                          style: TextStyle(
                              fontSize: 13,
                              color: Theme.of(context).primaryColor)),
                    ),
                  Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      crossAxisAlignment: CrossAxisAlignment.center,
                      children: <Widget>[
                        Container(
                          margin: const EdgeInsets.all(8),
                          child: ElevatedButton(
                              style: ElevatedButton.styleFrom(
                                  primary: Theme.of(context)
                                      .colorScheme
                                      .inversePrimary,
                                  onPrimary: Theme.of(context)
                                      .colorScheme
                                      .inverseSurface,
                                  minimumSize: Size(150.0, 50.0)),
                              onPressed: () => _goBackToMainPageScreen(context),
                              child: const Text('Back')),
                        ),
                        Container(
                          margin: const EdgeInsets.all(8),
                          child: ElevatedButton(
                              style: ElevatedButton.styleFrom(
                                  primary: Theme.of(context).primaryColor,
                                  onSurface: Colors.white,
                                  minimumSize: Size(150.0, 50.0)),
                              onPressed: () async {
                                await controller?.toggleFlash();
                                setState(() {});
                              },
                              child: FutureBuilder(
                                future: controller?.getFlashStatus(),
                                builder: (context, snapshot) {
                                  return Text(
                                      'Flash ${snapshot.data.toString() == 'false' ? 'Off' : 'On'}');
                                },
                              )),
                        )
                      ])
                ],
              ),
            ),
          )
        ],
      ),
    );
  }

  Widget _buildQrView(BuildContext context) {
    // For this example we check how width or tall the device is and change the scanArea and overlay accordingly.
    var scanArea = (MediaQuery.of(context).size.width < 400 ||
            MediaQuery.of(context).size.height < 400)
        ? 150.0
        : 300.0;
    // To ensure the Scanner view is properly sizes after rotation
    // we need to listen for Flutter SizeChanged notification and update controller
    return QRView(
      key: qrKey,
      onQRViewCreated: _onQRViewCreated,
      overlay: QrScannerOverlayShape(
          borderColor: Theme.of(context).primaryColor,
          borderRadius: 10,
          borderLength: 30,
          borderWidth: 10,
          cutOutSize: scanArea),
      onPermissionSet: (ctrl, p) => _onPermissionSet(context, ctrl, p),
    );
  }

  void _onQRViewCreated(QRViewController controller) {
    setState(() {
      this.controller = controller;
      controller.resumeCamera();
    });
    controller.scannedDataStream.listen((scanData) {
      setState(() {
        controller.stopCamera();
        result = scanData;
        _openWarehouseCargoReceiveScreen();
      });
    });
  }

  void _onPermissionSet(BuildContext context, QRViewController ctrl, bool p) {
    if (!p) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('no Permission')),
      );
    }
  }

  @override
  void dispose() {
    controller?.dispose();
    super.dispose();
  }

  void _openWarehouseCargoReceiveScreen() {
    Navigator.of(context).push(MaterialPageRoute(
        builder: (context) => WarehouseCargoReceiveScreen(code: result!)));
  }

  void _goBackToMainPageScreen(BuildContext context) {
    Navigator.of(context)
        .push(MaterialPageRoute(builder: (context) => MainAppScreen()));
  }
}
