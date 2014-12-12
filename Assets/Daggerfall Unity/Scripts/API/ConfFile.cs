#region Using Statements
using System;
using System.Text;
using System.IO;
using DaggerfallConnect.Utility;
#endregion

namespace DaggerfallConnect.Arena2
{
    /// <summary>
    /// Connects to a configuration file
    /// </summary>
    public class ConfFile
    {
        #region Class Variables

        /// <summary>
        /// Abstracts conf file to a managed disk or memory stream.
        /// </summary>
        private FileProxy managedFile = new FileProxy();

        #endregion

        #region Class Structures
        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ConfFile() {}

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="filePath">Absolute path to conf file.</param>
        /// <param name="usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="readOnly">File will be read-only if true, read-write if false.</param>
        public ConfFile(string filePath, FileUsage usage, bool readOnly)
        {
            Load(filePath, usage, readOnly);

            //Writing
            /*System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("name=test");
            System.IO.StreamWriter writer = managedFile.GetStreamWriter();
            writer.Write(sb.ToString());
            writer.Close();
            DaggerfallWorkshop.Game.DevConsole.displayText("written");
            */

            //Reading
            /*System.IO.StreamReader reader = managedFile.GetStreamReader();
            string line, name;
            string[] id_value = {"", ""};
            while ( (line = reader.ReadLine()) != null ) {
                id_value = line.Split('=');
                switch (id_value[0]) { 
                    case "name":
                        name = id_value[1].ToString();
                        break;
                } 
            }
            reader.Close();

            DaggerfallWorkshop.Game.DevConsole.displayText("read in name=" + id_value[1].ToString());
            */
            
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets default configuration file filename.
        /// </summary>
        static public string Filename
        {
            get { return "config.cfg"; }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Load Conf file.
        /// </summary>
        /// <param name="filePath">Absolute path to Conf file.</param>
        /// <param name="usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="readOnly">File will be read-only if true, read-write if false.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Load(string filePath, FileUsage usage, bool readOnly)
        {
            // Load file into memory
            if (!managedFile.Load(filePath, usage, readOnly))
                return false;

            // Read file
            if (!Read())
                return false;

            return true;
        }


        #endregion

        #region Internal Methods


        #endregion

        #region Private Methods

        #endregion

        #region Readers

        /// <summary>
        /// Read file.
        /// </summary>
        private bool Read()
        {
            try
            {
                // Step through file
                BinaryReader reader = managedFile.GetReader();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

      #endregion
    }
}
