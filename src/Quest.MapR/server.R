library(leaflet)
library(RColorBrewer)
library(scales)
library(lattice)
library(dplyr)
library(jsonlite)


function(input, output, session) {
  
  ## Interactive Map ###########################################
  
  # Create the map
  output$map <- renderLeaflet({
    leaflet() %>%
      addTiles(group = "OpenStreetMap") %>%
      addProviderTiles("Stamen.Toner", group = "Black and White") %>%
      addProviderTiles("Stamen.TonerBackground", group = "Black and White 2") %>%
      addProviderTiles("OpenStreetMap.BlackAndWhite", group = "Black and White 3") %>%
      setView(lng = -0.1, lat = 51.5, zoom = 10)  %>%
      addMeasure(
        position = "bottomleft",
        primaryLengthUnit = "meters",
        primaryAreaUnit = "sqmeters",
        activeColor = "#3D535D",
        completedColor = "#7D4479") %>%
      addLayersControl(
        options = layersControlOptions(collapsed = T),
        baseGroups = c("OpenStreetMap", "Black and White", "Black and White 2", "Black and White 3"),
        overlayGroups = c("Operational Areas")
      )
  })

  observe({
    event <- input$resources
    if (is.null(event))
      return()

    url.res = paste(baseurl, "Resources/GetMapItems", sep="")
    res = fromJSON(url.res)
    res$Resources$latitude = res$Resources$Y
    res$Resources$longitude = res$Resources$X
    
    isolate({
      proxy <- leafletProxy("map")
      proxy %>% addCircleMarkers(data=res$Resources, popup=~Callsign)
    })
    
  })
  
  ## Search ###########################################
  
  observe({
    event <- input$search.text
    if (is.null(event))
      return()

    if (input$search.text=="")
      return()

    b = input$map_bounds;
    
    url = paste(baseurl, "Search/Find?searchText=",
                URLencode(trimws(input$search.text)), 
                "&searchMode=", input$search.mode, 
                "&includeAggregates=false&skip=0&take=100",
                "&boundsfilter=", input$search.lock,
                "&w=", b["west"],
                "&s=", b["south"], 
                "&e=", b["east"], 
                "&n=", b["north"], 
                "&filterterms=&indexGroup=", input$search.areas,
                sep="")
    
    search.results = fromJSON(url)
    search.results$Documents$Latitude=search.results$Documents$l$lat
    search.results$Documents$Longitude=search.results$Documents$l$lon
    search.results$Documents$Action = paste('<a class="go-map" href="" data-lat="', search.results$Documents$Latitude, '" data-long="', search.results$Documents$Longitude, '" data-id="', search.results$Documents$ID, '"><i class="fa fa-crosshairs"></i></a>', sep="")

    
    if (search.results$Count==0)
      search.documents <- data.frame(ID=character(), Description=character(), Latitude=double(), Longitude=double())
    else
      search.documents <- search.results$Documents %>%
        select(
          ID=ID,
          Description = d,
          Latitude = Latitude,
          Longitude = Longitude,
          Action = Action
        )

    columnDefs = list(list(visible=FALSE, targets=c(1,3,4)))
    
    output$search.table <- DT::renderDataTable({
      action <- DT::dataTableAjax(session, search.documents)
      
      DT::datatable(search.documents, 
                    options = list(
                      ajax = list(url = action), 
                      columnDefs = columnDefs,
                      search = list(caseInsensitive = FALSE)), 
                    escape = FALSE)
    })
    
    isolate({

      proxy <- leafletProxy("map")
      
      # Fit the view to within these bounds (can also use setView)
      proxy %>% 
        clearPopups() %>% 
        clearMarkers() %>% 
        addCircleMarkers(data=search.documents, popup=~Description)
      
      if (!input$search.lock)
        proxy %>% fitBounds(min(search.documents$Longitude), min(search.documents$Latitude), max(search.documents$Longitude), max(search.documents$Latitude))
      
    })
  })

}