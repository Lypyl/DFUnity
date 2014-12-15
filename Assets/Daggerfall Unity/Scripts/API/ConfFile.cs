#region Using Statements
using System;
using System.Text;
using System.IO;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop;
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
            Logger.GetInstance().log("written");
            */

            //Reading
            System.IO.StreamReader reader = managedFile.GetStreamReader();
            string line, path;
            path = "";
            string[] id_value = {"", ""};
            while ( (line = reader.ReadLine()) != null ) {
                id_value = line.Split('=');
                switch (id_value[0]) { 
                    case "path":
                        path = id_value[1].ToString();
                        DaggerfallUnity dfUnity;
                        if(DaggerfallUnity.FindDaggerfallUnity(out dfUnity)) { 
                            dfUnity.Arena2Path = path;
                        }
                        break;
                } 
            }
            reader.Close();

            Logger.GetInstance().log("read in path=" + path);
            
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets default configuration file filename.
        /// </summary>
        static public string Filename
        {
            get { return "dfunity.cfg"; }
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
            if (!managedFile.Load(filePath, usage, readOnly, true))
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
