using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Quest.Lib.Trace;

namespace Quest.Lib.Search.Elastic
{
    /// <summary>
    /// Splits indexing into chunks and processes each chunk in a separate Task
    /// </summary>
    public static class BatchIndexer
    {

        public class BatchWork
        {
            public long Batch;
            public long StartIndex;
            public long StopIndex;
        }

        public static void ProcessBatches(ElasticIndexer indexer, BuildIndexSettings config, long startRecord, long stopRecord, long batchSize, int concurrentBatches, Action<BuildIndexSettings, BatchWork> batchWorker)
        {
            // create batches of work
            List<BatchWork> batches = new List<BatchWork>();
            for (long i = startRecord, batch = 0; i < stopRecord; i += batchSize, batch++)
                batches.Add(new BatchWork()
                {
                    Batch = batch,
                    StartIndex = i,
                    StopIndex = i + batchSize - 1,
                });

            // create set of tasks to process each batch
            List<Task> alltasks = batches.Select(x => new Task(() =>
            {
                try
                {
                    batchWorker(config, x);
                }
                catch (Exception ex)
                {
                    Logger.Write($"{indexer.GetType().Name}: batch failed {ex}", "BatchIndexer");
                }

            })).ToList();

            Logger.Write($"{indexer.GetType().Name}: Starting {batches.Count} batches","BatchIndexer");

            while (alltasks.Count > 0)
            {
                var subtasks = alltasks.Take(concurrentBatches).ToArray();
                subtasks.ForEach(x => { x.Start(); });

                // wait for task to compete
                Task.WaitAll(subtasks);

                // remove completed tasks from the list
                subtasks.ForEach(x => alltasks.Remove(x));

                //update progress stats
                ElasticIndexer.OutputProgressLogMessage(indexer, config);
            }

        }
    }
}
