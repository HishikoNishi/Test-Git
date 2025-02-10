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
    public partial class ThemMoiHoaDon : Form
    {
        private string connectionString = "Data Source=DESKTOP-RTCMKU1;Initial Catalog=QuanLyBanHangCafe;Integrated Security=True";
        public ThemMoiHoaDon()
        {
            InitializeComponent();
            txtMaHoaDon.Text = GenerateMaHoaDon(); // Tự động tạo mã hóa đơn
            dateTimePickerNgayLap.Value = DateTime.Now; // Ngày lập mặc định hiện tại
            cboTrangThai.SelectedIndex = 0; // Mặc định chọn "Chưa thanh toán"
        }
        private string GenerateMaHoaDon()
        {
            return "HD" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSoBan.Text) || cboTrangThai.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(txtSoBan.Text, out int soBan))
            {
                MessageBox.Show("Số bàn phải là số nguyên!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO HoaDon (NgayLap, SoBan, TrangThai, TongTien) OUTPUT INSERTED.MaHoaDon VALUES (@NgayLap, @SoBan, @TrangThai, @TongTien)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NgayLap", dateTimePickerNgayLap.Value);
                        cmd.Parameters.AddWithValue("@SoBan", soBan);
                        cmd.Parameters.AddWithValue("@TrangThai", cboTrangThai.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@TongTien", 0); // Đặt mặc định là 0

                        int newMaHoaDon = (int)cmd.ExecuteScalar();
                        txtMaHoaDon.Text = newMaHoaDon.ToString();
                    }
                }

                MessageBox.Show("Hóa đơn đã được tạo thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Chuyển sang form Chi Tiết Hóa Đơn
                HoaDonChiTiet hoadonct = new HoaDonChiTiet(txtMaHoaDon.Text);
                hoadonct.ShowDialog(); // Chặn các thao tác khác đến khi form này đóng
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaHoaDon.Text) || string.IsNullOrWhiteSpace(txtSoBan.Text) || cboTrangThai.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn cần cập nhật!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtSoBan.Text, out int soBan))
            {
                MessageBox.Show("Số bàn phải là số nguyên!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE HoaDon SET NgayLap = @NgayLap, SoBan = @SoBan, TrangThai = @TrangThai WHERE MaHoaDon = @MaHoaDon";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaHoaDon", txtMaHoaDon.Text);
                        cmd.Parameters.AddWithValue("@NgayLap", dateTimePickerNgayLap.Value);
                        cmd.Parameters.AddWithValue("@SoBan", soBan);
                        cmd.Parameters.AddWithValue("@TrangThai", cboTrangThai.SelectedItem.ToString());
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Hóa đơn đã được cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ThemMoiHoaDon_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM HoaDon";

                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvDanhSach.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaHoaDon.Text))
            {
                MessageBox.Show("Vui lòng chọn hóa đơn cần xóa!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM HoaDon WHERE MaHoaDon = @MaHoaDon";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaHoaDon", txtMaHoaDon.Text);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Hóa đơn đã được xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            txtMaHoaDon.Text = GenerateMaHoaDon();
            dateTimePickerNgayLap.Value = DateTime.Now;
            txtSoBan.Clear();
            cboTrangThai.SelectedIndex = 0; // Reset về "Chưa thanh toán"
            LoadData();
        }

        private void dgvDanhSach_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvDanhSach.Rows[e.RowIndex].Cells["MaHoaDon"].Value != null)
            {
                DataGridViewRow row = dgvDanhSach.Rows[e.RowIndex];

                txtMaHoaDon.Text = row.Cells["MaHoaDon"].Value.ToString();
                dateTimePickerNgayLap.Value = Convert.ToDateTime(row.Cells["NgayLap"].Value);
                txtSoBan.Text = row.Cells["SoBan"].Value.ToString();
                cboTrangThai.SelectedItem = row.Cells["TrangThai"].Value.ToString();
            }
        }
    }
}
