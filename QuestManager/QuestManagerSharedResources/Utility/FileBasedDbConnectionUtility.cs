using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace QuestManagerSharedResources.Utility
{
    /// <summary>
    /// Reads and writes JSON data to a file, with optional delimiter-based embedding for storing multiple datasets in one file.
    /// Uses a named EventWaitHandle to coordinate file access across processes.
    /// </summary>
    public class FileBasedDbConnectionUtility
    {
        EventWaitHandle _waitHandle;

        public FileBasedDbConnectionUtility(string handleName)
        {
            _waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, handleName);
        }

        /// <summary>
        /// Reads and deserializes a list of objects from a JSON file. If a delimiter is provided, only the delimited section is read.
        /// </summary>
        public List<T> GetListOfDataFromFile<T>(string filePath, string delimiter = null)
        {
            List<T> result = new List<T>();

            _waitHandle.WaitOne();

            try
            {
                if (!File.Exists(filePath))
                    CreateDirectory(filePath);

                string readFile = File.ReadAllText(filePath);

                if (!string.IsNullOrEmpty(delimiter))
                {
                    var jsonData = RetreiveDelimitedData(delimiter, readFile);
                    return JsonConvert.DeserializeObject<List<T>>(jsonData) ?? new List<T>();
                }
                else
                {

                    result = JsonConvert.DeserializeObject<List<T>>(readFile) ?? new List<T>();
                }
            }
            catch (Exception)
            {
                _waitHandle.Set();
                throw;
            }
            finally
            {
                _waitHandle.Set();
            }

            return result;
        }


        /// <summary>
        /// Serializes and writes a list of objects to a JSON file. If a delimiter is provided, only the delimited section is replaced; otherwise the entire file is overwritten.
        /// </summary>
        public void WriteListDataToFile<T>(string filePath, List<T> data, string delimiter = null)
        {
            _waitHandle.WaitOne();
            try
            {
                string readFile;

                if (!File.Exists(filePath))
                {
                    CreateDirectory(filePath);
                    readFile = string.Empty;
                }
                else
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        readFile = reader.ReadToEnd();
                    }
                }

                string fileData = string.Empty;
                if (!string.IsNullOrEmpty(delimiter))
                {
                    fileData = ReplaceDelimitedData<T>(delimiter, readFile, data);
                }
                else
                {
                    fileData = Serialize(data);
                }


                File.WriteAllText(filePath, fileData);
            }
            catch (Exception)
            {
                _waitHandle.Set();
                throw;
            }
            finally
            {
                _waitHandle.Set();
            }
        }

        /// <summary>
        /// Creates a backup of the current file data before a write. Copies to a `.Old.json` file, or updates a delimited backup section if a delimiter is provided.
        /// </summary>
        public void BackupDB(string filePath, string delimiter)
        {
            _waitHandle.WaitOne();
            try
            {
                if (string.IsNullOrEmpty(delimiter))
                {
                    File.Copy(filePath, filePath.Replace(".json", "Old.json"), true);
                    return;
                }

                string allBackupData;
                string delimiterBackup = delimiter + "Old";

                using (var reader = new StreamReader(filePath))
                {
                    allBackupData = reader.ReadToEnd();
                }

                var delimitedbackupData = RetreiveDelimitedData(delimiterBackup, allBackupData);

                ReplaceDelimitedData(delimiterBackup, allBackupData, delimitedbackupData);
            }
            catch (Exception)
            {
                _waitHandle.Set();
                throw;
            }
            finally
            {
                _waitHandle.Set();
            }
        }

        private static string Serialize<T>(List<T> data) =>
            JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                ContractResolver = new JsonReadOnlyPropertiesResolver()
            });

        private static string RetreiveDelimitedData(string delimiter, string readFile)
        {
            if (string.IsNullOrEmpty(readFile))
                return string.Empty;

            int startIndex = readFile.IndexOf(delimiter) + delimiter.Length;
            int endIndex = readFile.IndexOf(delimiter, startIndex);

            if (startIndex != -1 && endIndex != -1)
                return readFile.Substring(startIndex, endIndex - startIndex).Trim();

            return string.Empty;
        }

        private static string ReplaceDelimitedData<T>(string delimiter, string readFile, List<T> newData)
        {
            return ReplaceDelimitedData(delimiter, readFile, Serialize(newData));
        }

        private static string ReplaceDelimitedData(string delimiter, string readFile, string newData)
        {
            var delimiterStart = readFile.IndexOf(delimiter);

            int startIndex = -1;
            int endIndex = -1;

            if (delimiterStart != -1)
            {
                startIndex = delimiterStart + delimiter.Length;
                endIndex = readFile.IndexOf(delimiter, startIndex);
            }

            if (startIndex != -1 && endIndex != -1)
            {
                // Remove the delimiters and the delimited data if there is nothing to add
                if (string.IsNullOrEmpty(newData))
                    return readFile.Substring(0, startIndex - delimiter.Length) +
                                         readFile.Substring(endIndex + delimiter.Length);

                string updatedFile = readFile.Substring(0, startIndex) +
                                     newData +
                                     readFile.Substring(endIndex);

                return updatedFile;
            }

            readFile = readFile + delimiter + newData + delimiter;

            return readFile;
        }

        private static void CreateDirectory(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            var createdDb = File.Create(filePath);
            createdDb.Close();
        }
    }
}
