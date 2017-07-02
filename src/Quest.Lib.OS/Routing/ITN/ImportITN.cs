#define ONETHREADPERDIRECTORY

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Quest.Lib.DataModel;
using Quest.Lib.OS.Model;
using Quest.Lib.Trace;

namespace Quest.Lib.OS.Routing.ITN
{
    public class ImportItn
    {
        public int Queuesize;
        public int Threadnumber;
        private readonly int _batchsize = 15000;

        public event EventHandler<MessageArgs> Message;
        public event EventHandler FileCountChanged;


        private List<string> cmds = new List<string>();
        private AutoResetEvent signal = new AutoResetEvent(false);
        private bool _stopWriter;
        private HashSet<string> _fids =  new HashSet<string>();

        private void AddCommand(string cmd, string fid)
        {

            lock (cmds)
            {
                if (_fids.Contains(fid))
                    return;

                _fids.Add(fid);

                cmds.Add(cmd);
                if (cmds.Count > _batchsize)
                        signal.Set();
            }
        }

        private void WriterQueue()
        {
            using (var entities = new QuestEntities())
            {
                entities.Database.CommandTimeout = int.MaxValue;
                int count;
                do
                {
                    try
                    {

                    signal.WaitOne(new TimeSpan(0, 0, 0, 1));
                    lock (cmds)
                    {
                        if (cmds.Count > 0)
                        {
                            string cmd = string.Join("\n", cmds);
                            entities.Database.ExecuteSqlCommand(cmd);
                            cmds.Clear();
                        }
                        count = cmds.Count;
                    }
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.ToString());
                        throw;
                    }
                } while (!_stopWriter || count > 0);
            }
        }

        private void Worker(object state)
        {
            var wis = (Workitems) state;
            foreach (var wi in wis)
            {
                var filename = "";
                try
                {
                    filename = UncompressFile(wi.File);

                    var args = new MessageArgs {Message = "Uncompressing " + wi.File};

                    Message?.Invoke(this, args);

                    ImportSingleItnFile(wis.Thread, filename);
                    // System.IO.File.Delete(destinationFilename);
                }
                catch (Exception ex)
                {
                    Logger.Write($"**** Reprocess file: {filename} **** \n{ex}", GetType().Name);
                }
                Queuesize--;
                FileCountChanged?.Invoke(this, null);
            }
        }

        private static string UncompressFile(FileInfo fileToDecompress)
        {
            var newFileName = fileToDecompress.FullName + ".xml";

            using (var originalFileStream = fileToDecompress.OpenRead())
            {
                using (var decompressedFileStream = File.Create(newFileName))
                {
                    using (var decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
//                        Console.WriteLine($"Decompressed: {fileToDecompress.Name}");
                    }
                }
            }
            return newFileName;
        }

