using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLyBanHangCafe
{
    public partial class FrmSanPham : Form
    {
        private string connectionString = "Data Source=DESKTOP-RTCMKU1;Initial Catalog=QuanLyBanHangCafe;Integrated Security=True";
        public FrmSanPham()
        {
            InitializeComponent();
        }

        private void FrmSanPham_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM SanPham";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvDanhSach.DataSource = dt;
            }
        }

        private void btnThemMoi_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO SanPham (TenSanPham, GiaBan, LoaiSanPham) VALUES (@TenSanPham, @GiaBan, @LoaiSanPham)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TenSanPham", txtTenSanPham.Text);
                cmd.Parameters.AddWithValue("@GiaBan", Convert.ToDecimal(txtGia.Text));
                cmd.Parameters.AddWithValue("@LoaiSanPham", txtLoaiSanPham.Text);
                cmd.ExecuteNonQuery();
            }
            LoadData();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE SanPham SET TenSanPham = @TenSanPham, GiaBan = @GiaBan, LoaiSanPham = @LoaiSanPham WHERE MaSanPham = @MaSanPham";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaSanPham", txtMaSanPham.Text);
                cmd.Parameters.AddWithValue("@TenSanPham", txtTenSanPham.Text);
                cmd.Parameters.AddWithValue("@GiaBan", Convert.ToDecimal(txtGia.Text));
                cmd.Parameters.AddWithValue("@LoaiSanPham", txtLoaiSanPham.Text);
                cmd.ExecuteNonQuery();
            }
            LoadData();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM SanPham WHERE MaSanPham = @MaSanPham";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaSanPham", txtMaSanPham.Text);
                cmd.ExecuteNonQuery();
            }
            LoadData();
        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM SanPham WHERE TenSanPham LIKE @TenSanPham";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@TenSanPham", "%" + txtTenSanPham.Text + "%");
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvDanhSach.DataSource = dt;
            }
        }

        private void dgvDanhSach_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvDanhSach.Rows[e.RowIndex];
                txtMaSanPham.Text = row.Cells["MaSanPham"].Value.ToString();
                txtTenSanPham.Text = row.Cells["TenSanPham"].Value.ToString();
                txtGia.Text = row.Cells["GiaBan"].Value.ToString();
                txtLoaiSanPham.Text = row.Cells["LoaiSanPham"].Value.ToString();
            }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtMaSanPham.Clear();
            txtTenSanPham.Clear();
            txtGia.Clear();
            txtLoaiSanPham.Clear();
            LoadData();
        }
    }
}
