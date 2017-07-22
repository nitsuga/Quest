using Quest.Mobile.Job;
using System.Collections.Generic;

namespace Quest.Mobile.Service
{
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