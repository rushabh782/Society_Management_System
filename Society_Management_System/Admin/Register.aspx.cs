using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Society_Management_System.Admin
{
    public partial class Register : System.Web.UI.Page
    {
        private readonly string conStr = ConfigurationManager.ConnectionStrings["societyDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindSocieties();
                BindOccupancyTypes();

                //ddlSociety.Items.Insert(0, new ListItem("-- Select Society --", ""));
                //ddlBuilding.Items.Insert(0, new ListItem("-- Select Building --", ""));
                //ddlUnit.Items.Insert(0, new ListItem("-- Select Unit --", ""));
            }
        }

        private void BindSocieties()
        {
            using (SqlConnection conn = new SqlConnection(conStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT society_id, name FROM societies", conn);
                conn.Open();
                ddlSociety.DataSource = cmd.ExecuteReader();
                ddlSociety.DataTextField = "name";
                ddlSociety.DataValueField = "society_id";
                ddlSociety.DataBind();
            }
        }

        private void BindBuildings(long societyId)
        {
            using (SqlConnection conn = new SqlConnection(conStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT building_id, name FROM buildings WHERE society_id = @society_id", conn);
                cmd.Parameters.AddWithValue("@society_id", societyId);
                conn.Open();
                ddlBuilding.DataSource = cmd.ExecuteReader();
                ddlBuilding.DataTextField = "name";
                ddlBuilding.DataValueField = "building_id";
                ddlBuilding.DataBind();
            }

            ddlBuilding.Items.Insert(0, new ListItem("-- Select Building --", ""));
        }

        private void BindUnits(long buildingId)
        {
            using (SqlConnection conn = new SqlConnection(conStr))
            {
                string query = @"SELECT u.unit_no
                                 FROM units u
                                 LEFT JOIN unit_occupancies o
                                 ON u.unit_id = o.unit_id AND o.end_date IS NULL
                                 WHERE u.building_id = @building_id AND o.unit_id IS NULL
                                 ORDER BY u.unit_no";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@building_id", buildingId);
                conn.Open();
                ddlUnit.DataSource = cmd.ExecuteReader();
                ddlUnit.DataTextField = "unit_no";
                ddlUnit.DataValueField = "unit_no";
                ddlUnit.DataBind();
            }

            ddlUnit.Items.Insert(0, new ListItem("-- Select Unit --", ""));
        }

        private void BindOccupancyTypes()
        {
            ddlOccupancyType.Items.Clear();
            ddlOccupancyType.Items.Add(new ListItem("-- Select Occupancy Type --", ""));
            ddlOccupancyType.Items.Add(new ListItem("Owner", "Owner"));
            ddlOccupancyType.Items.Add(new ListItem("Tenant", "Tenant"));
            ddlOccupancyType.Items.Add(new ListItem("Vacant", "Vacant"));
        }

        protected void ddlSociety_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlBuilding.Items.Clear();
            ddlUnit.Items.Clear();
            ddlBuilding.Items.Insert(0, new ListItem("-- Select Building --", ""));
            ddlUnit.Items.Insert(0, new ListItem("-- Select Unit --", ""));

            if (!string.IsNullOrEmpty(ddlSociety.SelectedValue))
                BindBuildings(Convert.ToInt64(ddlSociety.SelectedValue));
        }

        protected void ddlBuilding_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlUnit.Items.Clear();
            ddlUnit.Items.Insert(0, new ListItem("-- Select Unit --", ""));

            if (!string.IsNullOrEmpty(ddlBuilding.SelectedValue))
                BindUnits(Convert.ToInt64(ddlBuilding.SelectedValue));
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtMobile.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string occupancyType = ddlOccupancyType.SelectedValue;

            if (string.IsNullOrEmpty(ddlSociety.SelectedValue) ||
                string.IsNullOrEmpty(ddlBuilding.SelectedValue) ||
                string.IsNullOrEmpty(ddlUnit.SelectedValue))
            {
                pnlError.Visible = true;
                lblError.Text = "⚠️ Please select Society, Building, and Unit.";
                return;
            }

            long societyId = Convert.ToInt64(ddlSociety.SelectedValue);
            long buildingId = Convert.ToInt64(ddlBuilding.SelectedValue);
            string unitNo = ddlUnit.SelectedValue;
            int roleId = 2;

            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    SqlCommand cmd = new SqlCommand("sp_RegisterUser", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@full_name", fullName);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password_hash", password);
                    cmd.Parameters.AddWithValue("@society_id", societyId);
                    cmd.Parameters.AddWithValue("@role_id", roleId);
                    cmd.Parameters.AddWithValue("@building_id", buildingId);
                    cmd.Parameters.AddWithValue("@unit_no", unitNo);
                    cmd.Parameters.AddWithValue("@occupancy_type", occupancyType);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                pnlSuccess.Visible = true;
                lblSuccess.Text = "🎉 Registration successful!";
                pnlError.Visible = false;
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "❌ Error: " + ex.Message;
                pnlSuccess.Visible = false;
            }
        }
    }
}
