using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Quest.Lib.Trace;
using System.Diagnostics;

namespace Quest.Lib.EISEC
{
    [Serializable]
    public class EisecConfig
    {
        public IpDetails[] IpList;
    }

    /// <summary>
    /// holds configuration information for the EISECSimulator
    /// </summary>
    [Serializable]
    public class SimulatorConfig
    {
        public bool Enabled;
        public int Port;
    }

    public static class Config
    {
        public static T LoadConfig<T>(string filename) where T:class
        {
            try
            {
                T config;

                filename = ""; //SettingsHelper.SubstituteDataDirectory(filename);

                if (!File.Exists(filename))
                    return null;

                    var formatter = new XmlSerializer(typeof(T), new[] {typeof(IpDetails[]), typeof(User), typeof(IpDetails)});
                    // Create a TextReader to read the file. 
                    using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        using (TextReader reader = new StreamReader(fs))
                        {
                            // Serialize using the XmlTextWriter.
                            config = (T)formatter.Deserialize(reader);
                            reader.Close();
                        }
                        fs.Close();
                    }
                    return config;
            }
            catch (UnauthorizedAccessException exunauth)
            {
                throw new Exception(exunauth.Message + ". Ensure ASP.NET has full rights to this file");
            }
            catch (Exception ex)
            {
                var msg = $"Failed to load configuration file '{filename}' : {ex.Message}";
                Logger.Write(msg, TraceEventType.Error, "Config loader");
                throw new ApplicationException(msg);
            }
        }

        public static void SaveConfig<T>(T config, string filename) where T : class
        {
            var formatter = new XmlSerializer(typeof(EisecConfig));

            filename = ""; // SettingsHelper.SubstituteDataDirectory(filename);

            using (var fs = new FileStream(filename, FileMode.Create))
            {
                using (TextWriter writer = new StreamWriter(fs, new UTF8Encoding()))
                {
                    // Serialize using the XmlTextWriter.
                    formatter.Serialize(writer, config);
                    writer.Close();
                }
                fs.Close();
            }
        }
    }
}
