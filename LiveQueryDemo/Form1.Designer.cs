using System.Windows.Forms;
using System.Drawing;

namespace LiveQueryDemo
{
    partial class Form1
    {
        private DataGridView grid;
        private Panel panelTop;
        private Panel panelButtons;
        private TextBox txtFirst;
        private TextBox txtLast;
        private TextBox txtTitle;
        private Label lblFirst;
        private Label lblLast;
        private Label lblTitle;
        private Button btnAppend;
        private Button btnEdit;
        private Button btnPost;
        private Button btnCancel;
        private Button btnDelete;

        private TextBox txtLocate;
        private Button btnLocate;
        private TextBox txtFilter;
        private Button btnFilter;
        private TextBox txtOrderBy;
        private Button btnOrderBy;
        private Label lblStatus;

        private void InitializeComponent()
        {
            this.grid = new DataGridView();
            this.panelTop = new Panel();
            this.panelButtons = new Panel();
            this.txtFirst = new TextBox();
            this.txtLast = new TextBox();
            this.txtTitle = new TextBox();
            this.lblFirst = new Label();
            this.lblLast = new Label();
            this.lblTitle = new Label();
            this.btnAppend = new Button();
            this.btnEdit = new Button();
            this.btnPost = new Button();
            this.btnCancel = new Button();
            this.btnDelete = new Button();
            this.txtLocate = new TextBox();
            this.btnLocate = new Button();
            this.txtFilter = new TextBox();
            this.btnFilter = new Button();
            this.txtOrderBy = new TextBox();
            this.btnOrderBy = new Button();
            this.lblStatus = new Label();

            // Form
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Text = "TLiveQuery Demo (Modern UI)";
            this.Font = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Regular);
            this.Load += new System.EventHandler(this.Form1_Load);

            // GRID
            this.grid.Location = new Point(10, 10);
            this.grid.Size = new Size(880, 330);
            this.grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.grid.ReadOnly = false;
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            this.grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.grid.EnableHeadersVisualStyles = false;
            this.grid.SelectionChanged += new System.EventHandler(this.grid_SelectionChanged);
            this.grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            // PANEL TOP (Inputs)
            this.panelTop.Location = new Point(10, 350);
            this.panelTop.Size = new Size(880, 100);
            this.panelTop.BorderStyle = BorderStyle.FixedSingle;

            // FirstName
            this.lblFirst.Text = "First:";
            this.lblFirst.Location = new Point(20, 15);

            this.txtFirst.Location = new Point(80, 12);
            this.txtFirst.Size = new Size(180, 25);

            // LastName
            this.lblLast.Text = "Last:";
            this.lblLast.Location = new Point(20, 50);

            this.txtLast.Location = new Point(80, 47);
            this.txtLast.Size = new Size(180, 25);

            // Title
            this.lblTitle.Text = "Title:";
            this.lblTitle.Location = new Point(300, 15);

            this.txtTitle.Location = new Point(350, 12);
            this.txtTitle.Size = new Size(180, 25);

            this.panelTop.Controls.AddRange(new Control[]
            {
                lblFirst, txtFirst,
                lblLast, txtLast,
                lblTitle, txtTitle
            });

            // PANEL BUTTONS
            this.panelButtons.Location = new Point(10, 460);
            this.panelButtons.Size = new Size(880, 50);

            btnAppend.Text = "Append";
            btnEdit.Text = "Edit";
            btnPost.Text = "Post";
            btnCancel.Text = "Cancel";
            btnDelete.Text = "Delete";

            Button[] btns = { btnAppend, btnEdit, btnPost, btnCancel, btnDelete };
            int offset = 10;
            foreach (var b in btns)
            {
                b.Size = new Size(100, 32);
                b.Location = new Point(offset, 10);
                b.BackColor = Color.FromArgb(240, 240, 240);
                b.FlatStyle = FlatStyle.Flat;
                offset += 110;
            }

            btnAppend.Click += btnAppend_Click;
            btnEdit.Click += btnEdit_Click;
            btnPost.Click += btnPost_Click;
            btnCancel.Click += btnCancel_Click;
            btnDelete.Click += btnDelete_Click;

            this.panelButtons.Controls.AddRange(btns);

            // Search / Filter / OrderBy
            this.txtLocate.Location = new Point(10, 520);
            this.txtLocate.Size = new Size(180, 25);
            this.txtLocate.PlaceholderText = "Locate...";

            this.btnLocate.Text = "Locate";
            this.btnLocate.Location = new Point(200, 518);
            this.btnLocate.Size = new Size(80, 28);
            this.btnLocate.Click += btnLocate_Click;

            this.txtFilter.Location = new Point(300, 520);
            this.txtFilter.Size = new Size(180, 25);
            this.txtFilter.PlaceholderText = "Filter...";

            this.btnFilter.Text = "Apply";
            this.btnFilter.Location = new Point(490, 518);
            this.btnFilter.Size = new Size(80, 28);
            this.btnFilter.Click += btnFilter_Click;

            this.txtOrderBy.Location = new Point(590, 520);
            this.txtOrderBy.Size = new Size(180, 25);
            this.txtOrderBy.PlaceholderText = "OrderBy...";

            this.btnOrderBy.Text = "Sort";
            this.btnOrderBy.Location = new Point(780, 518);
            this.btnOrderBy.Size = new Size(80, 28);
            this.btnOrderBy.Click += btnOrderBy_Click;

            // Status
            this.lblStatus.Location = new Point(10, 555);
            this.lblStatus.Size = new Size(880, 25);

            // Form Controls
            this.Controls.Add(this.grid);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.txtLocate);
            this.Controls.Add(this.btnLocate);
            this.Controls.Add(this.txtFilter);
            this.Controls.Add(this.btnFilter);
            this.Controls.Add(this.txtOrderBy);
            this.Controls.Add(this.btnOrderBy);
            this.Controls.Add(this.lblStatus);
        }
    }
}