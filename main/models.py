from django.db import models

class IphoneModel(models.Model):
    name = models.CharField(max_length=50)
    model_number = models.CharField(max_length=10)

class AppleStore(models.Model):
    name = models.CharField(max_length=30)
    id_number = models.CharField(max_length=4)
    models_in_stock = models.ManyToManyField(IphoneModel)
    updated_when = models.DateTimeField()