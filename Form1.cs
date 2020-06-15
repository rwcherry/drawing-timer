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
    public class TimeEntry
    {
        public ulong milliseconds = 0;
        public DateTime date;

        public override string ToString()
        {
            return string.Format("{0},{1}", this.milliseconds, this.date.ToShortDateString());
        }
    }

    public partial class Form1 : Form
    {
        bool started = false;
        ulong numMillisecondsTotal = 0; // Starting value
        List<TimeEntry> m_entries;
        TimeEntry thisSession;
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

        private void UpdateTimer()
        {
            TimeSpan t = TimeSpan.FromMilliseconds(numMillisecondsTotal);
            string answer = string.Format("{0:D2}hrs {1:D2}mins {2:D2}secs",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds);

            this.richTextBox1.Text = answer;

            foreach (var goal in goals)
            {
                var millisecondsGoal = goal * 60 * 60 * 1000;
                if (numMillisecondsTotal <= millisecondsGoal)
                {
                    double ratio = (double)numMillisecondsTotal / (double)millisecondsGoal;
                    int whatever = (int)(ratio * 100);
                    this.progressBar1.Value = whatever;
                    this.label1.Text = string.Format("Progress Towards {0} Hours", goal);
                    break;
                }
            }
        }

        private void tickTimer_Tick(object sender, EventArgs e)
        {
            numMillisecondsTotal += 100;
            thisSession.milliseconds += 100;
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
                    entry.milliseconds = (ulong)Int64.Parse(date_split[0]);
                    entry.date = Convert.ToDateTime(date_split[1]);
                    numMillisecondsTotal += entry.milliseconds;
                    m_entries.Add(entry);
                }
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
}
