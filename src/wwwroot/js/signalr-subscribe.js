var connection = new signalR.HubConnectionBuilder()
    .withUrl("/conges/CongesHub")
    .build();

if (connection != null)
{
    connection.on("Notification", function (type, mailFrom, mailTo, subject, body)
    {
        var currentMail = $('input#usermail').val()
        if (mailTo == currentMail)
        {                                         
            var hrefValue = "#";

            /*public enum NotifyTypeEnum {
                Test = 0,               // Test
                LeavePending = 1,       // Congés à valider
                LeaveValidated = 2,     // Congés validés
                LeaveRefused = 3,       // Congés refusés
                LeaveCanceled = 4,      // Congés annulés
                LeaveCancelPending = 5, // Congés à annuler
                DrhActionNeeded = 6     // Action nécessaire de la DRH
            }*/
            switch (type)
            {
                case 0: hrefValue = "/conges/"; break;
                case 1: hrefValue = "/conges/Manage/"; break;
                case 2: hrefValue = "/conges/"; break;
                case 3: hrefValue = "/conges/"; break;
                case 4: hrefValue = "/conges/"; break;
                case 5: hrefValue = "/conges/Manage/"; break;
                case 6: hrefValue = "/conges/RH/Compta/"; break;
            }

            console.info("href=" + hrefValue);

            $("#SignalRDisplay a").attr("href", hrefValue); 
            $("#SignalRSubject").text(subject);
            $("#SignalRBody").text(body);
            $("#SignalRDisplay").show();
            FaviconNotification.add();            

            var title = document.title;
            document.title = "(*) " + title;            
        }
    });

    connection.start();
    FaviconNotification.init({ url: '/conges/favicon.ico'});
}