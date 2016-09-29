# stockcheck
## Get realtime alerts when an iPhone 7 is in stock nearby.
![iPhone screenshot](https://github.com/AndrewBennet/stockcheck/raw/master/media/iphone_lockscreen.PNG)

A Windows app to provide alerts when a desired iPhone 7 model is available in a local Apple store.

Written in C#, builds in Visual Studio 2015. Edit the app.config (or stockcheck.exe.config in the output directory) to set the desired iPhone model types, and the UK postcode to search around.

Optionally, provide a [Pushbullet](http://www.pushbullet.com) authentication token to have the alerts sent to Pushbullet, instead of a Windows popup dialog.