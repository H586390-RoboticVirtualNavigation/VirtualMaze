# Developer Guide
* [Setting Up](#setting-up)
* [SR Research SDK](#SR-Research-Eyelink-SDK)
* [Building VirtualMaze](#Building-VirtualMaze)
* [Creating edfapi.bundle for MacOS](#Creating-edfapi.bundle-for-MacOS)
* [Documentation](#Documentation)

# Setting Up
#### Prerequisites
* Unity
* Git
* SR Research SDK

#### Downloading project files
1. Install Git and your preferred Git Gui Client. (preferably Sourcetree)
2. Clone the VirtualMaze to a location on your laptop

 *Repository URL location*
 ![web url location](/docs/images/web-url-location.PNG)

3. Open up Unity, select *Open* and select folder that you just cloned.

![unity-open](/docs/images/unity-open.PNG)

4. Search for the *Start* Scene in *Assets>Scenes* folder and open it.

5. You should have VirtualMaze ready for development now.

##### Note:
A good place to start understanding the project will be to look into *BasicLevelController.cs* and *ExperimentController.cs*. These 2 files are where most of the logic regarding the experiment process is held.

# SR Research Eyelink SDK

This project makes use of the Eyelink SDK. Information and the SDK can be found at the [SR Research Forum](https://www.sr-support.com/forum). To gain access to the forum create a free account.

# Building VirtualMaze

A folder named `out` in the root of the project folder is created for developers to build VirtualMaze into.

Developers can also build their games here for convenience as then contents of the folder is ignored by Git and will not be uploaded to the remote repository.

# Creating edfapi.bundle for MacOS
The MAC developer kit for EDF_ACCESS_API provided does not have an out of the box .bundle file to be used as a plugin Unity. This section serves to provide steps to create the .bundle file if the library needs to be updated.

Prerequisites:
- Eyelink SDK for MacOS is already installed on the MacOs Computer.

To create a plugin, use XCode create a new .bundle project.
In the project settings at the *General > Linked Frameworks and Libraries*, Select *Add Other...* and add the *edfapi.framework* from the MacOS *Library > Frameworks* folder.

Copy all of the .c and .h files from *Application > Eyelink > EDF_ACCESS_API > Example* into the project.

Run and build. A possible error that occurs is the missing *edf_data.h* file. A way to solve this is to copy the *edf_data.h* into the project.

# Documentation
To write this documentation, the text editor [Atom](https://atom.io/) was used as it supports syntax highlighting and preview of Markdown files(.md).

Any other editors can also be used as long as it works. Atom is just a suggestion.
