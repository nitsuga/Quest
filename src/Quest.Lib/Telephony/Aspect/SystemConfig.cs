using Quest.Lib.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Quest.Lib.Telephony.AspectCTIPS
{
    [Serializable]
    public class SystemConfig 
    {
        public CollabChannelConfig CollabChannelConfig {get; set; }

        public List<CTIChannelConfig> CTIChannelConfigs { get; set; }

        static public SystemConfig Load( string filename )
        {
            try
            {
                var formatter = new XmlSerializer(typeof(SystemConfig));

                if (File.Exists(filename))
                {
                    using (StreamReader m = new StreamReader(filename))
                    {
                        SystemConfig cfg = formatter.Deserialize(m) as SystemConfig;

                        return cfg;
                    }
                }
                else
                    SaveDummy(filename);
            }
            catch(Exception ex)
            {
                Logger.Write(string.Format("Failed to load config: {0}", ex.ToString()), TraceEventType.Information, "CTIPS");

                return SaveDummy(filename);
            }

            return null;
        }

        static public SystemConfig SaveDummy(string filename)
        {
            var formatter = new XmlSerializer(typeof(SystemConfig));

            SystemConfig cfg = new SystemConfig();
            cfg.CTIChannelConfigs = new List<CTIChannelConfig>();
            cfg.CTIChannelConfigs.Add(new CTIChannelConfig()
            {
                Name = "HQ CTI Primary",
                ClientCallbackURL = "http://localhost:8174/www.aspect.com/PublishEvents",
                Enabled = true,
                UserName = "collaboratortest",
                Password = "Police999",
                TenantId = 1,
                CTIPortalServiceAddress = "http://34ASPCES001:8170/Aspect.UnifiedIP.EDK.CTIPortalService"
            });

            cfg.CTIChannelConfigs.Add(new CTIChannelConfig()
            {
                Name = "HQ CTI Secondary",
                ClientCallbackURL = "http://localhost:8175/www.aspect.com/PublishEvents",
                Enabled = true,
                UserName = "collaboratortest",
                Password = "Police999",
                TenantId = 1,
                CTIPortalServiceAddress = "http://34ASPCES002:8170/Aspect.UnifiedIP.EDK.CTIPortalService"
            });

            cfg.CollabChannelConfig = new CollabChannelConfig()
            {
                Hostname = "localhost",
                Name = "Collaborator",
                Port = 6024
            };

            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                using (TextWriter writer = new StreamWriter(fs, new UTF8Encoding()))
                {
                    // Serialize using the XmlTextWriter.
                    formatter.Serialize(writer, cfg);
                    writer.Close();
                }
            }
            return cfg;
        }
    }

    [Serializable]
    public class CollabChannelConfig
    {
        public string Name { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
    }

    [Serializable]
    public class CTIChannelConfig
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string CTIPortalServiceAddress { get; set; }
        public string ClientCallbackURL { get; set; }
        public int TenantId { get; set; }
        public int DialoutServiceId { get; set; }
        public int HeartbeatTimeout { get; set; }
        public int StartupDelay { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }
}
