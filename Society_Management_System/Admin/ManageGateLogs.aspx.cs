using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Society_Management_System.Admin
{
    public partial class ManageGateLogs : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["societyDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user is logged in and is admin
            if (Session["user_id"] == null || Session["role"] == null)
            {
                Response.Redirect("~/Account/Login.aspx?msg=session_expired", false);
                return;
            }

            if (Session["role"].ToString().ToLower() != "admin")
            {
                Response.Redirect("~/Account/Login.aspx?msg=unauthorized", false);
                return;
            }

            if (!IsPostBack)
            {
                LoadSocieties();
                LoadGateLogs();
            }
        }

        #region Load Dropdowns

        private void LoadSocieties()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT society_id, name FROM societies ORDER BY name";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        ddlSociety.DataSource = reader;
                        ddlSociety.DataTextField = "name";
                        ddlSociety.DataValueField = "society_id";
                        ddlSociety.DataBind();
                        ddlSociety.Items.Insert(0, new ListItem("-- Select Society --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading societies: " + ex.Message, "danger");
            }
        }

        private void LoadBuildings(long societyId)
        {
            ddlBuilding.Items.Clear();
            ddlBuilding.Items.Insert(0, new ListItem("-- Select Building --", ""));
            ddlUnit.Items.Clear();
            ddlUnit.Items.Insert(0, new ListItem("-- Select Unit --", ""));

            if (societyId <= 0) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT building_id, name FROM buildings WHERE society_id = @society_id ORDER BY name";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@society_id", societyId);
                        conn.Open();

                        SqlDataReader reader = cmd.ExecuteReader();
                        ddlBuilding.DataSource = reader;
                        ddlBuilding.DataTextField = "name";
                        ddlBuilding.DataValueField = "building_id";
                        ddlBuilding.DataBind();
                        ddlBuilding.Items.Insert(0, new ListItem("-- Select Building --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading buildings: " + ex.Message, "danger");
            }
        }

        private void LoadUnits(long buildingId)
        {
            ddlUnit.Items.Clear();
            ddlUnit.Items.Insert(0, new ListItem("-- Select Unit --", ""));

            if (buildingId <= 0) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT unit_id, unit_no, floor_no FROM units WHERE building_id = @building_id ORDER BY floor_no, unit_no";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@building_id", buildingId);
                        conn.Open();

                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            string unitText = reader["unit_no"].ToString() + " (Floor " + reader["floor_no"].ToString() + ")";
                            ddlUnit.Items.Add(new ListItem(unitText, reader["unit_id"].ToString()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading units: " + ex.Message, "danger");
            }
        }

        #endregion

        #region Dropdown Events

        protected void ddlSociety_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (long.TryParse(ddlSociety.SelectedValue, out long societyId))
            {
                LoadBuildings(societyId);
            }
        }

        protected void ddlBuilding_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (long.TryParse(ddlBuilding.SelectedValue, out long buildingId))
            {
                LoadUnits(buildingId);
            }
        }

        #endregion

        #region Load Gate Logs

        private void LoadGateLogs()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            gl.gate_log_id,
                            gl.visitor_name,
                            gl.vehicle_no,
                            gl.purpose,
                            gl.check_in,
                            gl.check_out,
                            gl.status,
                            gl.society_id,
                            gl.building_id,
                            gl.unit_id,
                            s.name AS society_name,
                            b.name AS building_name,
                            u.unit_no
                        FROM gate_logs gl
                        INNER JOIN societies s ON gl.society_id = s.society_id
                        INNER JOIN buildings b ON gl.building_id = b.building_id
                        INNER JOIN units u ON gl.unit_id = u.unit_id
                        ORDER BY gl.check_in DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvGateLogs.DataSource = dt;
                        gvGateLogs.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading gate logs: " + ex.Message, "danger");
            }
        }

        #endregion

        #region Save/Update/Delete

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                long gateLogId = long.Parse(hfGateLogId.Value);
                long societyId = long.Parse(ddlSociety.SelectedValue);
                long buildingId = long.Parse(ddlBuilding.SelectedValue);
                long unitId = long.Parse(ddlUnit.SelectedValue);
                string visitorName = txtVisitorName.Text.Trim();
                string vehicleNo = string.IsNullOrWhiteSpace(txtVehicleNo.Text) ? null : txtVehicleNo.Text.Trim();
                string purpose = string.IsNullOrWhiteSpace(txtPurpose.Text) ? null : txtPurpose.Text.Trim();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    if (gateLogId == 0)
                    {
                        // Insert new gate log with automatic check-in time (GETDATE()) and Pending status
                        string insertQuery = @"
                            INSERT INTO gate_logs 
                            (society_id, building_id, unit_id, visitor_name, vehicle_no, purpose, check_in, check_out, status)
                            VALUES 
                            (@society_id, @building_id, @unit_id, @visitor_name, @vehicle_no, @purpose, GETDATE(), NULL, 'Pending')";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@society_id", societyId);
                            cmd.Parameters.AddWithValue("@building_id", buildingId);
                            cmd.Parameters.AddWithValue("@unit_id", unitId);
                            cmd.Parameters.AddWithValue("@visitor_name", visitorName);
                            cmd.Parameters.AddWithValue("@vehicle_no", (object)vehicleNo ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@purpose", (object)purpose ?? DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }
                        ShowMessage("Gate log added successfully! Visitor marked as Pending.", "success");
                    }
                    else
                    {
                        // Update existing gate log (only visitor details, not check-in/check-out/status)
                        string updateQuery = @"
                            UPDATE gate_logs
                            SET 
                                society_id = @society_id,
                                building_id = @building_id,
                                unit_id = @unit_id,
                                visitor_name = @visitor_name,
                                vehicle_no = @vehicle_no,
                                purpose = @purpose
                            WHERE gate_log_id = @gate_log_id";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@gate_log_id", gateLogId);
                            cmd.Parameters.AddWithValue("@society_id", societyId);
                            cmd.Parameters.AddWithValue("@building_id", buildingId);
                            cmd.Parameters.AddWithValue("@unit_id", unitId);
                            cmd.Parameters.AddWithValue("@visitor_name", visitorName);
                            cmd.Parameters.AddWithValue("@vehicle_no", (object)vehicleNo ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@purpose", (object)purpose ?? DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }
                        ShowMessage("Gate log updated successfully!", "success");
                    }
                }

                ClearForm();
                LoadGateLogs();
            }
            catch (Exception ex)
            {
                ShowMessage("Error saving gate log: " + ex.Message, "danger");
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void gvGateLogs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            long gateLogId = long.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "EditLog")
            {
                LoadGateLogForEdit(gateLogId);
            }
            else if (e.CommandName == "DeleteLog")
            {
                DeleteGateLog(gateLogId);
            }
        }

        private void LoadGateLogForEdit(long gateLogId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            gate_log_id,
                            society_id,
                            building_id,
                            unit_id,
                            visitor_name,
                            vehicle_no,
                            purpose,
                            status
                        FROM gate_logs
                        WHERE gate_log_id = @gate_log_id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@gate_log_id", gateLogId);
                        conn.Open();

                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            hfGateLogId.Value = reader["gate_log_id"].ToString();

                            // Load society first
                            ddlSociety.SelectedValue = reader["society_id"].ToString();
                            LoadBuildings(long.Parse(reader["society_id"].ToString()));

                            // Load building
                            ddlBuilding.SelectedValue = reader["building_id"].ToString();
                            LoadUnits(long.Parse(reader["building_id"].ToString()));

                            // Load unit
                            ddlUnit.SelectedValue = reader["unit_id"].ToString();

                            txtVisitorName.Text = reader["visitor_name"].ToString();
                            txtVehicleNo.Text = reader["vehicle_no"] != DBNull.Value ? reader["vehicle_no"].ToString() : "";
                            txtPurpose.Text = reader["purpose"] != DBNull.Value ? reader["purpose"].ToString() : "";

                            lblFormTitle.Text = "Edit Gate Log";
                            btnSave.Text = "Update Gate Log";
                        }
                    }
                }

                // Scroll to form
                ScriptManager.RegisterStartupScript(this, GetType(), "scrollToForm",
                    "window.scrollTo(0, 0);", true);
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading gate log: " + ex.Message, "danger");
            }
        }

        private void DeleteGateLog(long gateLogId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM gate_logs WHERE gate_log_id = @gate_log_id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@gate_log_id", gateLogId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("Gate log deleted successfully!", "success");
                LoadGateLogs();
            }
            catch (Exception ex)
            {
                ShowMessage("Error deleting gate log: " + ex.Message, "danger");
            }
        }

        #endregion

        #region GridView Events

        protected void gvGateLogs_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label lblStatus = (Label)e.Row.FindControl("lblStatus");
                if (lblStatus != null)
                {
                    string status = lblStatus.Text;
                    if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    {
                        lblStatus.CssClass = "badge-pending";
                    }
                    else if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    {
                        lblStatus.CssClass = "badge-approved";
                    }
                }
            }
        }

        private void ClearForm()
        {
            hfGateLogId.Value = "0";
            ddlSociety.SelectedIndex = 0;
            ddlBuilding.Items.Clear();
            ddlBuilding.Items.Insert(0, new ListItem("-- Select Building --", ""));
            ddlUnit.Items.Clear();
            ddlUnit.Items.Insert(0, new ListItem("-- Select Unit --", ""));
            txtVisitorName.Text = "";
            txtVehicleNo.Text = "";
            txtPurpose.Text = "";
            lblFormTitle.Text = "Add New Gate Log";
            btnSave.Text = "Save Gate Log";
        }

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = $"alert alert-{type} alert-dismissible fade show";
            lblMessage.Text = message;
        }

        #endregion
    }
}