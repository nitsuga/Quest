library(leaflet)
library(RColorBrewer)
library(scales)
library(lattice)
library(dplyr)
library(jsonlite)
library(devtools)
library(ggplot2)
library(leaflet.extras)
library(shiny)


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
  
    ## held calls
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
    
    heldcalls.sample.days=2
    heldcalls.lookback.minutes = 5 * 60
    heldcalls.samples = (60 * 24 * heldcalls.sample.days) + heldcalls.lookback.minutes
    heldcalls.time = Sys.time()
    heldcalls.starttime = heldcalls.time - (60 * heldcalls.lookback.minutes)
    heldcalls.yesterday = heldcalls.starttime - 86400
    heldcalls.yesterday.end = heldcalls.yesterday + (60 * heldcalls.lookback.minutes)
    
    c1 = round((brownian(heldcalls.samples)/2+1)*30,0)
    c2 = round((brownian(heldcalls.samples)/2+1)*30,0)
    c3 = round((brownian(heldcalls.samples)/2+1)*30,0)
    c4 = round((brownian(heldcalls.samples)/2+1)*30,0)

    # make up two days

    heldcalls.df <- data.frame(
      date = as.POSIXct(seq(heldcalls.yesterday, by = "min", length.out = heldcalls.samples)),
      c1 = c1,
      c2 = c2,
      c3 = c3,
      c4 = c4,
      total = c1+c2+c3+c4
    )

    #zoo.heldcalls <- as.zoo(df)
    
    heldcalls.df.today <- subset(heldcalls.df, date >= heldcalls.starttime & date <= heldcalls.time)
    heldcalls.df.yesterday <- subset(heldcalls.df, date >= heldcalls.yesterday & date <= heldcalls.yesterday.end)
    
    heldcalls.df.yesterday$date <- heldcalls.df.yesterday$date+86400
    
    heldcalls.final.df <- merge(heldcalls.df.today, heldcalls.df.yesterday, by = "date")
    
    output$heldcallsplott <- renderPlot({
      ggplot(heldcalls.final.df, aes(x=date, y=total.x)) + theme_bw() +geom_line() +geom_ribbon(aes(ymin=0, ymax=total.y), fill="lightblue3", color="lightblue3")+
        geom_line(color="red", lwd=1.25) 
    })
    
    output$heldcallsplot1 <- renderPlot({
      ggplot(heldcalls.final.df, aes(x=date, y=c1.x)) + theme_bw() +geom_line() +geom_ribbon(aes(ymin=0, ymax=c1.y), fill="lightblue3", color="lightblue3")+
        geom_line(color="red", lwd=1.25) 
    })
    
    output$heldcallsplot2 <- renderPlot({
      ggplot(heldcalls.final.df, aes(x=date, y=c2.x)) + theme_bw() +geom_line() +geom_ribbon(aes(ymin=0, ymax=c2.y), fill="lightblue3", color="lightblue3")+
        geom_line(color="red", lwd=1.25) 
    })
    
    output$heldcallsplot3 <- renderPlot({
      ggplot(heldcalls.final.df, aes(x=date, y=c3.x)) + theme_bw() +geom_line() +geom_ribbon(aes(ymin=0, ymax=c3.y), fill="lightblue3", color="lightblue3")+
        geom_line(color="red", lwd=1.25) 
    })
    
    output$heldcallsplot4 <- renderPlot({
      ggplot(heldcalls.final.df, aes(x=date, y=c4.x)) + theme_bw() +geom_line() +geom_ribbon(aes(ymin=0, ymax=c4.y), fill="lightblue3", color="lightblue3")+
        geom_line(color="red", lwd=1.25) 
    })
    
    
    })
    
    
}