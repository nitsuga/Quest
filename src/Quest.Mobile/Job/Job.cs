using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Quest.Mobile.Job
{
    [Serializable]
    public class Job<W> where W : WorkItem
    {
        public List<W> items=new List<W>();
        public int jobid;
        public bool cancelflag;
        public bool complete;

        public void Run(Action<W> action) 
        {
            new TaskFactory().StartNew(() =>
            {
                foreach (var item in items)
                {
                    item.Execute(this, action);
                }
                complete = true;
                Debug.Print("job complete");
            });
        }
    }
}