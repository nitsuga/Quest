library(dplyr)
library(jsonlite)

baseurl = paste("http://localhost:3840/api/", sep="")

search.documents <- data.frame(ID=character(), Description=character(), Latitude=double(), Longitude=double())

## get the index groups
url = paste(baseurl, "Search/IndexGroups", sep="")
si.json = fromJSON(url) 
si.g = si.json$Groups
search.indexgroups = si.g[si.g$isEnabled==T,"Name"]
search.indexgroups.selected = si.g[si.g$isDefault==T & si.g$isEnabled,"Name"] %>% first()

