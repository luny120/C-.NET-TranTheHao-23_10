using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace Bai2
{
    public partial class Form1 : Form
    {
        private BindingList<BookModel> _bookList = new BindingList<BookModel>();

        private BindingSource _bindingSource = new BindingSource();

        private string _selectedCoverPath = string.Empty;

        private Timer _clockTimer = new Timer();

        public Form1()
        {
            InitializeComponent();
            SetupApp();
            InitSampleData();
            UpdateStatusStrip();
        }

        #region Khởi tạo Giao diện & Cấu hình DataGridView bằng Code
        private void SetupApp()
        {
            this.Text = "Hệ Thống Quản Lý Tài Liệu Sách & Xuất Báo Cáo - Book Manager";
            this.StartPosition = FormStartPosition.CenterScreen;

            cboCategory.Items.Clear();
            cboCategory.Items.AddRange(new object[] { "Giáo trình", "Khoa học công nghệ", "Kinh tế - Quản trị", "Văn học nghệ thuật", "Kỹ năng sống" });
            cboCategory.SelectedIndex = 0;

            NumericUpDown nudP = GetPriceNumericUpDown();
            if (nudP != null)
            {
                nudP.Minimum = 0;
                nudP.Maximum = 1000000000;
                nudP.ThousandsSeparator = true;
                nudP.Increment = 5000;
                nudP.Value = 50000;
            }

            nudQuantity.Minimum = 1;
            nudQuantity.Maximum = 10000;
            nudQuantity.Value = 1;

            picBookCover.SizeMode = PictureBoxSizeMode.Zoom;
            picBookCover.BorderStyle = BorderStyle.FixedSingle;

            _clockTimer.Interval = 1000;
            _clockTimer.Tick += (s, e) =>
            {
                lblCurrentTime.Text = $"Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            };
            _clockTimer.Start();

            dgvBooks.AutoGenerateColumns = false;
            dgvBooks.RowHeadersVisible = false;
            dgvBooks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBooks.MultiSelect = false;
            dgvBooks.ReadOnly = true;
            dgvBooks.AllowUserToAddRows = false;
            dgvBooks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvBooks.Columns.Clear();
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(BookModel.BookId), HeaderText = "Mã Sách", FillWeight = 65 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(BookModel.Title), HeaderText = "Tên Sách", FillWeight = 140 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(BookModel.Author), HeaderText = "Tác Giả", FillWeight = 100 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(BookModel.Category), HeaderText = "Thể Loại", FillWeight = 90 });

            var colPrice = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BookModel.Price),
                HeaderText = "Đơn Giá",
                FillWeight = 80
            };
            colPrice.DefaultCellStyle.Format = "N0";
            colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvBooks.Columns.Add(colPrice);

            var colQty = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BookModel.Quantity),
                HeaderText = "SL",
                FillWeight = 45
            };
            colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvBooks.Columns.Add(colQty);

            var colTotal = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BookModel.TotalAmount),
                HeaderText = "Thành Tiền",
                FillWeight = 90
            };
            colTotal.DefaultCellStyle.Format = "N0";
            colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvBooks.Columns.Add(colTotal);

            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(BookModel.ShortCoverFileName), HeaderText = "Tên File Ảnh", FillWeight = 90 });

            _bindingSource.DataSource = _bookList;
            dgvBooks.DataSource = _bindingSource;

            dgvBooks.SelectionChanged += DgvBooks_SelectionChanged;

            _bookList.ListChanged += (s, e) => UpdateStatusStrip();
        }

        private NumericUpDown GetPriceNumericUpDown()
        {
            Control[] c = this.Controls.Find("nudPrice", true);
            if (c.Length > 0 && c[0] is NumericUpDown n1) return n1;

            c = this.Controls.Find("numericUpDown1", true);
            if (c.Length > 0 && c[0] is NumericUpDown n2) return n2;

            return null;
        }

        private void InitSampleData()
        {
            _bookList.Add(new BookModel
            {
                BookId = "BK001",
                Title = "Lập trình C# từ cơ bản đến nâng cao",
                Author = "Phạm Quang Huy",
                Category = "Khoa học công nghệ",
                Price = 120000,
                Quantity = 25,
                CoverImagePath = ""
            });

            _bookList.Add(new BookModel
            {
                BookId = "BK002",
                Title = "Thiết kế kiến trúc hướng dịch vụ",
                Author = "Nguyễn Văn Tuấn",
                Category = "Khoa học công nghệ",
                Price = 185000,
                Quantity = 15,
                CoverImagePath = ""
            });

            _bookList.Add(new BookModel
            {
                BookId = "BK003",
                Title = "Đắc Nhân Tâm",
                Author = "Dale Carnegie",
                Category = "Kỹ năng sống",
                Price = 85000,
                Quantity = 50,
                CoverImagePath = ""
            });
        }
        #endregion

        #region Yêu cầu 3: Cập nhật StatusStrip tự động
        private void UpdateStatusStrip()
        {
            int totalBooks = _bookList.Count;
            decimal totalValue = _bookList.Sum(b => b.TotalAmount);

            lblTotalBooks.Text = $"Tổng số đầu sách: {totalBooks}";
            lblTotalInventoryValue.Text = $"Tổng giá trị: {totalValue:N0} VNĐ";
        }
        #endregion

        #region Nạp dữ liệu khi chọn dòng & Hiển thị ảnh bìa
        private void DgvBooks_SelectionChanged(object sender, EventArgs e)
        {
            if (_bindingSource.Current is BookModel current)
            {
                txtBookId.Text = current.BookId;
                txtTitle.Text = current.Title;
                txtAuthor.Text = current.Author;
                cboCategory.SelectedItem = current.Category;

                NumericUpDown nudP = GetPriceNumericUpDown();
                if (nudP != null) nudP.Value = current.Price;

                nudQuantity.Value = current.Quantity;
                _selectedCoverPath = current.CoverImagePath;

                DisplayCoverImage(_selectedCoverPath);
                errorProvider1?.Clear();
            }
        }

        private void DisplayCoverImage(string path)
        {
            if (picBookCover.Image != null)
            {
                picBookCover.Image.Dispose();
                picBookCover.Image = null;
            }

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        picBookCover.Image = Image.FromStream(ms);
                    }
                }
                catch
                {
                    picBookCover.Image = null;
                }
            }
        }
        #endregion

        #region Yêu cầu 1: Thao tác Ảnh bìa (OpenFileDialog)
        private void btnUploadCover_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn ảnh bìa sách";
                ofd.Filter = "File Ảnh (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _selectedCoverPath = ofd.FileName;
                    DisplayCoverImage(_selectedCoverPath);
                }
            }
        }

        private void btnRemoveCover_Click(object sender, EventArgs e)
        {
            _selectedCoverPath = string.Empty;
            DisplayCoverImage(null);
        }
        #endregion

        #region Kiểm tra dữ liệu (Validation & ErrorProvider)
        private bool ValidateBookData(bool isAddingNew)
        {
            bool isValid = true;
            errorProvider1?.Clear();

            string id = txtBookId.Text.Trim();
            string title = txtTitle.Text.Trim();

            if (string.IsNullOrEmpty(id))
            {
                errorProvider1?.SetError(txtBookId, "Mã sách không được để trống!");
                isValid = false;
            }
            else if (isAddingNew && _bookList.Any(b => b.BookId.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {
                errorProvider1?.SetError(txtBookId, "Mã sách đã tồn tại!");
                isValid = false;
            }

            if (string.IsNullOrEmpty(title))
            {
                errorProvider1?.SetError(txtTitle, "Tên sách không được để trống!");
                isValid = false;
            }

            return isValid;
        }
        #endregion

        #region Thao tác CRUD (Thêm, Sửa, Xóa, Làm mới)
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateBookData(isAddingNew: true)) return;

            NumericUpDown nudP = GetPriceNumericUpDown();
            decimal price = nudP != null ? nudP.Value : 0;

            var newBook = new BookModel
            {
                BookId = txtBookId.Text.Trim().ToUpper(),
                Title = txtTitle.Text.Trim(),
                Author = txtAuthor.Text.Trim(),
                Category = cboCategory.SelectedItem?.ToString() ?? "Khác",
                Price = price,
                Quantity = (int)nudQuantity.Value,
                CoverImagePath = _selectedCoverPath
            };

            _bookList.Add(newBook);
            _bindingSource.Position = _bookList.IndexOf(newBook);

            MessageBox.Show("Thêm sách mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_bindingSource.Current is BookModel current)
            {
                if (!ValidateBookData(isAddingNew: false)) return;

                NumericUpDown nudP = GetPriceNumericUpDown();

                current.Title = txtTitle.Text.Trim();
                current.Author = txtAuthor.Text.Trim();
                current.Category = cboCategory.SelectedItem?.ToString() ?? "Khác";
                if (nudP != null) current.Price = nudP.Value;
                current.Quantity = (int)nudQuantity.Value;
                current.CoverImagePath = _selectedCoverPath;

                _bindingSource.ResetCurrentItem();
                UpdateStatusStrip();

                MessageBox.Show("Cập nhật thông tin sách thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một cuốn sách từ bảng để sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_bindingSource.Current is BookModel current)
            {
                var confirm = MessageBox.Show($"Bạn có chắc muốn xóa cuốn sách [{current.BookId}] - {current.Title}?",
                    "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm == DialogResult.Yes)
                {
                    _bookList.Remove(current);
                    btnClear_Click(sender, e);
                    MessageBox.Show("Đã xóa sách thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một cuốn sách để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtBookId.Text = "BK" + DateTime.Now.ToString("HHmmss");
            txtTitle.Clear();
            txtAuthor.Clear();
            cboCategory.SelectedIndex = 0;

            NumericUpDown nudP = GetPriceNumericUpDown();
            if (nudP != null) nudP.Value = 50000;

            nudQuantity.Value = 1;
            _selectedCoverPath = string.Empty;
            DisplayCoverImage(null);
            errorProvider1?.Clear();
            txtTitle.Focus();
        }
        #endregion

        #region Yêu cầu 2: Xuất / Mở file CSV (SaveFileDialog & OpenFileDialog)
        private void ExportCsv()
        {
            if (_bookList.Count == 0)
            {
                MessageBox.Show("Danh sách sách hiện đang trống, không thể xuất file!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Lưu danh sách sách ra file CSV";
                sfd.Filter = "CSV Files (*.csv)|*.csv";
                sfd.FileName = $"DanhSachSach_{DateTime.Now:yyyyMMdd_HHmm}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Mã Sách,Tên Sách,Tác Giả,Thể Loại,Đơn Giá,Số Lượng,Đường Dẫn Ảnh Bìa");

                        foreach (var b in _bookList)
                        {
                            sb.AppendLine($"{EscapeCsv(b.BookId)}," +
                                          $"{EscapeCsv(b.Title)}," +
                                          $"{EscapeCsv(b.Author)}," +
                                          $"{EscapeCsv(b.Category)}," +
                                          $"{b.Price}," +
                                          $"{b.Quantity}," +
                                          $"{EscapeCsv(b.CoverImagePath)}");
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show($"Đã xuất thành công {_bookList.Count} đầu sách ra file:\n{sfd.FileName}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi ghi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ImportCsv()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn file CSV chứa danh sách sách";
                ofd.Filter = "CSV Files (*.csv)|*.csv";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
                        if (lines.Length <= 1)
                        {
                            MessageBox.Show("File CSV không có dữ liệu hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        _bookList.Clear();

                        // Bỏ qua dòng tiêu đề đầu tiên
                        for (int i = 1; i < lines.Length; i++)
                        {
                            string line = lines[i].Trim();
                            if (string.IsNullOrEmpty(line)) continue;

                            string[] parts = ParseCsvLine(line);
                            if (parts.Length >= 6)
                            {
                                var book = new BookModel
                                {
                                    BookId = parts[0],
                                    Title = parts[1],
                                    Author = parts[2],
                                    Category = parts[3],
                                    Price = decimal.TryParse(parts[4], out decimal p) ? p : 0,
                                    Quantity = int.TryParse(parts[5], out int q) ? q : 0,
                                    CoverImagePath = parts.Length > 6 ? parts[6] : ""
                                };
                                _bookList.Add(book);
                            }
                        }

                        MessageBox.Show($"Đã nạp thành công {_bookList.Count} cuốn sách từ file CSV!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi đọc file CSV: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new System.Collections.Generic.List<string>();
            bool inQuotes = false;
            var currentField = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        currentField.Append('\"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }
            result.Add(currentField.ToString());
            return result.ToArray();
        }
        #endregion

        #region Các sự kiện MenuStrip & ToolStrip
        private void menuOpenFile_Click(object sender, EventArgs e) => ImportCsv();
        private void tsbOpen_Click(object sender, EventArgs e) => ImportCsv();

        private void menuSaveFile_Click(object sender, EventArgs e) => ExportCsv();
        private void tsbSave_Click(object sender, EventArgs e) => ExportCsv();

        private void menuAdd_Click(object sender, EventArgs e) => btnAdd_Click(sender, e);
        private void tsbAdd_Click(object sender, EventArgs e) => btnAdd_Click(sender, e);

        private void menuDelete_Click(object sender, EventArgs e) => btnDelete_Click(sender, e);

        private void menuSearch_Click(object sender, EventArgs e)
        {
            txtTitle.Focus();
            txtTitle.SelectAll();
        }

        private void menuExit_Click(object sender, EventArgs e) => Application.Exit();
        #endregion

        #region Các sự kiện giao diện phụ
        private void numericUpDown1_ValueChanged(object sender, EventArgs e) { }
        private void nudQuantity_ValueChanged(object sender, EventArgs e) { }
        private void picBookCover_Click(object sender, EventArgs e) { }
        private void txtBookId_TextChanged(object sender, EventArgs e) { }
        private void txtTitle_TextChanged(object sender, EventArgs e) { }
        private void txtAuthor_TextChanged(object sender, EventArgs e) { }
        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e) { }
        private void dgvBooks_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void fileToolStripMenuItem_Click(object sender, EventArgs e) { }
        private void editToolStripMenuItem_Click(object sender, EventArgs e) { }
        private void lblTotalBooks_Click(object sender, EventArgs e) { }
        private void lblTotalInventoryValue_Click(object sender, EventArgs e) { }
        private void lblCurrentTime_Click(object sender, EventArgs e) { }
        #endregion
    }

    #region Model Class: BookModel
    public class BookModel : INotifyPropertyChanged
    {
        private string _bookId;
        private string _title;
        private string _author;
        private string _category;
        private decimal _price;
        private int _quantity;
        private string _coverImagePath;

        public string BookId
        {
            get => _bookId;
            set { _bookId = value; OnPropertyChanged(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string Author
        {
            get => _author;
            set { _author = value; OnPropertyChanged(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public decimal TotalAmount => Price * Quantity;

        public string CoverImagePath
        {
            get => _coverImagePath;
            set
            {
                _coverImagePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShortCoverFileName));
            }
        }

        public string ShortCoverFileName => string.IsNullOrEmpty(CoverImagePath) ? "(Chưa có ảnh)" : Path.GetFileName(CoverImagePath);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    #endregion
}