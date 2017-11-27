library(leaflet)

navbarPage("Quest", id="nav",

       tabPanel("Held Calls",
                plotOutput("heldcallsplott"),
                plotOutput("heldcallsplot1"),
                plotOutput("heldcallsplot2"),
                plotOutput("heldcallsplot3"),
                plotOutput("heldcallsplot4"),
                selectInput("heldcalls.Category", "Category:", c("C1","C2"), multiple=TRUE)
       )
           
)