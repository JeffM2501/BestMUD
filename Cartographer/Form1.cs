using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.Databases.GameData;

namespace Cartographer
{
    public partial class Form1 : Form
    {
        public DirectoryInfo DataDir = null;

        public Form1()
        {
            InitializeComponent();

            string dataPath = FindDataDir();
            if (dataPath != string.Empty)
            {
                DataDir = new DirectoryInfo(dataPath);

                string dbPath = Path.Combine(dataPath, "databases");
                ZoneDB.Instance.Setup(Path.Combine(dbPath, "zones.db3"));
            }
        }

        private void setDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void setWorldDirToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public string FindDataDir()
        {
            string appLoc = System.Reflection.Assembly.GetExecutingAssembly().Location;
            DirectoryInfo checkDir = new DirectoryInfo(Path.GetDirectoryName(appLoc));

            while (checkDir != null)
            {
                string t = Path.Combine(checkDir.FullName, "data");
                if (Directory.Exists(t))
                    return t;

                checkDir = checkDir.Parent;

            }
            return string.Empty;
        }
    }
}
