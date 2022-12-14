var connection = new signalR.HubConnectionBuilder()
    .withUrl("/CongesHub")
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
                LeavePending = 1,       // Cong�s � valider
                LeaveValidated = 2,     // Cong�s valid�s
                LeaveRefused = 3,       // Cong�s refus�s
                LeaveCanceled = 4,      // Cong�s annul�s
                LeaveCancelPending = 5, // Cong�s � annuler
                DrhActionNeeded = 6     // Action n�cessaire de la DRH
            }*/
            switch (type)
            {
                default:
                case 0: hrefValue = "/"; break;
                case 1: hrefValue = "/Manage/"; break;
                case 2: hrefValue = "/"; break;
                case 3: hrefValue = "/"; break;
                case 4: hrefValue = "/"; break;
                case 5: hrefValue = "/Manage/"; break;
                case 6: hrefValue = "/RH/Compta/"; break;
            }            

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
    FaviconNotification.init({ url: '/favicon.ico'});
}