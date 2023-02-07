(function prikaziLokaciju(){
var lat = document.getElementById('lat').value;
var lng = document.getElementById('lng').value;


var baseMapLayer = new ol.layer.Tile({
  source: new ol.source.OSM()
});
var map = new ol.Map({
  target: 'mapa',
  layers: [ baseMapLayer],
  view: new ol.View({
          center: ol.proj.fromLonLat([lng,lat]), 
          zoom: 15 //Initial Zoom Level
        })
});
//Adding a marker on the map
var marker = new ol.Feature({
  geometry: new ol.geom.Point(
    ol.proj.fromLonLat([lng,lat])
  ),  // Cordinates of New York's Town Hall
});
var vectorSource = new ol.source.Vector({
  features: [marker]
});
var markerVectorLayer = new ol.layer.Vector({
  source: vectorSource,
});
map.addLayer(markerVectorLayer);
})();