# VersionControlSystem - Basic File Version Control
Simplified look at how the most basic file version control system functions

The most basic used parts of version control are:
-commit
-checkout
-fork

**commit** - taking a snapshot of a section of the local file system entries and storing it in a repository.
Each snapshot does not override the previous one. Each snapshot is typically named, not necesarrily uniquely e..g trunk
Each new snapshot has a pointer to which snapshot it is derived from. This maintains linearage. The repository can be a database or custom file format or even another list of file system entries. The repository doesn't need to reside on a local machine. A not often used feature is the commit to a totally different snapshot i.e. 'alternative way to fork'

**checkout** - this is making an exact duplicate of a snapshot of the commited file system entries from the repository to the local disk for purposes of editing. One doesn't work on the snapshot file system entries directly. So checkout copies the file system entries from a selected snapshot from the repository to the local file system. There are different types of checkout
1. Current snapshot, history and child snapshots.
2. Current snapshot and child snapshots.
3. Current snapshot and history.
4. Current snapshot.

**fork** - forking is the similar to checkout except the duplicate is made within the repository and possibly given a new name. The linearage is maintained. However, the same thing as forking can be achieved via checkout, then commit, not to the immediate parent snapshot, but to the grand parent snapshot. It is desirable to use the explicit fork functionality instead of the 'workaround'.

**Sample code**

` 

    class Program
    {
        static void Main(string[] args)
        {

            Node root = new Node("root");
            Node subroot = new Node("subroot");

            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            string[] filesAndDirs = Directory.GetFileSystemEntries(di.FullName, "*", SearchOption.AllDirectories);

            EnumerationOptions eo = new EnumerationOptions();
            eo.RecurseSubdirectories = true;

            List<SystemEntry> se = new List<SystemEntry>();
            foreach (string entry in Directory.GetDirectories(Environment.CurrentDirectory, "*", eo))
            {
                se.Add(DisplayFileSystemInfoAttributes(new DirectoryInfo(entry)));
            }
            //  Loop through all the files in C.
            foreach (string entry in Directory.GetFiles(Environment.CurrentDirectory, "*", eo))
            {
                se.Add(DisplayFileSystemInfoAttributes(new FileInfo(entry)));
            }

            //Create root repo. Dont add files.
            root.Commit(null, null, "root");
            //Add to root node with files.
            subroot.Commit(se, root, "subroot");
            //fork last commit
            Tuple<bool, object> result = subroot.Fork("Forked");
            if (!result.Item1)
            {
                Console.WriteLine(result.Item2.ToString());
            }


            //add new file to test new branch
            File.WriteAllText(DateTime.Now.ToString("dd HH mm ss"), DateTime.Now.ToString());
            se = new List<SystemEntry>();
            foreach (string entry in Directory.GetDirectories(Environment.CurrentDirectory, "*", eo))
            {
                se.Add(DisplayFileSystemInfoAttributes(new DirectoryInfo(entry)));
            }
            //  Loop through all the files in C.
            foreach (string entry in Directory.GetFiles(Environment.CurrentDirectory, "*", eo))
            {
                se.Add(DisplayFileSystemInfoAttributes(new FileInfo(entry)));
            }


            //test checkout full repo
            Node newRoot = root.Checkout(true, true);

            //test checkout full repo
            Node newRoot1 = subroot.Checkout(true, true);

            Node subroot1 = new Node("subroot1");
            subroot1.Commit(se, subroot, "subroot1");

            //test checkout node and subnodes
            Node test1 = subroot.Checkout(false, true);
            //test checkout node and history
            Node test2 = subroot.Checkout(true, false);
            //test checkout node , no nodes, no history
            Node test3 = subroot.Checkout(false, false); 

        }

        static SystemEntry DisplayFileSystemInfoAttributes(FileSystemInfo fsi)
        {
            byte[]? data = null;
            //  Assume that this entry is a file.
            FileOrDirectory entryType = FileOrDirectory.File;

            // Determine if entry is really a directory
            if ((fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                entryType = FileOrDirectory.Directory;
            }

            if (entryType == FileOrDirectory.File)
            {
                data = File.ReadAllBytes(fsi.FullName);
            }

            SystemEntry se = new SystemEntry(fsi.FullName, entryType, fsi.LastWriteTime, data);
            //  Show this entry's type, name, and creation date.
            Console.WriteLine("{0} entry {1} was created on {2:D}", entryType, fsi.FullName, fsi.LastWriteTime);

            return se;
        }
    }
    `
