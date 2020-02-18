﻿"use strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/BlocksHub").build();

connection.on("ReportBlock",
    function (blockId, blockIpAddress, blockCity, blockCountry, blockISP) {
       
        // Get reference to login attempts
        var elem = document.getElementById("blocks");

        var card = document.createElement("div");
        card.className = "card";
        card.style = "width: 18rem;";
        card.alt = blockId;

        var cardBody = document.createElement("div");
        cardBody.className = "card-body";

        // Create title
        var title = document.createElement("h5");
        title.className = "card-title";
        title.appendChild(document.createTextNode(blockCountry + ", " + blockCity));

        cardBody.appendChild(title);

        // Subtitle
        var subtitle = document.createElement("h6");
        subtitle.className = "card-subtitle mb-2 text-muted";
        subtitle.appendChild(document.createTextNode(blockIpAddress));

        cardBody.appendChild(subtitle);

        // Details
        var details = document.createElement("p");
        details.className = "card-text";
        details.appendChild(document.createTextNode(blockISP));

        cardBody.appendChild(details);

        // Add body to card
        card.appendChild(cardBody);

        // Add card to page
        elem.appendChild(card);
    });

connection.start().then(function () {
    console.log("Connected to BlocksHub");
}).catch(function (err) {
    return console.error(err.toString());
});