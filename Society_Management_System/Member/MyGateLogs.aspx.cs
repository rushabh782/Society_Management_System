using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Society_Management_System.Member
{
    public partial class MyGateLogs : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["societyDB"].ConnectionString;
        private long currentUserUnitId = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user is logged in
            if (Session["user_id"] == null)
            {
                Response.Redirect("~/Account/Login.aspx?msg=session_expired", false);
                return;
            }

            // Get user's unit ID from database using session user_id
            currentUserUnitId = GetUserUnitId();

            if (currentUserUnitId == 0)
            {
                ShowMessage("No unit found for your account. Please contact administrator.", "warning");
                return;
            }

            if (!IsPostBack)
            {
                LoadPendingGateLogs();
                LoadRecentlyApprovedLogs();
                UpdatePendingCount();
            }
        }

        #region Get User Unit ID

        private long GetUserUnitId()
        {
            try
            {
                string userId = Session["user_id"].ToString();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Get unit_id from user's occupancy
                    string query = @"
                        SELECT TOP 1 uo.unit_id
                        FROM users u
                        INNER JOIN members m ON u.member_id = m.member_id
                        INNER JOIN unit_occupancies uo ON m.member_id = uo.member_id
                        WHERE u.user_id = @user_id 
                        AND (uo.end_date IS NULL OR uo.end_date >= GETDATE())
                        ORDER BY uo.start_date DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", long.Parse(userId));
                        conn.Open();

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt64(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error getting user unit ID: " + ex.ToString());
            }

            return 0;
        }

        #endregion

        #region Load Gate Logs

        private void LoadPendingGateLogs()
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
                            gl.status,
                            s.name AS society_name,
                            b.name AS building_name,
                            u.unit_no
                        FROM gate_logs gl
                        INNER JOIN societies s ON gl.society_id = s.society_id
                        INNER JOIN buildings b ON gl.building_id = b.building_id
                        INNER JOIN units u ON gl.unit_id = u.unit_id
                        WHERE gl.unit_id = @unit_id 
                        AND gl.status = 'Pending'
                        ORDER BY gl.check_in DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@unit_id", currentUserUnitId);
                        conn.Open();

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvPendingLogs.DataSource = dt;
                        gvPendingLogs.DataBind();

                        // Update count
                        lblPendingCount.Text = dt.Rows.Count.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading pending gate logs: " + ex.Message, "danger");
            }
        }

        private void LoadRecentlyApprovedLogs()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Get recently approved logs (last 7 days)
                    string query = @"
                        SELECT TOP 10
                            gl.gate_log_id,
                            gl.visitor_name,
                            gl.vehicle_no,
                            gl.purpose,
                            gl.check_in,
                            gl.check_out,
                            gl.status,
                            s.name AS society_name,
                            b.name AS building_name,
                            u.unit_no
                        FROM gate_logs gl
                        INNER JOIN societies s ON gl.society_id = s.society_id
                        INNER JOIN buildings b ON gl.building_id = b.building_id
                        INNER JOIN units u ON gl.unit_id = u.unit_id
                        WHERE gl.unit_id = @unit_id 
                        AND gl.status = 'Approved'
                        AND gl.check_in >= DATEADD(day, -7, GETDATE())
                        ORDER BY gl.check_in DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@unit_id", currentUserUnitId);
                        conn.Open();

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvRecentlyApproved.DataSource = dt;
                        gvRecentlyApproved.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading recently approved logs: " + ex.Message, "danger");
            }
        }

        private void UpdatePendingCount()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT COUNT(*) 
                        FROM gate_logs 
                        WHERE unit_id = @unit_id 
                        AND status = 'Pending'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@unit_id", currentUserUnitId);
                        conn.Open();

                        int count = (int)cmd.ExecuteScalar();
                        lblPendingCount.Text = count.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                lblPendingCount.Text = "0";
            }
        }

        #endregion

        #region Approve and Checkout Gate Log

        protected void gvPendingLogs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ApproveLog")
            {
                long gateLogId = long.Parse(e.CommandArgument.ToString());
                ApproveGateLog(gateLogId);
            }
        }

        protected void gvRecentlyApproved_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "CheckoutLog")
            {
                long gateLogId = long.Parse(e.CommandArgument.ToString());
                CheckoutGateLog(gateLogId);
            }
        }

        private void ApproveGateLog(long gateLogId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Update status to Approved, checkout remains NULL
                    string query = @"
                        UPDATE gate_logs
                        SET status = 'Approved'
                        WHERE gate_log_id = @gate_log_id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@gate_log_id", gateLogId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("Visitor approved successfully! They can now proceed to your unit.", "success");

                // Reload the grids to reflect changes
                LoadPendingGateLogs();
                LoadRecentlyApprovedLogs();
                UpdatePendingCount();
            }
            catch (Exception ex)
            {
                ShowMessage("Error approving visitor: " + ex.Message, "danger");
            }
        }

        private void CheckoutGateLog(long gateLogId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Set checkout time to current time using GETDATE()
                    string query = @"
                        UPDATE gate_logs
                        SET check_out = GETDATE()
                        WHERE gate_log_id = @gate_log_id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@gate_log_id", gateLogId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("Visitor checked out successfully!", "success");

                // Reload the approved visitors grid
                LoadRecentlyApprovedLogs();
            }
            catch (Exception ex)
            {
                ShowMessage("Error checking out visitor: " + ex.Message, "danger");
            }
        }

        #endregion

        #region Helper Methods

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = $"alert alert-{type} alert-dismissible fade show";
            lblMessage.Text = message;

            // Auto-hide success messages after 5 seconds
            if (type == "success")
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "autoHideMessage",
                    "setTimeout(function(){ var alert = document.querySelector('.alert'); if(alert) alert.style.display='none'; }, 5000);", true);
            }
        }

        #endregion
    }
}