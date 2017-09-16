from django.db import models

class IphoneModel(models.Model):
    name = models.CharField(max_length=50)
    model_number = models.CharField(max_length=10)
    def __str__(self):
        return self.name

class AppleStore(models.Model):
    name = models.CharField(max_length=30)
    id_number = models.CharField(max_length=4)
    models_in_stock = models.ManyToManyField(IphoneModel)
    updated_when = models.DateTimeField()
    def __str__(self):
        return self.name