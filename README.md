# MesConges
Gestion de congés en opensource

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
2. ConnectionString: Indiquez la bonne chaine de connexion à la base de données
3. DbName: Nom de la base de données
4. Renseignez votre domaine et indiquez les emails pour le ou les admins, les emails pour le ou les DRH (Admins, Drh, Domain)
5. Paramètrez votre serveur LDAP (LdapServer, LdapBase, etc)
6. Paramètrez votre chaine de connexion au serveur SMTP (SmtpServer). Exemple: votresociete.mail.protection.outlook.com

Note: Déploiement sur serveur IIS très simple (fichier zip)
