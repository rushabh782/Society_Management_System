using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Society_Management_System.Admin
{
    public partial class ManageBuildings : System.Web.UI.Page
    {
        private SqlConnection con;

        protected void Page_Load(object sender, EventArgs e)
        {
            string cnf = ConfigurationManager.ConnectionStrings["societyDB"].ConnectionString;
            con = new SqlConnection(cnf);
            con.Open();

            if (!IsPostBack)
            {
                PopulateSocieties();
                BindBuildingGrid();
            }
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            if (con != null && con.State == ConnectionState.Open)
                con.Close();
        }

        private void PopulateSocieties()
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT society_id, name FROM societies", con))
                {
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

        private void BindBuildingGrid()
        {
            if (ddlSocieties.SelectedValue == "0" || string.IsNullOrEmpty(ddlSocieties.SelectedValue))
            {
                gvBuildings.DataSource = null;
                litSocietyName.Text = "No Society Selected";
            }
            else
            {
                try
                {
                    string query = "SELECT building_id, name, floors FROM buildings WHERE society_id = @SocietyID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SocietyID", Convert.ToInt64(ddlSocieties.SelectedValue));
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            gvBuildings.DataSource = dt;
                        }
                    }
                    litSocietyName.Text = ddlSocieties.SelectedItem.Text;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    gvBuildings.DataSource = null;
                }
            }
            gvBuildings.DataBind();
        }

        private void ClearForm()
        {
            hfBuildingID.Value = "0";
            txtName.Text = "";
            txtFloors.Text = "";
            lblFormTitle.Text = "Add New Building";
            btnSave.Text = "Save Building";
        }

        protected void ddlSocieties_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindBuildingGrid();
            ClearForm();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (ddlSocieties.SelectedValue == "0")
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Please select a Society first.');", true);
                return;
            }

            try
            {
                string buildingName = txtName.Text.Trim();
                long societyID = Convert.ToInt64(ddlSocieties.SelectedValue);
                int floors = Convert.ToInt32(txtFloors.Text);

                // ✅ Check for duplicate
                string checkQuery = "SELECT COUNT(*) FROM buildings WHERE society_id = @SocietyID AND LOWER(name) = LOWER(@Name)";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@SocietyID", societyID);
                    checkCmd.Parameters.AddWithValue("@Name", buildingName);
                    int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                    System.Diagnostics.Debug.WriteLine("Exists count: " + exists);

                    if (exists > 0 && hfBuildingID.Value == "0")
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('This Building/Wing already exists for the selected Society.');", true);
                        return;
                    }
                }

                // ✅ Insert or update
                if (hfBuildingID.Value == "0")
                {
                    string insertQuery = "INSERT INTO buildings (society_id, name, floors) VALUES (@SocietyID, @Name, @Floors)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@SocietyID", societyID);
                        cmd.Parameters.AddWithValue("@Name", buildingName);
                        cmd.Parameters.AddWithValue("@Floors", floors);
                        int rows = cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine("Inserted rows: " + rows);
                    }
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Building saved successfully.');", true);
                }
                else
                {
                    string updateQuery = "UPDATE buildings SET name = @Name, floors = @Floors WHERE building_id = @BuildingID";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@BuildingID", Convert.ToInt64(hfBuildingID.Value));
                        cmd.Parameters.AddWithValue("@Name", buildingName);
                        cmd.Parameters.AddWithValue("@Floors", floors);
                        cmd.ExecuteNonQuery();
                    }
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Building updated successfully.');", true);
                }

                BindBuildingGrid();
                ClearForm();
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Error: " + ex.Message.Replace("'", "\\'") + "');", true);
                System.Diagnostics.Debug.WriteLine("Error: " + ex.ToString());
            }
        }


        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void gvBuildings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditRow")
            {
                try
                {
                    GridViewRow row = (GridViewRow)((LinkButton)e.CommandSource).NamingContainer;
                    hfBuildingID.Value = e.CommandArgument.ToString();

                    txtName.Text = System.Web.HttpUtility.HtmlDecode(row.Cells[0].Text);
                    txtFloors.Text = System.Web.HttpUtility.HtmlDecode(row.Cells[1].Text);

                    lblFormTitle.Text = "Edit Building";
                    btnSave.Text = "Update Building";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        protected void gvBuildings_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                long buildingID = Convert.ToInt64(e.Keys[0]);

                // ✅ Inline DELETE query
                string deleteQuery = "DELETE FROM buildings WHERE building_id = @BuildingID";
                using (SqlCommand cmd = new SqlCommand(deleteQuery, con))
                {
                    cmd.Parameters.AddWithValue("@BuildingID", buildingID);
                    cmd.ExecuteNonQuery();
                }

                BindBuildingGrid();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Unable to delete building. It might have linked units.');", true);
            }
        }
    }
}
