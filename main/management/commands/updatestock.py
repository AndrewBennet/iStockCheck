from django.core.management.base import BaseCommand, CommandError
from django.utils import timezone
import requests
from main.models import IphoneModel, AppleStore
import logging
import socket
import sys
import time

class Command(BaseCommand):
    help = 'Updates iPhone stock level information'

    # Get an instance of a logger
    logger = logging.getLogger(__name__)


    lock_socket = None  # we want to keep the socket open until the very end of
                        # our script so we use a global variable to avoid going
                        # out of scope and being garbage-collected

    # Checks whether a lock is held - i.e. is this process already running?
    def is_lock_free(self):
        global lock_socket
        lock_socket = socket.socket(socket.AF_UNIX, socket.SOCK_DGRAM)
        try:
            lock_id = "andrewbennet.updatestock"   # this should be unique. using your username as a prefix is a convention
            lock_socket.bind('\0' + lock_id)
            logging.debug("Acquired lock %r" % (lock_id,))
            return True
        except socket.error:
            # socket already locked, task must already be running
            logging.info("Failed to acquire lock %r" % (lock_id,))
            return False

    # Batches a sequence in chunks of the specified size
    def batch(self, seq, size):
        return (seq[pos:pos + size] for pos in range(0, len(seq), size))

    # Update stock information from Apple's servers
    def update_stock(self, store):
        # We have to get the stock info in batches of 10
        # When we disabled iPhone 8 stock checking, this batch mechanism wasn't strictly necessary any more
        for model_batch in self.batch(IphoneModel.objects.all(), 10):
            query_string = ""

            # Build a query string for the 10 iPhone models and an Apple Store
            for index, model in enumerate(model_batch):
                query_string += "parts." + str(index) + "=" + model.model_number + "&"
            query_string += "store=" + store.id_number

            # Load some JSON from the Apple website
            self.logger.info('Requesting ' + query_string)
            time.sleep(1)
            request = requests.get("https://www.apple.com/uk/shop/retail/pickup-message?" + query_string)
            try:
                jsonResponse = request.json()
                parts_availability = jsonResponse["body"]["stores"][0]["partsAvailability"]
            except:
                # eek
                self.logger.error('JSON error occured at response from ' + query_string)
                return

            # For each model, check whether it is in stock, and update the store accordingly
            for model in model_batch:
                model_pickup_quote = parts_availability[model.model_number]["storePickupQuote"]
                if model_pickup_quote is not None and "unavailable" not in model_pickup_quote:
                    store.models_in_stock.add(model)
                else:
                    store.models_in_stock.remove(model)
                store.updated_when = timezone.now()
                store.save()
    
    def run(self):
        while(True):
            # For each store, update stock levels
            for store in AppleStore.objects.all():

                # Update the stock levels. 
                self.update_stock(store)

            self.stdout.write(self.style.SUCCESS('Stock update complete'))
            time.sleep(10)
    
    # Entrypoint
    def handle(self, *args, **options):

        if not self.is_lock_free():
            sys.exit()
        else:
            self.run()