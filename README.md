# dynamically load story images from sd card via ROS message.

## NOTICE
1. Please change storybook folder directory to match your local directory when running on Unity Editor.
          The place to change it is in MainGameController.cs
          Look for "string storyPath"
 
2. The recommended external directory on Android sd card is "edu.mit.media.prg.sar.opal.base", so please create a local directory with this name on your Andoird.
4. Image size for each image in a given story cannot be over 1MB when the app is running on a tablet.
5. For Android, please upload the opal-config.txt to the local app directory on the sd card and change the ROS IP address accordingly.
          

