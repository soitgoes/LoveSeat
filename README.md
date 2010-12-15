LoveSeat
========

Love Seat is a simply architected [CouchDB](http://couchdb.apache.org/) wrapper with the 
intent to abstract away just enough so that it's easy to use, but not enough so that you 
don't know what's going on.


Tested compatibility
====================

 * CouchDB 1.0.1
 * .NET 4.0 Framework
 * Mono 2.9 (compiled master branch form Nov 20 2010)


LoveSeat Basics
===============

**Everything in LoveSeat starts with a CouchClient.**

    var client = new CouchClient(); // assumes localhost:5984 with no credentials if left blank

**From here you can get the database**

    var db= client.GetDatabase("Northwind");
    
    // Set the default design doc (not required and can be overriden)
    db.SetDefaultDesignDoc("foo"); 
    
    // Get a Document By Id
    db.GetDocument("12345"); 

**Get a view results and populate your Domain object**

    var results = db.View<MyObject>("view_name");
    
    // ...or with parameters
    var options = new ViewOptions{Limit=10};
    options.StartKey.Add("abc");
    results = db.View<MyObject>("view_name", options);
    
    // loop through your strongly typed results
    
    foreach (var item in results.Items){ 
        // do something 
    
    }
    
**Get a view results and with more complex key parameters**

Assuming that your view keys have complex structure, for example:
["johny", ["work", "programming"]]

["joe", ["programming"]]

["johny", ["work"]]

    using Newtonsoft.Json.Linq;

    ...

    var options = new ViewOptions();
    options.StartKey.Add(new JRaw("[\"johny\",[\"work\"]"));
    options.EndKey.Add(new JRaw("[\"johny\",[\"work\",{}]]"));
    results = db.View<MyObject>("view_name", options);
    
    // loop through your strongly typed results
    
    foreach (var item in results.Items){ 
        // do something 
    
    }

This example will return only rows where first key contains "johny" and second key 
contains "work".


**Get the results of a List**

    var results = db.List("list_name")

LoveSeat Supports Replication and User Management off of the CouchClient as well.  Enjoy!



