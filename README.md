# stockcheck
<img src="https://github.com/AndrewBennet/stockcheck/blob/master/media/iphone_lockscreen.PNG" width="200" />

Windows app to provide alerts when a desired iPhone 7 model is available in a local Apple store. Polls an Apple reservation web page, and presents an alert box if any stock is found.

Builds in Visual Studio 2015. Edit the app.config (or stockcheck.exe.config in the output directory) to set the desired iPhone model types, and the UK postcode to search around.

Optionally, provide a [Pushbullet](http://www.pushbullet.com) authentication token to have the alerts sent to Pushbullet, instead of a Windows popup dialog.