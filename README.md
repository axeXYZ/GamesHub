# Workflow Git et Procédure de Mise en Production

Ce document décrit le flux de travail Git (Git Flow) et les conventions adoptées pour le développement et la mise en production de ce projet. L'objectif est d'assurer la stabilité du code, de faciliter la collaboration et de structurer le processus de release.

## Branches Principales

Nous utilisons deux branches principales avec une durée de vie infinie :

* ### `main`
    * **Rôle :** Représente la version la plus récente du code en **production**. Le code sur `main` doit toujours être stable et déployable.
    * **Protection :** Cette branche est protégée. Les commits directs sont interdits.
    * **Mise à jour :** Ne reçoit des modifications que par fusion (merge) depuis des branches `release` (procédure standard) ou `hotfix` (corrections urgentes). Chaque commit sur `main` devrait correspondre à une version de production et être tagué.

* ### `develop`
    * **Rôle :** Branche d'**intégration principale**. Elle contient les dernières fonctionnalités développées et testées, prêtes pour la prochaine release. C'est l'état le plus à jour du développement.
    * **Protection :** Les commits directs sont déconseillés voire interdits.
    * **Mise à jour :** Reçoit des modifications par fusion depuis les branches `feature` et `bugfix` via des Pull Requests / Merge Requests. Sert de base pour créer les branches `feature`, `bugfix` et `release`.

## Flux de Développement des Fonctionnalités (`feature`)

Toute nouvelle fonctionnalité doit être développée dans une branche dédiée.

1.  **Création de la Branche :**
    * Assurez-vous que votre branche `develop` locale est à jour (`git checkout develop; git pull origin develop`).
    * Créez votre nouvelle branche *depuis* `develop` :
        ```bash
        git checkout -b feature/nom-de-la-feature develop
        ```

2.  **Nommage de la Branche `feature` :**
    * Utilisez le préfixe `feature/`.
    * Suivez d'une description courte et en minuscules, séparée par des tirets (`-`).
    * Si vous utilisez un système de suivi (Jira, Trello, GitHub Issues), incluez l'ID du ticket.
    * **Exemples :**
        * `feature/ajout-connexion-google`
        * `feature/TICKET-123-refonte-dashboard-utilisateur`
        * `feature/amelioration-performances-recherche`

3.  **Développement :**
    * Effectuez votre travail sur cette branche.
    * Faites des commits atomiques et réguliers avec des messages clairs (voir section "Messages de Commit").
    * Poussez régulièrement votre branche sur le dépôt distant (`git push origin feature/nom-de-la-feature`) pour sauvegarder et permettre la visibilité.

4.  **Finalisation et Pull Request (PR) / Merge Request (MR) :**
    * Une fois la fonctionnalité terminée et testée localement :
        * Assurez-vous que votre branche est à jour avec les dernières modifications de `develop` (utilisez `git pull origin develop` puis `git rebase develop` sur votre branche feature, ou `git merge develop` si vous préférez les commits de fusion). Gérez les conflits si nécessaire.
        * Poussez les dernières modifications de votre branche feature.
        * Créez une Pull Request (PR) ou Merge Request (MR) depuis votre branche `feature/...` vers la branche `develop` via l'interface de la plateforme (GitHub, GitLab, etc.).

5.  **Revue de Code et Tests :**
    * Votre PR/MR doit être relue par au moins un autre membre de l'équipe.
    * Les tests automatisés (Intégration Continue) doivent passer avec succès.
    * Apportez les modifications demandées lors de la revue.

6.  **Fusion :**
    * Une fois la PR/MR approuvée et les tests validés, elle peut être fusionnée dans `develop`.
    * Privilégiez l'option **"Squash and Merge"** si disponible. Cela combine tous les commits de la branche feature en un seul commit sur `develop`, gardant l'historique de `develop` propre et lisible. Assurez-vous que le message du commit final résume bien la fonctionnalité ajoutée.

7.  **Nettoyage :**
    * Après la fusion réussie, supprimez la branche `feature` locale et distante :
        ```bash
        git checkout develop
        git pull origin develop # Se mettre à jour
        git branch -d feature/nom-de-la-feature # Supprimer localement
        git push origin --delete feature/nom-de-la-feature # Supprimer sur le dépôt distant
        ```

## Correction de Bugs (`bugfix`)

Pour les bugs non critiques découverts dans le code de `develop` (qui ne sont pas encore en production) :

* **Flux :** Similaire à celui des `feature`.
* **Nommage :** Utilisez le préfixe `bugfix/`. Ex: `bugfix/probleme-affichage-safari`, `bugfix/TICKET-456-calcul-incorrect-tva`.
* **Branche Source :** `develop`.
* **Branche Cible (PR/MR) :** `develop`.

## Corrections Urgentes en Production (`hotfix`) - (Optionnel)

Pour les bugs critiques nécessitant une correction immédiate en production :

* **Nommage :** Utilisez le préfixe `hotfix/`. Ex: `hotfix/v1.0.1-correction-crash-connexion`.
* **Branche Source :** `main`.
* **Branche Cible (PR/MR) :** Doit être fusionnée dans `main` (pour déploiement urgent) ET dans `develop` (pour intégrer la correction dans le flux de développement principal).

## Préparation de Release (`release`) - (Optionnel mais recommandé)

Quand `develop` contient assez de fonctionnalités prêtes pour une mise en production :

* **Nommage :** Utilisez le préfixe `release/`. Ex: `release/v1.1.0`.
* **Branche Source :** `develop`.
* **Objectif :** Permet de stabiliser la version (derniers tests, corrections mineures, mise à jour de version, documentation). Aucune nouvelle fonctionnalité ne doit être ajoutée ici.
* **Branche Cible (PR/MR) :** Fusionnée dans `main` (une fois prête, taguer ce commit sur `main`) ET dans `develop` (pour récupérer les dernières corrections faites pendant la release).

## Conventions de Nommage des Branches (Résumé)

* `main` : Branche de production.
* `develop` : Branche d'intégration et de développement principal.
* `feature/description-ou-ticket-id` : Pour les nouvelles fonctionnalités.
* `bugfix/description-ou-ticket-id` : Pour les corrections de bugs sur `develop`.
* `hotfix/description-ou-version` : (Optionnel) Pour les corrections urgentes sur `main`.
* `release/version` : (Optionnel) Pour préparer une mise en production.

Utilisez des noms descriptifs, en minuscules, séparés par des tirets (`-`).

## Messages de Commit

Utilisez des messages de commit clairs et significatifs. La convention [Conventional Commits](https://www.conventionalcommits.org/) est fortement recommandée pour standardiser les messages et faciliter la génération automatique de changelogs.

**Exemple :** `feat: ajoute la connexion via Google OAuth` ou `fix: corrige le calcul de la TVA pour les clients hors UE`.

## Mise en Production

1.  La mise en production se fait **exclusivement** à partir de la branche `main`.
2.  Le passage de `develop` à `main` se fait idéalement via une branche `release/` pour la stabilisation.
3.  Une fois la branche `release/` (ou `hotfix/`) fusionnée dans `main`, un **tag Git** (ex: `v1.0.0`, `v1.0.1`) doit être créé sur le commit de fusion sur `main` pour marquer la version.
    ```bash
    git checkout main
    git pull origin main
    # Fusion de la branche release/vX.Y.Z ou hotfix/... a été faite
    git tag -a vX.Y.Z -m "Release version X.Y.Z"
    git push origin vX.Y.Z # Pousser le tag
    ```
4.  Le déploiement est ensuite déclenché à partir de ce tag sur la branche `main`.
