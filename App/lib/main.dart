import 'dart:async';
// import 'dart:js';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'dart:convert' as convert;

void main() {
  runApp(MyApp());
}

class MyApp extends StatelessWidget {
  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'SIGUI',
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
        primarySwatch: Colors.blue,
      ),
      home: MyHomePage(title: 'SIGUI'),
    );
  }
}

class MyHomePage extends StatefulWidget {
  MyHomePage({Key key, this.title}) : super(key: key);

  // This widget is the home page of your application. It is stateful, meaning
  // that it has a State object (defined below) that contains fields that affect
  // how it looks.

  // This class is the configuration for the state. It holds the values (in this
  // case the title) provided by the parent (in this case the App widget) and
  // used by the build method of the State. Fields in a Widget subclass are
  // always marked "final".

  final String title;

  @override
  _MyHomePageState createState() => _MyHomePageState();
}

class MailWidgetList extends StatelessWidget{
  final List<MailWidget> mails;
  MailWidgetList({Key key, this.mails}):super(key: key);

  @override
  Widget build(BuildContext context) {
    return Column(
      children: mails,
    );
  }
}

class MailWidget extends StatelessWidget {
  final String from, subject;
  final int id;
  final Function ontap;
  MailWidget({Key key, this.id, this.from, this.subject, this.ontap}):super(key: key);

  factory MailWidget.FromJSON(Map<String, dynamic> json,{Function ontapin, Key key, BuildContext context})
  {
    // var clausura=ontapin;
    // if(ontapin==null)
    // {
    //   clausura=
    // }
    var result= MailWidget(
      id:json['docID'],
      from:json['from'].toString(),
      subject: json['subject'].toString(),
      ontap: (){
        Navigator.push(context,
        MaterialPageRoute(
          builder: (context)=>DocumentViewWidget(docID: json['docID'])
        )
        );
      },
      key: key,
    );
    return result;
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: ontap,
      child: Row(
      mainAxisAlignment: MainAxisAlignment.spaceAround,
      children: [
        Icon(Icons.mail,
        size: 100.0,),
        Column(
          mainAxisAlignment: MainAxisAlignment.spaceAround,
          children: [
            Text("From: $from",
            textScaleFactor: 1.2,),
            Text("Subject: $subject",
            textScaleFactor: 1.2,)
          ],
          ),
      ],
      )
      );
  }
  
}

Future<MailWidgetList> fetchMail(String query,{BuildContext context}) async {
  final response =
      await http.get(Uri.http('10.0.2.2:5000', 'api/document/Relevancia/Documentos/$query'));

  if (response.statusCode == 200) {
    // If the server did return a 200 OK response,
    // then parse the JSON.
    var json=convert.jsonDecode(response.body);
    List<MailWidget> lista=List.of({});
    for (var item in json) {
      lista.add(MailWidget.FromJSON(item, context: context,));
    }
    return MailWidgetList(mails:lista);
  } else {
    // If the server did not return a 200 OK response,
    // then throw an exception.
    throw Exception('Failed to load Results');
  }
}

Future<Widget> fetchDoc(int id) async {
  final response =
      await http.get(Uri.http('10.0.2.2:5000', 'api/document/$id'));

  if (response.statusCode == 200) {
    // If the server did return a 200 OK response,
    // then parse the JSON.
    return Text(response.body);
  } else {
    // If the server did not return a 200 OK response,
    // then throw an exception.
    return Text("Error");
  }
}

class DocumentViewWidget extends StatelessWidget
{
  final int docID;
  DocumentViewWidget({Key key, this.docID}):super(key: key);


  @override
  Widget build(BuildContext context) {
    var document=fetchDoc(docID);
    return Scaffold(
      appBar: AppBar(
        // Here we take the value from the MyHomePage object that was created by
        // the App.build method, and use it to set our appbar title.
        title: Text("Correo"),
      ),
      body: Center(
        child: ListView(
          children: [
            FutureBuilder(
                future: document,
                builder: (context, snapshot) {
              if (snapshot.hasData) {
                return snapshot.data;
              } else if (snapshot.hasError) {
                return Text("${snapshot.error}");
              }

              // By default, show a loading spinner.
              return Text("Loading...");
            },
            )
          ],
        ),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: ()=>Navigator.pop(context),
        tooltip: 'Back',
        child: Icon(Icons.arrow_back),
      ),
    );
    throw UnimplementedError();
  }
  
}

