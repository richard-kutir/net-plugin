#Using RhoConnect Plugins with MVC .NET

*This the second of three articles on how RhoConnect can be combined with various platforms such as [Ruby on Rails](https://developer.motorolasolutions.com/community/rhomobile-suite/rhomobile-community/rhomobile-blogs/blog/2014/08/29/rhoconnect-plugins-in-ruby), .NET, and Java.*

The Rho platform was developed as a cross-platform solution that offers powerful tools for uniting enterprise data with mobile devices.  One key part of that strategy is the RhoConnect sync server platform.  RhoConnect offers an easy-to-implement way of keeping the data on the mobile device and your back end storage synchronized.

As Motorola states:

> RhoConnect is the first of a new category of “mobile app integration” servers. 
> Using RhoConnect drastically simplifies the development of connectivity to an 
> enterprise backend app. The RhoConnect server and built-in RhoConnect client in 
> the smartphone app perform all the work to get data down to the device. This 
> eliminates 50 to 80 percent of the development effort in enterprise smartphone 
> apps: performing the backend application integration.

-- http://docs.rhomobile.com/en/5.0.0/rhoconnect/introduction

There are several ways to interface your back end systems to RhoConnect (and indirectly your mobile devices).  It's up to you, as the developer, to choose the best method suited to your needs:

### 1. RhoConnect REST API

[RhoConnect has a REST API](http://docs.rhomobile.com/en/5.0.0/rhoconnect/rest-api) that allows direct and custom services to be developed with it as the platform.  The API allows you to tie in complicated back end systems by giving you direct control of the RhoConnect service.  This means your solution can be as simple or as complicated as you wish it to be.

### 2. RhoConnect Source Adapters

A [RhoConnect source adapter](http://docs.rhomobile.com/en/5.0.0/rhoconnect/source-adapters-intro) gives you a framework that encapsulates the RhoConnect API, but gives you the leeway to include your own business logic through models and controllers.  The source adapter is a good choice if you wish to have a framework to automate the actual communication with RhoConnect, yet maintain the ability to inject custom business logic into the process.

### 3. RhoConnect Plugins

From Motorola: 

> Rhoconnect Plugins allow you to connect your backend apps seamlessly with 
> rhoconnect. You can write the source (query, create, update and delete 
> operations) into a backend application, and use a RhoConnect plugin in the 
> language that matchs your backend application, such as Java or .NET."

A [Rhoconnect plugin](http://docs.rhomobile.com/en/5.0.0/rhoconnect/plugin-intro)  encapsulates the integration of your back end solution and RhoConnect in a library that handles the communication seamlessly.  You simply implement your application's models and through the plugin RhoConnect will automatically synchronize your data across your mobile devices.

![RhoConnect plugin](https://s3.amazonaws.com/rhodocs/rhoconnect-service/rhoconnect-plugin-net.jpg) 

-- Image from Motorola: (http://docs.rhomobile.com/en/5.0.0/rhoconnect/net-plugin)


In this article we're going to examine how to implement a RhoConnect plugin using Microsoft's .NET framework as the back end application.  .NET provides an excellent framework for developing enterprise applications that communicate with a variety of database engines, such as Azure and SQL Server.  The RhoConnect plugin will connect our .NET application data to a Rhodes mobile application transparently.

To demonstrate RhoConnect plugins we're going to use three different apps:
* A RhoConnect server
* A .NET MVC 4 Application that will act as our primary application and database server
* A Rhodes mobile app (using the RhoSimulator)

We're using  Windows 8.1 as the development platform in this exercise.  Visual Studio and RhoStudio are excellent platforms to develop the server and mobile applications respectively.

The tools you'll need are:
- RhoStudio (5.0)
- Visual Studio Express (2013 used in this case)

For our example we're going to set up a very simple address book.  It will allow the end user to view a set of addresses, along with names and email addresses associated with those addresses.  We will be able to update this list of names from both the mobile device and the .NET back end.  Let's get started!

## The Setup

We're going to emulate the Internet on our local computer, meaning our applications will all be on `localhost`.  In a production or staging environment your RhoConnect and MVC app will be hosted on an application server, while your Rhodes app will reside on a mobile device such as an iPhone or Android device.  

The first thing we need to do is to install the RhoMobile Suite.  This will install this will install the basic toolchain we'll need to build and run RhoMobile apps.  You can download the latest version of RhoMobile Suite at https://developer.motorolasolutions.com/community/rhomobile-suite.  Follow the installation steps and install all of the components.

Let's create a root directory and create all of our applications as subdirectories from this root.

```
mkdir plugins
cd plugins
```

The source code for all the programs created in this article are available on Github at https://github.com/richard-kutir/net-plugin.

## Our Server Application

In order to demonstrate the RhoConnect plugin we're going to create a simple MVC 4 web application using Visual Studio.  Open Visual Studio and create a new C# Web Project within our plugins folder.  Name it AddressBook.  Let's also set the default port for our debugging web server to 3000 by right clicking on the project name in our Solution Explorer and choosing Properties > Web > Servers and making our URL http://localhost:3000.  It is important to know what port the application is communicating on.  By setting it we'll know the actual port to use when we configure RhoConnect.


![Imgur](http://i.imgur.com/pvrjvMs.png)

Once you've created your project, we need to add a reference to the RhoConnect library that will allow us to connect our .NET application to our mobile client.  You can download the project from Github using:

```
 git clone https://github.com/rhomobile/rhoconnect.NET.git
```

 Or download the zip file from https://github.com/rhomobile/rhoconnect.NET/archive/master.zip

Inside the project's `bin/Release` folder is the file `RhoconnectNET.dll`.  Add a reference to your newly created MVC app to this .DLL by right-clicking on References and selecting "Add Reference..." the click the "Browse" button.  Navigate to where you've downloaded the .DLL and click "Add".  This will add the reference to your application.
 
 ![Imgur](http://i.imgur.com/pt6pJiF.png)

Now create a new Address class and scaffold the controllers and views.  We'll use this to both allow us to manage the Address database on the server-side as well as facilitate communication with the RhoConnect server.

Within our .NET application create a new Address class within the `Models` directory:

```C#
using System.Data.Entity;

namespace AddressBook.Models
{
    public class Address
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Email { get; set; }

        public class AddressDBContext : DbContext
        {
            public DbSet<Address> Addresses { get; set; }
        }
    }
}
```

This is the basic schema that we will use with our application: a name, an address, and an email address, all strings.

Using this new model use the `Add > New Scaffolded Item` to add a new `MVC Controller with views, using Entity Framework`.  Select the `Address` class as the model and use the name `AddressesController` for the controller.  This will quickly create the views and the controller to allow our model to be accessible in our application.

![Imgur](http://i.imgur.com/lxGeP3l.png)

Within our controller we implement our connection to RhoConnect by inheriting from an interface named IRhoconnectCRUD.  Add it to the class definition in AddressesController:

```#
    public class AddressesController : Controller, IRhoconnectCRUD
```

You will need to add references to the RhoConnect library:

```
using RhoconnectNET;
using RhoconnectNET.Controllers;
```

The interface requires four methods to be implemented.  They provide the basic CRUD (Create, Read, Update, Destroy) functionality that our RhoConnect server will use to communicate with the app.  The signatures for the methods are:

```C#
        jsonresult rhoconnect_query_objects(string partition);
        actionresult rhoconnect_create(string objjson, string partition);
        actionresult rhoconnect_update(dictionary<string, object> changes, string partition);
        actionresult rhoconnect_delete(object objid, string partition);
```

Note that the `rhoconnect_query_objects` method is what is called when RhoConnect wishes to perform a query on the database.  In our case the query is quite simple: return all of the Addresses:

```C#
        public JsonResult rhoconnect_query_objects(String partition)
        {
            return Json(db.Addresses.ToDictionary(a => a.ID.ToString()));
        }
```

Notice that JSON is the primary data format that is used in the communication to RhoConnect.

In order to allow RhoConnect to update a record we implement the `rhoconnect_update` method:

```C#
        public ActionResult rhoconnect_update(Dictionary<string, object> changes, String partition)
        {
            int obj_id = Convert.ToInt32(changes["id"]);
            Address address_to_update = db.Addresses.First(a => a.ID == obj_id);
            // this method will update only the modified fields
            RhoconnectNET.Helpers.merge_changes(address_to_update, changes);
            db.Entry(address_to_update).State = EntityState.Modified;
            db.SaveChanges();
            return RhoconnectNET.Helpers.serialize_result(address_to_update.ID);
        }
```


The other methods are implemented in the sample application that is included with this post.

We also must tell the MVC app how to initiate the communication with the RhoConnect server.  We do this by adding the following methods to the Global.asax.cs file:

```C#
        private void init_rhoconnect()
        {
            // this call allows parsing JSON structures into Objects
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());

            RhoconnectNET.Client.set_app_endpoint("http://localhost:9292",
                                                  "http://localhost:3000",
                                                  "my-rhoconnect-token",
                                                  rhoconnect_authenticate);
        }

        private bool rhoconnect_authenticate(ref String username, String password, Hashtable auth_attrs)
        {
            return true;
        }
```

The `init_rhoconnect()` method sets up the end point that is used to communicate with the Rhomobile application.  The call `RhoConnect.Client.set_app_endpoint` takes the URL of the RhoConnect app, the URL of the MVC app, a shared token that we will use for the connection, as well as a method for authentication.  In our case the authentication is always returning true for this simple example.

We then add the `init_rhoconnect()` method call to the end of the `Application_Start` method in Global.asax.cs.

## RhoConnect Server

Next we need to configure the actual RhoConnect server.

Open a command prompt in our root `plugins` directory and type:

```
rhoconnect app address_server
cd address_server
```

This will create our RhoConnect server application.  The Rhoconnect server will manage the communication and synchronization between our mobile client and the .NET server application we'll develop. 

Next, using your editor or choice, within  the new app open the file  `settings/settings.yml` add the `adapter_url` line to the development section: 

```yml
:development:
  :licensefile: settings/license.key
  :redis: localhost:6379
  :syncserver: http://localhost:9292
  :push_server: http://someappname@localhost:8675/
  :api_token: my-rhoconnect-token  
  :adapter_url: http://localhost:3000
```

The `adapter_url` line will point to our .NET app.  In development we've set up our app to use port 3000 on `localhost`.  This line allows our RhoConnect app to know where the data resides when synchronizing with mobile apps.

Note the `api_token` is set to `my-rhoconnect-token`.  This token will be used with the .NET app to create a shared token between the apps.  In a production environment you'll want to use a more secure token.

Note also that the sync server is running on `localhost` at port 9292.  This is the URL and port of your RhoConnect server.  In production this would point to the IP on the Internet where your application is installed.  




## Create a Rhodes App

In order to test our architecture based on the plugin we're going to create a simple Rhodes app to display and update the list of addresses.  From our `/plugins` root directory do:

```
rhodes app addresses
cd addresses
```

Add an Address model:

```
rhodes model address Name,Address1,Email
```

Be sure not to add spaces between the fields.  This will create the files we need to support our Address model.

Edit the `app/Address/address.rb` to enable sync:

```ruby
# The model has already been created by the framework, and extends Rhom::RhomObject
# You can add more methods here
class Address
  include Rhom::PropertyBag

  # Uncomment the following line to enable sync with Address.
  enable :sync

  #add model specific code here
end
```

Edit the `rhomobile.txt` to point to the sync (RhoConnect) server:
```
syncserver = 'http://localhost:9292'
```

Also change the start path for the app to display our list of addresses:
```
start_path = '/app/Address'
```

And we're done!


## Testing the Apps

In order to test the app's ability to talk to each other we'll launch them all in separate shell (or command) windows.

### Start up the services:

Start redis:
```
cd address_server
rhoconnect redis-start

```
Open a new shell and start Rhoconnect from the same folder:
```
cd address_server
rhoconnect start
```

### Start the .NET app

Using Visual Studio hit the Run button to start the project in debug mode.  In production you would configure IIS to serve the application as a web site.


### Start the Rhodes app in the simulator (iPhone simulator):

For our example we'll run the Rhodes app in the RhoMobile simulator instead of on an actual device.  This allows us to use localhost to serve the components of our solution.

Run the simulator:

```
cd contacts
rake run:android:rhosimulator
```

Once the app starts up log in using the username "app" with no password.


### Testing the synchronization

Open a web browser and navigate to the Address controller in our web app by visiting: http://localhost:3000/address

Click the "Create New" link and create a new address by entering values in the fields:

![Imgur](http://i.imgur.com/Xtwf2vf.png)


The mobile device should show 1 entry.  

![Imgur](http://i.imgur.com/rKSMd3r.png)

On the mobile app enter:

![Imgur](http://i.imgur.com/MPo3sZa.png)

You should now see 2 listings on the mobile app.  


Press the sync button and return to the web app.

A view of the Address records shows the data on the .NET side at `http://localhost:3000`:

![Imgur](http://i.imgur.com/3ciJ6Ap.png)

That's it! Your .NET `Address` model is exposed to the Rhodes app for synchronization.  Data will automatically be updated on both ends whenever it changes on either end.

## Conclusion

RhoConnect plugins are a fast way to enable a back end to hook into the RhoMobile framework.  In this example we used .NET MVC as the back end platform, but it could have been any other platform that is supported by the plugin architecture.  

RhoConnect plugins allows you to use a variety of back-end solutions, whatever your legacy platform may be.  This gives you great flexibility in creating mobile applications that access enterprise data.

## About the Author
<a href="http://stackoverflow.com/users/704832/richard-brown">
<img src="http://stackoverflow.com/users/flair/704832.png" width="208" height="58" alt="profile for Richard Brown at Stack Overflow, Q&amp;A for professional and enthusiast programmers" title="profile for Richard Brown at Stack Overflow, Q&amp;A for professional and enthusiast programmers">
</a>

[Richard Brown](mailto:richard@kutirtech.com) is a Senior Developer and Architect at [Kutir Mobility](http://www.kutirmobility.com).  His specialties include iOS, Ruby on Rails, and RhoMobile development.

## About Us

Facing challenges with your RhoMobile implementation? Kutir Mobility, a Motorola Certified partner, can help you get your app done on time. Get in touch with us today at info@kutirtech.com , no strings attached.
