What is NVoice?
==============================
NVoice is a C# Google Voice library used to send SMS messages. It is originally based on the SharpVoice library, but has been adjusted to support the PCL (Portable Class Library).

Oh, and of course the N is to denote that it is a .NET project.

Why did you create it?
==============================
I wanted to create a Windows desktop application to send mass SMS messages through Google Voice to my poker buddies. I originally used the SharpVoice library, but realized I wanted to 
not only create a Windows desktop application, but a Windows Phone application as well as a Windows Store application.

Requirements
------------------------------

1. Visual Studio 2012
2. The Will To Win
3. Friends to annoy

Supported Frameworks
------------------------------

1. Windows Store
2. .NET 4.5+
3. Windows Phone 7.5+
4. Silverlight 4+

Using NVoice
------------------------------
Using NVoice is easy. Simply download the most recent NVoice build (NVoice.dll). Or pull down the latest NVoice project, build and use in your project.

Example:
> `var username = "yourgoogleusername";
> var password = "yourgooglepassword";
> var phoneNumber = "+123456789";
> var message = "Hi!";
> var totalMessages = 0;
> var sharpVoice = new NVoice.SharpVoice(username, password);
> var outcome = sharpVoice.SendSMS(phoneNumber, messages, out totalMessages)`

As you can see, we expect the phone format to be very specific. I've used libraries like libphonenumber-csharp for parsing, reading, etc. of phone numbers with <borat>great success!</borat>

The Future
------------------------------
I capitalized the F for a reason. I'd like to make nVoice a Nuget package, expand support for not just sending SMS messages, but also reading as well as other functionalities. Most of all, I'd like to use this
project to get my feet wet on GitHub.

Also, right now this PCL relies on the Microsoft BCL libraries. Because of this, NVoice does not support Windows Phone 7. I'd like to support Windows Phone 7 down the line, but right now it is not supported.