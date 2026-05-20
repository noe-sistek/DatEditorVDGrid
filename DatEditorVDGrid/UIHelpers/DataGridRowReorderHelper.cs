using System.Drawing;
using System.Windows.Forms;

namespace DatEditorVDGrid.UIHelpers
{
    internal static class DataGridRowReorderHelper
    {
        private static int _dragRowIndex = -1;
        private static Rectangle _dragBox = Rectangle.Empty;

        public static void Attach(DataGridView dgv)
        {
            dgv.AllowDrop = true;
            dgv.MouseDown += Dgv_MouseDown;
            dgv.MouseMove += Dgv_MouseMove;
            dgv.DragOver += Dgv_DragOver;
            dgv.DragDrop += Dgv_DragDrop;
            DisableHeaderSorting(dgv);
        }

        public static void DisableHeaderSorting(DataGridView dgv)
        {
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private static void Dgv_MouseDown(object sender, MouseEventArgs e)
        {
            if (!(sender is DataGridView dgv))
            {
                _dragRowIndex = -1;
                return;
            }

            if (e.Button != MouseButtons.Left)
            {
                _dragRowIndex = -1;
                return;
            }

            var hit = dgv.HitTest(e.X, e.Y);
            if (hit.RowIndex < 0)
            {
                _dragRowIndex = -1;
                return;
            }

            _dragRowIndex = hit.RowIndex;
            Size dragSize = SystemInformation.DragSize;
            _dragBox = new Rectangle(new Point(e.X - dragSize.Width / 2, e.Y - dragSize.Height / 2), dragSize);
        }

        private static void Dgv_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender is DataGridView dgv) || e.Button != MouseButtons.Left || _dragRowIndex < 0)
                return;

            if (_dragBox != Rectangle.Empty && !_dragBox.Contains(e.X, e.Y))
            {
                dgv.DoDragDrop(dgv.Rows[_dragRowIndex], DragDropEffects.Move);
            }
        }

        private static void Dgv_DragOver(object sender, DragEventArgs e)
        {
            if (!(sender is DataGridView dgv))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if (!e.Data.GetDataPresent(typeof(DataGridViewRow)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            Point clientPoint = dgv.PointToClient(new Point(e.X, e.Y));
            var hit = dgv.HitTest(clientPoint.X, clientPoint.Y);
            e.Effect = hit.RowIndex < 0 ? DragDropEffects.None : DragDropEffects.Move;
        }

        private static void Dgv_DragDrop(object sender, DragEventArgs e)
        {
            if (!(sender is DataGridView dgv) || !e.Data.GetDataPresent(typeof(DataGridViewRow)))
                return;

            Point clientPoint = dgv.PointToClient(new Point(e.X, e.Y));
            var hit = dgv.HitTest(clientPoint.X, clientPoint.Y);
            if (hit.RowIndex < 0)
                return;

            var dragRow = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
            if (dragRow == null)
                return;

            int targetIndex = hit.RowIndex;
            int sourceIndex = _dragRowIndex;
            if (sourceIndex < 0 || sourceIndex == targetIndex)
                return;

            if (targetIndex > dgv.Rows.Count - 1)
                targetIndex = dgv.Rows.Count - 1;

            dgv.Rows.RemoveAt(sourceIndex);
            if (targetIndex > sourceIndex)
                targetIndex--;

            dgv.Rows.Insert(targetIndex, dragRow);
            RefreshPosCells(dgv);
            _dragRowIndex = -1;
        }

        public static void RefreshPosCells(DataGridView dgv)
        {
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                dgv.Rows[i].Cells["Pos"].Value = i + 1;
            }
        }
    }
}
