LoveSeat
========

Love Seat is a simply architected [CouchDB](http://couchdb.apache.org/) C# client with the 
intent to abstract away just enough so that it's easy to use, but not enough so that you 
don't know what's going on.


Tested compatibility
====================

 * CouchDB 1.0.1
 * .NET Framework 4.0 or Mono 2.9 (compiled master branch from Nov 20 2010)


LoveSeat usage
==============

**Basics**

    // assumes localhost:5984 with no credentials if constructor is left blank
    var client = new CouchClient();
    var db= client.GetDatabase("Northwind");
    
    // set default design doc (not required)
    db.SetDefaultDesignDoc("docs"); 
    
    // get document by ID
    Document myDoc = db.GetDocument("12345");
    
    // get document by ID (strongly typed POCO version)
    MyObject myObj = db.GetDocument<MyObject>("12345"); 

**Simple view results**

    // get view results
    var results = db.View<MyObject>("view_name");
    
    // get view results with parameters
    var options = new ViewOptions();
    options.StartKey.Add("Atlanta");
    options.EndKey.Add("Washington");
    
    var results = db.View<MyObject>("view_name", options);
    
    // loop through strongly typed results
    foreach (var item in results.Items){ 
        // do something 
    
    }

**Generating more complex view parameters**

    var options = new ViewOptions();
    // generate ["foo"] startkey parameter
    options.StartKey.Add("foo");
    // generate ["foo",{},{}] endkey parameter
    options.EndKey.Add("foo", CouchValue.Empty, CouchValue.Empty);
    
    var results = db.View<MyObject>("view_name", options);
    
    // loop through strongly typed results
    foreach (var item in results.Items){ 
        // do something 
    
    }

**Customized view key parameters**

Assuming that view keys have complex structure, for example:

["johny", ["work", "programming"]]

["joe", ["programming"]]

["johny", ["work"]]

    using Newtonsoft.Json.Linq;

    ...

    var options = new ViewOptions(); 
    options.StartKey.Add(new JRaw("[\"johny\",[\"work\"]"));
    options.EndKey.Add(new JRaw("[\"johny\",[\"work\",{}]]"));  
    
    var results = db.View<MyObject>("view_name", options);
    
    foreach (var item in results.Items){ 
        // do something 
    
    }

This example will return only rows where first key contains "johny" and second key 
contains "work".

**Get the results of a List**

    var results = db.List("list_name")

LoveSeat supports replication and user management off of the CouchClient as well.  Enjoy!



