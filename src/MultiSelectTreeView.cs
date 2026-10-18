using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PrototypeUltimateTool
{
    /// <summary>
    /// TreeView with Ctrl-click and Shift-range multi-selection.
    /// WinForms TreeView natively supports only one selected node, so the selection
    /// set is tracked separately and rendered by owner-drawing the node text rows.
    /// SelectedNode remains the active/focused node for existing MainForm handlers.
    /// </summary>
    public sealed class MultiSelectTreeView : TreeView
    {
        private const int WmLButtonDown = 0x0201;
        private const int WmLButtonDoubleClick = 0x0203;
        private const int WmRButtonDown = 0x0204;
        private const int WmKeyDown = 0x0100;
        private const int TvsNoHScroll = 0x8000;

        private readonly List<TreeNode> _selectedNodes = new List<TreeNode>();
        private TreeNode _selectionAnchor;
        private bool _insideSelectionUpdate;

        public event EventHandler SelectedNodesChanged;

        /// <summary>True while a Ctrl/Shift selection gesture is being applied, including native AfterSelect notifications.</summary>
        public bool IsSelectionGestureInProgress { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color AdditionalSelectionBackColor { get; set; } = Color.FromArgb(58, 82, 112);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color AdditionalSelectionForeColor { get; set; } = Color.White;

        private void RaiseSelectedNodesChanged()
        {
            EventHandler handler = this.SelectedNodesChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public IList<TreeNode> SelectedNodes
        {
            get
            {
                PruneDetachedNodes();
                return _selectedNodes.ToArray();
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                // Keep long resource names clipped on the right (with the full name still
                // available through ToolTipText) instead of allowing native horizontal
                // scrolling to shift the start of every visible label off-screen.
                parameters.Style |= TvsNoHScroll;
                return parameters;
            }
        }

        public MultiSelectTreeView()
        {
            // DoubleBuffered is exposed to derived controls; avoid reflecting into the
            // private WinForms implementation from MainForm.
            DoubleBuffered = true;
            DrawMode = TreeViewDrawMode.OwnerDrawText;
            DrawNode += DrawMultiSelectedNode;
        }

        public bool IsNodeMultiSelected(TreeNode node)
        {
            PruneDetachedNodes();
            return node != null && _selectedNodes.Contains(node);
        }

        public void SelectSingleNode(TreeNode node)
        {
            _insideSelectionUpdate = true;
            try
            {
                _selectedNodes.Clear();
                if (node != null && node.TreeView == this)
                {
                    _selectedNodes.Add(node);
                    _selectionAnchor = node;
                    if (!ReferenceEquals(SelectedNode, node)) SelectedNode = node;
                }
                else
                {
                    _selectionAnchor = null;
                    if (node == null) SelectedNode = null;
                }
            }
            finally
            {
                _insideSelectionUpdate = false;
                Invalidate();
                RaiseSelectedNodesChanged();
            }
        }

        public void SetActiveNode(TreeNode node)
        {
            if (node == null || node.TreeView != this) return;
            bool changed = !ReferenceEquals(SelectedNode, node);
            if (changed) SelectedNode = node;
            Invalidate();
            if (changed) RaiseSelectedNodesChanged();
        }

        public void SetSelectedNodes(IEnumerable<TreeNode> nodes, TreeNode activeNode)
        {
            _insideSelectionUpdate = true;
            try
            {
                _selectedNodes.Clear();
                if (nodes != null)
                {
                    foreach (TreeNode node in nodes)
                    {
                        if (node != null && node.TreeView == this && !_selectedNodes.Contains(node))
                            _selectedNodes.Add(node);
                    }
                }

                _selectionAnchor = _selectedNodes.Count > 0 ? _selectedNodes[0] : null;
                TreeNode targetActiveNode = activeNode != null && activeNode.TreeView == this
                    ? activeNode : _selectedNodes.LastOrDefault();
                if (targetActiveNode != null)
                {
                    if (!ReferenceEquals(SelectedNode, targetActiveNode)) SelectedNode = targetActiveNode;
                }
                else if (SelectedNode != null)
                {
                    SelectedNode = null;
                }
            }
            finally
            {
                _insideSelectionUpdate = false;
                Invalidate();
                RaiseSelectedNodesChanged();
            }
        }

        public void ClearMultiSelection()
        {
            _selectedNodes.Clear();
            _selectionAnchor = null;
            Invalidate();
            RaiseSelectedNodesChanged();
        }

        protected override void WndProc(ref Message m)
        {
            // Keep MainForm from rendering the transient native single selection before
            // Ctrl/Shift multi-selection has been committed.
            if (m.Msg == WmLButtonDown)
            {
                Point point = PointToClient(Cursor.Position);
                TreeNode hitNode = GetNodeAt(point);
                bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
                bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
                bool previousGesture = IsSelectionGestureInProgress;
                IsSelectionGestureInProgress = ctrl || shift;
                try
                {
                    base.WndProc(ref m);
                    if (!_insideSelectionUpdate && hitNode != null)
                        ApplyMouseSelection(hitNode, ctrl, shift);
                }
                finally { IsSelectionGestureInProgress = previousGesture; }
                return;
            }

            if (m.Msg == WmRButtonDown)
            {
                Point point = PointToClient(Cursor.Position);
                TreeNode hitNode = GetNodeAt(point);
                base.WndProc(ref m);
                if (!_insideSelectionUpdate && hitNode != null)
                {
                    if (IsNodeMultiSelected(hitNode))
                        SetActiveNode(hitNode);
                    else
                        SelectSingleNode(hitNode);
                }
                return;
            }

            if (m.Msg == WmLButtonDoubleClick)
            {
                Point point = PointToClient(Cursor.Position);
                TreeNode hitNode = GetNodeAt(point);
                bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
                bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
                bool previousGesture = IsSelectionGestureInProgress;
                IsSelectionGestureInProgress = ctrl || shift;
                try
                {
                    base.WndProc(ref m);
                    if (!_insideSelectionUpdate && hitNode != null)
                    {
                        // The first click already applied the modifier. Do not toggle twice.
                        if (!ctrl && !shift) SelectSingleNode(hitNode);
                        else SetActiveNode(hitNode);
                    }
                }
                finally { IsSelectionGestureInProgress = previousGesture; }
                return;
            }

            if (m.Msg == WmKeyDown)
            {
                Keys key = (Keys)m.WParam.ToInt32();
                bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
                bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
                bool previousGesture = IsSelectionGestureInProgress;
                if (ctrl || shift) IsSelectionGestureInProgress = true;
                try
                {
                    if (ctrl && key == Keys.A)
                    {
                        SelectAllVisibleNodes();
                        return;
                    }

                    if (ctrl && key == Keys.Space)
                    {
                        ToggleNodeSelection(SelectedNode);
                        return;
                    }

                    TreeNode previousActive = SelectedNode;
                    base.WndProc(ref m);
                    if (_insideSelectionUpdate) return;

                    TreeNode active = SelectedNode;
                    if (active != null && IsNavigationKey(key))
                    {
                        if (shift)
                            SelectRange(active, ctrl);
                        else if (!ctrl)
                            SelectSingleNode(active);
                        else if (_selectedNodes.Count == 0)
                            _selectedNodes.Add(active);
                        else if (previousActive != null && !_selectedNodes.Contains(active))
                            Invalidate(); // Ctrl+navigation moves focus without discarding the group.
                    }
                }
                finally { IsSelectionGestureInProgress = previousGesture; }
                return;
            }

            base.WndProc(ref m);
        }

        private void ApplyMouseSelection(TreeNode node, bool ctrl, bool shift)
        {
            if (node == null || node.TreeView != this) return;
            if (shift)
            {
                SelectRange(node, ctrl);
                return;
            }

            if (ctrl)
            {
                ToggleNodeSelection(node);
                if (_selectionAnchor == null || _selectionAnchor.TreeView != this)
                    _selectionAnchor = node;
                return;
            }

            SelectSingleNode(node);
        }

        private void ToggleNodeSelection(TreeNode node)
        {
            if (node == null || node.TreeView != this) return;

            if (_selectedNodes.Contains(node))
                _selectedNodes.Remove(node);
            else
            {
                _selectedNodes.Add(node);
                if (_selectionAnchor == null || _selectionAnchor.TreeView != this)
                    _selectionAnchor = node;
            }

            Invalidate();
            RaiseSelectedNodesChanged();
        }

        private void SelectRange(TreeNode target, bool addToExisting)
        {
            if (target == null || target.TreeView != this) return;

            List<TreeNode> visibleNodes = GetVisibleNodes();
            TreeNode anchor = _selectionAnchor;
            if (anchor == null || anchor.TreeView != this || !visibleNodes.Contains(anchor))
                anchor = SelectedNode != null && visibleNodes.Contains(SelectedNode) ? SelectedNode : target;

            int start = visibleNodes.IndexOf(anchor);
            int end = visibleNodes.IndexOf(target);
            if (start < 0 || end < 0)
            {
                SelectSingleNode(target);
                return;
            }

            if (!addToExisting) _selectedNodes.Clear();
            int from = Math.Min(start, end);
            int to = Math.Max(start, end);
            for (int i = from; i <= to; i++)
            {
                if (!_selectedNodes.Contains(visibleNodes[i])) _selectedNodes.Add(visibleNodes[i]);
            }

            if (!ReferenceEquals(SelectedNode, target)) SelectedNode = target;
            Invalidate();
            RaiseSelectedNodesChanged();
        }

        private void SelectAllVisibleNodes()
        {
            _selectedNodes.Clear();
            _selectedNodes.AddRange(GetVisibleNodes());
            if (SelectedNode != null) _selectionAnchor = SelectedNode;
            else _selectionAnchor = _selectedNodes.FirstOrDefault();
            Invalidate();
            RaiseSelectedNodesChanged();
        }

        private List<TreeNode> GetVisibleNodes()
        {
            var result = new List<TreeNode>();
            foreach (TreeNode root in Nodes) AddVisibleNodeAndChildren(root, result);
            return result;
        }

        private static void AddVisibleNodeAndChildren(TreeNode node, List<TreeNode> result)
        {
            if (node == null) return;
            result.Add(node);
            if (!node.IsExpanded) return;
            foreach (TreeNode child in node.Nodes) AddVisibleNodeAndChildren(child, result);
        }

        private static bool IsNavigationKey(Keys key)
        {
            return key == Keys.Up || key == Keys.Down || key == Keys.Home || key == Keys.End ||
                   key == Keys.PageUp || key == Keys.PageDown;
        }

        private void PruneDetachedNodes()
        {
            _selectedNodes.RemoveAll(node => node == null || node.TreeView != this);
            if (_selectionAnchor != null && _selectionAnchor.TreeView != this)
                _selectionAnchor = null;
        }

        private void DrawMultiSelectedNode(object sender, DrawTreeNodeEventArgs e)
        {
            bool selected = _selectedNodes.Contains(e.Node);
            bool active = selected && ReferenceEquals(e.Node, SelectedNode);

            Color background = BackColor;
            Color foreground = ForeColor;
            if (active)
            {
                background = SystemColors.Highlight;
                foreground = SystemColors.HighlightText;
            }
            else if (selected)
            {
                background = AdditionalSelectionBackColor;
                foreground = AdditionalSelectionForeColor;
            }

            // OwnerDrawText lets Windows continue to render indentation and expand/collapse
            // glyphs. Extend only from the text start to the right edge so those glyphs are
            // not painted over while the whole row's text area remains visibly selected.
            Rectangle rowBounds = new Rectangle(e.Bounds.Left, e.Bounds.Top,
                Math.Max(e.Bounds.Width, ClientSize.Width - e.Bounds.Left), e.Bounds.Height);
            using (var brush = new SolidBrush(background))
                e.Graphics.FillRectangle(brush, rowBounds);

            Rectangle textBounds = new Rectangle(e.Bounds.Left, e.Bounds.Top,
                Math.Max(0, ClientSize.Width - e.Bounds.Left - 2), e.Bounds.Height);
            Font nodeFont = e.Node.NodeFont ?? Font;
            TextRenderer.DrawText(e.Graphics, e.Node.Text ?? string.Empty, nodeFont, textBounds,
                foreground, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            // The active row already has a distinct selection fill. Avoid an extra full-row
            // focus rectangle: it creates a bright outline around selected ranges in dark mode.
        }
    }
}
