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

        public static char ValidateDrive(string userPrompt)
    {
        char userInput = '\0';
        bool isValid = false;

        // Loop until the user enters a valid input
        while (!isValid)
        {
            Console.WriteLine($"{userPrompt}:");
            string input = Console.ReadLine();

            // Validate if the input is a single character
            if (char.TryParse(input, out userInput))
            {
                // Check if the input is a capital letter (A-Z)
                if (userInput >= 'A' && userInput <= 'Z')
                {
                    isValid = true; // Input is valid, exit the loop
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a capital letter (A-Z).");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a single capital letter (A-Z).");
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
            //declare the output list
            List<string> elementsToCopy = new List<string>();


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
                    int hiddenFileCount = allFiles.Count(f => (File.GetAttributes(f) & FileAttributes.Hidden) != 0);

                    //also check for hidden folders
                    int hiddenFoldersCount = allSubFolders.Count(sub =>
                        (File.GetAttributes(sub) & FileAttributes.Hidden) != 0
                    );

                    //if folder contains hidden elements, add them to the list
                    if (hiddenFileCount > ZERO || hiddenFoldersCount > ZERO)
                    {
                        foldersWithHiddenElements.Add(folder);
                    }

                    //if the folder is empty, or empty with some hidden elements, update isEmpty
                    if (allFiles.Length == hiddenFileCount && allSubFolders.Length == hiddenFoldersCount)
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

                    /* 
                        enum FileAttributes
                        {
                            ReadOnly = 1,               // 0000 0000 0000 0001
                            Hidden = 2,                 // 0000 0000 0000 0010
                            System = 4,                 // 0000 0000 0000 0100
                            Directory = 16,             // 0000 0000 0001 0000
                            Archive = 32,               // 0000 0000 0010 0000
                            Device = 64,                // 0000 0000 0100 0000
                            Normal = 128,               // 0000 0000 1000 0000
                            Temporary = 256,            // 0000 0001 0000 0000
                            SparseFile = 512,           // 0000 0010 0000 0000
                            ReparsePoint = 1024,        // 0000 0100 0000 0000
                            Compressed = 2048,          // 0000 1000 0000 0000
                            Offline = 4096,             // 0001 0000 0000 0000
                            NotContentIndexed = 8192,   // 0010 0000 0000 0000
                            Encrypted = 16384,          // 0100 0000 0000 0000
                            IntegrityStream = 32768,    // 1000 0000 0000 0000
                            NoScrubData = 131072   // 0010 0000 0000 0000 0000
                        }

                        NB: think of a C# enum like a mapping object in python or JS, except that the value stored in 
                        each object property is a constant. The FileAttributes enum shown above is basically a const object
                        with a number of properties that store integer values that increase by 2^x. For example, from Archive 32
                        to Device 64, the value (64) has increased by 2^5 (32). The int values stored in the enum increase
                        in this way to allow the binary numbers to be compared easily using the bitwise OR comparison. IE
                        so each attribute can be represented by a unique bit in a binary number. This allows multiple attributes 
                        to be combined and checked efficiently using bitwise operations.

                        FileAttributes attributes = File.GetAttributes(file);

                        Therefore this method first checks the file to see which of the attribute flags (as defined in the enum)
                        are present in the file param. It then "switches on" each of these attributes (e.g. FileAttributes.hidden) . 
                        All FileAttributes properties are initially off.The method then returns an integer value which is the result 
                        of the OR bitwise comparison between the binary representations of each of those flags that have been switched on. 
                        This int is assigned to attributes. 

                    */

                    // Check if the Hidden attribute is set
                    if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        topLevelHiddenFiles.Add(file);
                        fileList.RemoveAt(i);
                        break;
                    }
                    /* 
                        EG: so lets say we run this: FileAttributes attributes = File.GetAttributes(file);
                        and lets say that the file has hidden, read-only and system attributes, the attributes 
                        variable will be assigned 7 (1+2+4). The first part of the condition above e.g. (attributes & FileAttributes.Hidden)
                        performs a bitwise AND comparison between the binary representations of 7 and 2 (FileAttributes.Hidden is 
                        always 2 as defined in the enum):

                            0000 0000 0000 0010   // 2
                            0000 0000 0000 0111   // 7
                            --------------------
                            0000 0000 0000 0010  == 2

                        KP: The AND bitwise comparison between a specific FileAttribute enum flag binary value (e.g. 2) and 
                        the binary sum of the ON flags (e.g. 7), reveals whether that flag is switched ON if the result of 
                        the bitwsie AND comparison is equal to the originla value. In this case
                        it inidicates whether the specified file has a .hidden attribute.
                    */

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
                if (selectedChar == YES)
                {
                    //construct the full file path 
                    string fullPath = Path.Combine(userFolder, folder);

                    //check it
                    WriteLine("");
                    WriteLine($"Constructed path for folder: {folder}");
                    WriteLine(fullPath);

                    //add it to output list
                    elementsToCopy.Add(fullPath);

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
                if (selectedChar == YES)
                {
                    //construct the full file path 
                    string fullPath = Path.Combine(userFolder, file);

                    //check it
                    WriteLine("");
                    WriteLine($"Constructed path for top-level file: {file}");
                    WriteLine(fullPath);

                    //add it to output list
                    elementsToCopy.Add(fullPath);

                }
            }



            /* 
                Now up to this point, the Program has obtained a fully filtered list of strings (elementsToCopy) which
                represent all the of the files/folders in the root C:\\ user account that the user wishes to back up.
                To finish up, we need to:
                1- obtain a folder name for the new folder into which the folders, and files 
                will be copied into. We should output this for use in the batch file.
                1a- obtain from the user, the drive letter of their external storage device they want to copy to
                2- write the elements to copy to a .txt file that can be used by the batch script
            */

            //0
            //1a
            string drivePrompt = "Please enter the drive letter of your external storage device (e.g. E for E:\\ drive):";
            char destDrive = InputValidator.ValidateDrive(drivePrompt);
            string driveRoot = destDrive + @":\"; // e.g., "E:\"


            //1-Choose or prompt for a destination directory (e.g. external drive path, or a backup folder like E:\Backup_2025-10-02)
            string destPrompt = "Please enter the name of the new folder which will be created in the destination drive to store backed up files/folders";
            string destFolder = InputValidator.ValidateString(destPrompt, "");
            string checkPath = Path.Combine(driveRoot, destFolder);

            // Check if the directory exists
            while(Directory.Exists(checkPath))
            {
                Console.WriteLine($"Folder '{destFolder}' already exists in the root folder of your storage device.");
                string rePromptForFolder = "Please enter a unique name of the new folder which will be created in the destination drive to store backed up files/folders";
                destFolder = InputValidator.ValidateString(rePromptForFolder, "");
                checkPath = Path.Combine(driveRoot, destFolder);
            }
            

            //2-
            // Output file path (e.g., "elements_to_copy.txt")
            string outputFileName = "elements_to_copy.txt";
            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);

            //check outputPath. It should be: "C:\Users\natha\C#\backupFolders\elements_to_copy.txt"
            WriteLine("");
            WriteLine($"Check output path: {outputPath}");

            // check if there is already a elements_to_copy.txt file in specified location
            if (File.Exists(outputPath))
            {
                Console.WriteLine($"{outputFileName} already exists at {outputPath}. It will be overwritten.");
                // Optionally, prompt the user for confirmation here
            }


            /* 
                next we have to construct the output .txt file so that it is in the following form.
                the first line of the file needs to contain the name of the backup folder which the user chose
                and stored in the destFolder variable. The subsequent lines contain the paths of the files/folders
                which the batch script must copy. EG:
            
                Backup_2025-10-06
                C:\Users\natha\Documents
                C:\Users\natha\Pictures
                C:\Users\natha\file.txt

                To do this we use the following code which creates an output list, adds the destFolder
                string on the first line, then adds the elementsToCopy paths assigning each elements (...)
                to a new line.
            */

            List<string> outputLines = new List<string>();
            outputLines.Add(destDrive.ToString()); // e.g "E"
            outputLines.Add(destFolder); // First line: destination folder e.g. "hp_envy_backup"
            outputLines.AddRange(elementsToCopy); // Remaining lines: paths to copy

            // This will overwrite the file if it exists, or create it if it doesn't
            // Writes each element to the .txt file as a new line
            File.WriteAllLines(outputPath, outputLines);

            
            
        }
    }
}