class _MyHomePageState extends State<MyHomePage> {
  int _counter = 0;
  String _query="";
  Future<MailWidgetList> futureMail;

  void _incrementCounter() {
    setState(() {
      // This call to setState tells the Flutter framework that something has
      // changed in this State, which causes it to rerun the build method below
      // so that the display can reflect the updated values. If we changed
      // _counter without calling setState(), then the build method would not be
      // called again, and so nothing would appear to happen.
      _counter++;
      _query="foot";
      futureMail=fetchMail(_query);
    });
  }
  void _resetCounter() {
    setState(() {
      futureMail=fetchMail(_query, context:context);
    });
  }
  // void _setQuery() {
  //   setState(() {
  //     _query="fuck";
  //   });
  // }
  void _decrementCounter() {
    setState(() {
      _counter--;
    });
  }
  void searchQuery(String texto,{BuildContext context}) {
    setState(() {
      _query=texto;
      futureMail=fetchMail(_query, context:context);
    });
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
        // Here we take the value from the MyHomePage object that was created by
        // the App.build method, and use it to set our appbar title.
        title: Text(widget.title),
      ),
      body: Center(
        
        // Center is a layout widget. It takes a single child and positions it
        // in the middle of the parent.
        child: ListView(
                scrollDirection: Axis.vertical,
                // children: [
                //   Text("Elemento1"),
                //   Text("Elemento2"),
                //   Text("Elemento3"),
                //   Text("Elemento4"),
                //   Text("Elemento5"),
                //   Text("Elemento6"),
                //   Text("Elemento7"),
                //   Text("Elemento8"),
                //   Text("Elemento9"),
                //   Text("Elemento10"),
                //   Text("Elemento11"),
                //   Text("Elemento12"),
                //   Text("Elemento13"),
                //   Text("Elemento14"),
                //   Text("Elemento15"),],
                // ),
        // child: Column(
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
          // mainAxisAlignment: MainAxisAlignment.center,
          children: <Widget>[
            Column(
              crossAxisAlignment: CrossAxisAlignment.center,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Container(
                  margin: EdgeInsets.all(40.0),
                  padding: EdgeInsets.symmetric(vertical:8.0, horizontal: 8.0),
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(8.0),
                    color: Colors.blue.withOpacity(0.1),
                  ),
                  child: TextFormField(
                  // onSaved: (String saved)=> _incrementCounter(),
                  onFieldSubmitted: (String saved) => searchQuery(saved, context: context),
                  ),
                  ),
                
            //     Text(
            //   'You have pushed the button this many times:',
            // ),
            // Text(
            //   '$_counter',
            //   style: Theme.of(context).textTheme.headline4,
            // ),
            // Row(
            //   mainAxisAlignment: MainAxisAlignment.spaceAround,
            //   children: [
            //     ElevatedButton(
            //   onPressed: _decrementCounter,
            //   child: Text(
            //     "Decrement Counter"
            //   ),
            //   ),
            //   ElevatedButton(
            //   onPressed: _incrementCounter,
            //   child: Text(
            //     "Increment Counter"
            //   ),
            //   ),
            // ],),
              ],
              ),
              FutureBuilder(
                future: futureMail,
                builder: (context, snapshot) {
              if (snapshot.hasData) {
                return snapshot.data;
              } else if (snapshot.hasError) {
                return Text("${snapshot.error}");
              }

              // By default, show a loading spinner.
              return Center(child: Text("Results"),);
            },
                ),
              // MailWidget(
              //   id:1,
              //   from:"yo",
              //   subject:"Probando",
              //   ontap: _incrementCounter,
              //   ),
            
          ],
        ),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: _resetCounter,
        tooltip: 'Reset counter',
        child: Icon(Icons.search),
      ), // This trailing comma makes auto-formatting nicer for build methods.
    );
  }
}
