# backupFolders
A C# console application which can be used to quickly back up a PC

# How to Use to back up your PC data
1. Insert an external hard drive or flash drive. Open File Explorer and take note of its drive letter (e.g. E:\\)

2. Clone github project into a local project folder on your PC (e.g. "C:\Users\<userName>\backupFolders") using the following 
terminal command: git clone https://github.com/nathanLee256/backupFolders.git

3. Using Powershell terminal, navigate to the local project folder (e.g. cd backupFolders).

4. Run the C# console application using this command in your terminal (dotnet run).
    a- this will run the program which will prompt you the user to select which folders and files on your system
    you want to back up (i.e. that you want to be copied to your chosen external storage location). Follow prompts.

5. When the program has finished running it will have obtained the exact file paths of all your selected files/folders
to be copied. 

6. In File Explorer, navigate into the local project we cloned (e.g. "C:\Users\<userName>\backupFolders). Find the 
"execute_backup.bat" file and right-click it, then select "Run as Administrator". This will perform the backup which may take some time.

7. When it is complete, check your external storage device to confirm your data has been copied over. 

## Features
- Select multiple files and folders for backup
- Stores backup selections in a text file for later use
- Uses a batch script to perform fast, reliable copying
- Works with any external drive or flash storage

## Requirements
- Windows 10/11
- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download) or later
- Administrator privileges to run the batch script

## Installation
1. [Install .NET 6 SDK](https://dotnet.microsoft.com/en-us/download) if you don't have it.
2. Clone this repo:
   ```
   git clone https://github.com/<your-username>/backupFolders.git
   ```
3. Open the project folder in your terminal or editor.

## Troubleshooting
- If the batch script says it can't find `elements_to_copy.txt`, make sure you run it from the project directory and after running the C# app.
- Run the batch file as administrator to avoid permission errors during backup.
- If your external drive doesn't appear, check that it's connected and accessible via File Explorer.

## License
MIT License

## Contact
For questions or feedback, open an issue on GitHub or email <n.ahern@connect.qut.edu.au>