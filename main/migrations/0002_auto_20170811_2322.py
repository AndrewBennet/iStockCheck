# -*- coding: utf-8 -*-
# Generated by Django 1.11.4 on 2017-08-11 23:22
from __future__ import unicode_literals

from django.db import migrations, models
import django.db.models.deletion


class Migration(migrations.Migration):

    dependencies = [
        ('main', '0001_initial'),
    ]

    operations = [
        migrations.AlterField(
            model_name='stock',
            name='iphone_model',
            field=models.ForeignKey(on_delete=django.db.models.deletion.CASCADE, to='main.IphoneModel'),
        ),
    ]
