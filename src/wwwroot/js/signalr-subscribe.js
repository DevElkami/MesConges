var connection = new signalR.HubConnectionBuilder()
    .withUrl("/conges/CongesHub")
    .build(); 

if (connection != null) {
    connection.on("Notification", function (mailFrom, mailTo, subject, body)
    {
        $("#SignalRMessage").text(body);
        $("#SignalRDisplay").show();
    });

    connection.start();
}