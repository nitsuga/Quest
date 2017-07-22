using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Quest.Lib.Trace;

namespace Quest.Lib.Utils
{
    /// <summary>
    ///     this class maintains a list of timed tasks
    /// </summary>
    public class TimedEventQueue : IDisposable
    {
        private DateTime _actualStartTime = DateTime.MinValue;

        /// <summary>
        ///     keeps a sorted list of the events to fire
        /// </summary>
        private readonly SortedList<DateTime, TaskEntry> _internalList;

        /// <summary>
        ///     the current time of the task queue
        /// </summary>
        private DateTime _now;

        private DateTime _simStartTime = DateTime.MinValue;

        /// <summary>
        ///     flag that causes main loop to exit
        /// </summary>
        private bool _stopFlag;

        /// <summary>
        ///     the worker thread of the main loop
        /// </summary>
        private Thread _worker;

        /// <summary>
        ///     construct a new task queue
        /// </summary>
        public TimedEventQueue()
        {
            _internalList = new SortedList<DateTime, TaskEntry>();

            Speed = 1;
        }

        /// <summary>
        ///     The current time in the EISECSimulator
        /// </summary>
        public DateTime Now
        {
            get { return _now; }
            set
            {
                _now = value;
                Logger.Write("TQ: now: " + _now);

                if (_simStartTime == DateTime.MinValue)
                    _simStartTime = _now;

                TimeChanged?.Invoke(this, new TimeChangedEvent { Value = Now });
            }
        }

        /// <summary>
        ///     set the speed of the simulation - set to 1 for normal, 2 for x2 0.5 for half speed etc
        /// </summary>
        public double Speed { get; set; }

        public bool IsRunning { get; private set; }

        /// <summary>
        ///     fired whenever the simulation time changes
        /// </summary>
        public event EventHandler<TimeChangedEvent> TimeChanged;

        public event EventHandler<ExceptionArgs> Error;

        /// <summary>
        ///     starts the task queue firing
        /// </summary>
        public void Start()
        {
            Stop(); // stop if already running

            _stopFlag = false;
            _worker = new Thread(TaskWorker)
            {
                Name = "Task worker",
                IsBackground = true
            };
            _worker.Start();
            IsRunning = true;
        }

        /// <summary>
        ///     stop the task system firing events
        /// </summary>
        public void Stop()
        {
            if (_worker == null)
                return;

            _stopFlag = true; // signal loop to finish
            _worker.Join(); // wait for thread to finish
            _worker = null;
            IsRunning = false;
        }

        /// <summary>
        ///     Clear out the task queue
        /// </summary>
        public void Clear()
        {
            lock (_internalList)
            {
                _internalList.Clear();
            }
        }

        private TaskEntry Find(TaskKey key)
        {
            Debug.Assert(_internalList != null, "_internalList is null");

            lock (_internalList)
            {
                foreach (var k in _internalList.Values)
                    if (k?.Key != null && k.Key.Key == key.Key)
                        return k;
            }
            return null;
        }


        public void Remove(TaskKey key)
        {
            // remove any entry with the same key and a later firing time.
            TaskEntry k;
            do
            {
                lock (_internalList)
                {
                    k = Find(key);
                    if (k != null)
                    {
                        Logger.Write("TQ: remove: " + k, GetType().Name);
                        _internalList.Remove(k.TimeToFire);
                    }
                }
            } while (k != null);
        }

        /// <summary>
        ///     add a timed task to the list of things to do
        /// </summary>
        /// <param name="te"></param>
        public void Add(TaskEntry te)
        {
            lock (_internalList)
            {
                // remove any entry with the same key and a later firing time.
                Remove(te.Key);

                // ensure the time is unique      
                while (_internalList.Keys.Contains(te.TimeToFire))
                    te.TimeToFire = te.TimeToFire.AddMilliseconds(1);

                Logger.Write("Add: " + te, GetType().Name);

                // put the request into the event queue in order of time
                _internalList.Add(te.TimeToFire, te);
            }
        }

