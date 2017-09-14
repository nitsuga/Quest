using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Quest.WebCore.Services
{
    public class WorkItem
    {
        public string status;
        public bool complete;

        /// <summary>
        /// call the action for each work item and set the job status when done
        /// </summary>
        /// <typeparam name="J"></typeparam>
        /// <typeparam name="W"></typeparam>
        /// <param name="j"></param>
        /// <param name="action"></param>
        public void Execute<J,W>(J j, Action<W> action) where J : Job<W> where W : WorkItem
        {
            try
            {

                if (j.cancelflag)
                    return;

                var watch = new Stopwatch();
                watch.Start();

                action((W)this);

                watch.Stop();

                status = watch.ElapsedMilliseconds + "ms";
                complete = true;
            }
            catch (Exception ex)
            {
                status = ex.Message;
                complete = true;
            }

        }
    }

    public class JobService<T,W> where T : Job<W> where W : WorkItem
    {
        public static int _jobid;
        private static Dictionary<int, T> _jobs = new Dictionary<int, T>();

        public T GetJob(int jobid)
        {
            if (_jobs.ContainsKey(jobid))
                return _jobs[jobid];
            else
                return null;
        }

        public void DeleteJob(int jobid)
        {
            if (_jobs.ContainsKey(jobid))
                _jobs.Remove(jobid);
        }

        public void CancelJob(int jobid)
        {
            if (_jobs.ContainsKey(jobid))
                _jobs[jobid].cancelflag = true;
        }

        public T AddJob(T newjob)
        {
            newjob.jobid = ++_jobid;
            newjob.cancelflag = false;

            _jobs.Add(newjob.jobid, newjob);

            return newjob;
        }
    }
}