library(leaflet)
library(RColorBrewer)
library(scales)
library(lattice)
library(dplyr)
library(jsonlite)
library(devtools)
library(leaflet.extras)

#if (!require('devtools')) install.packages('devtools')
#  devtools::install_github('rstudio/leaflet')

#devtools::install_github('bhaskarvk/leaflet.extras')


function(input, output, session) {
  
  last.search <- ""
  
  ## Interactive Map ###########################################
  
  # Create the map
  output$map <- renderLeaflet({
    leaflet() %>%
      addWMSTiles(group = "Barts", "http://127.0.0.1:8090/cgi-bin/mapserv?MAP=/maps/extent.map&crs=EPSG:27700", layers="Barts", options = WMSTileOptions(format = "image/png", transparent = F)  ) %>%
      addProviderTiles("OpenStreetMap.Mapnik", group = "OpenStreetMap") %>%
      addProviderTiles("Stamen.Toner", group = "Black and White") %>%
      addProviderTiles("Stamen.TonerBackground", group = "Black and White 2") %>%
      addProviderTiles("OpenStreetMap.BlackAndWhite", group = "Black and White 3") %>%
      addDrawToolbar(targetGroup='draw', editOptions = editToolbarOptions(selectedPathOptions = selectedPathOptions()))  %>%
      addStyleEditor() %>%
      setView(lng = -0.1, lat = 51.5, zoom = 10)  %>%
      enableTileCaching() %>%
      addMeasure(
        position = "bottomleft",
        primaryLengthUnit = "meters",
        primaryAreaUnit = "sqmeters",
        activeColor = "#3D535D",
        completedColor = "#7D4479") %>%
      addMiniMap(toggleDisplay=T) %>%
      addEasyButton(easyButton(
        icon="fa-globe", title="Zoom to Level 1",
        onClick=JS("function(btn, map){ map.setView([51.5,-0.1],10) }"))) %>%
      addEasyButton(easyButton(
        icon="fa-crosshairs", title="Locate Me",
        onClick=JS("function(btn, map){ map.locate({setView: true}); }"))) %>%
      addLayersControl(
        options = layersControlOptions(collapsed = T),
        baseGroups = c("OpenStreetMap", "Barts", "Black and White", "Black and White 2", "Black and White 3"),
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
  ## search item link clicked, picked up by gomap.js
  observe({
    if (is.null(input$goto))
      return()
    isolate({
      map <- leafletProxy("map")
      map %>% clearPopups()
      dist <- 0.001
      lat <- input$goto$lat
      lng <- input$goto$lng
      label <- input$goto$label
      map %>% fitBounds(lng - dist, lat - dist, lng + dist, lat + dist)  %>%
      addPulseMarkers(
        lng=lng, lat=lat,
        label=label,
        icon = makePulseIcon(heartbeat = 0.5))
    })
  })
  
  ## Search ###########################################
  
  observe({
    event <- input$search.text
    
    if (is.null(event))
      return()

    if (input$search.text=="")
      return()

    # ignore repeated search
    if (last.search == input$search.text)
      return()

    last.search <<- input$search.text
    
    # build up he request
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
    
    # get results
    search.results = fromJSON(url)
    
    # flatten results
    search.results$Documents$Latitude=search.results$Documents$l$lat
    search.results$Documents$Longitude=search.results$Documents$l$lon
    search.results$Documents$Action = paste('<a class="go-map" href="" data-lat="', search.results$Documents$Latitude, '" data-long="', search.results$Documents$Longitude, '" data-id="', search.results$Documents$ID, '" data-label="', search.results$Documents$d, '"><i class="fa fa-crosshairs"></i></a>', sep="")

    
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

    output$search.table <- DT::renderDataTable(
      {
        action <- DT::dataTableAjax(session, search.documents)
        DT::datatable(search.documents, 
                      class="compact",
                      selection = "single",
                      options = list(
                        ajax = list(url = action), 
                        columnDefs = list(list(visible=FALSE, targets=c(1,3,4))),
                        search = list(caseInsensitive = FALSE)
                      ), 
                    escape = FALSE)
      }
    )
    
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
  
    
  observe({
    event <- input$heldcalls.Category
    
    brownian <- function(n=100)
    {
      t <- 1:n  # time
      sig2 <- 0.01
      ## first, simulate a set of random deviates
      x <- rnorm(n = length(t) - 1, sd = sqrt(sig2))
      ## now compute their cumulative sum
      x <- c(0, cumsum(x))
      return(x)
    }
    
    samples = 420
    
    now = round((brownian(samples)/2+1)*3,0)
    yesterday = round((brownian(samples)/2+1)*3,0)
    
    df <- data.frame(
      date = as.POSIXct(seq(Sys.time(), by = "min", length.out = samples)),
      now = now,
      yesterday = yesterday
    )
    
    zoo.heldcalls <- as.zoo(df)
    
    output$heldcallsplot <- renderPlot({
      ggplot(df, aes(x=date, y=now, factor())) + theme_bw() +geom_line() +geom_ribbon(aes(ymin=0, ymax=yesterday), fill="lightpink3", color="lightpink3")+
        geom_line(color="lightpink4", lwd=1)
      })
    
    
    })
    
    
}