using System;
using Quest.Lib.Simulation.Interfaces;
using Quest.Lib.DataModel;

#pragma warning disable 0169
#pragma warning disable 0649

namespace Quest.Lib.Simulation
{
    [Export]
    public class PerformanceRecorder : SimPart
    {
        /// <summary>
        ///  used to detect a change in hour in order to record orcon
        /// </summary>
        private DateTime _StartTime;

        private int _CatAIncidentsLimit;
        private int _CatBIncidentsLimit;

        // report every n minutes
        private const int REPORTEVERYMINS = 60;

        [Import]
        private ICadIn _Cad;

        [Import]
        Parameters Parameters;

        [Import]
        public SimEngine _simEngine;

        [Import]
        private TimedEventQueue EventQueue;
        
        public PerformanceRecorder()
        {
        }

        public override void Start()
        {
            _StartTime = EventQueue.Now;

            // add trigger to calculate running performance
            TaskKey key1 = new TaskKey("PERFORMANCE", "PERFORMANCE");
            new TaskEntry(EventQueue, key1, UpdatePerformance, null, _StartTime.AddHours(1));

        }

        public override void Stop()
        {
        }

        public override void Initialise(CompositionContainer container)
        {
            Logger.Write("PERF: Performance recorder initialising");

            PatchUpImports(container);

            if (_Cad != null)
            {
                _Cad.CloseIncidentEvent += new EventHandler<Incident>(_Cad_CloseIncidentEvent);
                _Cad.NewIncidentEvent += new EventHandler<Incident>(_Cad_NewIncidentEvent);
            }
            IsInitialised = true;
        }

        public override void Prepare()
        {
            _CatAIncidentsLimit = Parameters.GetFirstParameter("PERF.CatALimit", 8 * 60);
            _CatBIncidentsLimit = Parameters.GetFirstParameter("PERF.CatBLimit", 19 * 60);
        }

        private void UpdatePerformance(TaskEntry te)
        {
            CalculatePerformance();

            // add trigger to calculate running performance
            TaskKey key1 = new TaskKey("PERFORMANCE", "PERFORMANCE");
            new TaskEntry(EventQueue, key1, UpdatePerformance, null, 3600);
        }

        public float CalculatePerformance()
        {
            using (SimulatorEntities context = new SimulatorEntities())
            {
                context.UpdateOrcon(_simEngine.RunRecord.SimulationRunId);
                float? performance = (from v in context.SimulationRuns where v.SimulationRunId == _simEngine.RunRecord.SimulationRunId select v.Performance).FirstOrDefault();
                if (performance != null)
                    return (float)performance;
                else
                    return 0;
            }
        }

        void _Cad_NewIncidentEvent(object sender, Incident e)
        {
            CreateIncResultRecord(e);
        }

        void _Cad_CloseIncidentEvent(object sender, Incident e)
        {
            SaveIncidentStatistics(e);
        }

        /// <summary>
        /// record statistics every n minutes to the SimulatotionOrcon table
        /// </summary>
        /// <param name="inc"></param>
        private void RecordPerformance()
        {

            using (QuestEntities context = new QuestEntities())
            {
                var incs = from i in context.SimulationResults select i;

                foreach( var i in incs)
                {
                    // calulate the report id of this incident 
                    double TotalSimulationMinutes = EventQueue.Now.Subtract(_StartTime).TotalMinutes;

                }
            }

#if false
            // calculate the number of reports that should have been made by now made
            int TotalReports = (int)(TotalSimulationMinutes / REPORTEVERYMINS);

            // should we have recorded a new report?
            if (TotalReports > _LastReport)
            {
                // record the orcon data
                SimulationOrcon result = new SimulationOrcon() { CatA = 1, CatB = 1, CatACount = 0, CatBCount = 0 };
                if (_TotalCatAIncidentsLastHour != 0)
                    result.CatA = (float)_TotalCatAIncidentsLastHourIn / (float)_TotalCatAIncidentsLastHour;
                else
                    result.CatA = 0;
                result.CatACount = _TotalCatAIncidentsLastHour;

                if (_TotalCatBIncidentsLastHour != 0)
                    result.CatB = (float)_TotalCatBIncidentsLastHourIn / (float)_TotalCatBIncidentsLastHour;
                else
                    result.CatB = 0;
                result.CatBCount = _TotalCatBIncidentsLastHour;

                result.CatATravel = _TotalCatARunTime;
                result.CatBTravel = _TotalCatARunTime;

                result.SimulationRunId = _simEngine.RunRecord.SimulationRunId;
                result.CallStart = _StartTime + new TimeSpan(0, TotalReports * REPORTEVERYMINS, 0);

                using (SimulatorEntities context = new SimulatorEntities(_simEngine.SimulatorConnectionString))
                {
                    context.SimulationOrcons.InsertOnSubmit(result);
                    context.SubmitChanges();
                }

                // calculate the running performance
                using (SimulatorEntities context = new SimulatorEntities(_simEngine.SimulatorConnectionString))
                {
                    double catA = (from v in context.SimulationOrcons where v.SimulationRunId == _simEngine.RunRecord.SimulationRunId select v.CatA).Average();
                    double catB = (from v in context.SimulationOrcons where v.SimulationRunId == _simEngine.RunRecord.SimulationRunId select v.CatB).Average();

                    //TODO: Check this formula
                    _simEngine.FinalPerformance = ((catA * 5) + catB) / 6;

                    context.SubmitChanges();
                }

                // reset the orcon data
                _TotalCatAIncidentsLastHour = 0;
                _TotalCatBIncidentsLastHour = 0;
                _TotalCatAIncidentsLastHourIn = 0;
                _TotalCatBIncidentsLastHourIn = 0;
                _TotalCatARunTime = 0;
                _TotalCatBRunTime = 0;

                _LastReport = TotalReports;
            }

            UpdateCatTotals(inc);
#endif
        }

