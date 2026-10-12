using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D;
using Gibbed.Prototype.FileFormats.Pure3D.Prototype1;
using Gibbed.Prototype.FileFormats.Pure3D.Prototype2;
using Nixson.Common;
using Nixson.Prototype1;
using Nixson.Prototype2;

namespace PrototypeUltimateTool
{
    public class Model3DViewer : System.Windows.Forms.UserControl
    {
        private ElementHost host;
        private System.Windows.Controls.Grid wpfGrid;
        private Viewport3D viewport;
        private ModelVisual3D rootVisual;
        private Model3DGroup rootGroup;
        private PerspectiveCamera camera;

        // Гизмо осей
        private Border gizmoContainer;
        private Viewport3D gizmoViewport;
        private PerspectiveCamera gizmoCamera;
        private System.Windows.Controls.Label xLabel;
        private System.Windows.Controls.Label yLabel;
        private System.Windows.Controls.Label zLabel;

        private System.Windows.Forms.ToolStrip toolStrip;
        private ToolStripButton resetCamButton;
        private ToolStripButton lightingButton;
        private ToolStripButton textureButton;
        private ToolStripButton flipUvButton;
        private ToolStripComboBox textureOverrideCombo;
        private ToolStripLabel statsLabel;

        // Камера
        private double _yaw = -45.0;
        private double _pitch = 20.0;
        private double _distance = 2500.0;
        private double _targetX = 0.0;
        private double _targetY = 0.0;
        private double _targetZ = 0.0;
        private System.Windows.Point _lastMousePos;
        private bool _isOrbiting = false;
        private bool _isPanning = false;

        private double _boundingRadius = 1000.0;
        private bool _lighting = true;
        private bool _showTextures = true;
        private bool _flipUv = false;
        private List<Model3D> _loadedModels = new List<Model3D>();

        private BaseNode _lastNode;
        private Pure3DFile _lastP3D;

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public Model3DViewer()
        {
            this.BuildUI();
        }

        private void BuildUI()
        {
            this.toolStrip = new System.Windows.Forms.ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };

            this.resetCamButton = new ToolStripButton("🏠 Сброс вида", null, (s, e) => this.ResetCamera());
            this.textureButton = new ToolStripButton("🎨 Текстура", null, (s, e) =>
            {
                this._showTextures = !this._showTextures;
                this.textureButton.Checked = this._showTextures;
                if (this._lastNode != null) this.LoadNode(this._lastNode, this._lastP3D);
            })
            { CheckOnClick = true, Checked = true };

            this.flipUvButton = new ToolStripButton("🔄 Инверт UV", null, (s, e) =>
            {
                this._flipUv = !this._flipUv;
                this.flipUvButton.Checked = this._flipUv;
                if (this._lastNode != null) this.LoadNode(this._lastNode, this._lastP3D);
            })
            { CheckOnClick = true, Checked = false, ToolTipText = "Перевернуть координаты текстуры по вертикали" };

            this.textureOverrideCombo = new ToolStripComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new System.Drawing.Size(180, 25),
                ToolTipText = "Выбрать текстуру вручную для этой модели"
            };
            this.textureOverrideCombo.SelectedIndexChanged += (s, e) =>
            {
                if (this._lastNode != null) this.LoadNode(this._lastNode, this._lastP3D);
            };

            this.lightingButton = new ToolStripButton("💡 Свет", null, (s, e) =>
            {
                this._lighting = !this._lighting;
                this.lightingButton.Checked = this._lighting;
                this.UpdateLights();
            })
            { CheckOnClick = true, Checked = true };

            this.statsLabel = new ToolStripLabel { Alignment = ToolStripItemAlignment.Right, ForeColor = System.Drawing.SystemColors.GrayText };

            this.toolStrip.Items.AddRange(new ToolStripItem[]
            {
                this.resetCamButton,
                new ToolStripSeparator(),
                this.textureButton,
                this.flipUvButton,
                this.textureOverrideCombo,
                this.lightingButton,
                new ToolStripSeparator(),
                this.statsLabel
            });

            this.host = new ElementHost { Dock = DockStyle.Fill };

            this.viewport = new Viewport3D();
            this.camera = new PerspectiveCamera
            {
                FieldOfView = 45,
                NearPlaneDistance = 1.0,
                FarPlaneDistance = 500000.0,
                UpDirection = new Vector3D(0, 1, 0)
            };
            this.viewport.Camera = this.camera;

            this.rootGroup = new Model3DGroup();
            this.rootVisual = new ModelVisual3D { Content = this.rootGroup };
            this.viewport.Children.Add(this.rootVisual);

