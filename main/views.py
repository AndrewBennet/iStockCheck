from django.shortcuts import render
from django.http import HttpResponse
from django.utils import timezone
import requests
import json
from .models import IphoneModel, AppleStore

def index(request):
    return render(request, 'main/index.html');

def about(request):
    return render(request, 'main/about.html');

def stockchecker(request):
    return render(request, 'main/stockchecker.html', {
        'iphones': IphoneModel.objects.all(),
        'stores': AppleStore.objects.all(),
    })

def batch(seq, size):
    return (seq[pos:pos + size] for pos in range(0, len(seq), size))

def update_stock(store):
    all_models = list(IphoneModel.objects.all())

    # We have to get the stock info in batches of 10
    for model_batch in batch(all_models, 10):
        query_string = ""

        # Build a query string for the 10 iPhone models and an Apple Store
        for index, model in enumerate(model_batch):
            query_string += "parts." + str(index) + "=" + model.model_number + "&"
        query_string += "store=" + store.id_number

        # Load some JSON from the Apple website
        request = requests.get("https://www.apple.com/uk/shop/retail/pickup-message?" + query_string)
        parts_availability = request.json()["body"]["stores"][0]["partsAvailability"]

        # For each model, check whether it is in stock, and update the store accordingly
        for model in model_batch:
            model_pickup_quote = parts_availability[model.model_number]["storePickupQuote"]
            if model_pickup_quote is not None and "unavailable" not in model_pickup_quote:
                store.models_in_stock.add(model)
            else:
                store.models_in_stock.remove(model)
            store.save()

def stockcheck(request):
    store_ids = request.GET['stores'].split(",")
    model_ids = request.GET['models'].split(",")

    response = {}
    for store in [store for store in AppleStore.objects.all() if store.id_number in store_ids]:
        # For each store, update stock levels if they are more than 1 minute stale
        if (timezone.now() - store.updated_when).seconds >= 60:

            # Update the time first, so that other requests are less likely to do the same work
            store.updated_when = timezone.now()
            store.save()

            # Update the stock levels. TODO: it would be nice to queue this work on a background thread
            update_stock(store)

        # Record the model IDs which are in stock
        response[store.id_number] = [model.model_number for model in store.models_in_stock.all() if model.model_number in model_ids]

    return HttpResponse(json.dumps(response))
