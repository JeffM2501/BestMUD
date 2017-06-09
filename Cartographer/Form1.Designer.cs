namespace Cartographer
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.databaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setWorldDirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.RoomList = new System.Windows.Forms.ListView();
            this.RoomListContextMenus = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newRoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newZoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteRoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.MapFlowControl = new System.Windows.Forms.FlowLayoutPanel();
            this.MapImage = new System.Windows.Forms.PictureBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.RoomListContextMenus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.MapFlowControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MapImage)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.databaseToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(796, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // databaseToolStripMenuItem
            // 
            this.databaseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setDatabaseToolStripMenuItem,
            this.setWorldDirToolStripMenuItem});
            this.databaseToolStripMenuItem.Name = "databaseToolStripMenuItem";
            this.databaseToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.databaseToolStripMenuItem.Text = "Database";
            // 
            // setDatabaseToolStripMenuItem
            // 
            this.setDatabaseToolStripMenuItem.Name = "setDatabaseToolStripMenuItem";
            this.setDatabaseToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.setDatabaseToolStripMenuItem.Text = "Set Database...";
            this.setDatabaseToolStripMenuItem.Click += new System.EventHandler(this.setDatabaseToolStripMenuItem_Click);
            // 
            // setWorldDirToolStripMenuItem
            // 
            this.setWorldDirToolStripMenuItem.Name = "setWorldDirToolStripMenuItem";
            this.setWorldDirToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.setWorldDirToolStripMenuItem.Text = "Set World Dir...";
            this.setWorldDirToolStripMenuItem.Click += new System.EventHandler(this.setWorldDirToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.RoomList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(796, 446);
            this.splitContainer1.SplitterDistance = 265;
            this.splitContainer1.TabIndex = 1;
            // 
            // RoomList
            // 
            this.RoomList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.RoomList.ContextMenuStrip = this.RoomListContextMenus;
            this.RoomList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RoomList.FullRowSelect = true;
            this.RoomList.GridLines = true;
            this.RoomList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.RoomList.Location = new System.Drawing.Point(0, 0);
            this.RoomList.MultiSelect = false;
            this.RoomList.Name = "RoomList";
            this.RoomList.Size = new System.Drawing.Size(265, 446);
            this.RoomList.TabIndex = 0;
            this.RoomList.UseCompatibleStateImageBehavior = false;
            this.RoomList.View = System.Windows.Forms.View.Details;
            this.RoomList.SelectedIndexChanged += new System.EventHandler(this.RoomList_SelectedIndexChanged);
            // 
            // RoomListContextMenus
            // 
            this.RoomListContextMenus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newRoomToolStripMenuItem,
            this.newZoneToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteRoomToolStripMenuItem});
            this.RoomListContextMenus.Name = "RoomListContextMenus";
            this.RoomListContextMenus.Size = new System.Drawing.Size(143, 76);
            // 
            // newRoomToolStripMenuItem
            // 
            this.newRoomToolStripMenuItem.Name = "newRoomToolStripMenuItem";
            this.newRoomToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.newRoomToolStripMenuItem.Text = "New Room";
            this.newRoomToolStripMenuItem.Click += new System.EventHandler(this.newRoomToolStripMenuItem_Click);
            // 
            // newZoneToolStripMenuItem
            // 
            this.newZoneToolStripMenuItem.Name = "newZoneToolStripMenuItem";
            this.newZoneToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.newZoneToolStripMenuItem.Text = "New Zone";
            this.newZoneToolStripMenuItem.Click += new System.EventHandler(this.newZoneToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(139, 6);
            // 
            // deleteRoomToolStripMenuItem
            // 
            this.deleteRoomToolStripMenuItem.Name = "deleteRoomToolStripMenuItem";
            this.deleteRoomToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.deleteRoomToolStripMenuItem.Text = "Delete Room";
            this.deleteRoomToolStripMenuItem.Click += new System.EventHandler(this.deleteRoomToolStripMenuItem_Click);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "";
            this.columnHeader1.Width = 200;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.MapFlowControl);
            this.splitContainer2.Size = new System.Drawing.Size(527, 446);
            this.splitContainer2.SplitterDistance = 361;
            this.splitContainer2.TabIndex = 0;
            // 
            // MapFlowControl
            // 
            this.MapFlowControl.AutoScroll = true;
            this.MapFlowControl.Controls.Add(this.MapImage);
            this.MapFlowControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MapFlowControl.Location = new System.Drawing.Point(0, 0);
            this.MapFlowControl.Name = "MapFlowControl";
            this.MapFlowControl.Size = new System.Drawing.Size(361, 446);
            this.MapFlowControl.TabIndex = 0;
            // 
            // MapImage
            // 
            this.MapImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MapImage.Location = new System.Drawing.Point(3, 3);
            this.MapImage.Name = "MapImage";
            this.MapImage.Size = new System.Drawing.Size(100, 0);
            this.MapImage.TabIndex = 0;
            this.MapImage.TabStop = false;
            this.MapImage.Paint += new System.Windows.Forms.PaintEventHandler(this.MapImage_Paint);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(796, 470);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Cartographer";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.RoomListContextMenus.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.MapFlowControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MapImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem databaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setDatabaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setWorldDirToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView RoomList;
        private System.Windows.Forms.ContextMenuStrip RoomListContextMenus;
        private System.Windows.Forms.ToolStripMenuItem newRoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newZoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem deleteRoomToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.FlowLayoutPanel MapFlowControl;
        private System.Windows.Forms.PictureBox MapImage;
    }
}

