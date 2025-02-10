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
    public partial class HoaDonChiTiet : Form
    {
        private string connectionString = "Data Source=DESKTOP-RTCMKU1;Initial Catalog=QuanLyBanHangCafe;Integrated Security=True";
        private string maHoaDon;
        public HoaDonChiTiet(string maHoaDon)
        {
            InitializeComponent();
            if (string.IsNullOrEmpty(maHoaDon))
            {
                MessageBox.Show("Mã hóa đơn không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            this.maHoaDon = maHoaDon;
            txtMaHoaDon.Text = maHoaDon;

            try
            {
                LoadThongTinHoaDon(); // Load ngày lập, số bàn, trạng thái
                LoadChiTietHoaDon(); // Load danh sách sản phẩm đã thêm
                LoadSanPham(); // Load danh sách sản phẩm từ bảng SanPham
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadThongTinHoaDon()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT NgayLap, SoBan, TrangThai FROM HoaDon WHERE MaHoaDon = @MaHoaDon";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaHoaDon", maHoaDon);
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            dateTimePickerNgayLap.Value = Convert.ToDateTime(reader["NgayLap"]);
                            txtSoBan.Text = reader["SoBan"].ToString();
                            cboTrangThai.SelectedItem = reader["TrangThai"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy hóa đơn!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải thông tin hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void TinhTongTien()
        {
            try
            {
                decimal tongTien = 0;
                foreach (DataGridViewRow row in dgvDanhSach.Rows)
                {
                    if (row.Cells["SoLuong"].Value != null && row.Cells["DonGia"].Value != null)
                    {
                        int soLuong = Convert.ToInt32(row.Cells["SoLuong"].Value);
                        decimal donGia = Convert.ToDecimal(row.Cells["DonGia"].Value);
                        tongTien += soLuong * donGia;
                    }
                }
                lblTongTien.Text = $"Tổng tiền: {tongTien:N0} VND";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tính tổng tiền: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void LoadChiTietHoaDon()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT ct.MaChiTietHoaDon, sp.TenSanPham, ct.SoLuong, ct.DonGia 
                FROM ChiTietHoaDon ct
                INNER JOIN SanPham sp ON ct.MaSanPham = sp.MaSanPham
                WHERE ct.MaHoaDon = @MaHoaDon";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@MaHoaDon", maHoaDon);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvDanhSach.DataSource = dt;
                }

                TinhTongTien(); // Cập nhật tổng tiền sau khi load
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải chi tiết hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSanPham()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT TenSanPham FROM SanPham";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    cboChonSanPham.Items.Clear(); // Xóa dữ liệu cũ tránh trùng lặp

                    while (reader.Read())
                    {
                        cboChonSanPham.Items.Add(reader["TenSanPham"].ToString());
                    }

                    if (cboChonSanPham.Items.Count > 0)
                        cboChonSanPham.SelectedIndex = 0; // Chọn sản phẩm đầu tiên mặc định
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnThemSanPham_Click(object sender, EventArgs e)
        {
            if (cboChonSanPham.SelectedItem == null || string.IsNullOrWhiteSpace(txtSoLuong.Text))
            {
                MessageBox.Show("Vui lòng chọn sản phẩm và nhập số lượng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtSoLuong.Text, out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên dương!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Lấy MaSanPham và Giá Bán từ TenSanPham
                    string getMaSanPhamQuery = "SELECT MaSanPham, GiaBan FROM SanPham WHERE TenSanPham = @TenSanPham";
                    using (SqlCommand getCmd = new SqlCommand(getMaSanPhamQuery, conn))
                    {
                        getCmd.Parameters.AddWithValue("@TenSanPham", cboChonSanPham.SelectedItem.ToString());
                        SqlDataReader reader = getCmd.ExecuteReader();
                        if (reader.Read())
                        {
                            int maSanPham = Convert.ToInt32(reader["MaSanPham"]);
                            decimal donGia = Convert.ToDecimal(reader["GiaBan"]);
                            reader.Close();

                            // Chỉ chèn MaHoaDon, MaSanPham, SoLuong, DonGia (KHÔNG có ThanhTien)
                            string insertQuery = "INSERT INTO ChiTietHoaDon (MaHoaDon, MaSanPham, SoLuong, DonGia) VALUES (@MaHoaDon, @MaSanPham, @SoLuong, @DonGia)";
                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@MaHoaDon", maHoaDon);
                                insertCmd.Parameters.AddWithValue("@MaSanPham", maSanPham);
                                insertCmd.Parameters.AddWithValue("@SoLuong", soLuong);
                                insertCmd.Parameters.AddWithValue("@DonGia", donGia);
                                insertCmd.ExecuteNonQuery();
                            }

                            MessageBox.Show("Thêm sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadChiTietHoaDon(); // Load lại danh sách sau khi thêm
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy sản phẩm!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HoaDonChiTiet_Load(object sender, EventArgs e)
        {

        }

        private void btnCapNhatHoaDon_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE HoaDon SET TrangThai = @TrangThai WHERE MaHoaDon = @MaHoaDon";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaHoaDon", maHoaDon);
                cmd.Parameters.AddWithValue("@TrangThai", cboTrangThai.SelectedItem.ToString());
                cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Cập nhật hóa đơn thành công!");
        }

        private void btnXoaSanPham_Click(object sender, EventArgs e)
        {
            if (dgvDanhSach.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần xóa!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tenSanPham = dgvDanhSach.SelectedRows[0].Cells["TenSanPham"].Value.ToString();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Lấy MaSanPham từ TenSanPham
                    string getMaSanPhamQuery = "SELECT MaSanPham FROM SanPham WHERE TenSanPham = @TenSanPham";
                    using (SqlCommand getCmd = new SqlCommand(getMaSanPhamQuery, conn))
                    {
                        getCmd.Parameters.AddWithValue("@TenSanPham", tenSanPham);
                        object result = getCmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("Không tìm thấy sản phẩm!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        int maSanPham = Convert.ToInt32(result);

                        // Xóa sản phẩm khỏi ChiTietHoaDon
                        string deleteQuery = "DELETE FROM ChiTietHoaDon WHERE MaHoaDon = @MaHoaDon AND MaSanPham = @MaSanPham";
                        using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                        {
                            deleteCmd.Parameters.AddWithValue("@MaHoaDon", maHoaDon);
                            deleteCmd.Parameters.AddWithValue("@MaSanPham", maSanPham);
                            deleteCmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Xóa sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadChiTietHoaDon(); // Load lại danh sách sau khi xóa
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnInHoaDon_Click(object sender, EventArgs e)
        {
            MessageBox.Show("In hóa đơn: " + maHoaDon);
        }

        private void dgvDanhSach_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvDanhSach.Rows[e.RowIndex];
                cboChonSanPham.SelectedItem = row.Cells["TenSanPham"].Value.ToString();
                txtSoLuong.Text = row.Cells["SoLuong"].Value.ToString();
            }
        }
    }
}
