library("htmlwidgets")
library("dplyr")
library("leaflet")
library(jsonlite)
library("rgdal")

baseurl = paste("http://localhost:3840/api/", sep="")

search.documents <- data.frame(ID=character(), Description=character(), Latitude=double(), Longitude=double())

url = paste(baseurl, "Search/IndexGroups", sep="")

si.json = fromJSON(url) 
si.g = si.json$Groups
search.indexgroups = si.g[si.g$isEnabled==T,"Name"]
search.indexgroups.selected = si.g[si.g$isDefault==T & si.g$isEnabled,"Name"] %>% first()


m = leaflet() %>% addTiles() %>%   
  addLegend("bottomright", colors = c("#03F", "green"), labels = c("Actual", "Routing")) %>%   
  addControl(html="<input id=\"slide\" type=\"range\" min=\"0\" max=\"1\" step=\"0.1\" value=\"1\">") %>%
  onRender("function(el,x,data){
  var map = this;
  var evthandler = function(e){
    var labels = map.layerManager._byGroup.Labels;
    Object.keys(labels).forEach(function(el){
      labels[el]._container.style.opacity = +e.target.value;
    });
  };
  $('#slide').on('mousemove',L.DomEvent.stopPropagation);
  $('#slide').on('input', evthandler);
}")
m


url = "http://localhost:3840/api/Search/Find?searchText=br6&searchMode=0&includeAggregates=false&skip=0&take=100&boundsfilter=false&w=-0.9448242187500001&s=51.23182767977129&e=0.7566833496093751&n=51.7406361640977&filterterms=&indexGroup=London"
search.results = fromJSON(url)

search.results$Documents$Latitude=search.results$Documents$l$lat
search.results$Documents$Longitude=search.results$Documents$l$lon
search.results$Documents$Action = paste('<a class="go-map" href="" data-lat="', search.results$Documents$Latitude, '" data-long="', search.results$Documents$Longitude, '" data-id="', search.results$Documents$ID, '"><i class="fa fa-crosshairs"></i></a>', sep="")

length(search.results$Documents)

search.documents <- search.results$Documents %>%
  select(
    Description = d,
    Latitude = Latitude,
    Longitude = Longitude
  )


#m %>% addMarkers(~l$lon, ~l$lat, data=search.results$Documents, popup=~d)
m %>% addMarkers(data=search.documents, popup=~Description)

url2 = paste(baseurl, "Resources/GetMapItems", sep="")
res = fromJSON(url2)
res$Resources$latitude = res$Resources$Y
res$Resources$longitude = res$Resources$X
m %>% addCircleMarkers(data=res$Resources, popup=~Callsign)
