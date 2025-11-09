using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;

namespace Society_Management_System.Admin
{
    public partial class ManageBills : System.Web.UI.Page
    {
        private static readonly string ConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["SocietyDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSocieties();
                DdlUnit.Items.Clear();
                DdlUnit.Items.Insert(0, new ListItem("--Select Unit--", "0"));
                TxtBillMonth.Text = DateTime.Now.ToString("yyyy-MM-01");
                Session["BillItems"] = new List<BillItem>();
                LoadBills();
                TxtTotalAmount.Text = "0.00";
            }
        }

        protected void LoadSocieties()
        {
            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                string query = "SELECT society_id, name FROM societies ORDER BY name";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                DdlSociety.DataSource = dt;
                DdlSociety.DataTextField = "name";
                DdlSociety.DataValueField = "society_id";
                DdlSociety.DataBind();
            }
            DdlSociety.Items.Insert(0, new ListItem("--Select Society--", "0"));
        }

        protected void DdlSociety_SelectedIndexChanged(object sender, EventArgs e)
        {
            long societyId = 0;
            if (long.TryParse(DdlSociety.SelectedValue, out societyId) && societyId > 0)
            {
                using (SqlConnection con = new SqlConnection(ConnStr))
                {
                    string query = @"
                SELECT U.unit_id, U.unit_no
                FROM units U
                INNER JOIN buildings B ON U.building_id = B.building_id
                WHERE B.society_id = @society_id
                ORDER BY U.unit_no";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.SelectCommand.Parameters.AddWithValue("@society_id", societyId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    DdlUnit.DataSource = dt;
                    DdlUnit.DataTextField = "unit_no";
                    DdlUnit.DataValueField = "unit_id";
                    DdlUnit.DataBind();
                }
            }
            else
            {
                DdlUnit.Items.Clear();
            }
            DdlUnit.Items.Insert(0, new ListItem("--Select Unit--", "0"));
            UpdateTotalAmount();
        }


        protected void LoadBills()
        {
            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                string query = @"
            SELECT MB.bill_id, S.name, U.unit_no, MB.bill_month, MB.due_date, MB.total_amount, MB.status
            FROM maintenance_bills MB
            INNER JOIN societies S ON MB.society_id = S.society_id
            INNER JOIN units U ON MB.unit_id = U.unit_id
            ORDER BY MB.bill_id DESC";  // Order by bill_id descending for latest first
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                GvBills.DataSource = dt;
                GvBills.DataBind();
            }
        }


        protected void BtnSave_Click(object sender, EventArgs e)
        {
            long billId = string.IsNullOrEmpty(HfBillId.Value) ? 0 : Convert.ToInt64(HfBillId.Value);
            long societyId = Convert.ToInt64(DdlSociety.SelectedValue);
            long unitId = Convert.ToInt64(DdlUnit.SelectedValue);
            DateTime billMonth = Convert.ToDateTime(TxtBillMonth.Text);
            DateTime dueDate = Convert.ToDateTime(TxtDueDate.Text);
            decimal totalAmount = Convert.ToDecimal(TxtTotalAmount.Text);
            string status = "Unpaid"; // default status always

            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                con.Open();
                SqlTransaction trans = con.BeginTransaction();
                try
                {
                    if (billId == 0)
                    {
                        string insertBill = @"
                            INSERT INTO maintenance_bills (society_id, unit_id, bill_month, due_date, total_amount, status)
                            VALUES (@society_id, @unit_id, @bill_month, @due_date, @total_amount, @status);
                            SELECT SCOPE_IDENTITY();";
                        using (SqlCommand cmd = new SqlCommand(insertBill, con, trans))
                        {
                            cmd.Parameters.AddWithValue("@society_id", societyId);
                            cmd.Parameters.AddWithValue("@unit_id", unitId);
                            cmd.Parameters.AddWithValue("@bill_month", billMonth);
                            cmd.Parameters.AddWithValue("@due_date", dueDate);
                            cmd.Parameters.AddWithValue("@total_amount", totalAmount);
                            cmd.Parameters.AddWithValue("@status", status);
                            billId = Convert.ToInt64(cmd.ExecuteScalar());
                        }
                    }
                    else
                    {
                        string updateBill = @"
                            UPDATE maintenance_bills SET 
                            society_id = @society_id, unit_id = @unit_id, bill_month = @bill_month, due_date = @due_date, 
                            total_amount = @total_amount, status = @status WHERE bill_id = @bill_id";
                        using (SqlCommand cmd = new SqlCommand(updateBill, con, trans))
                        {
                            cmd.Parameters.AddWithValue("@society_id", societyId);
                            cmd.Parameters.AddWithValue("@unit_id", unitId);
                            cmd.Parameters.AddWithValue("@bill_month", billMonth);
                            cmd.Parameters.AddWithValue("@due_date", dueDate);
                            cmd.Parameters.AddWithValue("@total_amount", totalAmount);
                            cmd.Parameters.AddWithValue("@status", status);
                            cmd.Parameters.AddWithValue("@bill_id", billId);
                            cmd.ExecuteNonQuery();
                        }

                        string delItems = "DELETE FROM bill_items WHERE bill_id = @bill_id";
                        using (SqlCommand cmdDel = new SqlCommand(delItems, con, trans))
                        {
                            cmdDel.Parameters.AddWithValue("@bill_id", billId);
                            cmdDel.ExecuteNonQuery();
                        }
                    }

                    var items = Session["BillItems"] as List<BillItem>;
                    foreach (var item in items)
                    {
                        string insertItem = @"INSERT INTO bill_items (bill_id, description, amount) VALUES (@bill_id, @desc, @amt)";
                        using (SqlCommand cmdItem = new SqlCommand(insertItem, con, trans))
                        {
                            cmdItem.Parameters.AddWithValue("@bill_id", billId);
                            cmdItem.Parameters.AddWithValue("@desc", item.Description);
                            cmdItem.Parameters.AddWithValue("@amt", item.Amount);
                            cmdItem.ExecuteNonQuery();
                        }
                    }

                    trans.Commit();
                    LblMessage.Text = "Bill saved successfully!";
                }
                catch
                {
                    trans.Rollback();
                    LblMessage.Text = "Error saving bill.";
                }
            }

            HfBillId.Value = "";
            Session["BillItems"] = new List<BillItem>();
            ClearFields();
            LoadBills();
            GvBillItems.DataSource = null;
            GvBillItems.DataBind();
            TxtTotalAmount.Text = "0.00";
        }

        protected void BtnCancel_Click(object sender, EventArgs e)
        {
            HfBillId.Value = "";
            ClearFields();
            Session["BillItems"] = new List<BillItem>();
            GvBillItems.DataSource = null;
            GvBillItems.DataBind();
            TxtTotalAmount.Text = "0.00";
        }

        private void ClearFields()
        {
            DdlSociety.SelectedIndex = 0;
            DdlUnit.Items.Clear();
            DdlUnit.Items.Insert(0, new ListItem("--Select Unit--", "0"));
            TxtBillMonth.Text = DateTime.Now.ToString("yyyy-MM-01");
            TxtDueDate.Text = "";
            TxtTotalAmount.Text = "0.00";
            LblMessage.Text = "";
        }

        protected void BtnAddItem_Click(object sender, EventArgs e)
        {
            var items = Session["BillItems"] as List<BillItem>;
            items.Add(new BillItem
            {
                ItemId = 0,
                Description = TxtItemDesc.Text.Trim(),
                Amount = string.IsNullOrEmpty(TxtItemAmount.Text) ? 0 : Convert.ToDecimal(TxtItemAmount.Text)
            });
            Session["BillItems"] = items;
            GvBillItems.DataSource = items;
            GvBillItems.DataBind();
            TxtItemDesc.Text = "";
            TxtItemAmount.Text = "";

            UpdateTotalAmount();
        }

        protected void GvBillItems_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
        {
            var items = Session["BillItems"] as List<BillItem>;
            if (items != null && e.RowIndex >= 0 && e.RowIndex < items.Count)
            {
                items.RemoveAt(e.RowIndex);
                Session["BillItems"] = items;
                GvBillItems.DataSource = items;
                GvBillItems.DataBind();
                UpdateTotalAmount();
            }
        }

        protected void GvBills_RowEditing(object sender, System.Web.UI.WebControls.GridViewEditEventArgs e)
        {
            long billId = Convert.ToInt64(GvBills.DataKeys[e.NewEditIndex].Value);
            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                string query = "SELECT * FROM maintenance_bills WHERE bill_id = @bill_id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@bill_id", billId);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    HfBillId.Value = dr["bill_id"].ToString();
                    DdlSociety.SelectedValue = dr["society_id"].ToString();
                    DdlSociety_SelectedIndexChanged(null, null);
                    DdlUnit.SelectedValue = dr["unit_id"].ToString();
                    TxtBillMonth.Text = Convert.ToDateTime(dr["bill_month"]).ToString("yyyy-MM-dd");
                    TxtDueDate.Text = Convert.ToDateTime(dr["due_date"]).ToString("yyyy-MM-dd");
                    TxtTotalAmount.Text = Convert.ToDecimal(dr["total_amount"]).ToString("F2");
                    LblMessage.Text = "";
                }
                dr.Close();

                string itemQuery = "SELECT item_id, description, amount FROM bill_items WHERE bill_id = @bill_id";
                SqlCommand cmdItems = new SqlCommand(itemQuery, con);
                cmdItems.Parameters.AddWithValue("@bill_id", billId);
                SqlDataAdapter da = new SqlDataAdapter(cmdItems);
                DataTable dt = new DataTable();
                da.Fill(dt);

                var items = new List<BillItem>();
                foreach (DataRow row in dt.Rows)
                {
                    items.Add(new BillItem
                    {
                        ItemId = Convert.ToInt64(row["item_id"]),
                        Description = row["description"].ToString(),
                        Amount = Convert.ToDecimal(row["amount"])
                    });
                }
                Session["BillItems"] = items;
                GvBillItems.DataSource = items;
                GvBillItems.DataBind();
            }
        }

        protected void GvBills_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
        {
            long billId = Convert.ToInt64(GvBills.DataKeys[e.RowIndex].Value);
            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                con.Open();
                string delItems = "DELETE FROM bill_items WHERE bill_id = @bill_id";
                SqlCommand cmdDelItems = new SqlCommand(delItems, con);
                cmdDelItems.Parameters.AddWithValue("@bill_id", billId);
                cmdDelItems.ExecuteNonQuery();

                string delBill = "DELETE FROM maintenance_bills WHERE bill_id = @bill_id";
                SqlCommand cmdDelBill = new SqlCommand(delBill, con);
                cmdDelBill.Parameters.AddWithValue("@bill_id", billId);
                cmdDelBill.ExecuteNonQuery();
            }
            LoadBills();
            BtnCancel_Click(null, null);
        }

        protected void GvBills_RowUpdating(object sender, System.Web.UI.WebControls.GridViewUpdateEventArgs e) { }

        protected void GvBills_RowCancelingEdit(object sender, System.Web.UI.WebControls.GridViewCancelEditEventArgs e) { }

        protected void TxtDueDate_TextChanged(object sender, EventArgs e)
        {
            DateTime selectedDate;
            if (DateTime.TryParse(TxtDueDate.Text, out selectedDate))
            {
                if (selectedDate <= DateTime.Now)
                {
                    LblMessage.Text = "Due date must be a future date!";
                    TxtDueDate.Text = "";
                }
                else
                {
                    LblMessage.Text = "";
                }
            }
            else
            {
                LblMessage.Text = "Invalid due date format!";
            }
        }

        private decimal GetSocietyExpenses(long societyId)
        {
            decimal total = 0;
            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                string query = "SELECT ISNULL(SUM(amount), 0) FROM expenses WHERE society_id = @society_id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@society_id", societyId);
                con.Open();
                total = Convert.ToDecimal(cmd.ExecuteScalar());
            }
            return total;
        }

        private int GetUnitCount(long societyId)
        {
            int count = 1;
            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                string query = @"
            SELECT COUNT(*) FROM units U
            INNER JOIN buildings B ON U.building_id = B.building_id
            WHERE B.society_id = @society_id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@society_id", societyId);
                con.Open();
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return count > 0 ? count : 1;
        }


        private void UpdateTotalAmount()
        {
            decimal total = 0;
            long societyId = 0;
            if (long.TryParse(DdlSociety.SelectedValue, out societyId) && societyId > 0)
            {
                var items = Session["BillItems"] as List<BillItem>;
                decimal itemsTotal = items?.Sum(x => x.Amount) ?? 0;
                decimal expensesTotal = GetSocietyExpenses(societyId);
                int unitCount = GetUnitCount(societyId);

                // Add full bill items total to expenses divided by unit count
                total = itemsTotal + (expensesTotal / unitCount);
            }
            TxtTotalAmount.Text = total.ToString("F2");
        }


    }

    [Serializable]
    public class BillItem
    {
        public long ItemId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
}