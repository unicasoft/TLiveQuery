
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using LiveQueryLib;

namespace LiveQueryDemo
{
    public partial class Form1 : Form
    {
        private SqlConnection _conn;
        private TLiveQuery _q;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Bağlantı cümleni buradan değiştir
            _conn = new SqlConnection("Server=.;Database=TestDB;Trusted_Connection=True;");
            _conn.Open();

            _q = new TLiveQuery("SELECT Id, FirstName, LastName, Title FROM Employees", _conn);
            _q.OnCalcFields += Q_OnCalcFields;
            _q.OnAfterScroll += Q_OnAfterScroll;

            _q.Open();

            grid.DataSource = _q.Binding;

            EditControlsFromCurrent();
            UpdateStatus();
        }

        private void Q_OnCalcFields(DataRow row)
        {
            if (!_q.Table.Columns.Contains("FullName"))
            {
                var col = _q.Table.Columns.Add("FullName", typeof(string));
                col.ReadOnly = true;
            }

            row["FullName"] = string.Format("{0} {1}", row["FirstName"], row["LastName"]);
        }

        private void Q_OnAfterScroll()
        {
            EditControlsFromCurrent();
            UpdateStatus();
        }

        private void btnAppend_Click(object sender, EventArgs e)
        {
            _q.Append();
            EditControlsFromCurrent();
            UpdateStatus();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            _q.Edit();
            EditControlsFromCurrent();
            UpdateStatus();
        }

        private void btnPost_Click(object sender, EventArgs e)
        {
            CurrentFromControls();
            _q.Post();
            UpdateStatus();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _q.Cancel();
            EditControlsFromCurrent();
            UpdateStatus();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            _q.Delete();
            EditControlsFromCurrent();
            UpdateStatus();
        }

        private void grid_SelectionChanged(object sender, EventArgs e)
        {
            EditControlsFromCurrent();
            UpdateStatus();
        }

        private void EditControlsFromCurrent()
        {
            if (_q == null || _q.CurrentRow == null) return;
            txtFirst.Text = _q.FieldByName("FirstName").AsString;
            txtLast.Text = _q.FieldByName("LastName").AsString;
            txtTitle.Text = _q.FieldByName("Title").AsString;
        }

        private void CurrentFromControls()
        {
            if (_q == null || _q.CurrentRow == null) return;
            _q.FieldByName("FirstName").AsString = txtFirst.Text;
            _q.FieldByName("LastName").AsString = txtLast.Text;
            _q.FieldByName("Title").AsString = txtTitle.Text;
        }

        private void btnLocate_Click(object sender, EventArgs e)
        {
            _q.Locate("FirstName", txtLocate.Text,
                LocateOptions.CaseInsensitive | LocateOptions.PartialKey);
            EditControlsFromCurrent();
            UpdateStatus();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFilter.Text))
            {
                _q.SetFiltered(false);
            }
            else
            {
                _q.SetFilter(txtFilter.Text);
            }
            UpdateStatus();
        }

        private void btnOrderBy_Click(object sender, EventArgs e)
        {
            _q.OrderBy(txtOrderBy.Text);
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (_q == null) return;
            lblStatus.Text = string.Format("RecNo: {0}/{1}  State: {2}",
                _q.RecNo, _q.RecordCount, _q.State);
        }
    }
}
