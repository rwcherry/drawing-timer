using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DrawingTimer
{
    public partial class Form1 : Form
    {
        bool started = false;
        ulong numSecondsTotal = 0; // Starting value

        

        Dictionary<string, ulong> dateToTime;
        Dictionary<int, DateRangeInfo> dateRangeInfo;

        List<TimeEntry> m_entries;
        TimeEntry thisSession;

        DateTime startDate;

        ulong[] goals = {
            10,
            50,
            100,
            150,
            250,
            500,
            750,
            1000
        };

        public Form1()
        {
            InitializeComponent();
            m_entries = new List<TimeEntry>();
            thisSession = new TimeEntry();
            thisSession.date = DateTime.Today;

            // First day I started timing my drawing
            startDate = new DateTime(2020, 6, 7);
            dateToTime = new Dictionary<string, ulong>();
            dateRangeInfo = new Dictionary<int, DateRangeInfo>();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!started)
            {
                this.button1.Text = "Pause";
                this.tickTimer.Enabled = true;
            }
            else
            {
                this.button1.Text = "Start";
                this.tickTimer.Enabled = false;
            }

            started = !started;
            
        }

        /// https://stackoverflow.com/questions/1847580/how-do-i-loop-through-a-date-range
        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        private string UpdateRolling(short range)
        {
            var daysAgo = DateTime.Today.AddDays(-range);
            DateRangeInfo info = new DateRangeInfo();
            info.range = range;

            foreach (DateTime day in EachDay(daysAgo, DateTime.Today))
            {
                var key = day.ToShortDateString();
                if (dateToTime.ContainsKey(key))
                {
                    info.rangeAvg += dateToTime[key];
                }
                else
                {
                    info.rangeMissed++;
                }
            }

            info.rangeAvg = info.rangeAvg / info.range;
            dateRangeInfo[range] = info;

            return string.Format(
                "{0} day average: {1}\n" +
                "Days missed in last {0} days: {2}\n", info.range, info.rangeAvg / 60, info.rangeMissed);
        }

        private void UpdateStats()
        {
            double daysSince = (DateTime.Today.Subtract(startDate).TotalDays);
            double avgSecondsPerDay = numSecondsTotal / daysSince;
            double timeRemaining = ((1000 * 60 * 60) - numSecondsTotal);
            double avgDaysRemaining = timeRemaining / avgSecondsPerDay;

            this.labelStats.Text = string.Format(
                "Days since start: {0}\n" +
                "Avg minutes per day: {1:F2}\n" +
                "Avg days remaining: {2:F2}\n", daysSince, avgSecondsPerDay / 60, avgDaysRemaining);
        }

        private void UpdateTimer()
        {
            TimeSpan t = TimeSpan.FromSeconds(numSecondsTotal);
            string answer = string.Format("{0:D2}hrs {1:D2}mins {2:D2}secs",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds);

            this.richTextBox1.Text = answer;

            t = TimeSpan.FromSeconds(thisSession.seconds);
            answer = string.Format("{0:D2}hrs {1:D2}mins {2:D2}secs",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds);
            this.richTextBox2.Text = answer;

            foreach (var goal in goals)
            {
                var goalInSeconds = goal * 60 * 60;
                if (numSecondsTotal <= goalInSeconds)
                {
                    double ratio = (double)numSecondsTotal / (double)goalInSeconds;
                    int whatever = (int)(ratio * 100);
                    this.progressBar1.Value = whatever;
                    this.label4.Text = string.Format("{0}%", whatever);
                    this.label1.Text = string.Format("Progress Towards {0} Hours", goal);
                    break;
                }
            }

            UpdateStats();
        }

        private void tickTimer_Tick(object sender, EventArgs e)
        {
            ++numSecondsTotal;
            ++thisSession.seconds;
            UpdateTimer();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            if (File.Exists("time.data"))
            {
                var fileText = File.ReadAllText("time.data");
                var lines = fileText.Split('\n');
                foreach (string line in lines)
                {
                    var date_split = line.Split(',');
                    if (date_split.Length < 2)
                        continue;
                    TimeEntry entry = new TimeEntry();
                    entry.seconds = (ulong)Int64.Parse(date_split[0]);
                    entry.date = Convert.ToDateTime(date_split[1]);

                    var dateString = entry.date.ToShortDateString();
                    if (dateToTime.ContainsKey(dateString))
                    {
                        dateToTime[dateString] = dateToTime[dateString] + entry.seconds;
                    }
                    else
                    {
                        dateToTime[dateString] = entry.seconds;
                    }

                    numSecondsTotal += entry.seconds;
                    m_entries.Add(entry);
                }
                string sevenDayStats = UpdateRolling(7);
                string thirtyDayStats = UpdateRolling(30);
                this.labelStats2.Text = sevenDayStats + thirtyDayStats;
                UpdateTimer();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.tickTimer.Enabled = false;
            m_entries.Add(thisSession);
            List<string> convertList = new List<string>();
            foreach (var entry in m_entries)
            {
                convertList.Add(entry.ToString());
            }
            File.WriteAllLines("time.data", convertList.ToArray());
        }
    }

    public class TimeEntry
    {
        public ulong seconds = 0;
        public DateTime date;

        public override string ToString()
        {
            return string.Format("{0},{1}", this.seconds, this.date.ToShortDateString());
        }
    }

    public class DateRangeInfo
    {
        public short range = 0;
        public double rangeAvg = 0;
        public double rangeMissed = 0;
    }
}
