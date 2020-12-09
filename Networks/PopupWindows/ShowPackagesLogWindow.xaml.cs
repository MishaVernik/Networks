using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Networks.PopupWindows
{
    public class PackageLog
    {
        public PackageLog()
        {

        }
        public PackageLog(string from, int id, long ticks, int size, bool t)
        {
            this.from = from;
            this.id = id;
            this.ticks = ticks;
            this.size = size;
            isFinished = t;
        }

        public String from { get; set; }
        public int id { get; set; }
        public long ticks { get; set; }
        public int size { get; set; }
        public int totalCost { get; set; }
        public bool isFinished { get; set; }
        public String type { get; set; }
        public String rType { get; set; }
        public int totalPackages { get; set; }

    }
    /// <summary>
    /// Interaction logic for ShowPackagesLogWindow.xaml
    /// </summary>
    public partial class ShowPackagesLogWindow : Window
    {
        List<List<AnimateMessage>> animateMessages;
        public void ShowLog()
        {
            this.dataGridPackageLogs.Items.Clear();
            this.dataGridPackageLogs.Columns.Clear();
            DataGridTextColumn c1 = new DataGridTextColumn();
            c1.Header = "ID";
            c1.Binding = new Binding("id");
            c1.Width = 110;
            this.dataGridPackageLogs.Columns.Add(c1);
            DataGridTextColumn c7 = new DataGridTextColumn();
            c7.Header = "Type";
            c7.Width = 110;
            c7.Binding = new Binding("type");
            this.dataGridPackageLogs.Columns.Add(c7);
            DataGridTextColumn c2 = new DataGridTextColumn();
            c2.Header = "Connected";
            c2.Width = 110;
            c2.Binding = new Binding("from");
            this.dataGridPackageLogs.Columns.Add(c2);

            DataGridTextColumn c12 = new DataGridTextColumn();
            c12.Header = "Routing type";
            c12.Width = 110;
            c12.Binding = new Binding("rType");
            this.dataGridPackageLogs.Columns.Add(c12);

            DataGridTextColumn c112 = new DataGridTextColumn();
            c112.Header = "Total packages";
            c112.Width = 110;
            c112.Binding = new Binding("totalPackages");
            this.dataGridPackageLogs.Columns.Add(c112);

            DataGridTextColumn c3 = new DataGridTextColumn();
            c3.Header = "Time Spent";
            c3.Width = 110;
            c3.Binding = new Binding("ticks");
            this.dataGridPackageLogs.Columns.Add(c3);
            DataGridTextColumn c4 = new DataGridTextColumn();
            c4.Header = "Traffic";
            c4.Width = 110;
            c4.Binding = new Binding("size");
            this.dataGridPackageLogs.Columns.Add(c4);
            DataGridTextColumn c5 = new DataGridTextColumn();
            c5.Header = "Total Cost";
            c5.Width = 110;
            c5.Binding = new Binding("totalCost");
            this.dataGridPackageLogs.Columns.Add(c5);
            DataGridTextColumn c6 = new DataGridTextColumn();
            c6.Header = "Finished Delivery";
            c6.Width = 110;
            c6.Binding = new Binding("isFinished");
            this.dataGridPackageLogs.Columns.Add(c6);

            int id = 0;
            foreach (var messages in animateMessages)
            {
                PackageLog packageLogSystem = new PackageLog();
                PackageLog packageLogInfo = new PackageLog();
                id++;
                packageLogInfo.id = id;
                packageLogSystem.id = id;
                bool isFinishedInfo = true;
                bool isFinishedSystem = true;

                bool isInfo = false;
                bool isSystem = false;
                int cntInf = 0;
                int cntSys = 0;
                foreach (var message in messages)
                {
                    switch (message.packageType)
                    {
                        case Data.PackageType.System:
                            packageLogSystem.ticks += message.ticksMade;
                            packageLogSystem.size += message.size;
                            if (cntSys == 0)
                            {
                                cntSys++;
                                packageLogSystem.from = message.sourceID + " -> " + message.targetID;
                            }
                            packageLogSystem.totalCost += message.totalCost;
                            isFinishedSystem &= message.isFinished;
                            packageLogSystem.rType = message.messageType.ToString();

                            isSystem = true;
                            break;
                        case Data.PackageType.Info:
                            packageLogInfo.ticks += message.ticksMade;
                            packageLogInfo.size += message.size;
                            if (cntInf == 0)
                            {
                                cntInf++;
                                packageLogInfo.from = message.sourceID + " -> " + message.targetID;
                            }
                            packageLogInfo.from = message.sourceID + " -> " + message.targetID;
                            packageLogInfo.totalCost += message.totalCost;
                            packageLogInfo.rType = message.messageType.ToString();
                            isFinishedInfo &= message.isFinished;
                            isInfo = true;
                            break;
                        default:
                            break;
                    }
                }
                if (isInfo)
                {
                    this.dataGridPackageLogs.Items.Add(new PackageLog()
                    {
                        rType = packageLogInfo.rType,
                        type = "Info",
                        ticks = packageLogInfo.ticks,
                        isFinished = isFinishedInfo,
                        from = packageLogInfo.from,
                        id = packageLogInfo.id,
                        size = packageLogInfo.size,
                        totalCost = packageLogInfo.totalCost,
                        totalPackages = packageLogInfo.size / 128
                    });
                }
                if (isSystem)
                {
                    this.dataGridPackageLogs.Items.Add(new PackageLog()
                    {
                        rType = packageLogSystem.rType,
                        type = "System",
                        ticks = packageLogSystem.ticks,
                        isFinished = isFinishedSystem,
                        from = packageLogSystem.from,
                        id = packageLogSystem.id,
                        size = packageLogSystem.size,
                        totalCost = packageLogSystem.totalCost,
                        totalPackages = 4
                    });
                }
            }
        }
        public ShowPackagesLogWindow(List<List<AnimateMessage>> animateMessages)
        {
            InitializeComponent();
            this.animateMessages = animateMessages;
            ShowLog();
        }
    }
}