        /// <summary>
        /// update the incident details in the database
        /// </summary>
        /// <param name="inc"></param>
        private void SaveIncidentStatistics(Incident inc)
        {
            try
            {

                using (QuestEntities context = new QuestEntities())
                {
                    SimulationResult finalresult = (from i in context.SimulationResults where i.Incidentid == inc.IncidentId && i.SimulationRunId == _simEngine.RunRecord.SimulationRunId select i).FirstOrDefault();

                    finalresult.FRDelay = inc.FirstResponderArrivalDelay;
                    finalresult.FRResourceId = inc.FirstResponderArrivalId;
                    finalresult.AmbDelay = inc.AmbulanceArrivalDelay;
                    finalresult.AmbResourceId = inc.AmbulanceArrivalId;

                    if (finalresult.FRDelay == 0 || finalresult.FRDelay == null)
                        finalresult.FRDelay = int.MaxValue;

                    if (finalresult.AmbResourceId == 0)
                        finalresult.AmbResourceId = null;

                    if (finalresult.FRResourceId == 0)
                        finalresult.FRResourceId = null;

                    finalresult.TurnAround = inc.TurnaroundTime;
                    finalresult.OnScene = inc.OnSceneDelay;
                    if (inc.AtHospitalTime != null)
                        finalresult.HospitalDelay = (int)((inc.AtHospitalTime - inc.LeftHospitalTime).TotalSeconds);
                    else
                        finalresult.HospitalDelay = 0;

                    finalresult.Closed = EventQueue.Now;

                    if (inc.Category == 1)
                    {
                        finalresult.Inside = (int)(finalresult.FRDelay) < _CatAIncidentsLimit;
                        finalresult.Category = inc.Category;
                    }
                    else
                    {
                        finalresult.Inside = (int)(finalresult.FRDelay) < _CatBIncidentsLimit;
                        finalresult.Category = 3;
                    }

                    context.SaveChanges();
                }
            }
            catch 
            { }

        }

        /// <summary>
        /// record the incident details in the database
        /// </summary>
        /// <param name="inc"></param>
        private void CreateIncResultRecord(Incident inc)
        {
            SimulationResult result = new SimulationResult();
            result.Incidentid = inc.IncidentId;
            result.FRDelay = 0;
            result.FRResourceId = inc.FirstResponderArrivalId;
            result.AmbDelay = 0;

            result.AmbResourceId = null;
            result.FRResourceId = null;

            result.Incidentid = inc.IncidentId;
            result.TurnAround = 0;
            result.OnScene = 0;
            result.HospitalDelay = 0;

            result.SimulationRunId = _simEngine.RunRecord.SimulationRunId;
            result.CallStart = EventQueue.Now;
            result.Closed = null;
            result.HourOrdinal = (int)((inc.CallStart-_StartTime).TotalHours);

            using (SimulatorEntities context = new SimulatorEntities())
            {
                context.SimulationResults.Add(result);
                context.SaveChanges();
            }
        }
    }
}
