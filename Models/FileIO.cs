using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;

namespace PAENN2.Models
{
    public static class FileIO
    {
        public static void Newfile()
        {
            foreach (Node node in VarHolder.NodesList)
            {
                node.RemoveFromCanvas();
            }

            foreach (Member member in VarHolder.MembersList)
            {
                member.RemoveFromCanvas();
            }

            VarHolder.NodesList = new ObservableCollection<Node>();
            VarHolder.MembersList = new ObservableCollection<Member>();
            VarHolder.MaterialsList = new ObservableCollection<MemberMat>();
            VarHolder.SectionsList = new ObservableCollection<MemberSec>();
            VarHolder.LoadcasesList = new List<string> { "Caso 01" };
            VarHolder.CurrentLoadcase = "Caso 01";

            ActionHandler.ClearStates();
        }

        public static void SaveAs(string filepath)
        {
            FileState current_state = new FileState();
            BinarySerialization.WriteToBinaryFile(filepath, current_state);
        }

        public static void Open(string filepath)
        {
            FileState open_state = BinarySerialization.ReadFromBinaryFile<FileState>(filepath);
            VarHolder.NodesList = open_state.Nodes;
            VarHolder.MembersList = open_state.Members;
            VarHolder.MaterialsList = open_state.Materials;
            VarHolder.SectionsList = open_state.Sections;
            VarHolder.LoadcasesList = open_state.Loadcases;
            VarHolder.CurrentLoadcase = VarHolder.LoadcasesList[0];

            foreach (Node node in VarHolder.NodesList)
            {
                node.Reposition();
            }
            foreach (Member member in VarHolder.MembersList)
            {
                member.Reposition();
            }
        }
    }

    [Serializable]
    public class FileState
    {
        public ObservableCollection<Node> Nodes;
        public ObservableCollection<Member> Members;
        public ObservableCollection<MemberMat> Materials;
        public ObservableCollection<MemberSec> Sections;
        public List<string> Loadcases;

        public FileState()
        {
            Nodes = VarHolder.NodesList;
            Members = VarHolder.MembersList;
            Materials = VarHolder.MaterialsList;
            Sections = VarHolder.SectionsList;
            Loadcases = VarHolder.LoadcasesList;

        }
    }

    /// <summary>
    /// Functions for performing common binary Serialization operations.
    /// Original code by Daniel Schroeder (https://blog.danskingdom.com/saving-and-loading-a-c-objects-data-to-an-xml-json-or-binary-file/).
    /// <para>All properties and variables will be serialized.</para>
    /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
    /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
    /// </summary>
    public static class BinarySerialization
    {
        /// <summary>
        /// Writes the given object instance to a binary file.
        /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
        /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the XML file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the XML file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite)
        {
            using (Stream stream = File.Open(filePath, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        /// <summary>
        /// Reads an object instance from a binary file.
        /// </summary>
        /// <typeparam name="T">The type of object to read from the XML.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the binary file.</returns>
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }
    }
}
