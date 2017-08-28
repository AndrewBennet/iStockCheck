from django.conf.urls import url

from . import views

urlpatterns = [
    url(r'^$', views.index, name='index'),
	url(r'^about$', views.about, name='about'),
	url(r'^stockchecker$', views.stockchecker, name='stockchecker'),
    url(r'^stockcheck$', views.stockcheck, name='stockcheck'),
]
