from django.shortcuts import render
from django.http import HttpResponse
from main.models import IphoneModel, AppleStore
import json

def index(request):
    return render(request, 'main/index.html')

def about(request):
    return render(request, 'main/about.html')

def stockchecker(request):
    return render(request, 'main/stockchecker.html')

# Called by AJAX GET requests; returns JSON of stock levels per stores
def stockcheck(request):
    # The stores and models we are interested in are in the query string
    store_ids = request.GET['stores'].split(",")
    model_ids = request.GET['models'].split(",")

    response = {}

    # Loop through each store for which its store ID is in the stores query parameter
    for store in [store for store in AppleStore.objects.all() if store.id_number in store_ids]:

        # Add the model IDs which we are interested in, and are in stock, to the response object
        response[store.name] = [model.name for model in store.models_in_stock.all() if model.model_number in model_ids]

    # Return the in-stock models as a JSON object
    return HttpResponse(json.dumps(response))