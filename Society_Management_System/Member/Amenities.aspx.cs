using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Society_Management_System.Member
{
    public partial class Amenities : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["societyDB"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadAmenities();
                LoadMyBookings();
            }
        }

        private void LoadAmenities()
        {
            long memberId = Convert.ToInt64(Session["member_id"]);

            // Fetch society_id of the logged-in member
            SqlCommand cmdSociety = new SqlCommand("SELECT society_id FROM members WHERE member_id = @member_id", con);
            cmdSociety.Parameters.AddWithValue("@member_id", memberId);
            con.Open();
            long societyId = Convert.ToInt64(cmdSociety.ExecuteScalar());
            con.Close();

            // Fetch amenities of that society
            SqlCommand cmd = new SqlCommand("SELECT * FROM amenities WHERE society_id = @society_id", con);
            cmd.Parameters.AddWithValue("@society_id", societyId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            gvAmenities.DataSource = dt;
            gvAmenities.DataBind();
        }

        protected void btnBook_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int amenityId = Convert.ToInt32(btn.CommandArgument);
            Response.Redirect("BookAmenity.aspx?amenity_id=" + amenityId);
        }

        private void LoadMyBookings()
        {
            long userId = Convert.ToInt64(Session["user_id"]);
            SqlCommand cmd = new SqlCommand(@"
                SELECT ab.booking_id, a.name AS amenity_name, ab.start_time, ab.end_time, ab.status
                FROM amenity_bookings ab
                JOIN amenities a ON ab.amenity_id = a.amenity_id
                WHERE ab.user_id = @user_id
                ORDER BY ab.start_time DESC", con);
            cmd.Parameters.AddWithValue("@user_id", userId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            gvBookings.DataSource = dt;
            gvBookings.DataBind();
        }
    }
}
