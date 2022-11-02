# Mes congés
Gestion de congés en opensource (Blazor / ASP.NET Core 6)

- Gestion des services (un seul responsable par service)
- Import rapide des utilisateurs depuis le LDAP
- Gestions automatique des jours fériés
- Possibilité de définir des jours non travaillés pour une année particulière
- Interfaces spécifiques pour les managers, les admins, les DRH (ces derniers peuvent exporter les congés sous excel)
- Calendier offrant une vision globale des congés posés (et calendrier par services)
- Différents types de congés (congés payés, événements familiaux, enfants malades, récupérations, sans solde, absences temporaires)
- Validation par le responsable des congés posés et notification par emails

Etapes pour configurer l'application:
1. Ouvrez le fichier src/appsettings.json

2. Configurez l'application:

  - AppAdminLogin	        -> Login du gestionnaire de l'application
  - AppAdminPwd		        -> Mot de passe du gestionnaire de l'application (SHA 512 encodé en base 64)
  - AppAdminEmail		      -> Email du gestionnaire de l'application
  - DbType 			          -> Type de base de donnée souhaitée (sqlite ou mysql)
  - DbName 			          -> Nom de la base de donnée (uniquement mysql)
  - DbConnectionString    -> Chaine de connexion à la base de donnée (uniquement mysql)
  - ExportDir      			  -> Nom du répertoire des export
  - Ldap         				  -> Activation du LDAP
  - LdapConnectionString  -> Chaine de connexion du LDAP
  - LdapFilter 		        -> Filtre du LDAP
  - SmtpServer 		        -> Nom ou ip du serveur de mail (Serveur SMTP anonyme seulement: aucun compte requis)
  
3. Déployez. Déploiement possible sur:
- Azure
- Docker
- IIS
- Via fichier zip (Profil par défaut)

![](https://github.com/DevElkami/MesConges/blob/main/calendrier.png)
![](https://github.com/DevElkami/MesConges/blob/main/ldap.png)
![](https://github.com/DevElkami/MesConges/blob/main/rh.png)
![](https://github.com/DevElkami/MesConges/blob/main/service.png)
![](https://github.com/DevElkami/MesConges/blob/main/screen.png)
