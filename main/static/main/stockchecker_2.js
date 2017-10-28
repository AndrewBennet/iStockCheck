jQuery(function($){
    function getSelectedModels() {
        return $.map($('.iphone-checkbox:checked'), function(v, i) {
            return $(v).val();
        });
    }

    function getSelectedStores() {
        return $.map($('.store-checkbox:checked'), function(v, i) {
            return $(v).val();
        });
    }

    function getUrlParam(name) {
        var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(window.location.href);
        if(results) {
            return results[1] || null;
        } else {
            return null;
        }
    }

    function notifyOrAlert(title, body) {
        if($('#use-browser-notification').is(':checked')) {
            if(notifySupported && !Notify.needsPermission) {
                new Notify(title, {
                    body: body
                }).show();
            }
            else {
                alert(title + ': ' + body)
            }
        }
        if($('#pushbullet-access-token').val().length > 0) {
            $.ajax({
                url: 'https://api.pushbullet.com/v2/pushes',
                type: 'post',
                data: JSON.stringify({
                    'body': body,
                    'title': title,
                    'type': 'note'
                }),
                headers: {
                    'Content-Type': 'application/json',
                    'Access-Token': $('#pushbullet-access-token').val(),
                },
                dataType: 'json'
            });
        }
    }

    function arraysEqual(arr1, arr2) {
        if(arr1.length !== arr2.length)
            return false;
        for(var i = arr1.length; i--;) {
            if(arr1[i] !== arr2[i])
                return false;
        }
        return true;
    }

    var storesToModels = {};

    function checkStock() {
        var models = getSelectedModels(),
            stores = getSelectedStores();
        if(models.length && stores.length) {
            $.get('stockcheck?stores=' + stores.join() + '&models=' + models.join(), function(data) {
                var responseJson = JSON.parse(data);
                var displayHtml = "";
                for(var store in responseJson) {
                    var models = responseJson[store].join(', ');

                    if (responseJson[store].length > 0){
                        displayHtml += "<li>" + store + " - " + models + "</li>";
                        if(!storesToModels[store] || !arraysEqual(storesToModels[store], responseJson[store])){
                            //stock!
                            notifyOrAlert('Stock at ' + store, models);
                        }
                    }
                 }
                 storesToModels = responseJson;

                 // Now set up the stock display
                 if(displayHtml === "") {
                    $('#current-stock').html("Nothing found");
                 }
                 else {
                    $('#current-stock').html(displayHtml);
                 }
            });
        }
    }

    function requestNotificationPermissionIfNecessary() {
        if($('#use-browser-notification').is(':checked')) {
            if(notifySupported){
                if(Notify.needsPermission) {
                    Notify.requestPermission(function(){
                        notifyOrAlert('iStockCheck Notification', 'Notifications of new iPhone stock will be sent like this.');
                    });
                }
            }
        }
    }

    function updateState(){
        var selectedModels = getSelectedModels(),
            selectedStores = getSelectedStores();

        if(selectedModels.length !== 0 && selectedStores.length !== 0 && ($('#use-browser-notification').prop('checked') || $('#pushbullet-access-token').val().length > 0)) {
            stockcheckerRunning = true;
            clearInterval(stockchecker);
            clearInterval(dotDotDotIncrementer);
            $('#status-text').text('Monitoring stock');
            dotDotDotIncrementer = setInterval(function(){
                if(dotDotDots > 3) {
                    dotDotDots = 0;
                }
                else {
                    dotDotDots += 1;
                }
                $('#status-text').text('Monitoring stock' + Array(dotDotDots).join('.'));
            }, 800);
            $(document).attr("title", "iStockCheck - Monitoring Stock...");

            requestNotificationPermissionIfNecessary();
            stockchecker = setInterval(function(){
                checkStock();
            }, 60000)
        }
        else {
            stockcheckerRunning = false;
            clearInterval(stockchecker);
            clearInterval(dotDotDotIncrementer);
            $('#status-text').text('Select at least one model and store, and set your notification settings, to start checking for stock');
            $(document).attr("title", "iStockCheck");
        }

        // Adjust the URL, and just to be helpful, update the nav link to whatever it is now
        history.replaceState(null, null, '?models=' + selectedModels.join() + '&stores=' + selectedStores.join()
            + '&browserNotifications=' + $('#use-browser-notification').prop('checked') + '&accessToken=' + $('#pushbullet-access-token').val());
        $('#run-nav-link').attr('href', window.location);
    }

    // Update state when a checkbox is clicked
    $('input[type="checkbox"]').click(function() {
        updateState();
    });

    var urlModels = getUrlParam('models'),
        urlStores = getUrlParam('stores'),
        useBrowserNotifications = getUrlParam('browserNotifications'),
        pushbulletAccessToken = getUrlParam('accessToken'),
        notifySupported = !Notify.needsPermission || Notify.isSupported(),
        stockcheckerRunning = false,
        stockchecker,
        dotDotDotIncrementer,
        dotDotDots = 0;

    // Tick the boxes which correspond to the url parameters
    if(urlModels) {
        urlModels.split(',').forEach(function(element) {
            $('.iphone-checkbox[value="' + element + '"]').attr('checked', 'checked');
        }, this);
    }
    if(urlStores){
        urlStores.split(',').forEach(function(element) {
            $('.store-checkbox[value="' + element + '"]').attr('checked', 'checked');
        }, this);
    }

    if(useBrowserNotifications === "false") {
        $('#use-browser-notification').prop('checked', false);
    }

    if(pushbulletAccessToken && pushbulletAccessToken.length > 0) {
        $('#pushbullet-access-token').val(pushbulletAccessToken);
    }

    $(window).on('beforeunload', function () {
        // If running, ask the user before exiting
        if (stockcheckerRunning) {
			return "Are you sure you want to stop checking for stock?"
		}
	});

    $('#test-notifications').click(function() {
        requestNotificationPermissionIfNecessary();
        notifyOrAlert('iStockCheck Test Notification', 'Stock alerts will be sent like this.')
    });

    $('#use-browser-notifications').change(function(){
        updateState();
        requestNotificationPermissionIfNecessary();
    });

    $('#pushbullet-access-token').change(function(){
        updateState();
    });

    updateState();
})
