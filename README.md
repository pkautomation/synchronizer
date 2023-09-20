# Synchronizer task:
Please implement a program that synchronizes two folders: source and replica. The program should maintain a full,
identical copy of source folder at replica folder.

Synchronization must be one-way: after the synchronization content of the replica folder should
be modified to exactly match content of the source folder;

Synchronization should be performed periodically.

File creation/copying/removal operations should be logged to a file and to the console output;

Folder paths, synchronization interval and log file path should be provided using the command line arguments;

It is undesirable to use third-party libraries that implement folder synchronization;

It is allowed(and recommended) to use external libraries implementing other well-known algorithms.
For example, there is no point in implementing yet another function that calculates MD5 if you need it for the task 
it is perfectly acceptable to use a third-party (or built-in) library.

# Build 
```synchronizer\Synchronizer> dotnet build Synchronizer```

# Run (example)
```synchronizer\Synchronizer> dotnet run --project Synchronizer c:\\test\sourceFolder c:\\test\replicaFolder 10 c:\\test\logfile.log```


# Application logging
To tune down logging a bit you can modify log4net.config file.
Change configuration.root.level value from 
```
<level value="ALL" />
```
to
```
<level value="INFO" />
```
To see only the logs requested within the task

# General Notes
App was tested on Windows, but it has potential to work also on Linux.
I have not checked the app behaviour if the user has no permission to perform action like: create directory, delete the file, create file. In case of issues that I did not handle the app should exit gracefully with exception being logged in a logger.

Todo: unit tests, perhaps refactor to add strategy pattern here and there and using async methods where applicable (example: Synchronizator.ProcessFiles method)
