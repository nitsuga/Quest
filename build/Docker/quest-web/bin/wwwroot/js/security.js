
$(function () {

    UpdateSecurity();
});

function UpdateSecurity() {
    $.ajax({
        url: getURL("Security/GetNetwork"),
        dataType: "json",
        success: function (response) {
            DisplayNetwork(response);
        },
        error: function () {
        }
    });
}

function DisplayNetwork(data) {
    var nodes = new vis.DataSet();
    var edges = new vis.DataSet();

    data.Items.forEach(function (node) {
        // if (node.SecuredItemName!=="permission")
            nodes.add(
                {
                    id: node.SecuredItemID,
                    label: node.SecuredItemName + "\n" + node.SecuredValue + " (" + node.SecuredItemID+ ")",
                    group: node.SecuredItemName,
                    shadow: true
                });
    });

    data.Links.forEach(function (node) {
        edges.add({ from: node.SecuredItemIDParent, to: node.SecuredItemIDChild });
    });

    // create a network
    var container = document.getElementById('network');

    // provide the data in the vis format
    var networkdata = {
        nodes: nodes,
        edges: edges
    };
    
    var options = {
        layout: { hierarchical: true },
        configure: true,
        manipulation: true,
        edges: { arrows: "from"},
        groups: {
            root: { color: { background: 'gold' }, borderWidth: 1, level: 1 },
            organisation: { color: { background: 'violet' }, borderWidth: 1, level: 2 },
            user: { color: { background: 'lime' }, borderWidth: 1, level: 3 },
            group: { color: { background: 'teal' }, borderWidth: 1, level: 4, mass: 7 },
            feature: { color: { background: 'lightblue' }, borderWidth: 1, level: 5, mass: 7 },
            permission: { color: { background: 'orange' }, borderWidth: 1, level: 6 },
            application: { color: { background: 'red' }, borderWidth: 1, level: 7 }
    }
    };

    // initialize your network!
    var network = new vis.Network(container, networkdata, options);

}

function getURL(url) {
    var s = getBaseURL() + "/" + url;
    //console.debug("g url = " + s);
    return s;
}

function getBaseURL() {
    var url = location.href;  // entire url including querystring - also: window.location.href;
    var baseUrl = url.substring(0, url.indexOf("/", 10));
    //console.debug("b url = " + baseURL);
    return baseUrl;
}
