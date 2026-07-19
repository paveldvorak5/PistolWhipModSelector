namespace PistolWhipModSelector
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.OriginalSongShowIDCheckBox = new System.Windows.Forms.CheckBox();
            this.OriginalSongShowNameCheckBox = new System.Windows.Forms.CheckBox();
            this.CustomSongsDataGridView = new System.Windows.Forms.DataGridView();
            this.songTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.songAuthor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.songPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReloadAllButton = new System.Windows.Forms.Button();
            this.CustomSongsResetButton = new System.Windows.Forms.Button();
            this.CustomSongsReplaceButton = new System.Windows.Forms.Button();
            this.CustomSongsDeleteButton = new System.Windows.Forms.Button();
            this.CustomSongsEditButton = new System.Windows.Forms.Button();
            this.Creator = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.CopyStateLabel = new System.Windows.Forms.Label();
            this.songsTreeView = new System.Windows.Forms.TreeView();
            this.CustomSongFullPathLabel = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.CustomSongsDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // OriginalSongShowIDCheckBox
            // 
            this.OriginalSongShowIDCheckBox.AutoSize = true;
            this.OriginalSongShowIDCheckBox.Checked = true;
            this.OriginalSongShowIDCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OriginalSongShowIDCheckBox.Location = new System.Drawing.Point(12, 21);
            this.OriginalSongShowIDCheckBox.Name = "OriginalSongShowIDCheckBox";
            this.OriginalSongShowIDCheckBox.Size = new System.Drawing.Size(67, 17);
            this.OriginalSongShowIDCheckBox.TabIndex = 1;
            this.OriginalSongShowIDCheckBox.Text = "Show ID";
            this.OriginalSongShowIDCheckBox.UseVisualStyleBackColor = true;
            this.OriginalSongShowIDCheckBox.CheckedChanged += new System.EventHandler(this.OriginalSongCheckBox_CheckedChanged);
            // 
            // OriginalSongShowNameCheckBox
            // 
            this.OriginalSongShowNameCheckBox.AutoSize = true;
            this.OriginalSongShowNameCheckBox.Checked = true;
            this.OriginalSongShowNameCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OriginalSongShowNameCheckBox.Location = new System.Drawing.Point(85, 21);
            this.OriginalSongShowNameCheckBox.Name = "OriginalSongShowNameCheckBox";
            this.OriginalSongShowNameCheckBox.Size = new System.Drawing.Size(84, 17);
            this.OriginalSongShowNameCheckBox.TabIndex = 2;
            this.OriginalSongShowNameCheckBox.Text = "Show Name";
            this.OriginalSongShowNameCheckBox.UseVisualStyleBackColor = true;
            this.OriginalSongShowNameCheckBox.CheckedChanged += new System.EventHandler(this.OriginalSongCheckBox_CheckedChanged);
            // 
            // CustomSongsDataGridView
            // 
            this.CustomSongsDataGridView.AllowUserToAddRows = false;
            this.CustomSongsDataGridView.AllowUserToDeleteRows = false;
            this.CustomSongsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CustomSongsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.CustomSongsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.CustomSongsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.songTitle,
            this.songAuthor,
            this.songPath});
            this.CustomSongsDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.CustomSongsDataGridView.Location = new System.Drawing.Point(370, 44);
            this.CustomSongsDataGridView.MultiSelect = false;
            this.CustomSongsDataGridView.Name = "CustomSongsDataGridView";
            this.CustomSongsDataGridView.RowHeadersVisible = false;
            this.CustomSongsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.CustomSongsDataGridView.ShowEditingIcon = false;
            this.CustomSongsDataGridView.Size = new System.Drawing.Size(418, 352);
            this.CustomSongsDataGridView.TabIndex = 3;
            this.CustomSongsDataGridView.SelectionChanged += new System.EventHandler(this.CustomSongsDataGridView_SelectionChanged);
            this.CustomSongsDataGridView.DragDrop += new System.Windows.Forms.DragEventHandler(this.CustomSongsDataGridView_DragDrop);
            this.CustomSongsDataGridView.DragEnter += new System.Windows.Forms.DragEventHandler(this.CustomSongsDataGridView_DragEnter);
            // 
            // Columns
            // 
            this.songTitle.HeaderText = "Title";
            this.songTitle.Name = "songTitle";
            this.songAuthor.HeaderText = "Author";
            this.songAuthor.Name = "songAuthor";
            this.songPath.HeaderText = "Path";
            this.songPath.Name = "songPath";
            // 
            // Buttons (placeholders)
            // 
            this.ReloadAllButton.Location = new System.Drawing.Point(12, 60);
            this.ReloadAllButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.ReloadAllButton.Name = "ReloadAllButton";
            this.ReloadAllButton.Size = new System.Drawing.Size(32, 32);
            this.ReloadAllButton.TabIndex = 4;
            this.ReloadAllButton.UseVisualStyleBackColor = true;
            this.ReloadAllButton.Click += new System.EventHandler(this.ReloadAllButton_Click);

            this.CustomSongsResetButton.Location = new System.Drawing.Point(410, 402);
            this.CustomSongsResetButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.CustomSongsResetButton.Name = "CustomSongsResetButton";
            this.CustomSongsResetButton.Size = new System.Drawing.Size(75, 23);
            this.CustomSongsResetButton.TabIndex = 5;
            this.CustomSongsResetButton.Text = "Reset";
            this.CustomSongsResetButton.UseVisualStyleBackColor = true;
            this.CustomSongsResetButton.Click += new System.EventHandler(this.CustomSongsResetButton_Click);

            this.CustomSongsReplaceButton.Location = new System.Drawing.Point(490, 402);
            this.CustomSongsReplaceButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.CustomSongsReplaceButton.Name = "CustomSongsReplaceButton";
            this.CustomSongsReplaceButton.Size = new System.Drawing.Size(75, 23);
            this.CustomSongsReplaceButton.TabIndex = 6;
            this.CustomSongsReplaceButton.Text = "Replace";
            this.CustomSongsReplaceButton.UseVisualStyleBackColor = true;
            this.CustomSongsReplaceButton.Click += new System.EventHandler(this.CustomSongsReplaceButton_Click);

            this.CustomSongsDeleteButton.Location = new System.Drawing.Point(570, 402);
            this.CustomSongsDeleteButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.CustomSongsDeleteButton.Name = "CustomSongsDeleteButton";
            this.CustomSongsDeleteButton.Size = new System.Drawing.Size(75, 23);
            this.CustomSongsDeleteButton.TabIndex = 7;
            this.CustomSongsDeleteButton.Text = "Delete";
            this.CustomSongsDeleteButton.UseVisualStyleBackColor = true;
            this.CustomSongsDeleteButton.Click += new System.EventHandler(this.CustomSongsDeleteButton_Click);

            this.CustomSongsEditButton.Location = new System.Drawing.Point(650, 402);
            this.CustomSongsEditButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.CustomSongsEditButton.Name = "CustomSongsEditButton";
            this.CustomSongsEditButton.Size = new System.Drawing.Size(75, 23);
            this.CustomSongsEditButton.TabIndex = 8;
            this.CustomSongsEditButton.Text = "Edit";
            this.CustomSongsEditButton.UseVisualStyleBackColor = true;
            this.CustomSongsEditButton.Click += new System.EventHandler(this.CustomSongsEditButton_Click);

            // 
            // Labels
            // 
            this.CopyStateLabel.Location = new System.Drawing.Point(12, 402);
            this.CopyStateLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.CopyStateLabel.Name = "CopyStateLabel";
            this.CopyStateLabel.Size = new System.Drawing.Size(352, 23);
            this.CopyStateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.CustomSongFullPathLabel.Location = new System.Drawing.Point(370, 44);
            this.CustomSongFullPathLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.CustomSongFullPathLabel.Name = "CustomSongFullPathLabel";
            this.CustomSongFullPathLabel.Size = new System.Drawing.Size(418, 16);

            // 
            // songsTreeView
            // 
            this.songsTreeView.Location = new System.Drawing.Point(12, 70);
            this.songsTreeView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.songsTreeView.Name = "songsTreeView";
            this.songsTreeView.Size = new System.Drawing.Size(352, 326);
            this.songsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.songsTreeView_AfterSelect);

            // Play/Stop buttons
            this.PlayButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            //
            // PlayButton
            //
            this.PlayButton.Location = new System.Drawing.Point(370, 12);
            this.PlayButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.PlayButton.Name = "PlayButton";
            this.PlayButton.Size = new System.Drawing.Size(75, 23);
            this.PlayButton.TabIndex = 20;
            this.PlayButton.Text = "Play";
            this.PlayButton.UseVisualStyleBackColor = true;
            this.PlayButton.Click += new System.EventHandler(this.PlayButton_Click);
            //
            // StopButton
            //
            this.StopButton.Location = new System.Drawing.Point(455, 12);
            this.StopButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(75, 23);
            this.StopButton.TabIndex = 21;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // playbackTrackBar
            // 
            this.playbackTrackBar = new System.Windows.Forms.TrackBar();
            this.playbackTrackBar.Location = new System.Drawing.Point(540, 12);
            this.playbackTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.playbackTrackBar.Name = "playbackTrackBar";
            this.playbackTrackBar.Size = new System.Drawing.Size(248, 45);
            this.playbackTrackBar.TabIndex = 22;
            this.playbackTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.playbackTrackBar.Minimum = 0;
            this.playbackTrackBar.Maximum = 1000;
            this.playbackTrackBar.Scroll += new System.EventHandler(this.playbackTrackBar_Scroll);
            this.playbackTrackBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.playbackTrackBar_MouseDown);
            this.playbackTrackBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.playbackTrackBar_MouseUp);

            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.OriginalSongShowIDCheckBox);
            this.Controls.Add(this.OriginalSongShowNameCheckBox);
            this.Controls.Add(this.CustomSongsDataGridView);
            this.Controls.Add(this.ReloadAllButton);
            this.Controls.Add(this.CustomSongsResetButton);
            this.Controls.Add(this.CustomSongsReplaceButton);
            this.Controls.Add(this.CustomSongsDeleteButton);
            this.Controls.Add(this.CustomSongsEditButton);
            this.Controls.Add(this.CopyStateLabel);
            this.Controls.Add(this.songsTreeView);
            this.Controls.Add(this.CustomSongFullPathLabel);
            this.Controls.Add(this.PlayButton);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.playbackTrackBar);
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);

            ((System.ComponentModel.ISupportInitialize)(this.CustomSongsDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.CheckBox OriginalSongShowIDCheckBox;
        private System.Windows.Forms.CheckBox OriginalSongShowNameCheckBox;
        private System.Windows.Forms.DataGridView CustomSongsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn songTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn songAuthor;
        private System.Windows.Forms.DataGridViewTextBoxColumn songPath;
        private System.Windows.Forms.Button ReloadAllButton;
        private System.Windows.Forms.Button CustomSongsResetButton;
        private System.Windows.Forms.Button CustomSongsReplaceButton;
        private System.Windows.Forms.Button CustomSongsDeleteButton;
        private System.Windows.Forms.Button CustomSongsEditButton;
        private System.Windows.Forms.Label Creator;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label CopyStateLabel;
        private System.Windows.Forms.TreeView songsTreeView;
        private System.Windows.Forms.Label CustomSongFullPathLabel;
        private System.Windows.Forms.Button PlayButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.TrackBar playbackTrackBar;
    }
}
