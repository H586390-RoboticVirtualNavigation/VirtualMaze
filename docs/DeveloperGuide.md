# Developer Guide
* [Setting Up](#setting-up)
* [Gitflow Workflow](#gitflow-workflow)
* [Gitflow with Sourcetree](#gitflow-with-sourcetree)
* [Documentation](#Documentation)

# Setting Up
#### Prerequisites
* Unity
* Git

#### Downloading project files
1. Install Git and your preferred Git Gui Client. (preferably Sourcetree)
2. Clone the VirtualMaze to a location on your laptop

 *Repository URL location*
 ![web url location](/docs/images/web-url-location.PNG)

3. Open up Unity, select *Open* and select folder that you just cloned.

![unity-open](/docs/images/unity-open.PNG)

4. You should have VirtualMaze ready for development now.

# Gitflow Workflow
This guide is adapted from [Atlassian](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow) and it is meant to be a brief explanation of the Gitflow workflow. Click on the link above to read more.

##### Master Branch
Meant to store be the final versions of VirtualMaze. **No development** is to be committed to this branch. Only final releases from the `Release` are to be merged into this branch.

##### Development Branch
Completed features are merged into the `Development` branch. When this branch is merged with enough features for a release, this branch will be merged into the `Release` branch for final checking/bug fixes.

This branch is created from `master`. It acts as a secondary master branch.

##### Feature Branch
Each new feature should exist in its own branch. This is to allow concurrent development as developers would not have to worry about features that other developers are working on affect their work. Features only interact with `Develop` branch.

This branch is created from `develop` and will be merged back into 'develop' once the feature is completed.

##### Release Branch
No new features should be merged into the `Release` branch. Developers working on this branch should only focus on bug fixes, updating version numbers or documentations.

This branch is created from `develop` and when completed, it will be merged into both `master` and `develop` branches.

##### Hotfix Branch
This branch is to quickly patch issues that occur in previous versions in the master branch.

If there are errors or bugs in master, this branch is created and merged into both `develop` and `master` branches

# Gitflow with Sourcetree
1. Clone the project. See [*Setting Up*](#setting-up)
  - Optional set up Remote Account.
![Sourcetree cloning](/docs/images/clone-project.PNG)

2. Once the VirtualMaze is cloned, click the gitflow button found on the top right.
![gitflow button](/docs/images/gitflow-button.PNG)

3. Leave all the gitflow settings as default and click ok.

4. Now you are ready to start using the Gitflow workflow. Click the gitflow button again to start developing a feature.

5. To work on a branch that is not in your local repository, double click on the desired branch in the `Log/History` and `Checkout New Branch`.

![checkout existing branches](/docs/images/check-out-existing-branches.PNG)

# Documentation
To write this documentation, the text editor [Atom](https://atom.io/) was used as it supports syntax highlighting and preview of Markdown files(.md).

Any other editors can also be used as long as it works. Atom is just a suggestion.