        /// <summary>
        ///     create an index out of a Dotted eyes AddressView Plus CSV file
        /// </summary>
        /// <param name="thread"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private void ImportSingleItnFile(int thread, string filename)
        {
            var args = new MessageArgs();
            var line = 0;

            args.Database = filename;
            args.Message = filename;
            Message?.Invoke(this, args);

            var srt = SubRecordType.None;
            var data = new CollectedData();

            using (var sr = new XmlTextReader(filename))
            {
                sr.WhitespaceHandling = WhitespaceHandling.None;

                while (sr.Read())
                {

                    try
                    {
                        line++;

                        if (line%1000 == 0)
                        {
                            args.Database = filename;
                            args.Message = $"Thread {thread} File {filename} Line {line}";
//                            Message?.Invoke(this, args);

//                            Debug.Print(args.message);
                        }

                        switch (sr.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (sr.Name)
                                {
                                    case "osgb:RoadLink":
                                        srt = SubRecordType.None;
                                        data = new CollectedData();

                                        if (sr.MoveToAttribute("fid"))
                                            data.Fid = sr.Value;

                                        break;
                                    case "osgb:RoadNode":
                                        srt = SubRecordType.None;
                                        data = new CollectedData();

                                        if (sr.MoveToAttribute("fid"))
                                            data.Fid = sr.Value;

                                        break;

                                    case "osgb:Road":
                                        srt = SubRecordType.None;
                                        data = new CollectedData();

                                        if (sr.MoveToAttribute("fid"))
                                            data.Fid = sr.Value;

                                        break;

                                    case "osgb:RoadRouteInformation":
                                        srt = SubRecordType.None;
                                        data = new CollectedData();

                                        if (sr.MoveToAttribute("fid"))
                                            data.Fid = sr.Value;

                                        break;

                                    case "osgb:RoadLinkInformation":
                                        srt = SubRecordType.None;
                                        data = new CollectedData();
                                        break;

                                    case "gml:coordinates":
                                        srt = SubRecordType.Coordinates;
                                        break;

                                    case "osgb:roadName":
                                        srt = SubRecordType.RoadName;
                                        break;

                                    case "osgb:networkMember":
                                        srt = SubRecordType.NetworkMembers;
                                        if (sr.MoveToAttribute("xlink:href"))
                                        {
                                            if (sr.Value.StartsWith("#"))
                                                data.NetworkMembers.Add(sr.Value.Substring(1));
                                            else
                                                data.NetworkMembers.Add(sr.Value);
                                        }
                                        break;

                                    case "osgb:directedNode":
                                        srt = SubRecordType.DirectedNode;
                                        var orientation = sr.GetAttribute("orientation");
                                        var xref = sr.GetAttribute("xlink:href");
                                        var grade = sr.GetAttribute("gradeSeparation");


                                        if (orientation == "-")
                                        {
                                            int.TryParse(grade ?? "0", out data.StartGrade);
                                            data.Starthref = xref;
                                            if (data.Starthref.StartsWith("#"))
                                                data.Starthref = data.Starthref.Substring(1);
                                        }
                                        else
                                        {
                                            data.Endhref = xref;
                                            int.TryParse(grade ?? "0", out data.EndGrade);
                                            if (data.Endhref.StartsWith("#"))
                                                data.Endhref = data.Endhref.Substring(1);
                                        }
                                        break;

                                    case "osgb:directedLink":
                                        srt = SubRecordType.DirectedLink;
                                        var orientation2 = "";
                                        for (var i = 0; i < sr.AttributeCount; i++)
                                        {
                                            sr.MoveToAttribute(i);
                                            switch (sr.Name)
                                            {
                                                case "orientation":
                                                    orientation2 = sr.Value;
                                                    break;
                                                case "xlink:href":
                                                    if (orientation2 == "-")
                                                    {
                                                        data.Starthref = sr.Value;
                                                        if (data.Starthref.StartsWith("#"))
                                                            data.Starthref = data.Starthref.Substring(1);
                                                    }
                                                    else
                                                    {
                                                        data.Endhref = sr.Value;
                                                        if (data.Endhref.StartsWith("#"))
                                                            data.Endhref = data.Endhref.Substring(1);
                                                    }
                                                    break;
                                            }
                                        }

                                        break;
                                    case "osgb:descriptiveTerm":
                                        srt = SubRecordType.DescriptiveTerm;
                                        break;

                                    case "osgb:natureOfRoad":
                                        srt = SubRecordType.NatureOfRoad;
                                        break;


                                    case "osgb:classification":
                                        srt = SubRecordType.Classification;
                                        break;

                                    case "osgb:instruction":
                                        srt = SubRecordType.Instruction;
                                        break;
                                }

                                break;
                            case XmlNodeType.EndElement:
                                switch (sr.Name)
                                {
                                    case "osgb:RoadLink":
                                        srt = SubRecordType.None;

                                        data.Polyline = data.Polyline.Trim().Replace(" ", "-");
                                        data.Polyline = data.Polyline.Replace(",", " ");
                                        data.Polyline = data.Polyline.Replace("-", ",");

                                        var wkt = "MULTILINESTRING ((" + data.Polyline + "))";

                                        //entities.AddRoadLink(data.Fid, data.DescriptiveTerm, data.NatureOfRoad,Wkt, data.Starthref,data.StartGrade, data.EndGrade, data.Endhref);

                                        string cmd3 = $"exec AddRoadLink @fid='{data.Fid}', @roadtype='{data.DescriptiveTerm}', @natureOfRoad='{data.NatureOfRoad}',  @WKT='{wkt}',  @FromFid='{data.Starthref}',  @FromGrade={data.StartGrade}, @ToFid='{data.Endhref}', @ToGrade={data.EndGrade} ";
                                        AddCommand(cmd3, data.Fid);
                                        //entities.Database.ExecuteSqlCommand(cmd3);


                                        break;
                                    case "osgb:RoadNode":
                                        srt = SubRecordType.None;

                                        var coords = data.Polyline.Split(',');
                                        if (coords.Length == 2)
                                        {
                                            double x, y;
                                            double.TryParse(coords[0], out x);
                                            double.TryParse(coords[1], out y);

                                            string cmd = $"exec AddRoadNode @fid='{data.Fid}', @X={(int) x}, @Y={(int) y};";
                                            AddCommand(cmd, data.Fid); 
                                            //entities.Database.ExecuteSqlCommand(cmd);
                                        }


                                        break;

                                    case "osgb:Road":
                                        srt = SubRecordType.None;

                                        //entities.AddRoad(data.Fid, data.Roadname);
                                        data.Roadname = data.Roadname.Replace(@"'", @"''");
                                        string cmd4 = $"exec AddRoad @fid='{data.Fid}', @roadname='{data.Roadname}';";
                                        AddCommand(cmd4, data.Fid);

                                        foreach (var networkfid in data.NetworkMembers)
                                        {
                                            // entities.AddRoadNetworkMember(data.Fid, networkfid);

                                            string cmd1 = $"exec AddRoadNetworkMember @roadfid='{data.Fid}', @networkfid='{networkfid}';";
                                            AddCommand(cmd1, data.Fid+networkfid);
                                            //entities.Database.ExecuteSqlCommandAsync(cmd1);

                                        }

                                        break;
                                    case "osgb:RoadLinkInformation":
                                        srt = SubRecordType.None;
                                        break;
                                    case "osgb:RoadRouteInformation":
                                        srt = SubRecordType.None;

                                        // entities.AddRoadRouteInfo(data.Fid, data.Instruction, data.Starthref, data.Endhref);

                                        string cmd2 =
                                            $"exec AddRoadRouteInfo @fid='{data.Fid}', @instruction='{data.Instruction}', @FromFid='{data.Starthref}', @ToFid='{data.Endhref}';";
                                        AddCommand(cmd2, data.Fid);
                                        //entities.Database.ExecuteSqlCommand(cmd2);

                                        break;
                                    case "gml:coordinates":
                                        srt = SubRecordType.None;
                                        break;
                                    case "osgb:directedNode":
                                        srt = SubRecordType.None;
                                        break;
                                    case "osgb:directedLink":
                                        srt = SubRecordType.None;
                                        break;
                                    case "osgb:descriptiveTerm":
                                        srt = SubRecordType.None;
                                        break;
                                    case "osgb:natureOfRoad":
                                        srt = SubRecordType.None;
                                        break;
                                    case "osgb:classification":
                                        srt = SubRecordType.None;
                                        break;
                                    case "osgb:instruction":
                                        srt = SubRecordType.None;
                                        break;
                                }
                                break;

                            case XmlNodeType.Text:

                                switch (srt)
                                {
                                    case SubRecordType.DescriptiveTerm:
                                        data.DescriptiveTerm = sr.Value;
                                        break;
                                    case SubRecordType.NatureOfRoad:
                                        data.NatureOfRoad = sr.Value;
                                        break;
                                    case SubRecordType.Coordinates:
                                        data.Polyline = sr.Value;
                                        break;
                                    case SubRecordType.Point:
                                        //data.Point = sr.Value;
                                        break;
                                    case SubRecordType.RoadName:
                                        data.Roadname = sr.Value;
                                        break;
                                    case SubRecordType.Classification:
                                        data.Classification = sr.Value;
                                        break;
                                    case SubRecordType.Instruction:
                                        data.Instruction = sr.Value;
                                        break;
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Write($"**** Bad file: {filename} **** \n{ex}", GetType().Name);
                    }
                }

            }

            Console.WriteLine($"**** file complete: {filename} ****");
        }

        public void LoadItnDirectory(string directory)
        {
            Clean();
            ProcessItnDirectory(directory);
            AddRoadIndexes();
            PostProcess();
            MakeStaticRoadinks();
            PopulateJunctions();
            // run ItnIndexer
            // run JunctionIndexer
        }

        private void ProcessItnDirectory(string directory)
        {
            var alltasks = ProcessItnDirectory(directory, 0);

            var concurrentBatches = 32;

            Task writer = new Task(WriterQueue);
            writer.Start();

            while (alltasks.Count > 0)
            {
                var subtasks = alltasks.Take(concurrentBatches).ToArray();
                subtasks.ForEach(x => { x.Start(); });

                // wait for task to compete
                Task.WaitAll(subtasks);

                // remove completed tasks from the list
                subtasks.ForEach(x => alltasks.Remove(x));

            }

            _stopWriter = true;
            writer.Wait();

        }

    /// <summary>
        ///     process a directory of .gz files - each /directory/ gets its own thread out of the thread pool
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="level"></param>
        private List<Task> ProcessItnDirectory(string directory, int level)
        {
            List < Task > myTasks = new List<Task>();
            foreach (var d in Directory.GetDirectories(directory))
            {
                myTasks.AddRange(ProcessItnDirectory(d, level++));
            }


#if ONETHREADPERDIRECTORY
            Workitems wis = new Workitems();
#endif

            var directorySelected = new DirectoryInfo(directory);

            foreach (var f in directorySelected.GetFiles("*.gz"))
            {
#if !ONETHREADPERDIRECTORY
                var wis = new Workitems();
#endif

                var wi = new Workitem();
                wi.File = f;

                Queuesize++;
                if (FileCountChanged != null)
                    FileCountChanged(this, null);

                wis.Add(wi);

#if !ONETHREADPERDIRECTORY
                Threadnumber++;
                wis.Thread = Threadnumber;
                ThreadPool.QueueUserWorkItem(Worker, wis);
#endif
            }

#if ONETHREADPERDIRECTORY
            Threadnumber++;
            wis.Thread = Threadnumber;

            myTasks.Add(new Task(Worker, wis));
            //ThreadPool.QueueUserWorkItem(Worker, wis);

            Debug.Print($"Queued thread {Threadnumber} with {wis.Count} files to process");
#endif

            return myTasks;

        }

        private void Clean()
        {
            using (var entities = new QuestEntities())
            {
                entities.CleanRoads();
            }
        }

        private void AddRoadIndexes()
        {
            using (var entities = new QuestEntities())
            {
                entities.Database.ExecuteSqlCommand("exec AddRoadIndexes");
            }
        }

        private void MakeStaticRoadinks()
        {
            using (var entities = new QuestEntities())
            {
                entities.Database.ExecuteSqlCommand("exec MakeStaticRoadinks");
            }
        }

        private void PopulateJunctions()
        {
            using (var entities = new QuestEntities())
            {
                entities.Database.ExecuteSqlCommand("exec PopulateJunctions");
            }
        }

        private void PostProcess()
        {
            using (var entities = new QuestOSEntities())
            {
                entities.PostprocessITNLoad();
            }
        }

        private enum SubRecordType
        {
            None,
            DirectedNode,
            DirectedLink,
            Coordinates,
            Point,
            DescriptiveTerm,
            RoadName,
            NetworkMembers,
            Classification,
            NatureOfRoad,
            Instruction
        }

        public class MessageArgs : EventArgs
        {
            public string Database;
            public string Message;
        }

        private class Workitems : List<Workitem>
        {
            public int Thread;
        }

        private class Workitem
        {
            public FileInfo File;
        }


        private class CollectedData
        {
            public string Classification = "";
            public string DescriptiveTerm = "";
            public string Fid = "";
            public string Instruction = "";
            public string NatureOfRoad = "";
            public readonly List<string> NetworkMembers = new List<string>();
            public string Polyline = "";
            public string Roadname = "";
            public string Starthref = "";
            public string Endhref = "";
            public int StartGrade;
            public int EndGrade;
        }
    }
}