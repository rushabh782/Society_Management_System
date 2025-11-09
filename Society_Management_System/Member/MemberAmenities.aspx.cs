using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Society_Management_System.Member
{
    public partial class MemberAmenities : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadAmenities();
        }

        private void LoadAmenities()
        {
            string connStr = ConfigurationManager.ConnectionStrings["societyDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT a.amenity_id, a.name, a.booking_required, s.name AS society_name
                    FROM amenities a
                    INNER JOIN societies s ON a.society_id = s.society_id
                    ORDER BY a.amenity_id DESC", con);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptAmenities.DataSource = dt;
                rptAmenities.DataBind();
            }
        }
    }
}
