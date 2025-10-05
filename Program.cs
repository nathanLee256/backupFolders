using System;
using System.IO;
using System.Collections.Generic;
using static System.Console;
using System.Text.RegularExpressions;




namespace backupFolders{
    /* First define a class which a public Input validator method */
    public class InputValidator
    {     //class with methods which validate user input
        public static char ValidateChar(string userPrompt, char char1, char char2)
        {
            char userInput = '\0';
            bool isValid = false;

            // Loop until the user enters a valid input
            while (!isValid)
            {
                WriteLine($"{userPrompt}:");
                string input = ReadLine();

                // Validate if the input is a char
                if (char.TryParse(input, out userInput))
                {
                    // Check if the input is one of the valid options
                    if (userInput == char1 || userInput == char2)
                    {
                        isValid = true; // Input is valid, exit the loop
                    }
                    else
                    {
                        WriteLine("Invalid input. Please enter one of the valid options.");
                    }
                }
                else
                {
                    WriteLine($"Invalid input. Please enter {char1} or {char2}.");
                }
            }
            return userInput;
        }

        public static string ValidateString(string userPrompt, string notAllowed = "")
        {
            string userInput = "";
            bool isValid = false;

            // Regex string to allow only letters, numbers, and underscores
            string pattern = @"^[a-zA-Z0-9_]+$";

            // continue to prompt until player enters valid input
            while (!isValid)
            {
                WriteLine($"{userPrompt}:");
                userInput = ReadLine();

                // Validate if the input matches the pattern (letters and numbers only)
                if (Regex.IsMatch(userInput, pattern))
                {

                    if (userInput != notAllowed)
                    {
                        isValid = true; // Input is valid, exit the loop
                    }
                    else
                    {
                        WriteLine("Invalid input. Name has been taken. Please choose a unique name.");
                    }
                }
                else
                {
                    WriteLine("Invalid input. Please enter only letters, numbers, and underscores with no special characters.");
                }
            }
            return userInput;
        }  
    };
    
    /* 
        Next, we define a class which contains recursive method which will be called in the final for loops which loop through 
        the filtered files and folders, to copy each folder/file into a directory with the same name in the destination location 
        (e.g. E:\old_hp_pc_data)
    */
    public class CopyDirectories
    {
        public static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            // Copy files
            string[] files;
            try
            {
                files = Directory.GetFiles(sourceDir);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"[Skipped: No access to files in {sourceDir}]");
                return;
            }
            foreach (var file in files)
            {
                try
                {
                    string destFile = Path.Combine(destDir, Path.GetFileName(file));
                    File.Copy(file, destFile, overwrite: true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Skipped: Error copying {file}: {ex.Message}]");
                }
            }