            this.UpdateLights();
            this.BuildGizmoUI();

            this.wpfGrid = new System.Windows.Controls.Grid
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 26, 26))
            };
            this.wpfGrid.Children.Add(this.viewport);
            this.wpfGrid.Children.Add(this.gizmoContainer);

            this.wpfGrid.MouseDown += this.OnWpfMouseDown;
            this.wpfGrid.MouseMove += this.OnWpfMouseMove;
            this.wpfGrid.MouseUp += this.OnWpfMouseUp;
            this.wpfGrid.MouseWheel += this.OnWpfMouseWheel;

            this.host.Child = this.wpfGrid;

            this.Controls.Add(this.host);
            this.Controls.Add(this.toolStrip);
        }

        private void BuildGizmoUI()
        {
            this.gizmoViewport = new Viewport3D();
            this.gizmoCamera = new PerspectiveCamera
            {
                FieldOfView = 45,
                NearPlaneDistance = 0.1,
                FarPlaneDistance = 100.0,
                UpDirection = new Vector3D(0, 1, 0)
            };
            this.gizmoViewport.Camera = this.gizmoCamera;

            var gizmoGroup = new Model3DGroup();
            gizmoGroup.Children.Add(new AmbientLight(System.Windows.Media.Color.FromRgb(120, 120, 120)));
            gizmoGroup.Children.Add(new DirectionalLight(System.Windows.Media.Color.FromRgb(200, 200, 200), new Vector3D(-1, -1, -2)));

            double r = 0.06;
            gizmoGroup.Children.Add(CreateBox(new Point3D(0, 0, 0), new Point3D(1.1, 0, 0), r, System.Windows.Media.Color.FromRgb(229, 57, 53)));
            gizmoGroup.Children.Add(CreateBox(new Point3D(0, 0, 0), new Point3D(0, 1.1, 0), r, System.Windows.Media.Color.FromRgb(67, 160, 71)));
            gizmoGroup.Children.Add(CreateBox(new Point3D(0, 0, 0), new Point3D(0, 0, 1.1), r, System.Windows.Media.Color.FromRgb(30, 136, 229)));
            gizmoGroup.Children.Add(CreateBox(new Point3D(-0.08, -0.08, -0.08), new Point3D(0.08, 0.08, 0.08), 0.08, System.Windows.Media.Color.FromRgb(176, 190, 197)));

            this.gizmoViewport.Children.Add(new ModelVisual3D { Content = gizmoGroup });

            this.xLabel = new System.Windows.Controls.Label { Content = "X", Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 82, 82)), FontWeight = FontWeights.Bold, FontSize = 11, Padding = new Thickness(0) };
            this.yLabel = new System.Windows.Controls.Label { Content = "Y", Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(105, 240, 174)), FontWeight = FontWeights.Bold, FontSize = 11, Padding = new Thickness(0) };
            this.zLabel = new System.Windows.Controls.Label { Content = "Z", Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 196, 255)), FontWeight = FontWeights.Bold, FontSize = 11, Padding = new Thickness(0) };

            var gizmoCanvas = new Canvas { IsHitTestVisible = false };
            gizmoCanvas.Children.Add(this.xLabel);
            gizmoCanvas.Children.Add(this.yLabel);
            gizmoCanvas.Children.Add(this.zLabel);

            var gizmoInnerGrid = new System.Windows.Controls.Grid();
            gizmoInnerGrid.Children.Add(this.gizmoViewport);
            gizmoInnerGrid.Children.Add(gizmoCanvas);

            this.gizmoContainer = new Border
            {
                Width = 100,
                Height = 100,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                Margin = new Thickness(0, 10, 15, 0),
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(130, 20, 20, 22)),
                CornerRadius = new CornerRadius(50),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(70, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                Child = gizmoInnerGrid,
                IsHitTestVisible = false
            };
        }

        private static GeometryModel3D CreateBox(Point3D p1, Point3D p2, double radius, System.Windows.Media.Color color)
        {
            var mesh = new MeshGeometry3D();
            double minX = Math.Min(p1.X, p2.X) - radius, maxX = Math.Max(p1.X, p2.X) + radius;
            double minY = Math.Min(p1.Y, p2.Y) - radius, maxY = Math.Max(p1.Y, p2.Y) + radius;
            double minZ = Math.Min(p1.Z, p2.Z) - radius, maxZ = Math.Max(p1.Z, p2.Z) + radius;

            Point3D[] pts = new Point3D[]
            {
                new Point3D(minX, minY, minZ), new Point3D(maxX, minY, minZ),
                new Point3D(maxX, maxY, minZ), new Point3D(minX, maxY, minZ),
                new Point3D(minX, minY, maxZ), new Point3D(maxX, minY, maxZ),
                new Point3D(maxX, maxY, maxZ), new Point3D(minX, maxY, maxZ)
            };
            foreach (var pt in pts) mesh.Positions.Add(pt);

            int[] idxs = new int[]
            {
                0,2,1, 0,3,2,  1,6,5, 1,2,6,  5,7,4, 5,6,7,
                4,3,0, 4,7,3,  3,6,2, 3,7,6,  4,1,5, 4,0,1
            };
            foreach (var i in idxs) mesh.TriangleIndices.Add(i);

            var mat = new DiffuseMaterial(new SolidColorBrush(color));
            return new GeometryModel3D(mesh, mat);
        }

        private static GeometryModel3D CreateBlenderBone(Point3D head, Point3D tail, double boneWidth, System.Windows.Media.Color color)
        {
            var mesh = new MeshGeometry3D();
            Vector3D dir = tail - head;
            double len = dir.Length;
            if (len < 0.001) return CreateBox(head, tail, boneWidth, color);

            dir.Normalize();
            Vector3D up = Math.Abs(dir.Y) < 0.9 ? new Vector3D(0, 1, 0) : new Vector3D(1, 0, 0);
            Vector3D side = Vector3D.CrossProduct(dir, up);
            side.Normalize();
            up = Vector3D.CrossProduct(side, dir);
            up.Normalize();

            Point3D mid = head + dir * (len * 0.2);

            mesh.Positions.Add(head);
            mesh.Positions.Add(tail);
            mesh.Positions.Add(mid + side * boneWidth);
            mesh.Positions.Add(mid + up * boneWidth);
            mesh.Positions.Add(mid - side * boneWidth);
            mesh.Positions.Add(mid - up * boneWidth);

            int[] idxs = new int[]
            {
                0, 2, 3,  0, 3, 4,  0, 4, 5,  0, 5, 2,
                1, 3, 2,  1, 4, 3,  1, 5, 4,  1, 2, 5
            };
            foreach (var i in idxs) mesh.TriangleIndices.Add(i);

            var mat = new DiffuseMaterial(new SolidColorBrush(color));
            return new GeometryModel3D(mesh, mat) { BackMaterial = mat };
        }

        public void ApplyTheme(bool isDark)
        {
            System.Drawing.Color bg = isDark ? System.Drawing.Color.FromArgb(32, 32, 34) : System.Drawing.SystemColors.Control;
            System.Drawing.Color fg = isDark ? System.Drawing.Color.FromArgb(220, 220, 220) : System.Drawing.SystemColors.ControlText;

            this.toolStrip.BackColor = bg;
            this.toolStrip.ForeColor = fg;
            this.statsLabel.ForeColor = isDark ? System.Drawing.Color.FromArgb(160, 160, 160) : System.Drawing.SystemColors.GrayText;

            byte c = (byte)(isDark ? 26 : 240);
            this.wpfGrid.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(c, c, c));
        }

        private void UpdateLights()
        {
            for (int i = this.rootGroup.Children.Count - 1; i >= 0; i--)
            {
                if (this.rootGroup.Children[i] is Light) this.rootGroup.Children.RemoveAt(i);
            }

            if (this._lighting)
            {
                this.rootGroup.Children.Add(new AmbientLight(System.Windows.Media.Color.FromRgb(95, 95, 100)));
                this.rootGroup.Children.Add(new DirectionalLight(System.Windows.Media.Color.FromRgb(225, 225, 230), new Vector3D(-1, -2, -1.5)));
                this.rootGroup.Children.Add(new DirectionalLight(System.Windows.Media.Color.FromRgb(130, 130, 140), new Vector3D(1, 1.5, 2)));
            }
            else
            {
                this.rootGroup.Children.Add(new AmbientLight(System.Windows.Media.Color.FromRgb(255, 255, 255)));
            }
        }

        private void PopulateTextureDropdown(Pure3DFile p3d)
        {
            this.textureOverrideCombo.Items.Clear();
            this.textureOverrideCombo.Items.Add("Автовыбор текстуры");

            if (p3d != null && p3d.Nodes != null)
            {
                foreach (var n in p3d.Nodes)
                {
                    if (n.Exportable || n.ToString().Contains("Texture"))
                    {
                        string name = GetCleanName(n);
                        this.textureOverrideCombo.Items.Add(name);
                    }
                }
            }
            this.textureOverrideCombo.SelectedIndex = 0;
        }

        public bool LoadNode(BaseNode baseNode, Pure3DFile p3d)
        {
            this._lastNode = baseNode;
            if (this._lastP3D != p3d)
            {
                this._lastP3D = p3d;
                this.PopulateTextureDropdown(p3d);
            }

            foreach (var m in this._loadedModels) this.rootGroup.Children.Remove(m);
            this._loadedModels.Clear();

            if (baseNode == null)
            {
                this.statsLabel.Text = "";
                return false;
            }

            string nodeStr = baseNode.ToString() ?? "";

            // 1. СКЕЛЕТ
            if (nodeStr.Contains("Skeleton"))
            {
                return this.LoadSkeleton(baseNode);
            }

            // 2. 3D МОДЕЛИ
            int totalVerts = 0, totalTris = 0;
            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
            string lastError = null;

            try
            {
                List<KeyValuePair<PrimitiveGroup, string>> p1Groups = new List<KeyValuePair<PrimitiveGroup, string>>();

                if (baseNode is PrimitiveGroup directPg)
                {
                    p1Groups.Add(new KeyValuePair<PrimitiveGroup, string>(directPg, GetCleanName(baseNode)));
                }
                else if (baseNode is PolySkin ps)
                {
                    CollectPrimitiveGroupsWithContext(ps, GetCleanName(ps), p1Groups);
                }
                else if (baseNode is CompositeDrawable cd)
                {
                    CollectPrimitiveGroupsWithContext(cd, GetCleanName(cd), p1Groups);

                    if (p1Groups.Count == 0 && p3d != null && p3d.Nodes != null)
                    {
                        string cleanName = GetCleanName(cd);
                        foreach (var n in p3d.Nodes)
                        {
                            if (n is PolySkin skin && GetCleanName(skin).IndexOf(cleanName, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                CollectPrimitiveGroupsWithContext(skin, GetCleanName(skin), p1Groups);
                            }
                        }
                    }
                }

                foreach (var pair in p1Groups)
                {
                    var pg = pair.Key;
                    string contextName = pair.Value;

                    try
                    {
                        var rawVerts = GetPositionVerticesSafe(pg);
                        var rawFaces = Nixson.Prototype1.ImportP3D.GetFaces(pg);
                        if (rawVerts == null || rawFaces == null || rawVerts.Count == 0 || rawFaces.Count == 0) continue;

                        List<UVCoordinate> rawUVs = GetUVsSafe(pg);

                        var mesh = new MeshGeometry3D();
                        var positions = new Point3DCollection(rawVerts.Count);
                        PointCollection uvs = (rawUVs != null && rawUVs.Count == rawVerts.Count) ? new PointCollection(rawUVs.Count) : null;

                        for (int i = 0; i < rawVerts.Count; i++)
                        {
                            double x = rawVerts[i].X * 1000.0, y = rawVerts[i].Z * -1000.0, z = rawVerts[i].Y * 1000.0;
                            if (x < minX) minX = x; if (x > maxX) maxX = x;
                            if (y < minY) minY = y; if (y > maxY) maxY = y;
                            if (z < minZ) minZ = z; if (z > maxZ) maxZ = z;
                            positions.Add(new Point3D(x, y, z));

                            if (uvs != null)
                            {
                                double u = rawUVs[i].U;
                                double v = this._flipUv ? (1.0 - rawUVs[i].V) : rawUVs[i].V;
                                uvs.Add(new System.Windows.Point(u, v));
                            }
                        }
                        mesh.Positions = positions;
                        if (uvs != null) mesh.TextureCoordinates = uvs;

                        var indices = new Int32Collection(rawFaces.Count * 3);
                        for (int i = 0; i < rawFaces.Count; i++)
                        {
                            indices.Add((int)rawFaces[i].Point1);
                            indices.Add((int)rawFaces[i].Point3);
                            indices.Add((int)rawFaces[i].Point2);
                        }
                        mesh.TriangleIndices = indices;

                        // Поиск текстуры через шейдеры игры или сопоставление
                        ImageSource textureSource = null;
                        if (this._showTextures && p3d != null)
                        {
                            textureSource = ResolveTexture(pg, contextName, p3d);
                        }

                        Material mat;
                        if (textureSource != null && uvs != null)
                        {
                            mat = new DiffuseMaterial(new ImageBrush(textureSource));
                        }
                        else
                        {
                            mat = new DiffuseMaterial(new SolidColorBrush(System.Windows.Media.Color.FromRgb(215, 215, 220)));
                        }

                        var model = new GeometryModel3D(mesh, mat) { BackMaterial = mat };
                        this.rootGroup.Children.Add(model);
                        this._loadedModels.Add(model);

                        totalVerts += rawVerts.Count;
                        totalTris += rawFaces.Count;
                    }
                    catch (Exception ex)
                    {
                        lastError = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }

            if (this._loadedModels.Count == 0)
            {
                this.statsLabel.Text = lastError != null ? ("Ошибка: " + lastError) : "В этом узле нет 3D геометрии";
                return false;
            }

            this._targetX = (minX + maxX) * 0.5;
            this._targetY = (minY + maxY) * 0.5;
            this._targetZ = (minZ + maxZ) * 0.5;

            double dx = maxX - minX, dy = maxY - minY, dz = maxZ - minZ;
            this._boundingRadius = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (this._boundingRadius < 1.0) this._boundingRadius = 1000.0;

            this.ResetCamera();
            this.statsLabel.Text = string.Format("Вершин: {0:N0} | Полигонов: {1:N0}", totalVerts, totalTris);
            return true;
        }

        private static int FindIndexOfDescription(DescriptionTypeEnum descriptionType, VertexDescriptionList vertexDescription)
        {
            if (vertexDescription == null || vertexDescription.Descriptions == null) return -1;
            for (int i = 0; i < vertexDescription.Descriptions.Length; i++)
            {
                if (vertexDescription.Descriptions[i].BufferType != null &&
                    vertexDescription.Descriptions[i].BufferType.EnumValue == descriptionType)
                {
                    return i;
                }
            }
            return -1;
        }

        private static bool IsRawBuffer(PrimitiveGroup primitiveGroup)
        {
            if (primitiveGroup == null) return false;
            return primitiveGroup.GetChildNodes<RawPositionBuffer>().Count > 0;
        }

        // Автономное извлечение вершин из всех буферов ноды (решает проблему AlexVestShape)
        private static List<Nixson.Common.Vector3> GetPositionVerticesSafe(PrimitiveGroup pg)
        {
            if (IsRawBuffer(pg))
            {
                try { return Nixson.Prototype1.ImportP3D.GetPositionVertices(pg); } catch { }
            }

            List<VertexBuffer> childNodes = pg.GetChildNodes<VertexBuffer>();
            foreach (VertexBuffer vb in childNodes)
            {
                if (vb.Description == null) continue;
                int idx = FindIndexOfDescription(DescriptionTypeEnum.Position, vb.Description);
                if (idx != -1 && vb.BufferItems != null)
                {
                    List<Nixson.Common.Vector3> list = new List<Nixson.Common.Vector3>();
                    int stride = (int)vb.Description.AmountOfDescriptions;
                    for (int i = 0; i < (int)pg.NumVertices; i++)
                    {
                        int itemIdx = i * stride + idx;
                        if (itemIdx < vb.BufferItems.Length)
                        {
                            object val = vb.BufferItems[itemIdx].GetValueP1();
                            if (val is Nixson.Common.Vector3 v) list.Add(v);
                            else if (val != null)
                            {
                                try
                                {
                                    var px = val.GetType().GetProperty("X");
                                    var py = val.GetType().GetProperty("Y");
                                    var pz = val.GetType().GetProperty("Z");
                                    if (px != null && py != null && pz != null)
                                    {
                                        float x = Convert.ToSingle(px.GetValue(val, null));
                                        float y = Convert.ToSingle(py.GetValue(val, null));
                                        float z = Convert.ToSingle(pz.GetValue(val, null));
                                        list.Add(new Nixson.Common.Vector3 { X = x, Y = y, Z = z });
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    if (list.Count > 0) return list;
                }
            }

            return Nixson.Prototype1.ImportP3D.GetPositionVertices(pg);
        }

        private static List<UVCoordinate> GetUVsSafe(PrimitiveGroup pg)
        {
            if (IsRawBuffer(pg))
            {
                try { return Nixson.Prototype1.ImportP3D.GetUVs(pg); } catch { }
            }

            List<VertexBuffer> childNodes = pg.GetChildNodes<VertexBuffer>();
            foreach (VertexBuffer vb in childNodes)
            {
                if (vb.Description == null) continue;
                int idx = FindIndexOfDescription(DescriptionTypeEnum.UV, vb.Description);
                if (idx != -1 && vb.BufferItems != null)
                {
                    List<UVCoordinate> list = new List<UVCoordinate>();
                    int stride = (int)vb.Description.AmountOfDescriptions;
                    for (int i = 0; i < (int)pg.NumVertices; i++)
                    {
                        int itemIdx = i * stride + idx;
                        if (itemIdx < vb.BufferItems.Length)
                        {
                            object val = vb.BufferItems[itemIdx].GetValueP1();
                            if (val is UVCoordinate uv) list.Add(uv);
                        }
                    }
                    if (list.Count > 0) return list;
                }
            }

            try { return Nixson.Prototype1.ImportP3D.GetUVs(pg); } catch { }
            return null;
        }

        // Полноценный сопоставитель текстур через Шейдеры игры и префиксы персонажей
        private ImageSource ResolveTexture(PrimitiveGroup pg, string shapeName, Pure3DFile p3d)
        {
            if (this.textureOverrideCombo.SelectedIndex > 0)
            {
                string picked = this.textureOverrideCombo.SelectedItem.ToString();
                foreach (var n in p3d.Nodes)
                {
                    if (GetCleanName(n).Equals(picked, StringComparison.OrdinalIgnoreCase))
                    {
                        using (var bmp = DdsDecoder.ExtractBitmap(n, false)) return BitmapToImageSource(bmp);
                    }
                }
            }

            // 1. Проверяем шейдер, на который ссылается PrimitiveGroup
            string shaderName = pg.ShaderName;
            if (!string.IsNullOrWhiteSpace(shaderName))
            {
                foreach (var n in p3d.Nodes)
                {
                    if (n.ToString().Contains("Shader") && GetCleanName(n).Equals(shaderName, StringComparison.OrdinalIgnoreCase))
                    {
                        string texName = ExtractTextureNameFromShader(n);
                        if (!string.IsNullOrEmpty(texName))
                        {
                            foreach (var texNode in p3d.Nodes)
                            {
                                if (GetCleanName(texNode).IndexOf(texName, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    using (var bmp = DdsDecoder.ExtractBitmap(texNode, false)) return BitmapToImageSource(bmp);
                                }
                            }
                        }
                    }
                }
            }

            // 2. Умное сопоставление по персонажу и частям тела
            string[] shapeTokens = shapeName.ToLowerInvariant().Split(new char[] { '_', ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);

            BaseNode bestTexNode = null;
            int bestScore = 0;

            foreach (var node in p3d.Nodes)
            {
                if (node.Exportable || node.ToString().Contains("Texture"))
                {
                    string texName = GetCleanName(node).ToLowerInvariant();
                    if (!texName.Contains("_dm") && !texName.Contains("diffuse") && !texName.Contains("color")) continue;

                    int score = 0;
                    foreach (var tok in shapeTokens)
                    {
                        if (tok.Length < 3) continue;
                        if (texName.Contains(tok)) score += 10;
                    }

                    // Приоритеты анатомии
                    if ((shapeName.Contains("head") || shapeName.Contains("face")) && texName.Contains("head")) score += 25;
                    if ((shapeName.Contains("body") || shapeName.Contains("arm") || shapeName.Contains("vest") || shapeName.Contains("torso") || shapeName.Contains("leg")) && texName.Contains("body")) score += 20;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTexNode = node;
                    }
                }
            }

            if (bestTexNode != null)
            {
                using (var bmp = DdsDecoder.ExtractBitmap(bestTexNode, false)) return BitmapToImageSource(bmp);
            }

            return null;
        }

        private static string ExtractTextureNameFromShader(BaseNode shaderNode)
        {
            if (shaderNode.Children == null) return null;
            foreach (var c in shaderNode.Children)
            {
                var pVal = c.GetType().GetProperty("Value") ?? c.GetType().GetProperty("TextureName");
                if (pVal != null)
                {
                    string s = pVal.GetValue(c, null) as string;
                    if (!string.IsNullOrWhiteSpace(s)) return s;
                }
                string str = c.ToString();
                if (str.Contains(".dds"))
                {
                    int o = str.IndexOf('('), cl = str.LastIndexOf(')');
                    if (o >= 0 && cl > o) return str.Substring(o + 1, cl - o - 1);
                }
            }
            return null;
        }

        private static ImageSource BitmapToImageSource(Bitmap bmp)
        {
            if (bmp == null) return null;
            IntPtr hBitmap = bmp.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }

        private bool LoadSkeleton(BaseNode skeletonNode)
        {
            var joints = this.ExtractJoints(skeletonNode);
            if (joints.Count == 0)
            {
                this.statsLabel.Text = "Скелет пуст";
                return false;
            }

            Point3D[] worldPositions = new Point3D[joints.Count];
            for (int i = 0; i < joints.Count; i++)
            {
                int pIdx = joints[i].ParentIndex;
                if (pIdx >= 0 && pIdx < i)
                {
                    worldPositions[i] = new Point3D(
                        worldPositions[pIdx].X + joints[i].LocalPosition.X,
                        worldPositions[pIdx].Y + joints[i].LocalPosition.Y,
                        worldPositions[pIdx].Z + joints[i].LocalPosition.Z
                    );
                }
                else
                {
                    worldPositions[i] = joints[i].LocalPosition;
                }
            }

            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

            int boneCount = 0;
            System.Windows.Media.Color boneColor = System.Windows.Media.Color.FromRgb(230, 175, 70);
            System.Windows.Media.Color jointColor = System.Windows.Media.Color.FromRgb(64, 196, 255);

            for (int i = 0; i < joints.Count; i++)
            {
                Point3D pos = worldPositions[i];
                if (pos.X < minX) minX = pos.X; if (pos.X > maxX) maxX = pos.X;
                if (pos.Y < minY) minY = pos.Y; if (pos.Y > maxY) maxY = pos.Y;
                if (pos.Z < minZ) minZ = pos.Z; if (pos.Z > maxZ) maxZ = pos.Z;

                var jointSphere = CreateBox(pos, pos, 8.0, jointColor);
                this.rootGroup.Children.Add(jointSphere);
                this._loadedModels.Add(jointSphere);

                int pIdx = joints[i].ParentIndex;
                if (pIdx >= 0 && pIdx < joints.Count && pIdx != i)
                {
                    Point3D parentPos = worldPositions[pIdx];
                    double dist = (pos - parentPos).Length;
                    double width = Math.Max(4.0, Math.Min(20.0, dist * 0.14));

                    var bone = CreateBlenderBone(parentPos, pos, width, boneColor);
                    this.rootGroup.Children.Add(bone);
                    this._loadedModels.Add(bone);
                    boneCount++;
                }
            }

            this._targetX = (minX + maxX) * 0.5;
            this._targetY = (minY + maxY) * 0.5;
            this._targetZ = (minZ + maxZ) * 0.5;

            double dx = maxX - minX, dy = maxY - minY, dz = maxZ - minZ;
            this._boundingRadius = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (this._boundingRadius < 10.0) this._boundingRadius = 1200.0;

            this.ResetCamera();
            this.statsLabel.Text = string.Format("🦴 Скелет: {0} суставов | {1} костей", joints.Count, boneCount);
            return true;
        }

        private class JointData
        {
            public string Name;
            public int ParentIndex;
            public Point3D LocalPosition;
        }

        private List<JointData> ExtractJoints(BaseNode skeletonNode)
        {
            var result = new List<JointData>();
            if (skeletonNode == null) return result;

            IEnumerable list = null;
            var prop = skeletonNode.GetType().GetProperty("Joints") ?? skeletonNode.GetType().GetProperty("SkeletonJoints");
            if (prop != null) list = prop.GetValue(skeletonNode, null) as IEnumerable;

            if (list == null)
            {
                var field = skeletonNode.GetType().GetField("Joints", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         ?? skeletonNode.GetType().GetField("_joints", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) list = field.GetValue(skeletonNode) as IEnumerable;
            }

            if (list == null && skeletonNode.Children != null) list = skeletonNode.Children;
            if (list == null) return result;

            int idx = 0;
            foreach (var item in list)
            {
                if (item == null) continue;
                string name = item.ToString();
                int parentIdx = -1;

                var pParent = item.GetType().GetProperty("ParentIndex") ?? item.GetType().GetProperty("Parent");
                if (pParent != null)
                {
                    try { parentIdx = Convert.ToInt32(pParent.GetValue(item, null)); } catch { }
                }

                Point3D pos = new Point3D(0, idx * 30.0, 0);

                var pPos = item.GetType().GetProperty("Translation") ?? item.GetType().GetProperty("Position") ?? item.GetType().GetProperty("RestPose");
                if (pPos != null)
                {
                    try
                    {
                        object v = pPos.GetValue(item, null);
                        if (v != null)
                        {
                            var px = v.GetType().GetProperty("X") ?? v.GetType().GetProperty("M41");
                            var py = v.GetType().GetProperty("Y") ?? v.GetType().GetProperty("M42");
                            var pz = v.GetType().GetProperty("Z") ?? v.GetType().GetProperty("M43");
                            if (px != null && py != null && pz != null)
                            {
                                double x = Convert.ToDouble(px.GetValue(v, null)) * 1000.0;
                                double y = Convert.ToDouble(pz.GetValue(v, null)) * -1000.0;
                                double z = Convert.ToDouble(py.GetValue(v, null)) * 1000.0;
                                pos = new Point3D(x, y, z);
                            }
                        }
                    }
                    catch { }
                }

                result.Add(new JointData { Name = name, ParentIndex = parentIdx, LocalPosition = pos });
                idx++;
            }

            return result;
        }

        private static void CollectPrimitiveGroupsWithContext(BaseNode node, string contextName, List<KeyValuePair<PrimitiveGroup, string>> list)
        {
            if (node == null) return;
            if (node is PrimitiveGroup pg)
            {
                list.Add(new KeyValuePair<PrimitiveGroup, string>(pg, contextName));
                return;
            }
            if (node.Children != null)
            {
                foreach (var c in node.Children)
                {
                    CollectPrimitiveGroupsWithContext(c, contextName, list);
                }
            }
        }

        private static string GetCleanName(BaseNode node)
        {
            if (node == null) return "";
            var p = node.GetType().GetProperty("Name");
            if (p != null && p.GetValue(node, null) is string n && !string.IsNullOrWhiteSpace(n)) return n;
            string s = node.ToString() ?? "";
            int o = s.IndexOf('('), c = s.LastIndexOf(')');
            return (o >= 0 && c > o) ? s.Substring(o + 1, c - o - 1) : s;
        }

        private void ResetCamera()
        {
            this._yaw = -45.0;
            this._pitch = 15.0;
            this._distance = Math.Max(100.0, this._boundingRadius * 1.6);
            this.UpdateCamera();
        }

        private void UpdateCamera()
        {
            double radYaw = this._yaw * Math.PI / 180.0;
            double radPitch = this._pitch * Math.PI / 180.0;

            double camX = this._targetX + this._distance * Math.Cos(radPitch) * Math.Sin(radYaw);
            double camY = this._targetY + this._distance * Math.Sin(radPitch);
            double camZ = this._targetZ + this._distance * Math.Cos(radPitch) * Math.Cos(radYaw);

            this.camera.Position = new Point3D(camX, camY, camZ);
            this.camera.LookDirection = new Vector3D(this._targetX - camX, this._targetY - camY, this._targetZ - camZ);

            double gzDist = 3.6;
            double gzX = gzDist * Math.Cos(radPitch) * Math.Sin(radYaw);
            double gzY = gzDist * Math.Sin(radPitch);
            double gzZ = gzDist * Math.Cos(radPitch) * Math.Cos(radYaw);

            this.gizmoCamera.Position = new Point3D(gzX, gzY, gzZ);
            this.gizmoCamera.LookDirection = new Vector3D(-gzX, -gzY, -gzZ);

            double cosY = Math.Cos(radYaw), sinY = Math.Sin(radYaw);
            double cosP = Math.Cos(radPitch), sinP = Math.Sin(radPitch);

            Canvas.SetLeft(this.xLabel, 45.0 + 32.0 * cosY);
            Canvas.SetTop(this.xLabel, 45.0 - 32.0 * (-sinP * sinY));

            Canvas.SetLeft(this.yLabel, 45.0);
            Canvas.SetTop(this.yLabel, 45.0 - 32.0 * cosP);

            Canvas.SetLeft(this.zLabel, 45.0 + 32.0 * (-sinY));
            Canvas.SetTop(this.zLabel, 45.0 - 32.0 * (-sinP * cosY));
        }

        private void OnWpfMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this._lastMousePos = e.GetPosition(this.wpfGrid);
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this._isOrbiting = true;
                this.wpfGrid.CaptureMouse();
            }
            else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this._isPanning = true;
                this.wpfGrid.CaptureMouse();
            }
        }

        private void OnWpfMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!this._isOrbiting && !this._isPanning) return;

            System.Windows.Point cur = e.GetPosition(this.wpfGrid);
            double dx = cur.X - this._lastMousePos.X;
            double dy = cur.Y - this._lastMousePos.Y;
            this._lastMousePos = cur;

            if (this._isOrbiting)
            {
                this._yaw -= dx * 0.5;
                this._pitch += dy * 0.5;
                this._pitch = Math.Max(-89.0, Math.Min(89.0, this._pitch));
                this.UpdateCamera();
            }
            else if (this._isPanning)
            {
                double factor = this._distance * 0.0015;
                this._targetX -= dx * factor;
                this._targetY += dy * factor;
                this.UpdateCamera();
            }
        }

        private void OnWpfMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Released) this._isOrbiting = false;
            if (e.RightButton == System.Windows.Input.MouseButtonState.Released) this._isPanning = false;

            if (!this._isOrbiting && !this._isPanning) this.wpfGrid.ReleaseMouseCapture();
        }

        private void OnWpfMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double factor = e.Delta > 0 ? 0.85 : 1.15;
            this._distance = Math.Max(10.0, this._distance * factor);
            this.UpdateCamera();
        }
    }
}