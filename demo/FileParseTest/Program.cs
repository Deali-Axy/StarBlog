// See https://aka.ms/new-console-template for more information

var log = new System.Collections.Specialized.StringCollection();
var exclusionDirs = new List<string> {".git"};

const string path = @"E:\Documents\0_Write\0_blog\";

WalkDirectoryTree(new DirectoryInfo(path));

void WalkDirectoryTree(DirectoryInfo root) {
    FileInfo[] files = null;
    DirectoryInfo[] subDirs = null;

    // First, process all the files directly under this folder
    try {
        files = root.GetFiles("*.*");
    }
    // This is thrown if even one of the files requires permissions greater
    // than the application provides.
    catch (UnauthorizedAccessException e) {
        // This code just writes out the message and continues to recurse.
        // You may decide to do something different here. For example, you
        // can try to elevate your privileges and access the file again.
        log.Add(e.Message);
    }

    catch (DirectoryNotFoundException e) {
        Console.WriteLine(e.Message);
    }

    if (files != null) {
        foreach (FileInfo fi in files) {
            // In this example, we only access the existing FileInfo object. If we
            // want to open, delete or modify the file, then
            // a try-catch block is required here to handle the case
            // where the file has been deleted since the call to TraverseTree().
            Console.WriteLine(fi.FullName);
            Console.WriteLine(fi.DirectoryName.Replace(path, ""));
        }

        // Now find all the subdirectories under this directory.
        subDirs = root.GetDirectories();

        foreach (DirectoryInfo dirInfo in subDirs) {
            if (exclusionDirs.Contains(dirInfo.Name)) {
                continue;
            }

            if (dirInfo.Name.EndsWith(".assets")) {
                continue;
            }

            // Resursive call for each subdirectory.
            WalkDirectoryTree(dirInfo);
        }
    }
}