using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace Society_Management_System.Member
{
    public partial class Announcements : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["societyDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // require login
            if (Session["user_id"] == null)
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadAnnouncements();
            }
        }

        private void LoadAnnouncements()
        {
            long societyId = 0;

            // 1) Try session first
            if (Session["society_id"] != null)
            {
                long.TryParse(Session["society_id"].ToString(), out societyId);
            }

            // 2) If not in session, try to fetch from users table using UserID
            if (societyId == 0 && Session["user_id"] != null)
            {
                long userId;
                if (long.TryParse(Session["user_id"].ToString(), out userId))
                {
                    using (SqlConnection con = new SqlConnection(connStr))
                    {
                        string q = "SELECT society_id FROM users WHERE user_id = @uid";
                        using (SqlCommand cmd = new SqlCommand(q, con))
                        {
                            cmd.Parameters.AddWithValue("@uid", userId);
                            con.Open();
                            object o = cmd.ExecuteScalar();
                            con.Close();
                            if (o != null && o != DBNull.Value)
                                long.TryParse(o.ToString(), out societyId);
                        }
                    }
                }
            }

            if (societyId == 0)
            {
                // If we still don't have society id, show no data (or you can redirect)
                pnlAnnouncements.Visible = false;
                pnlEmpty.Visible = true;
                return;
            }

            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT announcement_id, title, content, visible_from, visible_to
                    FROM announcements
                    WHERE society_id = @sid
                      AND visible_from <= CAST(GETDATE() AS DATE)
                      AND (visible_to IS NULL OR visible_to >= CAST(GETDATE() AS DATE))
                    ORDER BY visible_from DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@sid", societyId);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                pnlAnnouncements.Visible = true;
                pnlEmpty.Visible = false;
                rptAnnouncements.DataSource = dt;
                rptAnnouncements.DataBind();
            }
            else
            {
                pnlAnnouncements.Visible = false;
                pnlEmpty.Visible = true;
            }
        }

        protected void rptAnnouncements_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView drv = (DataRowView)e.Item.DataItem;
                string rawContent = drv["content"] == DBNull.Value ? "" : drv["content"].ToString();

                // HTML-encode for safety and preserve line breaks
                string safe = HttpUtility.HtmlEncode(rawContent).Replace("\r\n", "<br/>").Replace("\n", "<br/>");

                Literal lit = (Literal)e.Item.FindControl("litContent");
                if (lit != null) lit.Text = safe;
            }
        }
    }
}