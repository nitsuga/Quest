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