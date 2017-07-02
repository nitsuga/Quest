using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Quest.Mobile.Job
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
        public void Execute<J, W>(J j, Action<W> action) where J : Job<W> where W : WorkItem
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

}