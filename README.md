Love Seat is a simply architected CouchDB wrapper with the intent to abstract away just 
enough so that it's easy to use, but not enough so that you don't know what's going on.

Tested with 1.0.1

LoveSeat Basics
=================

**Everything in LoveSeat starts with a CouchClient.**

    var client = new CouchClient(); //assumes localhost:5984 with no credentials if left blank

**From here you can get the database**

    var db= client.GetDatabase("Northwind");
    
    //Set the default design doc (not required and can be overriden
    
    db.SetDefaultDesignDoc("foo"); 
    
    //Get a Document By Id
    db.GetDocument("12345"); 

**Get a view results and populate your Domain object**

    var results = db.View<MyObject>("view_name");
    
    //...or with parameters
    
    var options = new ViewOptions{Limit=10};
    options.SetStartKey("abc");
    results = db.View<MyObject>("view_name", options);
    
    //loop through your strongly typed results
    
    foreach (var item in results.Items){ 
    // do something 
    
    }

**Get the results of a List**

    var results = db.List("list_name")

LoveSeat Supports Replication and User Management off of the CouchClient as well.  Enjoy!



