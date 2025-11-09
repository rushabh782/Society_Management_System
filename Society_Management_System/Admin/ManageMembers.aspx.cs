using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Society_Management_System.Admin
{
    public partial class ManageMembers : System.Web.UI.Page
    {
        private SqlConnection con;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Initialize and open connection
            string cnf = ConfigurationManager.ConnectionStrings["societyDB"].ConnectionString;
            con = new SqlConnection(cnf);
            con.Open();

            if (!IsPostBack)
            {
                PopulateSocieties();
                BindMemberGrid();
            }
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            if (con != null && con.State == ConnectionState.Open)
            {
                con.Close();
            }
        }

        private void PopulateSocieties()
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_Societies_GetAll", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        ddlSocieties.DataSource = dt;
                        ddlSocieties.DataTextField = "name";
                        ddlSocieties.DataValueField = "society_id";
                        ddlSocieties.DataBind();
                    }
                }
                ddlSocieties.Items.Insert(0, new ListItem("-- Select Society --", "0"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void PopulateBuildings(long societyId)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_Buildings_GetBySocietyID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SocietyID", societyId);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        ddlBuildings.DataSource = dt;
                        ddlBuildings.DataTextField = "name";
                        ddlBuildings.DataValueField = "building_id";
                        ddlBuildings.DataBind();
                    }
                }
                ddlBuildings.Items.Insert(0, new ListItem("-- Select Building --", "0"));
                ddlUnits.Items.Clear();
                ddlUnits.Items.Insert(0, new ListItem("-- Select Unit --", "0"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void PopulateUnits(long buildingId)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_Units_GetByBuildingID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BuildingID", buildingId);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        ddlUnits.DataSource = dt;
                        ddlUnits.DataTextField = "unit_no";
                        ddlUnits.DataValueField = "unit_id";
                        ddlUnits.DataBind();
                    }
                }
                ddlUnits.Items.Insert(0, new ListItem("-- Select Unit --", "0"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }


        //private void BindMemberGrid()
        //{
        //    if (ddlSocieties.SelectedValue == "0" || string.IsNullOrEmpty(ddlSocieties.SelectedValue))
        //    {
        //        gvMembers.DataSource = null;
        //        litSocietyName.Text = "No Society Selected";
        //    }
        //    else
        //    {
        //        try
        //        {
        //            using (SqlCommand cmd = new SqlCommand("sp_Members_GetBySocietyID", con))
        //            {
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                cmd.Parameters.AddWithValue("@SocietyID", Convert.ToInt64(ddlSocieties.SelectedValue));
        //                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        //                {
        //                    DataTable dt = new DataTable();
        //                    da.Fill(dt);
        //                    gvMembers.DataSource = dt;
        //                }
        //            }
        //            litSocietyName.Text = ddlSocieties.SelectedItem.Text;
        //        }
        //        catch (Exception ex)
        //        {
        //            System.Diagnostics.Debug.WriteLine(ex.Message);
        //            gvMembers.DataSource = null;
        //        }
        //    }
        //    gvMembers.DataBind();
        //}

        private void BindMemberGrid()
        {
            if (ddlSocieties.SelectedValue == "0" || string.IsNullOrEmpty(ddlSocieties.SelectedValue))
            {
                gvMembers.DataSource = null;
                litSocietyName.Text = "No Society Selected";
            }
            else
            {
                try
                {
                    string query = @"
                            SELECT 
                                m.member_id,
                                m.full_name,
                                m.email,
                                m.phone,
                                m.status,
                                b.name AS building_name,
                                u.unit_no
                            FROM members m
                            LEFT JOIN unit_occupancies uo ON uo.member_id = m.member_id
                            LEFT JOIN units u ON u.unit_id = uo.unit_id
                            LEFT JOIN buildings b ON b.building_id = u.building_id
                            WHERE m.society_id = @SocietyID
                            ORDER BY m.member_id DESC";


                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SocietyID", Convert.ToInt64(ddlSocieties.SelectedValue));

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            gvMembers.DataSource = dt;
                        }
                    }
                    litSocietyName.Text = ddlSocieties.SelectedItem.Text;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    gvMembers.DataSource = null;
                }
            }
            gvMembers.DataBind();
        }


        private void ClearForm()
        {
            hfMemberID.Value = "0";
            txtFullName.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtPhone.Text = string.Empty;
            ddlStatus.SelectedValue = "Active";
            lblFormTitle.Text = "Add New Member";
            btnSave.Text = "Save Member";
            // Keep ddlSocieties selected
        }

        protected void ddlSocieties_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlSocieties.SelectedValue != "0")
            {
                PopulateBuildings(Convert.ToInt64(ddlSocieties.SelectedValue));
                BindMemberGrid();
            }
            else
            {
                ddlBuildings.Items.Clear();
                ddlBuildings.Items.Insert(0, new ListItem("-- Select Building --", "0"));
                ddlUnits.Items.Clear();
                ddlUnits.Items.Insert(0, new ListItem("-- Select Unit --", "0"));
            }
            ClearForm();
        }

        protected void ddlBuildings_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlBuildings.SelectedValue != "0")
            {
                PopulateUnits(Convert.ToInt64(ddlBuildings.SelectedValue));
            }
            else
            {
                ddlUnits.Items.Clear();
                ddlUnits.Items.Insert(0, new ListItem("-- Select Unit --", "0"));
            }
        }



        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (ddlSocieties.SelectedValue == "0")
            {
                // Validator will catch
                return;
            }

          


            try
            {
                string spName;

                if (hfMemberID.Value == "0")
                {
                    // INSERT
                    spName = "sp_Members_Insert";
                    using (SqlCommand cmd = new SqlCommand(spName, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SocietyID", Convert.ToInt64(ddlSocieties.SelectedValue));
                        cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);

                        cmd.Parameters.AddWithValue("@BuildingID", Convert.ToInt64(ddlBuildings.SelectedValue));
                        cmd.Parameters.AddWithValue("@UnitID", Convert.ToInt64(ddlUnits.SelectedValue));
                        cmd.Parameters.AddWithValue("@Type", "Owner"); // or "Tenant" if applicable


                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // UPDATE
                    spName = "sp_Members_Update";
                    using (SqlCommand cmd = new SqlCommand(spName, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MemberID", Convert.ToInt64(hfMemberID.Value));
                        cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                        cmd.ExecuteNonQuery();
                    }
                }

                BindMemberGrid();
                ClearForm();
            }
            catch (SqlException sqlex)
            {
                System.Diagnostics.Debug.WriteLine(sqlex.Message);
                if (sqlex.Number == 2601 || sqlex.Number == 2627) // Unique constraint violation
                {
                    // Show error to user about duplicate email
                    // e.g., Page.ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('A member with this email already exists.');", true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void gvMembers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditRow")
            {
                try
                {
                    long memberID = Convert.ToInt64(e.CommandArgument);
                    using (SqlCommand cmd = new SqlCommand("sp_Members_GetByID", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MemberID", memberID);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                DataRow row = dt.Rows[0];
                                hfMemberID.Value = row["member_id"].ToString();
                                txtFullName.Text = row["full_name"].ToString();
                                txtEmail.Text = row["email"].ToString();
                                txtPhone.Text = row["phone"].ToString();

                                // Safely set dropdown values
                                if (ddlStatus.Items.FindByValue(row["status"].ToString()) != null)
                                {
                                    ddlStatus.SelectedValue = row["status"].ToString();
                                }
                                if (ddlSocieties.Items.FindByValue(row["society_id"].ToString()) != null)
                                {
                                    ddlSocieties.SelectedValue = row["society_id"].ToString();
                                }

                                lblFormTitle.Text = "Edit Member";
                                btnSave.Text = "Update Member";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        protected void gvMembers_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                long memberID = Convert.ToInt64(e.Keys[0]);
                using (SqlCommand cmd = new SqlCommand("sp_Members_Delete", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MemberID", memberID);
                    cmd.ExecuteNonQuery();
                }
                BindMemberGrid();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                // Handle error (e.g., if member is tied to a user or unit)
            }
        }
    }
}