            // Copy subdirectories recursively
            string[] subdirs;
            try
            {
                subdirs = Directory.GetDirectories(sourceDir);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"[Skipped: No access to subdirs in {sourceDir}]");
                return;
            }
            foreach (var subdir in subdirs)
            {
                try
                {
                    string destSubDir = Path.Combine(destDir, Path.GetFileName(subdir));
                    CopyDirectory(subdir, destSubDir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Skipped: Error copying subdir {subdir}: {ex.Message}]");
                }
            }
        }

    }

    //class which contains a public static method that renders a summary table that is displayed in console to help user decide which hidden files/folders to backup
    public class Table
    {
        public static void renderTable()
        {
            // Empty line to start
            Console.WriteLine("");

            // Instructions
            Console.WriteLine("Refer to the table below to help decide whether to backup hidden files.");

            // Summary
            Console.WriteLine("");
            Console.WriteLine("-Don't back up hidden/system files like desktop.ini or Thumbs.db.");
            Console.WriteLine("-Consider backing up AppData if you want to preserve application data. But be aware it can be large and contain lots of cache/temp files.");
            Console.WriteLine("");

            // Table headers
            Console.WriteLine("{0,-25} {1,-30} {2,-45}", "Hidden File/Folder", "Backup Needed?", "Reason");
            Console.WriteLine(new string('-', 100));

            // Table rows
            Console.WriteLine("{0,-25} {1,-30} {2,-45}", "desktop.ini", "No", "Folder appearance only");
            Console.WriteLine("{0,-25} {1,-30} {2,-45}", "Thumbs.db", "No", "Thumbnails cache");
            Console.WriteLine("{0,-25} {1,-30} {2,-45}", ".DS_Store", "No (Windows)", "Mac only");
            Console.WriteLine("{0,-25} {1,-30} {2,-45}", ".git, .vs folders", "Maybe (if you use git/VS)", "For developer projects");
            Console.WriteLine("{0,-25} {1,-30} {2,-45}", "AppData (hidden dir)", "Yes, if you want app data", "App configs, emails, browser profiles");
            Console.WriteLine("");
        }
    }
    
    
    /* 
        This program needs to:
        1- loop over the files and folders contained within the C:\Users\userName root folder
        
        2-for every folder that doesn't start with a "." (i.e. all the file folders), the program
        must prompt user to enter a response (Y/N) and choose whether they want the contents of that
        folder backed up.
            a- if they choose yes, the Program must add the string name of the folder to an array.
            b- if they choose no, nothing happens, and the Program prompts user about the folder.

        3- After the loop has finished we should have an array of strings representing the names of the 
        folders that the current user wishes to back up. The contents of this array are written to a .txt file
        which will be saved in a prominent location in the fs, where it can be accessed by a windows batch script
        that actually copies the contents of the selected folders into a external/ flash drive. 
    */
    class Program
    {
        static void Main(string[] args)
        {
            //declare the output array
            string[] foldersToCopy;

            //extract the string file path of the root folder for current user (e.g C:\Users\<username>)
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            Console.WriteLine("");
            Console.WriteLine("Extracted folder:" + userFolder);

            //extract the string folder names from userFolder
            string[] folders = Directory.GetDirectories(userFolder);

            //convert to list to allow elements to be removed easily
            List<string> folderList = new List<string>(folders);

            //also create a list to store folders which contain hidden files OR hidden folders
            List<string> foldersWithHiddenElements = new List<string>();

            //loop through the folderList and filter it
            const char DOT = '.';
            const int ZERO = 0;

            Console.WriteLine("");
            Console.WriteLine("Folders:");

            //loop through list from back to front
            // e.g. ["Documents", "Downloads", "Pictures", "OneDrive"]

            for (int i = folderList.Count - 1; i >= 0; i--)
            {

                string folder = folderList[i]; //folder will store the full path ("C:\Users\natha\3D Objects")

                //first print folder to check extraction
                Console.WriteLine(folder);


                bool isEmpty = false;
                bool accessError = false;

                try
                {
                    //check if folder has files or subfolders
                    string[] allFiles = Directory.GetFiles(folder); //allFiles will be an array of string paths
                    string[] allSubFolders = Directory.GetDirectories(folder);

                    //check if any files are hidden files
                    //I assume the code below filters the allFiles array to select only hidden files
                    bool hasHiddenFiles = allFiles.Any(f => (File.GetAttributes(f) & FileAttributes.Hidden) != 0);

                    //also check for hidden folders
                    bool hasHiddenSubFolders = allSubFolders.Any(sub =>
                        (File.GetAttributes(sub) & FileAttributes.Hidden) != 0
                    );

                    //if folder contains hidden elements, add them to the list
                    if (hasHiddenFiles || hasHiddenSubFolders)
                    {
                        foldersWithHiddenElements.Add(folder);
                    }

                    //if the folder is empty, update isEmpty
                    if (allFiles.Length == ZERO && allSubFolders.Length == ZERO)
                    {
                        isEmpty = true;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Skipped: Error accessing {folder}: {ex.Message}]");
                    accessError = true;
                }


                //now remove all folders if they start with "." char, or are empty, or there was an accessError
                //this will not remove folders with hidden files or subfolders-we will prompt user about them later
                string folderName = Path.GetFileName(folder);
                if ((folderName.Length > 0 && folderName[0] == '.') || isEmpty || accessError)
                {
                    folderList.RemoveAt(i);
                }

            }

            //extract top-level files within userFolder
            string[] files = Directory.GetFiles(userFolder);

            //convert to list to allow elements to be removed easily
            List<string> fileList = new List<string>(files);

            //also create a list to store all top-level hidden files in the root folder 
            List<string> topLevelHiddenFiles = new List<string>();

            Console.WriteLine("");
            Console.WriteLine("Files:");

            //loop through list from back to front
            // e.g. ["oneFile.txt", "file.csv", "file.bat", "file.doc"

            for (int i = fileList.Count - 1; i >= 0; i--)
            {
                string file = fileList[i];
                bool accessError = false;

                //print to check
                Console.WriteLine(file);
                try
                {
                    //check if file is hidden by checking its attributes
                    FileAttributes attributes = File.GetAttributes(file);

                    // Check if the Hidden attribute is set
                    if ((attributes & FileAttributes.Hidden) != 0)
                    {
                        topLevelHiddenFiles.Add(file);
                    }

                    //remove file if it starts with "." char
                    string fileName = Path.GetFileName(file);
                    if (fileName.Length > ZERO && fileName[ZERO] == '.')
                    {
                        //remove from array
                        fileList.RemoveAt(i);
                    }
                }
                catch (Exception ex)
                {
                    //this block will catch any errors from the try block-specifically an error which may occur when attempting to get the file attributes
                    Console.WriteLine($"[Skipped: Error accessing {file}: {ex.Message}]");
                    accessError = true;
                }



            }

            //check that filtering was successful
            Console.WriteLine("");
            Console.WriteLine("Folders after filtering:");
            foreach (string folder in folderList)
            {
                Console.WriteLine(folder);
            }

            //check that filtering was successful
            Console.WriteLine("");
            Console.WriteLine("Files after filtering:");
            foreach (string file in fileList)
            {
                Console.WriteLine(file);
            }

            //call the renderTable() method 
            Table.renderTable();

            //loop again through filtered folders, prompting user to see if they want folder to be backed up
            for (int i = folderList.Count - 1; i >= 0; i--)
            {
                string folder = folderList[i];

                const char YES = 'Y';
                const char NO = 'N';

                string prompt = $"\nDo you wish to backup this folder:{folder}? Enter Y or N:";
                //first check if the folder has hidden elements
                if (foldersWithHiddenElements.Contains(folder))
                {
                    //add to prompt
                    string appendix = "Folder contains hidden files/folders.";
                    prompt = $"\n{appendix} Do you wish to backup this folder:{folder}? Enter Y or N:";
                }
                char selectedChar = InputValidator.ValidateChar(prompt, YES, NO);
                if (selectedChar != YES)
                {
                    //remove folder from filtered list
                    folderList.RemoveAt(i);
                }
            }

            //loop again through filtered files, prompting user to see if they want file to be backed up
            for (int i = fileList.Count - 1; i >= 0; i--)
            {
                string file = fileList[i];

                const char YES = 'Y';
                const char NO = 'N';
                string prompt = $"\nDo you wish to backup this file: {file}? Enter Y or N:";
                if (topLevelHiddenFiles.Contains(file))
                {
                    //add to prompt
                    string appendix = "File is hidden.";
                    prompt = $"\n{appendix}Do you wish to backup this file:{file}? Enter Y or N:";
                }
                char selectedChar = InputValidator.ValidateChar(prompt, YES, NO);
                if (selectedChar != YES)
                {
                    //remove file from filtered list
                    fileList.RemoveAt(i);
                }
            }

            /* 
                Now up to this point, the Program has obtained a fully filtered list of strings (fileList, folderList) which
                represent all the of the files/folders in the root C:\\ user account that the user wishes to back up.
                Next, we need to execute the backup.
            */

            //Choose or prompt for a destination directory (e.g. external drive path, or a backup folder like E:\Backup_2025-10-02)
            string destPrompt = "Please enter the name of the new folder which will be created in the destination drive to store backed up files/folders";
            string newFolder = InputValidator.ValidateString(destPrompt, "");

            //START check that drive exists and is writable
            string driveLetter = @"E:\";
            string destinationFolder = Path.Combine(driveLetter, newFolder); // e.g. backupRoot = "E:\old_pc_data"

            bool driveExists = Directory.Exists(driveLetter);
            bool driveReady = false;

            try
            {
                driveReady = new DriveInfo(driveLetter).IsReady;
            }
            catch
            {
                driveReady = false;
            }

            if (!driveExists || !driveReady)
            {
                Console.WriteLine("Destination drive does not exist or is not ready.");
                // handle error...
            }
            else
            {
                Directory.CreateDirectory(destinationFolder); //If the directory does not exist already, it is created here
                string testPath = Path.Combine(destinationFolder, "temp_file.txt");
                try
                {
                    File.WriteAllText(testPath, "test");
                    File.Delete(testPath);
                    Console.WriteLine("Destination is writable.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot write to the destination folder: " + ex.Message);
                    // handle error...
                }
            }

            //END check

            //Now finally we are ready to loop through the filtered files and folders and copy them over to selected destination

            //first let's copy the files over which is easy
            foreach (string file in fileList)
            {
                string fileName = Path.GetFileName(file); // just the filename, not the full path
                string destPath = Path.Combine(destinationFolder, fileName);
                File.Copy(file, destPath, overwrite: true); //file will be overwritten if it already exists in the destination location
            }

            //and the folders next
            foreach (string folder in folderList)
            {
                Console.WriteLine($"Copying: {folder}");
                string folderName = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                string destFolder = Path.Combine(destinationFolder, folderName);
                try
                {
                    CopyDirectories.CopyDirectory(folder, destFolder); // Your recursive or iterative copy function
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Skipped: Error copying {folder}: {ex.Message}]");
                }
            }

            //finally we should iterate over each folder and file in the destination folder and check if they are present in the filtered lists
            //display a success message if they are

            //folders
            bool foldersCopied = true;
            string[] copiedFolders = Directory.GetDirectories(destinationFolder);
            foreach (string folder in copiedFolders)
            {
                if (!folderList.Contains(folder))
                {
                    foldersCopied = false;
                }
            }

            if (foldersCopied)
            {
                //success
                WriteLine("Success! All folders have been copied into destination folder.");
            }
            else
            {
                //failure
                WriteLine("Failure. One or more folders failed to copy.");
            }

            //files
            bool filesCopied = true;
            string[] copiedFiles = Directory.GetFiles(destinationFolder); 
            foreach (string file in copiedFiles)
            {
                if (!fileList.Contains(file))
                {
                    filesCopied = false;
                }
            }

            if (filesCopied)
            {
                //success
                WriteLine("Success! All top-level files have been copied into destination folder.");
            }
            else
            {
                //failure
                WriteLine("Failure. One or more top-level files failed to copy.");
            }
            
        }
    }
}


