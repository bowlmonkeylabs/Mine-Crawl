# Configuring automated build for a new project
## Repository configuration
Each project needs to request a license from Unity. (really each GitHub runner/machine used, but the machine should persist for the repo).
  - First run the "unity-activation" workflow, then follow the [game-ci instructions here|https://game.ci/docs/github/activation#personal-license] to registed the generated license artifact with Unity.
  - Then save the contents of the license provided by Unity as a repository secrete UNITY_LICENSE

## Organization configuration
The build script also depends on several secrets which we have saved at the organization level.
These do not need to be updated per repo, but they are noted here for awareness.

- REPOS_ACCESS_TOKEN: This is GitHub-generated Personal Access Token used to access the sub-repositories of the project during the build. Currently this token was provided from maxlep's account and will expire after 30 days. We need to remember to refresh this token, and should possibly consider creating a dedicated GitHub user for this purpose. https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token
- UNITY_EMAIL: This must be the same Unity email used in the license generation step above. (According to game-ci, this is used during the build step to reactivate the license). Currently generated from Max's personal unity account, but we might consider making an organization account for this purpose.
- UNITY_PASSWORD: Password to access the above Unity account.
