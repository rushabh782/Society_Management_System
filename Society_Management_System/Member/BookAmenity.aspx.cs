using System;
using System.Configuration;
using System.Data.SqlClient;

namespace Society_Management_System.Member
{
    public partial class BookAmenity : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string amenityId = Request.QueryString["amenity_id"];
                if (!string.IsNullOrEmpty(amenityId))
                {
                    LoadAmenityDetails(Convert.ToInt64(amenityId));
                }
                else
                {
                    Response.Redirect("MemberAmenities.aspx");
                }
            }
        }

        private void LoadAmenityDetails(long amenityId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["societyDB"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                string query = "SELECT name FROM amenities WHERE amenity_id = @amenity_id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@amenity_id", amenityId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        lblAmenityName.Text = reader["name"].ToString();
                        ViewState["AmenityId"] = amenityId;
                    }
                }
            }
        }

        protected void btnBook_Click(object sender, EventArgs e)
        {
            try
            {
                if (ViewState["AmenityId"] == null)
                    throw new Exception("Amenity not found.");

                long amenityId = Convert.ToInt64(ViewState["AmenityId"]);

                DateTime startTime = Convert.ToDateTime(txtStartTime.Text);
                DateTime endTime = Convert.ToDateTime(txtEndTime.Text);

                // Replace with actual logged-in user_id
                long userId = Convert.ToInt64(Session["user_id"]);

                string connStr = ConfigurationManager.ConnectionStrings["societyDB"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();
                    string query = @"INSERT INTO amenity_bookings (amenity_id, user_id, start_time, end_time, status)
                                     VALUES (@amenity_id, @user_id, @start_time, @end_time, 'Booked')";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@amenity_id", amenityId);
                        cmd.Parameters.AddWithValue("@user_id", userId);
                        cmd.Parameters.AddWithValue("@start_time", startTime);
                        cmd.Parameters.AddWithValue("@end_time", endTime);

                        cmd.ExecuteNonQuery();
                    }
                }

                pnlMessage.CssClass = "alert alert-success";
                pnlMessage.Visible = true;
                pnlMessage.Controls.Add(new System.Web.UI.LiteralControl("✅ Booking successful!"));
            }
            catch (Exception ex)
            {
                pnlMessage.CssClass = "alert alert-error";
                pnlMessage.Visible = true;
                pnlMessage.Controls.Add(new System.Web.UI.LiteralControl("❌ " + ex.Message));
            }
        }
    }
}
