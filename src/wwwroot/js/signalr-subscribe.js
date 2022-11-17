var connection = new signalR.HubConnectionBuilder()
    .withUrl("/conges/CongesHub")
    .build();

if (connection != null)
{
    connection.on("Notification", function (mailFrom, mailTo, subject, body)
    {
        var currentMail = $('input#usermail').val()
        if (mailTo == currentMail)
        {                        
            $("#SignalRMessage").text(subject + " - " + body);
            $("#SignalRDisplay").show();
            FaviconNotification.add();
        }
    });

    connection.start();
    FaviconNotification.init({ url: '/conges/favicon.ico'});
}

function onAlertClosePress() {    
    FaviconNotification.remove();
}