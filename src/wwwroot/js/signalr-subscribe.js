var connection = new signalR.HubConnectionBuilder()
    .withUrl("/conges/CongesHub")
    .build(); 

if (connection != null) {
    connection.on("Notification", function (message) {
        var li = document.createElement("li");
        document.getElementById("messagesList").appendChild(li);
        li.textContent = message;
    });

    connection.start();
}