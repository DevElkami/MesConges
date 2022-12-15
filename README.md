# Mes congés :sunrise:
Gestion de congés en opensource (Blazor / ASP.NET Core 6)

- Gestion des services (un seul responsable par service)
- Import rapide des utilisateurs depuis le LDAP
- Gestions automatique des jours fériés
- Possibilité de définir des jours non travaillés pour une année particulière
- Interfaces spécifiques pour les managers, les admins, les DRH (ces derniers peuvent exporter les congés sous excel)
- Calendier offrant une vision globale des congés posés (et calendrier par services)
- Différents types de congés (congés payés, événements familiaux, enfants malades, récupérations, sans solde, absences temporaires)
- Validation par le responsable des congés posés et notification par emails

Déployez l'application:
- [Docker](https://github.com/DevElkami?tab=packages&repo_name=MesConges)
- IIS

Etapes pour configurer l'application:
1. Connectez-vous: login = administrator et mot de passe = holidays

2. Paramétrez l'application
  ![](https://github.com/DevElkami/MesConges/blob/main/admin.png)
  
3. [Option] : Si vous souhaitez distribuer l'application sous forme d'exécutable
- npm install -g nativefier
- nativefier --name 'Gestion des congés' 'https://votre.serveur.com'
- L'exécutable généré utilise [Electron](https://www.electronjs.org/) et [Node.js](https://nodejs.org/fr/)

---
Ils l'utilisent: :sparkling_heart:
- :camera: [i2S](https://www.i2s.fr/fr/)
- :articulated_lorry: [CGI](https://conges.cgi-formation.fr/)

Vous aussi :question: Faites le moi savoir: cette application est libre, et un peu de pub pour le développeur serait la bienvenue :pray:

---
Quelques captures d'écrans:

![](https://github.com/DevElkami/MesConges/blob/main/calendrier.png)
![](https://github.com/DevElkami/MesConges/blob/main/ldap.png)
![](https://github.com/DevElkami/MesConges/blob/main/rh.png)
![](https://github.com/DevElkami/MesConges/blob/main/service.png)
![](https://github.com/DevElkami/MesConges/blob/main/screen.png)
