jQuery(function($){

		$('#run-nav-link').attr('href', window.location);

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
        if(notifySupported && !Notify.needsPermission) {
            new Notify(title, {
                body: body
            }).show();
        }
        else {
            alert(title + ': ' + body)
        }
    }

    function checkStock() {
        var models = getSelectedModels(),
            stores = getSelectedStores();
        if(models.length && stores.length) {
            $.get('stockcheck?stores=' + stores.join() + '&models=' + models.join(), function(data) {
                //notifyOrAlert('New Stock at Stratford', 'iPhone 7 Plus 128GB Jet Black available')
            });
        }
    }

    function startOrStopRunning(){
        var selectedModels = getSelectedModels(),
            selectedStores = getSelectedStores();

        if(selectedModels.length !== 0 && selectedStores.length !== 0) {
            clearInterval(stockchecker);
            clearInterval(dotDotDotIncrementer);
            $('#status-text').text('Checking for stock');
            $('#instruction-text').toggleClass('hidden', false);
            dotDotDotIncrementer = setInterval(function(){
                if(dotDotDots > 3) {
                    dotDotDots = 0;
                }
                else {
                    dotDotDots += 1;
                }
                $('#status-text').text('Checking for stock' + Array(dotDotDots).join('.'));
            }, 800);
            if(notifySupported){
                if(Notify.needsPermission) {
                    Notify.requestPermission(function(){
                        notifyOrAlert('iStockCheck Notification', 'Notifications of new iPhone stock will be sent like this.');
                    });
                }
            }
            stockchecker = setInterval(function(){
                checkStock();
            }, 60000)
        }
        else {
            clearInterval(stockchecker);
            clearInterval(dotDotDotIncrementer);
            $('#status-text').text('Select at least one model and store to start checking for stock');
            $('#instruction-text').toggleClass('hidden', true);
        }

        // Adjust the URL
        history.replaceState(null, null, '?models=' + selectedModels.join() + '&stores=' + selectedStores.join());
				$('#run-nav-link').attr('href', window.location);
    }

    var urlModels = getUrlParam('models'),
        urlStores = getUrlParam('stores'),
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

    startOrStopRunning();

    // Update the URL when a checkbox is clicked
    $('input[type="checkbox"]').click(function() {
        startOrStopRunning();
    });
})
