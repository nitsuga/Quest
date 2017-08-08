library(leaflet)

navbarPage("Quest", id="nav",
           
           tabPanel("Interactive map",
                    div(class="outer",
                        
                        tags$head(
                          # Include our custom CSS
                          includeCSS("styles.css"),
                          includeScript("clientstore.js"),
                          includeScript("gomap.js"),
                          includeScript("notify.js"),
                          includeScript("telephony.js"),
                          includeScript("livemap.js")
                        ),
                        
                        leafletOutput("map", width="100%", height="100%"),
                        
                        # panel selector
                        absolutePanel(id = "options", class = "fadepanel panel panel-default", fixed = TRUE,
                                      draggable = TRUE, top = 60, left = "auto", right = "70", bottom = "auto",
                                      width = "100", height = "auto",
                                      checkboxGroupInput(inputId='panelOptions', 
                                                         label='Show',
                                                         choices=c('Objects', 'Search'),
                                                         selected='Search')
                           ),
                        
                        # Search
                        conditionalPanel(condition="input.panelOptions.indexOf('Search') != -1",
                          absolutePanel(id = "search", class = "fadepanel panel panel-default", fixed = TRUE,
                                        draggable = TRUE, top = 70, left = "60", right = NULL, bottom = "auto",
                                        width = "500", height = "auto",
                                        textInput("search.text",label="",placeholder="search text" ),
                                        fluidRow(
                                          column(3, offset = 0,  checkboxInput("search.lock",label="Lock")),
                                          column(3, offset = 3,  checkboxInput("search.coord",label="Coord"))
                                        ),
                                        fluidRow(
                                          column(3, offset = 0,  selectInput("search.areas", "Areas:", search.indexgroups,selectize = TRUE, multiple = F, selected = search.indexgroups.selected )),
                                          column(3, offset = 3,  selectInput("search.mode", "Mode:", c("exact"=0,"relax"=1,"fuzzy"=2),selectize = TRUE, multiple = F, selected = "exact" ))
                                        ),
                                        div()
                                        hr(),
                                        DT::dataTableOutput("search.table")
                                        
                          )
                        ),
                        
                        
                        # Objects panel
                        conditionalPanel(condition="input.panelOptions.indexOf('Objects') != -1",
                          absolutePanel(id = "objects", class = "fadepanel panel panel-default", fixed = TRUE,
                                        class=".fadepanel panel panel-default",
                                        draggable = TRUE, top = 120, left = "auto", right = "10", bottom = "auto",
                                        width = 200, height = "auto",
                                        selectInput("resources", "Resources:",
                                                    c("Unavailable" = "U",
                                                      "Available" = "A",
                                                      "Busy" = "B"),selectize = TRUE, multiple = T),
                                        
                                        selectInput("events", "Events:",
                                                    c("Waiting" = "W",
                                                      "In Progress" = "A",
                                                      "C1" = "C1",
                                                      "C2" = "C2",
                                                      "C3" = "C3",
                                                      "C4" = "C4"
                                                    ),selectize = TRUE, multiple = T),
                                        
                                        selectInput("coverage", "Coverage:",
                                                    c("Events" = "U",
                                                      "AEU" = "A",
                                                      "FRU" = "B",
                                                      "Uncovered"="U"
                                                      ),selectize = TRUE, multiple = T)
                          )
                        )
                        
                    )
           ),
           tabPanel("Held Calls",
                    plotOutput("heldcallsplott"),
                    plotOutput("heldcallsplot1"),
                    plotOutput("heldcallsplot2"),
                    plotOutput("heldcallsplot3"),
                    plotOutput("heldcallsplot4"),
                    selectInput("heldcalls.Category", "Category:", c("C1","C2"), multiple=TRUE)
           ),
           conditionalPanel("true", icon("crosshair"))
           
           
)