        /// <summary>
        ///     worker thread that processes the queue
        /// </summary>
        private void TaskWorker()
        {
            _actualStartTime = DateTime.UtcNow;

            while (!_stopFlag)
            {
                try
                {
                    if (Math.Abs(Speed) < 0.01)
                    {
                        if (_internalList.Count == 0)
                            Thread.Sleep(1000);
                        else
                        {
                            lock (_internalList)
                            {
                                if (Now < _internalList.First().Value.TimeToFire)
                                    Now = _internalList.First().Value.TimeToFire;
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                        var secondsSinceStart = DateTime.UtcNow.Subtract(_actualStartTime).TotalSeconds;
                        Now = _simStartTime.AddMilliseconds(secondsSinceStart * Speed * 1000);
                    }

                    // fire any events that need to be extracted
                    FireOldEvents();
                }
                catch (Exception e)
                {
                    Error?.Invoke(this, new ExceptionArgs { Exception = e });
                }
            }
        }

        /// <summary>
        ///     fire any events in the queue that need to be
        /// </summary>
        private void FireOldEvents()
        {
            var fireList = new SortedList<DateTime, TaskEntry>();
            // step through events (in ascending data order) and add those items that need to be
            // fired into a separate list. This is to prevent deadlocks
            // get exclusive access the the list
            lock (_internalList)
            {
                while (_internalList.Keys.Count > 0)
                {
                    // get the first event that needs firing

                    // this event needs to be fired - copy into a firing array
                    var te = _internalList.Values[0];

                    if (te.TimeToFire > _now)
                        break;

                    Logger.Write("TQ: Fire: " + te, GetType().Name);

                    _internalList.RemoveAt(0);

                    // add to firing array 
                    fireList.Add(te.TimeToFire, te);
                }
            }

            // fire any items OUTSIDE of the lock
            foreach (var te in fireList.Values)
                fireDelegate(te);
        }

        /// <summary>
        ///     called by the worker thread to fire the TaskEntry
        /// </summary>
        /// <param name="o"></param>
        private void fireDelegate(object o)
        {
            var te = (TaskEntry)o;
            te.TimerFunction(te);
        }

        #region IDisposable Members

        /// <summary>
        ///     dispose the resources used by the task.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///     dispose the resources used by the task.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // being explicitly closed
            if (disposing)
            {
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     destructor
        /// </summary>
        ~TimedEventQueue()
        {
            Dispose(false);
        }

        #endregion
    }

    /// <summary>
    ///     the delegate to use for calling back
    /// </summary>
    /// <param name="te"></param>
    public delegate void TaskCallback(TaskEntry te);

    public class TimeChangedEvent : EventArgs
    {
        public DateTime Value;
    }

    public class ExceptionArgs : EventArgs
    {
        public Exception Exception;
    }

    public class TimedAction<T>
    {
        /// <summary>
        ///     defines hows we access this task
        /// </summary>
        public TaskKey Key { get; set; }

        public Action<T> TimeAction;

        /// <summary>
        ///     user-defined data
        /// </summary>
        public object DataTag { get; set; }

        /// <summary>
        ///     when to fire the task
        /// </summary>
        public DateTime TimeToFire { get; set; }
    }

    /// <summary>
    ///     represents a single timed task
    /// </summary>
    public class TaskEntry
    {
        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="key"></param>
        /// <param name="tc"></param>
        /// <param name="tag"></param>
        /// <param name="ts"></param>
        public TaskEntry(TimedEventQueue queue, TaskKey key, TaskCallback tc, object tag, TimeSpan ts)
        {
            Key = key;
            TimerFunction = tc;
            DataTag = tag;
            TimeToFire = queue.Now.Add(ts);
            queue.Add(this);
        }


        public TaskEntry(TimedEventQueue queue, TaskKey key, TaskCallback tc, object tag, int seconds)
        {
            Key = key;
            TimerFunction = tc;
            DataTag = tag;
            TimeToFire = queue.Now.AddSeconds(seconds);
            queue.Add(this);
        }

        public TaskEntry(TimedEventQueue queue, TaskKey key, TaskCallback tc, object tag, DateTime date)
        {
            Key = key;
            TimerFunction = tc;
            DataTag = tag;
            TimeToFire = date;
            queue.Add(this);
        }

        public TaskEntry(TaskKey key, TaskCallback tc, object tag, DateTime date)
        {
            Key = key;
            TimerFunction = tc;
            DataTag = tag;
            TimeToFire = date;
        }

        /// <summary>
        ///     defines hows we access this task
        /// </summary>
        public TaskKey Key { get; set; }

        /// <summary>
        ///     what functions gets executed when the task fires
        /// </summary>
        public TaskCallback TimerFunction { get; set; }

        /// <summary>
        ///     user-defined data
        /// </summary>
        public object DataTag { get; set; }

        /// <summary>
        ///     when to fire the task
        /// </summary>
        public DateTime TimeToFire { get; set; }

        /// <summary>
        ///     description of this task if known
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Key + (DataTag ?? "").ToString() + " @ " + TimeToFire.Date.ToShortDateString() + " " +
                   TimeToFire.TimeOfDay + "->" + TimerFunction.Method.Name;
        }
    }

    /// <summary>
    ///     class that defines how to identify a task
    /// </summary>
    public class TaskKey
    {
        /// <summary>
        ///     supplimental data
        /// </summary>
        private string _data1;

        /// <summary>
        ///     data that can travel with the task key
        /// </summary>
        private string _data2;

        /// <summary>
        ///     the main key of the task
        /// </summary>
        private string _key;

        public TaskKey(int key)
            : this(key.ToString(), null, null)
        {
        }

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="key"></param>
        public TaskKey(string key)
            : this(key, null, null)
        {
        }

        public TaskKey(int id, string data1)
            : this(id.ToString(), data1)
        {
        }

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data1"></param>
        public TaskKey(string key, string data1)
            : this(key, data1, null)
        {
        }

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        public TaskKey(string key, string data1, string data2)
        {
            _key = key;
            _data1 = data1;
            _data2 = data2;
        }

        /// <summary>
        ///     Key used to uniquely identify this task
        /// </summary>
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        /// <summary>
        ///     data that can travel with the task key
        /// </summary>
        public string Data1
        {
            get { return _data1; }
            set { _data1 = value; }
        }

        /// <summary>
        ///     data that can travel with the task key
        /// </summary>
        public string Data2
        {
            get { return _data2; }
            set { _data2 = value; }
        }

        /// <summary>
        ///     description of this key
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _key + ":" + _data1 + ":" + _data2;
        }
    }
}