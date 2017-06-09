using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.Data.Game;
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

                Core.Data.Paths.DataPath = new DirectoryInfo(dataPath);

                DataDir = new DirectoryInfo(dataPath);

                string dbPath = Path.Combine(dataPath, "databases");
                ZoneDB.Instance.Setup(Path.Combine(dbPath, "zones.db3"));
            }

            LoadRoomList(-1);
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

        private void LoadRoomList(int selectedRoom)
        {
            var zones = ZoneDB.Instance.GetAllZones();

            RoomList.SuspendLayout();
            RoomList.Items.Clear();

            foreach (var z in zones)
            {
                ListViewGroup group = new ListViewGroup(z.ID.ToString() + "_" + z.Name);
                group.Tag = z;
                RoomList.Groups.Add(group);

                foreach (var r in z.Rooms)
                {
                    ListViewItem item = new ListViewItem(r.UID + "_" + r.Name);
                    item.Group = group;
                    RoomList.Items.Add(item);

                    if (r.UID == selectedRoom)
                        item.Selected = true;
                }
            }

            RoomList.ResumeLayout();
        }

        private void newRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void newZoneToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void deleteRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        Room GetSelectedRoom()
        {
            if (RoomList.SelectedItems.Count == 0)
                return null;

            return RoomList.SelectedItems[0].Tag as Room;
        }

        private void RoomList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var r = GetSelectedRoom();
            if (r == null)
                return;
        }

        private void MapImage_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            foreach (var room in )
        }
    }
}
