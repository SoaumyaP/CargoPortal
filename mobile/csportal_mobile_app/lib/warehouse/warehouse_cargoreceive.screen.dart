import 'dart:async';

import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:csportal_mobile_app/main.dart';
import 'package:csportal_mobile_app/warehouse/models/warehouse_cargoreceive.model.dart';
import 'package:csportal_mobile_app/warehouse/warehouse.service.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:intl/intl.dart';
import 'package:qr_code_scanner/qr_code_scanner.dart';

import 'models/warehouse_booking.model.dart';

class WarehouseCargoReceiveScreen extends StatefulWidget {
  late Barcode _code;

  WarehouseCargoReceiveScreen({Key? key, required Barcode code})
      : super(key: key) {
    _code = code;
  }

  @override
  State<WarehouseCargoReceiveScreen> createState() =>
      _WarehouseCargoReceiveScreenState();
}

class _WarehouseCargoReceiveScreenState
    extends State<WarehouseCargoReceiveScreen> {
  ///To request to server for warehouse details (location)
  late Future<WarehouseCargoReceiveModel> _cargoReceiveRequest$;

  ///Model of screen
  late WarehouseBookingModel model;

  ///Indicate if it is currently requesting to server
  bool isRequestProcessing = false;

  final Connectivity _connectivity = Connectivity();
  late StreamSubscription<ConnectivityResult> _connectivitySubscription;

  final noConnectionSnackbar = SnackBar(
    content: Text(
      'No connection. You are offline.',
    ),
    backgroundColor: Colors.orange,
    duration: Duration(days: 1),
  );

  @override
  initState() {
    super.initState();
    model = WarehouseBookingModel(widget._code.code);
    if (model.isValid()) {
      _cargoReceiveRequest$ = WarehouseService()
          .getWarehouseCargoReceive(model.bookingNumber)
          .then((value) => _cargoReceiveRequestCallback(value));
    }
    initConnectivity();

    _connectivitySubscription =
        _connectivity.onConnectivityChanged.listen(_updateConnectionStatus);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        leading: BackButton(
            color: Colors.white,
            onPressed: () => _goBackMainPageScreen(context)),
        title: Image.asset(
          'assets/images/main_app.png',
          width: 100,
          fit: BoxFit.fitHeight,
        ),
        centerTitle: true,
      ),
      body: _screenContentBuilder(model),
    );
  }

  @override
  void dispose() {
    _connectivitySubscription.cancel();
    super.dispose();
  }

  Widget _screenContentBuilder(WarehouseBookingModel model) {
    double textPaddingLeftRight = 25;
    if (!model.isValid()) {
      return WillPopScope(
          onWillPop: () async {
            _goBackMainPageScreen(context);
            return false;
          },
          child: Center(
            child: Column(
                mainAxisAlignment: MainAxisAlignment.start,
                children: <Widget>[
                  Container(
                    margin: EdgeInsets.fromLTRB(
                        textPaddingLeftRight, 50, textPaddingLeftRight, 30),
                    child: Icon(
                      Icons.error_outline,
                      size: 120,
                      color: Theme.of(context).errorColor,
                    ),
                  ),
                  Container(
                    margin: EdgeInsets.all(8),
                    child: Text("Sorry,",
                        style: TextStyle(
                            color: Theme.of(context).errorColor,
                            fontSize: 15.0)),
                  ),
                  Container(
                    margin: EdgeInsets.all(8),
                    child: Text(
                        "It is not valid Warehouse Code, please try again.",
                        style: TextStyle(
                            color: Theme.of(context).errorColor,
                            fontSize: 15.0)),
                  )
                ]),
          ));
    }

    //To setup event for Android system's default back button.
    return WillPopScope(
      onWillPop: () async {
        _goBackMainPageScreen(context);
        return false;
      },
      child: FutureBuilder<WarehouseCargoReceiveModel>(
          future: _cargoReceiveRequest$,
          builder: (context, snapshot) {
            var receivedDate = model.cargoReceivedDate ?? DateTime.now();
            var formattedDate = (model.stage ?? 10) >= 30
                ? DateFormat('dd MMM, yyyy hh:mm a')
                    .format(receivedDate.toLocal())
                : '--';

            return Center(
                child: Column(
              mainAxisAlignment: MainAxisAlignment.start,
              children: <Widget>[
                //Indicate of current request is processing
                if (snapshot.connectionState != ConnectionState.done ||
                    isRequestProcessing)
                  LinearProgressIndicator(
                    valueColor: AlwaysStoppedAnimation<Color>(
                        Theme.of(context).primaryColor),
                  ),
                Container(
                    margin: EdgeInsets.fromLTRB(
                        textPaddingLeftRight, 50, textPaddingLeftRight, 50),
                    child: Text(
                      'Cargo receive information',
                      style: TextStyle(
                          color: Theme.of(context).colorScheme.primary,
                          fontWeight: FontWeight.w700,
                          fontSize: 18.0),
                    )),
                Container(
                    margin: EdgeInsets.fromLTRB(
                        textPaddingLeftRight, 8, textPaddingLeftRight, 8),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Text('Booking#:'),
                        Container(
                            margin: EdgeInsets.only(left: 10.0),
                            child: Text(
                              '${model.bookingNumber}',
                              style: TextStyle(fontWeight: FontWeight.bold),
                            ))
                      ],
                    )),
                Container(
                    margin: EdgeInsets.fromLTRB(
                        textPaddingLeftRight, 8, textPaddingLeftRight, 8),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Text('SO#:'),
                        Container(
                            margin: EdgeInsets.only(left: 10.0),
                            child: Text(
                              '${model.shipmentNumber}',
                              style: TextStyle(fontWeight: FontWeight.bold),
                            ))
                      ],
                    )),
                Container(
                    margin: EdgeInsets.fromLTRB(
                        textPaddingLeftRight, 8, textPaddingLeftRight, 8),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Text('Received Date:'),
                        Container(
                            margin: EdgeInsets.only(left: 10.0),
                            child: Text(
                              '$formattedDate',
                              style: TextStyle(fontWeight: FontWeight.bold),
                            ))
                      ],
                    )),
                Container(
                    margin: EdgeInsets.fromLTRB(
                        textPaddingLeftRight, 8, textPaddingLeftRight, 8),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text('Warehouse:'),
                        Flexible(
                            child: Container(
                                margin: EdgeInsets.only(left: 10.0),
                                child: Text(
                                  '${model.warehouseLocation ?? '--'}',
                                  style: TextStyle(fontWeight: FontWeight.bold),
                                ))),
                      ],
                    )),
                //Show command buttons only if valid
                if (model.cargoReceivedDate == null &&
                    (model.stage ?? 10) >= 30)
                  Container(
                    margin: EdgeInsets.fromLTRB(8, 50, 8, 8),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceAround,
                      children: [
                        ElevatedButton(
                            style: ElevatedButton.styleFrom(
                                primary: Theme.of(context)
                                    .colorScheme
                                    .inversePrimary,
                                onPrimary: Theme.of(context)
                                    .colorScheme
                                    .inverseSurface,
                                minimumSize: Size(150.0, 50.0)),
                            onPressed: () => _goBackMainPageScreen(context),
                            child: const Text('Back')),
                        if (isRequestProcessing)
                          ElevatedButton(
                              style: ElevatedButton.styleFrom(
                                  primary: Colors.grey[350],
                                  onSurface: Colors.grey[200],
                                  minimumSize: Size(150.0, 50.0)),
                              onPressed: () => {},
                              child: const Text('Confirm'))
                        else
                          ElevatedButton(
                              style: ElevatedButton.styleFrom(
                                  primary: Theme.of(context).primaryColor,
                                  onSurface: Colors.white,
                                  minimumSize: Size(150.0, 50.0)),
                              onPressed: () =>
                                  {_receiveCargoForWarehouseBooking(context)},
                              child: const Text('Confirm'))
                      ],
                    ),
                  )
              ],
            ));
          }),
    );
  }

  Future _receiveCargoForWarehouseBooking(context) async {
    setState(() {
      isRequestProcessing = true;
    });

    var result =
        await WarehouseService().putWarehouseCargoReceive(model.bookingNumber);
    var newModel = model;

    newModel.cargoReceivedDate = DateTime.tryParse(result);
    setState(() {
      model = newModel;
      isRequestProcessing = false;
    });

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(
          'The cargo has been received successfully.',
        ),
        backgroundColor: Colors.lightGreen,
      ),
    );
  }

  void _goBackMainPageScreen(BuildContext context) {
    ScaffoldMessenger.of(context).removeCurrentSnackBar();
    Navigator.of(context)
        .push(MaterialPageRoute(builder: (context) => MainAppScreen()));
  }

  _cargoReceiveRequestCallback(WarehouseCargoReceiveModel value) {
    {
      model.stage = value.stage;
      model.warehouseLocation = value.location;

      if (value.stage < 30) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
              content: Text(
                'Booking stage is not confirmed yet.',
              ),
              backgroundColor: Colors.orange,
              duration: Duration(seconds: 5)),
        );
        return;
      }

      model.cargoReceivedDate = value.cargoReceivedDate;

      if (model.cargoReceivedDate != null) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
              content: Text(
                'The cargo has already been received.',
              ),
              backgroundColor: Colors.orange,
              duration: Duration(seconds: 5)),
        );
      }
    }
  }

  // Platform messages are asynchronous, so we initialize in an async method.
  Future<void> initConnectivity() async {
    late ConnectivityResult result;
    // Platform messages may fail, so we use a try/catch PlatformException.
    try {
      result = await _connectivity.checkConnectivity();
    } on PlatformException catch (e) {
      // developer.log('Couldn\'t check connectivity status', error: e);
      return;
    }

    // If the widget was removed from the tree while the asynchronous platform
    // message was in flight, we want to discard the reply rather than calling
    // setState to update our non-existent appearance.
    if (!mounted) {
      return Future.value(null);
    }

    return _updateConnectionStatus(result);
  }

  Future<void> _updateConnectionStatus(ConnectivityResult result) async {
    switch (result) {
      case ConnectivityResult.bluetooth:
      case ConnectivityResult.none:
        ScaffoldMessenger.of(context).showSnackBar(noConnectionSnackbar);
        break;
      case ConnectivityResult.ethernet:
      case ConnectivityResult.wifi:
      case ConnectivityResult.mobile:
        ScaffoldMessenger.of(context).removeCurrentSnackBar();
        // ScaffoldMessenger.of(context).showSnackBar(
        //   SnackBar(
        //     content: Text(
        //       'The cargo has already been received.',
        //     ),
        //     backgroundColor: Colors.green,
        //     duration: Duration(seconds: 5),
        //   ),
        // );
        break;
    }
  }
}
