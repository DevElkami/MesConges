# Mes congés
Gestion de congés en opensource (Blazor / ASP.NET Core 5)

- Gestion des services (un seul responsable par service)
- Import rapide des utilisateurs depuis le LDAP
- Gestions automatique des jours fériés
- Possibilité de définir des jours non travaillés pour une année particulière
- Interfaces spécifiques pour les managers, les admins, les DRH (ces derniers peuvent exporter les congés sous excel)
- Calendier offrant une vision globale des congés posés (et calendrier par services)
- Différents types de congés (congés payés, événements familiaux, enfants malades, récupérations, sans solde, absences temporaires)
- Validation par le responsable des congés posés et notification par emails

![](https://github.com/DevElkami/MesConges/blob/main/screen.png)

Etapes pour configurer l'application:
1. Ouvrez le fichier src/appsettings.json
2. ConnectionString: Indiquez la bonne chaine de connexion à la base de données (qui doit être existante et s'appeler "conges")
                     Pour docker, utiliser host.docker.internal à la place de localhost
3. DbName: Nom de la base de données
4. Renseignez votre domaine et indiquez les emails pour le ou les admins, les emails pour le ou les DRH (Admins, Drh, Domain)
5. Paramètrez votre serveur LDAP (LdapServer, LdapBase, etc)
6. Paramètrez votre chaine de connexion au serveur SMTP (SmtpServer). Exemple: votresociete.mail.protection.outlook.com

Déploiement possible sur :
- Azure
- Docker
- IIS
- Via fichier zip (Profil par défaut)
