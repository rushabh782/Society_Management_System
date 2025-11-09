using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Society_Management_System.Admin
{
    public partial class ManageAmenities : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["societyDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSocieties();
                LoadAmenities();
            }
        }

        private void LoadSocieties()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT society_id, name FROM societies", con);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                ddlSociety.DataSource = rdr;
                ddlSociety.DataTextField = "name";
                ddlSociety.DataValueField = "society_id";
                ddlSociety.DataBind();
                ddlSociety.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Select Society --", ""));
                con.Close();
            }
        }

        private void LoadAmenities()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"SELECT a.amenity_id, s.name AS society_name, a.name, a.booking_required
                                 FROM amenities a
                                 INNER JOIN societies s ON a.society_id = s.society_id";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvAmenities.DataSource = dt;
                gvAmenities.DataBind();
            }
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if (ddlSociety.SelectedValue == "")
                return;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "INSERT INTO amenities (society_id, name, booking_required) VALUES (@society_id, @name, @booking_required)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@society_id", ddlSociety.SelectedValue);
                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@booking_required", chkBookingRequired.Checked);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            ClearForm();
            LoadAmenities();
        }

        protected void gvAmenities_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditAmenity")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                LoadAmenityForEdit(id);
            }
            else if (e.CommandName == "DeleteAmenity")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                DeleteAmenity(id);
            }
        }

        private void LoadAmenityForEdit(int amenityId)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM amenities WHERE amenity_id=@id", con);
                cmd.Parameters.AddWithValue("@id", amenityId);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    ddlSociety.SelectedValue = rdr["society_id"].ToString();
                    txtName.Text = rdr["name"].ToString();
                    chkBookingRequired.Checked = Convert.ToBoolean(rdr["booking_required"]);

                    ViewState["AmenityID"] = amenityId;
                    btnAdd.Visible = false;
                    btnUpdate.Visible = true;
                    btnCancel.Visible = true;
                }
                con.Close();
            }
        }

        private void DeleteAmenity(int id)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM amenities WHERE amenity_id=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            LoadAmenities();
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (ViewState["AmenityID"] == null)
                return;

            int id = Convert.ToInt32(ViewState["AmenityID"]);

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"UPDATE amenities
                                 SET society_id=@society_id, name=@name, booking_required=@booking_required
                                 WHERE amenity_id=@id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@society_id", ddlSociety.SelectedValue);
                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@booking_required", chkBookingRequired.Checked);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            ClearForm();
            LoadAmenities();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            ddlSociety.SelectedIndex = 0;
            txtName.Text = "";
            chkBookingRequired.Checked = false;
            btnAdd.Visible = true;
            btnUpdate.Visible = false;
            btnCancel.Visible = false;
            ViewState["AmenityID"] = null;
        }
    }
}
