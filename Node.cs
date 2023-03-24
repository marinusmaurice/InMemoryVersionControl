/*
Author: Maurice Marinus
Position: Symbolic Architect
Company: Symbolic Computing
Project: Source Code Management  or (Simple Source Code Management)
Description: Oversimplification of source control management software
License: MIT
*/
using System;
using System.Collections.Generic;
using System.IO;

namespace suscm
{
    /*
    This represents the virtual storage structure of the source code repository
    Each node represents a branch. 
    Branches are continual if it has the same name
    Each branch contains a list of files and directories.
    Whether or not a full instance or delta of differences are stored as system entries is up to the implementor
    This obviously has an effect on the fetch command implementation

    */
    public class Node
    {
        //TODO: remove name. Branch name only on commit
        public Node(string name)
        {
            this.branchName = name;
            this.commitedEntries = new List<SystemEntry>();
            this.nodes = new List<Node>();
            this.parent = null;
            this.key = Guid.Empty;
        }
        //Every single commit has a unique key (well as unique as guid's get)
        private Guid key;
        private List<SystemEntry> commitedEntries;
        private List<Node> nodes;
        private string branchName = "";
        private DateTime commitedDate;
        //The parent node of the commit
        private Node? parent;

        //Represents each commit to a parent node
        public List<Node>? Nodes => nodes;
        //A list of files and folders per commit
        public List<SystemEntry>? CommitedEntries => commitedEntries;
        //The name of commit. Commits with the same name are seen as the same path e.g. Commits named 'main or trunk' can be seen to represent the (classically known as the) 'trunk' branch
        public string BranchName => branchName;
        //The date and time a commit took place
        public DateTime CommitedDate => commitedDate;


        public bool Commit(List<SystemEntry> systemEntries, Node parentNode, string branchName = "")
        {
            if (systemEntries != null)
                this.commitedEntries.AddRange(systemEntries);
            this.branchName = branchName;
            this.commitedDate = DateTime.UtcNow;
            this.key = Guid.NewGuid();
            this.parent = parentNode;
            if (parentNode != null)
                parentNode.nodes.Add(this);
            return true;
        }

        //Makes a localcopy of branch
        public Node Checkout(bool includingHistory, bool includeChildNodes)
        {

            if (includingHistory && includeChildNodes)  //includes up to root, current snapshot and all subnodes
            { 
                Node tmpNode = this;
                while(tmpNode != null)
                {
                    if (tmpNode.parent == null) //we at tip of graph
                    {
                        Node clone = new Node(tmpNode.branchName);
                        clone.commitedDate = tmpNode.commitedDate;
                        clone.commitedEntries = new List<SystemEntry>();
                        clone.commitedEntries.AddRange(tmpNode.CommitedEntries ?? new List<SystemEntry>());
                        clone.key = tmpNode.key;
                        clone.nodes = new List<Node>();
                        clone.nodes.AddRange(tmpNode.nodes);

                        return tmpNode;
                    }
                    tmpNode = tmpNode.parent;
                }
            }
            else if (!includingHistory && !includeChildNodes) //not up to root and no subnodes. i.e. current snapshot only
            {

                Node clone = new Node(this.branchName);
                clone.commitedDate = this.commitedDate;
                clone.commitedEntries = new List<SystemEntry>();
                clone.commitedEntries.AddRange(this.CommitedEntries ?? new List<SystemEntry>());
                clone.key = this.key;
                //clone.parent = this.parent;
                return clone;

            }
            else if (includingHistory && !includeChildNodes) //up to root, current snapshot and no subnodes
            {
                List<Node> tmpNodeStack = new List<Node>();
                Node tmpNode = this;
                while (tmpNode != null)
                { 
                    Node clone = new Node(tmpNode.branchName);
                    clone.commitedDate = tmpNode.commitedDate;
                    clone.commitedEntries = new List<SystemEntry>();
                    clone.commitedEntries.AddRange(tmpNode.CommitedEntries ?? new List<SystemEntry>());
                    clone.key = tmpNode.key; 
                    clone.parent = tmpNode.parent;

                    tmpNodeStack.Add(clone);
                  
                    tmpNode = tmpNode.parent;
                }

                //Simply add the current node pointer to the previous node list
                for (int i = tmpNodeStack.Count-1; i >= 0; i--)
                {
                    if (i > 0)
                    {
                        int j = i - 1;
                        tmpNodeStack[i].nodes.Add(tmpNodeStack[j]);
                    }
                }

                //return the last node which should be the root node which now has list of all single line nodes leading to current node
                return tmpNodeStack[tmpNodeStack.Count-1];
            }
            else if (!includingHistory && includeChildNodes) //current snapshot and subnodes
            {
                Node clone = new Node(this.branchName);
                clone.commitedDate = this.commitedDate;
                clone.commitedEntries = new List<SystemEntry>();
                clone.commitedEntries.AddRange(this.CommitedEntries ?? new List<SystemEntry>());
                clone.key = this.key;
                //clone.parent = this.parent;
                clone.nodes = new List<Node>();
                clone.nodes.AddRange(this.nodes);
                return clone;
            }

            return null;
        }

        //Forking duplicates a point in repositories time and not a history from a point in time. The fork name has to be different to the existing fork and no other fork should exist with same name
        public Tuple<bool, object> Fork(string name)
        {
            if (string.IsNullOrEmpty(name) || name == this.BranchName)
            {
                return new Tuple<bool, object>(false, "A fork name cannot be empty or be the same name as the current branch");
            }

            //Test for uniqueness of name. This is applicable only to immediate subnodes
            if (this.nodes != null && this.nodes.Count > 0)
            {
                foreach (Node n in this.nodes)
                {
                    if (n.branchName == name)
                    {
                        return new Tuple<bool, object>(false, "A fork with the name exists on the parent already");
                    }
                }
            }

            //Do not clone sub nodes  (fork is duplicating point in time and not history from a point in time)
            Node clone = new Node(name);
            clone.commitedDate = this.commitedDate;
            clone.commitedEntries = new List<SystemEntry>();
            clone.commitedEntries.AddRange(this.CommitedEntries ?? new List<SystemEntry>());
            //Should key be duplicated or new'd. Going with new. 
            clone.key = Guid.NewGuid();
            clone.parent = this.parent;
            this.parent.nodes.Add(clone);

            return new Tuple<bool, object>(true, null);
        }
    }

    public class SystemEntry
    {
        private string name;
        private FileOrDirectory fileOrDirectory;
        private byte[] data;
        private DateTime lastWriteTime;

        public SystemEntry(string name, FileOrDirectory fileOrDirectory, DateTime lastWriteTime, byte[] data)
        {
            this.name = name;
            this.fileOrDirectory = fileOrDirectory;
            this.lastWriteTime = lastWriteTime;
            if (fileOrDirectory == FileOrDirectory.File)
            {
                this.data = data;
            }
        }
        public FileOrDirectory FileOrDirectory { get => fileOrDirectory; }
        public string Name { get => name; }
        public byte[] Data { get => data; }
        public DateTime LastWriteTime { get => lastWriteTime; }
    }

    public enum FileOrDirectory
    {
        File = 0,
        Directory = 1
    }
}
