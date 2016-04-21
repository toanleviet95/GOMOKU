namespace Gomoku.Models
{
    using Gomoku.Properties;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    
    /// <summary>
    /// Nguồn tham khảo: Lê Bá Khánh Trình - ĐH Sư Phạm Kỹ Thuật
    /// </summary>
    public class AI
    {
        public static int BoardSize { get; set; }   // Kích thước bàn cờ
        public List<OCo> CacNuocDaDi { get; set; }  // Các nước đã đánh
        public List<OCo> CacNuocUndo { get; set; }  // Các nước đã Undo - Sử dụng để redo trở lại

        private static int[] MangDiemTanCong = new int[5] { 0, 2, 18, 162, 1458 };  // Bảng điểm tấn công
        private static int[] MangDiemPhongNgu = new int[5] { 0, 1, 9, 81, 729 };    // Bảng điểm phòng ngự

        private static BangDiem BangDiem;   // Bảng điểm của tất cả các ô


        private static int maxDepth = 12;
        private static int maxMove = 3;
        private static int depth = 0;
        private static bool fWin = false;


        private static OCo[] PCMove = new OCo[maxMove + 2];
        private static OCo[] HumanMove = new OCo[maxMove + 2];
        private static OCo[] WinMove = new OCo[maxDepth + 2];
        private static OCo[] LoseMove = new OCo[maxDepth + 2];

        /// <summary>
        /// Khởi tạo thể hiện mới cho class AI
        /// </summary>
        public AI()
        {
            BoardSize = Settings.Default.BOARD_SIZE;
            BangDiem = new BangDiem();
            CacNuocDaDi = new List<OCo>();
            CacNuocUndo = new List<OCo>();
        }

        public void Reset()
        {
            BangDiem.ResetDiem();
            CacNuocDaDi.Clear();
            CacNuocUndo.Clear();
        }

        /// <summary>
        /// Khởi động tìm ô đánh tối ưu nhất và trả về
        /// </summary>
        public OCo KhoiDongComputer()
        {
            OCo OCoSeDanh = new OCo();

            // Nước đánh đầu tiên
            if (CacNuocDaDi.Count == 0)
            {
                OCoSeDanh.Row = BoardSize / 2;
                OCoSeDanh.Col = BoardSize / 2;
            }
            else
            {
                //Reset
                for (int i = 0; i < maxMove; i++)
                {
                    WinMove[i] = new OCo();
                    PCMove[i] = new OCo();
                    HumanMove[i] = new OCo();
                }

                depth = 0;

                // Tìm nước đi
                TimKiemNuocDi();

                // Kiểm tra và lưu lại nước đi tối ưu vào OCoSeDanh
                if (fWin)   // nước đi => chiến thắng
                {
                    OCoSeDanh.Row = WinMove[0].Row;
                    OCoSeDanh.Col = WinMove[0].Col;
                    OCoSeDanh.OfPlayer = WinMove[0].OfPlayer;
                }
                else        // chưa thể => chiến thắng
                {
                    // Duyệt và lưu điểm cho các ô cờ trong BangDiem
                    EvalChessBoard(OCo.CellValues.Player2, ref BangDiem);
                    OCo temp = new OCo();
                    // Lấy ô cờ có điểm cao nhất
                    temp = BangDiem.MaxPos();
                    OCoSeDanh.Row = temp.Row;
                    OCoSeDanh.Col = temp.Col;
                    OCoSeDanh.OfPlayer = temp.OfPlayer;
                }
            }

            return OCoSeDanh;
        }

        private void TimKiemNuocDi()
        {
            if (depth > maxDepth)
            {
                return;
            }

            depth++;

            fWin = false;
            bool fLose = false;

            // Reset
            OCo pcMove = new OCo();
            OCo humanMove = new OCo();

            int countMove = 0;

            // Duyệt các ô cờ, tính và lưu điểm
            EvalChessBoard(OCo.CellValues.Player2, ref BangDiem);

            //Lay ra MaxMove buoc di co diem cao nhat
            OCo temp = new OCo();
            for (int i = 0; i < maxMove; i++)
            {
                temp = BangDiem.MaxPos();
                PCMove[i] = temp;
                BangDiem.Diem[temp.Row, temp.Col] = 0;
            }

            //Lay nuoc di trong PCMove[] ra danh thu
            countMove = 0;
            while (countMove < maxMove)
            {
                pcMove = PCMove[countMove++];
                BanCo.Cells[pcMove.Row, pcMove.Col] = OCo.CellValues.Player2;
                WinMove.SetValue(pcMove, depth - 1);

                //Tim cac nuoc di toi uu cua nguoi
                BangDiem.ResetDiem();
                EvalChessBoard(OCo.CellValues.Player1, ref BangDiem);
                //Lay ra maxMove nuoc di co diem cao nhat cua nguoi
                for (int i = 0; i < maxMove; i++)
                {
                    temp = BangDiem.MaxPos();
                    HumanMove[i] = temp;
                    BangDiem.Diem[temp.Row, temp.Col] = 0;
                }
                //Danh thu cac nuoc di
                for (int i = 0; i < maxMove; i++)
                {
                    humanMove = HumanMove[i];
                    BanCo.Cells[humanMove.Row, humanMove.Col] = OCo.CellValues.Player1;
                   
                    if (fLose)
                    {
                        BanCo.Cells[pcMove.Row, pcMove.Col] = OCo.CellValues.None;
                        BanCo.Cells[humanMove.Row, humanMove.Col] = OCo.CellValues.None;
                        break;
                    }

                    if (fWin)
                    {
                        BanCo.Cells[pcMove.Row, pcMove.Col] = 0;
                        BanCo.Cells[humanMove.Row, humanMove.Col] = 0;
                        return;
                    }

                    TimKiemNuocDi();
                    BanCo.Cells[humanMove.Row, humanMove.Col] = OCo.CellValues.None;
                }
                BanCo.Cells[pcMove.Row, pcMove.Col] = OCo.CellValues.None;
            }
        }

        #region  Duyệt bảng điểm
        private void EvalChessBoard(OCo.CellValues player, ref BangDiem eBoard)
        {
            int rw, cl, ePC, eHuman;
            eBoard.ResetDiem();

            // Duyệt theo dòng
            for (rw = 0; rw < BoardSize; rw++)
                for (cl = 0; cl < BoardSize - 4; cl++)
                {
                    ePC = 0; eHuman = 0;
                    // Duyệt 5 ô liên tiếp nhau
                    for (int i = 0; i < 5; i++)
                    {
                        if (BanCo.Cells[rw, cl + i] == OCo.CellValues.Player1) 
                            eHuman++;
                        if (BanCo.Cells[rw, cl + i] == OCo.CellValues.Player2) 
                            ePC++;
                    }

                    if (eHuman * ePC == 0 && eHuman != ePC)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (BanCo.Cells[rw, cl + i] == OCo.CellValues.None) // Nếu ô chưa được đánh
                            {
                                if (eHuman == 0)
                                {
                                    if (player == OCo.CellValues.Player1)
                                        BangDiem.Diem[rw, cl + i] += MangDiemPhongNgu[ePC];
                                    else
                                        BangDiem.Diem[rw, cl + i] += MangDiemTanCong[ePC];
                                }
                                if (ePC == 0)
                                {
                                    if (player == OCo.CellValues.Player2)
                                        BangDiem.Diem[rw, cl + i] += MangDiemPhongNgu[eHuman];
                                    else
                                        BangDiem.Diem[rw, cl + i] += MangDiemTanCong[eHuman];
                                }
                                if (eHuman == 4 || ePC == 4)
                                {
                                    BangDiem.Diem[rw, cl + i] *= 2;
                                }
                            }
                        }

                    }
                }

            // Duyệt theo cột
            for (cl = 0; cl < BoardSize; cl++)
                for (rw = 0; rw < BoardSize - 4; rw++)
                {
                    ePC = 0; eHuman = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (BanCo.Cells[rw + i, cl] == OCo.CellValues.Player1) 
                            eHuman++;
                        if (BanCo.Cells[rw + i, cl] == OCo.CellValues.Player2) 
                            ePC++;
                    }

                    if (eHuman * ePC == 0 && eHuman != ePC)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (BanCo.Cells[rw + i, cl] == OCo.CellValues.None) // Nếu ô chưa được đánh
                            {
                                if (eHuman == 0)
                                    if (player == OCo.CellValues.Player1)
                                        BangDiem.Diem[rw + i, cl] += MangDiemPhongNgu[ePC];
                                    else
                                        BangDiem.Diem[rw + i, cl] += MangDiemTanCong[ePC];
                                if (ePC == 0)
                                    if (player == OCo.CellValues.Player2)
                                        BangDiem.Diem[rw + i, cl] += MangDiemPhongNgu[eHuman];
                                    else
                                        BangDiem.Diem[rw + i, cl] += MangDiemTanCong[eHuman];
                                if (eHuman == 4 || ePC == 4)
                                    BangDiem.Diem[rw + i, cl] *= 2;
                            }
                        }

                    }
                }

            // Duyệt chéo xuống
            for (cl = 0; cl < BoardSize - 4; cl++)
                for (rw = 0; rw < BoardSize - 4; rw++)
                {
                    ePC = 0; eHuman = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (BanCo.Cells[rw + i, cl + i] == OCo.CellValues.Player1) 
                            eHuman++;
                        if (BanCo.Cells[rw + i, cl + i] == OCo.CellValues.Player2) 
                            ePC++;
                    }

                    if (eHuman * ePC == 0 && eHuman != ePC)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (BanCo.Cells[rw + i, cl + i] ==  OCo.CellValues.None) // Nếu ô chưa được đánh
                            {
                                if (eHuman == 0)
                                    if (player == OCo.CellValues.Player1)
                                        BangDiem.Diem[rw + i, cl + i] += MangDiemPhongNgu[ePC];
                                    else
                                        BangDiem.Diem[rw + i, cl + i] += MangDiemTanCong[ePC];
                                if (ePC == 0)
                                    if (player == OCo.CellValues.Player2)
                                        BangDiem.Diem[rw + i, cl + i] += MangDiemPhongNgu[eHuman];
                                    else
                                        BangDiem.Diem[rw + i, cl + i] += MangDiemTanCong[eHuman];
                                if (eHuman == 4 || ePC == 4)
                                    BangDiem.Diem[rw + i, cl + i] *= 2;
                            }
                        }

                    }
                }

            // Duyệt chéo lên
            for (rw = 4; rw < BoardSize; rw++)
                for (cl = 0; cl < BoardSize - 4; cl++)
                {
                    ePC = 0; eHuman = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (BanCo.Cells[rw - i, cl + i] == OCo.CellValues.Player1) 
                            eHuman++;
                        if (BanCo.Cells[rw - i, cl + i] == OCo.CellValues.Player2) 
                            ePC++;
                    }

                    if (eHuman * ePC == 0 && eHuman != ePC)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (BanCo.Cells[rw - i, cl + i] == OCo.CellValues.None) // Nếu ô chưa được đánh
                            {
                                if (eHuman == 0)
                                    if (player == OCo.CellValues.Player1)
                                        BangDiem.Diem[rw - i, cl + i] += MangDiemPhongNgu[ePC];
                                    else 
                                        BangDiem.Diem[rw - i, cl + i] += MangDiemTanCong[ePC];
                                if (ePC == 0)
                                    if (player == OCo.CellValues.Player2)
                                        BangDiem.Diem[rw - i, cl + i] += MangDiemPhongNgu[eHuman];
                                    else
                                        BangDiem.Diem[rw - i, cl + i] += MangDiemTanCong[eHuman];
                                if (eHuman == 4 || ePC == 4)
                                    BangDiem.Diem[rw - i, cl + i] *= 2;
                            }
                        }
                    }
                }
        }

        #endregion
    }
}
