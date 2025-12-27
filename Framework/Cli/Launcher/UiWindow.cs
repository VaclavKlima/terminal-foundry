using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PhpCompiler
{
    internal sealed class UiWindow
    {
        private readonly IPhpUiSession _session;
        private readonly string[] _baseArgs;
        private readonly LauncherLog _logger;
        private readonly bool _debug;
        private readonly bool _debugTree;
        private readonly Dictionary<string, string> _inputState = new Dictionary<string, string>();

        public UiWindow(IPhpUiSession session, string[] baseArgs, LauncherLog logger, bool debug, bool debugTree)
        {
            _session = session;
            _baseArgs = baseArgs ?? Array.Empty<string>();
            _logger = logger;
            _debug = debug;
            _debugTree = debugTree;
        }

        public void Show(UiPayload payload, string title)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new Form
            {
                Text = title,
                StartPosition = FormStartPosition.CenterScreen,
                Width = 900,
                Height = 600,
                BackColor = Color.FromArgb(22, 24, 28)
            };

            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(18, 20, 24)
            };

            var stack = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(18, 20, 24)
            };

            scroll.Controls.Add(stack);
            form.Controls.Add(scroll);
            Label debugLabel = null;
            if (_debug)
            {
                debugLabel = new Label
                {
                    AutoSize = true,
                    ForeColor = Color.Black,
                    BackColor = Color.Gold,
                    Font = new Font("Consolas", 8f, FontStyle.Bold),
                    Padding = new Padding(6, 4, 6, 4)
                };

                var debugBar = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 24,
                    BackColor = Color.Gold
                };
                debugBar.Controls.Add(debugLabel);
                form.Controls.Add(debugBar);
            }

            var labels = new List<Label>();
            var tables = new List<ListView>();
            var toolTip = new ToolTip();

            Action render = null;
            Action<UiPayload> setPayload = p => payload = p;
            render = () =>
            {
                try
                {
                    labels.Clear();
                    tables.Clear();

                    int scrollValue = scroll.VerticalScroll.Value;
                    scroll.SuspendLayout();

                    if (payload.Nodes != null && payload.Nodes.ContainsKey("type"))
                    {
                        if (_debug)
                        {
                            _logger.Log("Rendering nodes payload.");
                        }
                        RenderRoot(payload.Nodes, stack, labels, tables, toolTip, render, setPayload);
                    }
                    else
                    {
                        string text = string.IsNullOrWhiteSpace(payload.Text)
                            ? "Empty payload received from PHP."
                            : payload.Text;
                        if (_debug)
                        {
                            _logger.Log("Rendering text payload.");
                        }
                        stack.Controls.Clear();
                        var fallback = CreateTextLabel(text, labels);
                        stack.Controls.Add(fallback);
                    }

                    if (stack.Controls.Count == 0)
                    {
                        if (_debug)
                        {
                            _logger.Log("Render produced zero controls.");
                        }
                        stack.Controls.Add(CreateTextLabel("Render produced no controls.", labels));
                    }

                    UpdateLayout(scroll, labels, tables);
                    scroll.ResumeLayout(true);
                    scroll.AutoScrollPosition = new Point(0, scrollValue);
                    if (_debug && debugLabel != null)
                    {
                        debugLabel.Text = string.Format(
                            "controls={0} labels={1} tables={2} width={3} height={4}",
                            stack.Controls.Count,
                            labels.Count,
                            tables.Count,
                            stack.Width,
                            stack.Height);
                        _logger.Log("Render stats: " + debugLabel.Text);
                        if (_debugTree)
                        {
                            LogControlTree(stack, 0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log("Render exception: " + ex);
                    stack.Controls.Clear();
                    labels.Clear();
                    tables.Clear();
                    stack.Controls.Add(CreateTextLabel("Render exception: " + ex.Message, labels));
                    if (_debug && debugLabel != null)
                    {
                        debugLabel.Text = "render exception";
                    }
                }
            };

            form.SizeChanged += delegate { UpdateLayout(scroll, labels, tables); };
            form.Shown += delegate
            {
                render();
                UpdateLayout(scroll, labels, tables);
                if (_debug && _debugTree)
                {
                    form.BeginInvoke((Action)delegate
                    {
                        UpdateLayout(scroll, labels, tables);
                        LogControlTree(stack, 0);
                    });
                }
            };
            form.FormClosed += delegate
            {
                var disposable = _session as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            };

            render();
            Application.Run(form);
        }

        private void LogControlTree(Control control, int depth)
        {
            string indent = new string(' ', depth * 2);
            _logger.Log(string.Format(
                "{0}{1} visible={2} bounds={3} children={4}",
                indent,
                control.GetType().Name,
                control.Visible,
                control.Bounds,
                control.Controls.Count));

            foreach (Control child in control.Controls)
            {
                LogControlTree(child, depth + 1);
            }
        }

        private static void UpdateLayout(Panel scroll, List<Label> labels, List<ListView> tables)
        {
            int width = scroll.ClientSize.Width - 32;
            if (width <= 0)
            {
                return;
            }
            width = Math.Max(200, width);
            foreach (var label in labels)
            {
                label.MaximumSize = new Size(width, 0);
            }

            foreach (var table in tables)
            {
                table.Width = width;
                foreach (ColumnHeader column in table.Columns)
                {
                    column.Width = -2;
                }
            }
        }

        private static Label CreateTextLabel(string text, List<Label> labels, string id = null)
        {
            var label = new Label
            {
                AutoSize = true,
                ForeColor = Color.Gainsboro,
                Font = new Font("Consolas", 10f),
                Text = text
            };
            labels.Add(label);
            label.Tag = new NodeTag(id, "text");
            return label;
        }

        private void RenderRoot(
            Dictionary<string, object> root,
            FlowLayoutPanel container,
            List<Label> labels,
            List<ListView> tables,
            ToolTip toolTip,
            Action rerender,
            Action<UiPayload> setPayload)
        {
            string type = UiNodeReader.GetString(root, "type");
            if (string.IsNullOrWhiteSpace(type))
            {
                container.Controls.Clear();
                return;
            }

            if (type == "app" || type == "page" || type == "section" || type == "card")
            {
                SyncContainer(container, UiNodeReader.GetArray(root, "children"), labels, tables, toolTip, rerender, setPayload);
                return;
            }

            container.Controls.Clear();
            Control control = BuildControl(root, labels, tables, toolTip, rerender, setPayload);
            if (control != null)
            {
                container.Controls.Add(control);
            }
        }

        private void SyncContainer(
            FlowLayoutPanel container,
            object[] children,
            List<Label> labels,
            List<ListView> tables,
            ToolTip toolTip,
            Action rerender,
            Action<UiPayload> setPayload)
        {
            container.SuspendLayout();
            var existing = new Dictionary<string, Control>();
            foreach (Control child in container.Controls)
            {
                var tag = child.Tag as NodeTag;
                if (tag != null && !string.IsNullOrWhiteSpace(tag.Id))
                {
                    existing[tag.Id] = child;
                }
            }

            var desired = new List<Control>();
            foreach (var childNode in children)
            {
                var dict = childNode as Dictionary<string, object>;
                if (dict == null)
                {
                    continue;
                }

                string id = UiNodeReader.GetString(dict, "id");
                Control reuse = null;
                if (!string.IsNullOrWhiteSpace(id))
                {
                    existing.TryGetValue(id, out reuse);
                }

                Control control = CreateOrUpdateControl(dict, reuse, labels, tables, toolTip, rerender, setPayload);
                if (control != null)
                {
                    desired.Add(control);
                }
            }

            foreach (var control in desired)
            {
                if (!container.Controls.Contains(control))
                {
                    container.Controls.Add(control);
                }
            }

            for (int i = 0; i < desired.Count; i++)
            {
                container.Controls.SetChildIndex(desired[i], i);
            }

            var current = container.Controls.Cast<Control>().ToList();
            foreach (var control in current)
            {
                if (!desired.Contains(control))
                {
                    container.Controls.Remove(control);
                    control.Dispose();
                }
            }
            container.ResumeLayout(true);
        }

        private Control CreateOrUpdateControl(
            Dictionary<string, object> dict,
            Control existing,
            List<Label> labels,
            List<ListView> tables,
            ToolTip toolTip,
            Action rerender,
            Action<UiPayload> setPayload)
        {
            string type = UiNodeReader.GetString(dict, "type");
            if (string.IsNullOrWhiteSpace(type))
            {
                return null;
            }

            var tag = existing != null ? existing.Tag as NodeTag : null;
            if (tag != null && tag.Type == type)
            {
                UpdateControl(existing, dict, labels, tables, toolTip, rerender, setPayload);
                return existing;
            }

            return BuildControl(dict, labels, tables, toolTip, rerender, setPayload);
        }

        private Control BuildControl(
            Dictionary<string, object> dict,
            List<Label> labels,
            List<ListView> tables,
            ToolTip toolTip,
            Action rerender,
            Action<UiPayload> setPayload)
        {
            string type = UiNodeReader.GetString(dict, "type");
            if (string.IsNullOrWhiteSpace(type))
            {
                return null;
            }
            string id = UiNodeReader.GetString(dict, "id");

            switch (type)
            {
                case "app":
                case "page":
                case "section":
                case "card":
                    return BuildGroup(dict, labels, tables, toolTip, rerender, setPayload);
                case "text":
                    return CreateTextLabel(UiNodeReader.GetString(dict, "text") ?? string.Empty, labels, id);
                case "table":
                    return CreateTable(dict, tables, id);
                case "buttonRow":
                    return CreateButtonRow(dict, toolTip, rerender, setPayload, id);
                case "textInput":
                    return CreateTextInput(dict, rerender, setPayload, id);
                case "select":
                    return CreateSelect(dict, rerender, setPayload, id);
                default:
                    return CreateTextLabel("Unsupported element: " + type, labels, id);
            }
        }

        private void UpdateControl(
            Control control,
            Dictionary<string, object> dict,
            List<Label> labels,
            List<ListView> tables,
            ToolTip toolTip,
            Action rerender,
            Action<UiPayload> setPayload)
        {
            var tag = control.Tag as NodeTag;
            if (tag == null)
            {
                return;
            }

            switch (tag.Type)
            {
                case "app":
                case "page":
                case "section":
                case "card":
                    UpdateGroup(control, dict, labels, tables, toolTip, rerender, setPayload);
                    break;
                case "text":
                    var label = control as Label;
                    if (label != null)
                    {
                        label.Text = UiNodeReader.GetString(dict, "text") ?? string.Empty;
                        labels.Add(label);
                    }
                    break;
                case "table":
                    UpdateTable(control as ListView, dict, tables);
                    break;
                case "buttonRow":
                    UpdateButtonRow(control as FlowLayoutPanel, dict, toolTip, rerender, setPayload);
                    break;
                case "textInput":
                    UpdateTextInput(control as Panel, dict, rerender, setPayload);
                    break;
                case "select":
                    UpdateSelect(control as Panel, dict, rerender, setPayload);
                    break;
            }
        }

        private void AddNode(
            object node,
            FlowLayoutPanel container,
            List<Label> labels,
            List<ListView> tables,
            ToolTip toolTip,
            Action rerender,
            Action<UiPayload> setPayload)
        {
            var dict = node as Dictionary<string, object>;
            if (dict == null)
            {
                return;
            }

            string type = UiNodeReader.GetString(dict, "type");
            if (string.IsNullOrWhiteSpace(type))
            {
                return;
            }

            switch (type)
            {
                case "app":
                case "page":
                case "section":
                case "card":
                    var group = BuildGroup(dict, labels, tables, toolTip, rerender, setPayload);
                    if (group != null)
                    {
                        container.Controls.Add(group);
                    }
                    break;
                case "text":
                    container.Controls.Add(CreateTextLabel(UiNodeReader.GetString(dict, "text") ?? string.Empty, labels, UiNodeReader.GetString(dict, "id")));
                    break;
                case "table":
                    container.Controls.Add(CreateTable(dict, tables, UiNodeReader.GetString(dict, "id")));
                    break;
                case "buttonRow":
                    container.Controls.Add(CreateButtonRow(dict, toolTip, rerender, setPayload, UiNodeReader.GetString(dict, "id")));
                    break;
                case "textInput":
                    container.Controls.Add(CreateTextInput(dict, rerender, setPayload, UiNodeReader.GetString(dict, "id")));
                    break;
                case "select":
                    container.Controls.Add(CreateSelect(dict, rerender, setPayload, UiNodeReader.GetString(dict, "id")));
                    break;
                default:
                    container.Controls.Add(CreateTextLabel("Unsupported element: " + type, labels));
                    break;
            }
        }

        private Control BuildGroup(
            Dictionary<string, object> dict,
            List<Label> labels,
            List<ListView> tables,
            ToolTip toolTip,
            Action rerender,
            Action<UiPayload> setPayload)
        {
            string type = UiNodeReader.GetString(dict, "type") ?? "section";
            string title = UiNodeReader.GetString(dict, "title") ?? string.Empty;
            string id = UiNodeReader.GetString(dict, "id");

            Control wrapper;
            FlowLayoutPanel inner;
            Label header = null;

            if (type == "card")
            {
                var group = new GroupBox
                {
                    Text = title,
                    ForeColor = Color.Gainsboro,
                    Font = new Font("Consolas", 9f, FontStyle.Bold),
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(12, 18, 12, 12),
                    BackColor = Color.FromArgb(18, 20, 24)
                };
                inner = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(0, 4, 0, 6),
                    Margin = new Padding(0),
                    Location = new Point(12, 24)
                };
                group.Controls.Add(inner);
                wrapper = group;
            }
            else
            {
                var panel = new Panel
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BackColor = Color.FromArgb(18, 20, 24),
                    Padding = new Padding(0, 0, 0, 6)
                };

                if (!string.IsNullOrWhiteSpace(title))
                {
                    header = new Label
                    {
                        AutoSize = true,
                        ForeColor = Color.Silver,
                        Font = new Font("Consolas", type == "page" ? 12f : 10f, FontStyle.Bold),
                        Text = title,
                        Margin = new Padding(0, 0, 0, 4)
                    };
                    labels.Add(header);
                    panel.Controls.Add(header);
                }

                inner = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(type == "section" ? 12 : 0, 4, 0, 8),
                    Margin = new Padding(0),
                    Top = header != null ? header.Bottom : 0
                };
                panel.Controls.Add(inner);
                wrapper = panel;
            }

            var children = UiNodeReader.GetArray(dict, "children");
            if (_debug && children.Length == 0 && dict.ContainsKey("children") && dict["children"] != null)
            {
                _logger.Log(string.Format(
                    "Children present but not parsed. Type={0}",
                    dict["children"].GetType().FullName));
            }

            SyncContainer(inner, children, labels, tables, toolTip, rerender, setPayload);

            wrapper.Tag = new NodeTag(id, type, inner, header);
            return wrapper;
        }

        private static Control CreateTable(Dictionary<string, object> dict, List<ListView> tables, string id)
        {
            var listView = new ListView
            {
                View = View.Details,
                FullRowSelect = false,
                GridLines = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                BackColor = Color.FromArgb(22, 24, 28),
                ForeColor = Color.Gainsboro
            };

            var headers = UiNodeReader.GetArray(dict, "headers");
            foreach (var header in headers)
            {
                listView.Columns.Add(header != null ? header.ToString() : string.Empty);
            }

            foreach (var rowObj in UiNodeReader.GetArray(dict, "rows"))
            {
                var rowArr = rowObj as object[] ?? (rowObj as ArrayList != null ? ((ArrayList)rowObj).Cast<object>().ToArray() : new object[0]);
                if (rowArr.Length == 0)
                {
                    continue;
                }

                var items = rowArr.Select(cell => cell != null ? cell.ToString() : string.Empty).ToArray();
                var item = new ListViewItem(items);
                listView.Items.Add(item);
            }

            tables.Add(listView);
            listView.Tag = new NodeTag(id, "table");
            return listView;
        }

        private Control CreateButtonRow(
            Dictionary<string, object> dict,
            ToolTip toolTip,
            Action rerender,
            Action<UiPayload> setPayload,
            string id)
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(18, 20, 24)
            };

            PopulateButtonRow(panel, dict, toolTip, rerender, setPayload);

            panel.Tag = new NodeTag(id, "buttonRow");
            return panel;
        }

        private void UpdateButtonRow(
            FlowLayoutPanel panel,
            Dictionary<string, object> dict,
            ToolTip toolTip,
            Action rerender,
            Action<UiPayload> setPayload)
        {
            if (panel == null)
            {
                return;
            }

            PopulateButtonRow(panel, dict, toolTip, rerender, setPayload);
        }

        private void PopulateButtonRow(
            FlowLayoutPanel panel,
            Dictionary<string, object> dict,
            ToolTip toolTip,
            Action rerender,
            Action<UiPayload> setPayload)
        {
            panel.SuspendLayout();
            panel.Controls.Clear();

            foreach (var buttonObj in UiNodeReader.GetArray(dict, "buttons"))
            {
                var buttonDict = buttonObj as Dictionary<string, object>;
                if (buttonDict == null)
                {
                    continue;
                }

                string label = UiNodeReader.GetString(buttonDict, "label") ?? "Action";
                string hint = UiNodeReader.GetString(buttonDict, "hint");
                var args = UiNodeReader.GetArray(buttonDict, "args");

                var button = new Button
                {
                    Text = label,
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(32, 36, 44),
                    ForeColor = Color.Gainsboro,
                    Margin = new Padding(6, 4, 6, 4),
                    Padding = new Padding(10, 6, 10, 6),
                    Font = new Font("Consolas", 9f, FontStyle.Bold)
                };
                button.FlatAppearance.BorderColor = Color.FromArgb(80, 90, 110);
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 110, 170);
                button.FlatAppearance.MouseDownBackColor = Color.FromArgb(55, 75, 120);

                if (!string.IsNullOrWhiteSpace(hint))
                {
                    toolTip.SetToolTip(button, hint);
                }

                button.Click += delegate
                {
                    string[] actionArgs = args.Select(a => a != null ? a.ToString() : string.Empty).ToArray();
                    string[] finalArgs = BuildArgsForAction(_baseArgs, actionArgs);
                    if (_debug)
                    {
                        _logger.Log("Button click args: " + string.Join(" ", actionArgs));
                        _logger.Log("Button click final args: " + string.Join(" ", finalArgs));
                    }
                    int ignored;
                    UiPayload next = _session.Execute(finalArgs, out ignored);
                    setPayload(next);
                    rerender();
                };

                panel.Controls.Add(button);
            }

            panel.ResumeLayout(true);
        }

        private Control CreateTextInput(
            Dictionary<string, object> dict,
            Action rerender,
            Action<UiPayload> setPayload,
            string id)
        {
            string label = UiNodeReader.GetString(dict, "label") ?? UiNodeReader.GetString(dict, "name") ?? "input";
            string name = UiNodeReader.GetString(dict, "name") ?? label;
            bool required = UiNodeReader.GetBool(dict, "required");
            string helper = UiNodeReader.GetString(dict, "helperText");
            string placeholder = UiNodeReader.GetString(dict, "placeholder");
            string value = UiNodeReader.GetString(dict, "value") ?? string.Empty;
            string onChangeAction = UiNodeReader.GetString(dict, "onChangeAction");

            var panel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.FromArgb(18, 20, 24)
            };

            var labelControl = new Label
            {
                AutoSize = true,
                ForeColor = Color.Gainsboro,
                Font = new Font("Consolas", 9f, FontStyle.Bold),
                Text = required ? label + " *" : label
            };

            var input = new TextBox
            {
                Width = 240,
                ForeColor = Color.Gainsboro,
                BackColor = Color.FromArgb(32, 36, 44),
                BorderStyle = BorderStyle.FixedSingle
            };

            string cachedValue;
            if (_inputState.TryGetValue(name, out cachedValue))
            {
                input.Text = cachedValue;
                input.ForeColor = Color.Gainsboro;
            }
            else if (!string.IsNullOrWhiteSpace(value))
            {
                input.Text = value;
            }
            else if (!string.IsNullOrWhiteSpace(placeholder))
            {
                input.Text = placeholder;
                input.ForeColor = Color.DimGray;
            }

            var inputTag = new InputTag(name, onChangeAction, labelControl, input, null);
            panel.Tag = new NodeTag(id, "textInput", null, null, inputTag);

            if (!string.IsNullOrWhiteSpace(onChangeAction))
            {
                string oldValue = input.Text;
                bool ready = false;
                var debounce = new Timer { Interval = 250 };
                debounce.Tick += delegate
                {
                    debounce.Stop();
                    string newValue = input.Text;
                    if (newValue == oldValue)
                    {
                        return;
                    }

                    _inputState[name] = newValue;
                    string actionId = inputTag.ActionId;
                    if (string.IsNullOrWhiteSpace(actionId))
                    {
                        oldValue = newValue;
                        return;
                    }
                    string[] finalArgs = BuildArgsForAction(_baseArgs, new[] { "--action", actionId, "--value", newValue, "--old", oldValue });
                    int ignored;
                    UiPayload next = _session.Execute(finalArgs, out ignored);
                    setPayload(next);
                    rerender();
                    oldValue = newValue;
                };

                input.TextChanged += delegate
                {
                    if (!ready)
                    {
                        ready = true;
                        oldValue = input.Text;
                        return;
                    }

                    _inputState[name] = input.Text;
                    debounce.Stop();
                    debounce.Start();
                };
            }
            else
            {
                input.TextChanged += delegate
                {
                    _inputState[name] = input.Text;
                };
            }

            panel.Controls.Add(labelControl);
            panel.Controls.Add(input);
            input.Top = labelControl.Bottom + 4;

            if (!string.IsNullOrWhiteSpace(helper))
            {
                var helperLabel = new Label
                {
                    AutoSize = true,
                    ForeColor = Color.DimGray,
                    Font = new Font("Consolas", 8f),
                    Text = helper,
                    Top = input.Bottom + 4
                };
                panel.Controls.Add(helperLabel);
                inputTag.Helper = helperLabel;
            }

            return panel;
        }

        private Control CreateSelect(
            Dictionary<string, object> dict,
            Action rerender,
            Action<UiPayload> setPayload,
            string id)
        {
            string label = UiNodeReader.GetString(dict, "label") ?? UiNodeReader.GetString(dict, "name") ?? "select";
            string name = UiNodeReader.GetString(dict, "name") ?? label;
            bool required = UiNodeReader.GetBool(dict, "required");
            string helper = UiNodeReader.GetString(dict, "helperText");
            var optionsDict = dict.ContainsKey("options") ? dict["options"] as Dictionary<string, object> : null;
            string value = UiNodeReader.GetString(dict, "value") ?? string.Empty;
            string onChangeAction = UiNodeReader.GetString(dict, "onChangeAction");

            var panel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.FromArgb(18, 20, 24)
            };

            var labelControl = new Label
            {
                AutoSize = true,
                ForeColor = Color.Gainsboro,
                Font = new Font("Consolas", 9f, FontStyle.Bold),
                Text = required ? label + " *" : label
            };

            var combo = new ComboBox
            {
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(32, 36, 44),
                ForeColor = Color.Gainsboro
            };

            if (optionsDict != null)
            {
                foreach (var entry in optionsDict)
                {
                    combo.Items.Add(new ComboItem(entry.Key, entry.Value != null ? entry.Value.ToString() : entry.Key));
                }
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                foreach (ComboItem item in combo.Items)
                {
                    if (item.Value == value || item.Key == value)
                    {
                        combo.SelectedItem = item;
                        break;
                    }
                }
            }

            var selectTag = new SelectTag(name, onChangeAction, labelControl, combo, null);
            panel.Tag = new NodeTag(id, "select", null, null, selectTag);

            string cachedValue;
            if (_inputState.TryGetValue(name, out cachedValue))
            {
                foreach (ComboItem item in combo.Items)
                {
                    if (item.Value == cachedValue || item.Key == cachedValue)
                    {
                        combo.SelectedItem = item;
                        break;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(onChangeAction))
            {
                string oldValue = GetComboValue(combo.SelectedItem as ComboItem);
                bool ready = false;
                combo.SelectedIndexChanged += delegate
                {
                    if (!ready)
                    {
                        ready = true;
                        oldValue = GetComboValue(combo.SelectedItem as ComboItem);
                        return;
                    }

                    string newValue = GetComboValue(combo.SelectedItem as ComboItem);
                    _inputState[name] = newValue;
                    string actionId = selectTag.ActionId;
                    if (string.IsNullOrWhiteSpace(actionId))
                    {
                        oldValue = newValue;
                        return;
                    }
                    string[] finalArgs = BuildArgsForAction(_baseArgs, new[] { "--action", actionId, "--value", newValue, "--old", oldValue });
                    int ignored;
                    UiPayload next = _session.Execute(finalArgs, out ignored);
                    setPayload(next);
                    rerender();
                    oldValue = newValue;
                };
            }
            else
            {
                combo.SelectedIndexChanged += delegate
                {
                    _inputState[name] = GetComboValue(combo.SelectedItem as ComboItem);
                };
            }

            panel.Controls.Add(labelControl);
            panel.Controls.Add(combo);
            combo.Top = labelControl.Bottom + 4;

            if (!string.IsNullOrWhiteSpace(helper))
            {
                var helperLabel = new Label
                {
                    AutoSize = true,
                    ForeColor = Color.DimGray,
                    Font = new Font("Consolas", 8f),
                    Text = helper,
                    Top = combo.Bottom + 4
                };
                panel.Controls.Add(helperLabel);
                selectTag.Helper = helperLabel;
            }

            return panel;
        }

        private void UpdateGroup(
            Control wrapper,
            Dictionary<string, object> dict,
            List<Label> labels,
            List<ListView> tables,
            ToolTip toolTip,
            Action rerender,
            Action<UiPayload> setPayload)
        {
            var tag = wrapper.Tag as NodeTag;
            if (tag == null)
            {
                return;
            }

            string title = UiNodeReader.GetString(dict, "title") ?? string.Empty;
            var groupBox = wrapper as GroupBox;
            if (groupBox != null)
            {
                groupBox.Text = title;
            }

            if (tag.Header != null)
            {
                tag.Header.Text = title;
                labels.Add(tag.Header);
            }

            var inner = tag.Inner ?? wrapper.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
            if (inner != null)
            {
                SyncContainer(inner, UiNodeReader.GetArray(dict, "children"), labels, tables, toolTip, rerender, setPayload);
            }
        }

        private void UpdateTable(ListView table, Dictionary<string, object> dict, List<ListView> tables)
        {
            if (table == null)
            {
                return;
            }

            table.BeginUpdate();
            table.Columns.Clear();
            table.Items.Clear();

            var headers = UiNodeReader.GetArray(dict, "headers");
            foreach (var header in headers)
            {
                table.Columns.Add(header != null ? header.ToString() : string.Empty);
            }

            foreach (var rowObj in UiNodeReader.GetArray(dict, "rows"))
            {
                var rowArr = rowObj as object[] ?? (rowObj as ArrayList != null ? ((ArrayList)rowObj).Cast<object>().ToArray() : new object[0]);
                if (rowArr.Length == 0)
                {
                    continue;
                }

                var items = rowArr.Select(cell => cell != null ? cell.ToString() : string.Empty).ToArray();
                var item = new ListViewItem(items);
                table.Items.Add(item);
            }

            foreach (ColumnHeader column in table.Columns)
            {
                column.Width = -2;
            }

            table.EndUpdate();
            tables.Add(table);
        }

        private void UpdateTextInput(
            Panel panel,
            Dictionary<string, object> dict,
            Action rerender,
            Action<UiPayload> setPayload)
        {
            if (panel == null)
            {
                return;
            }

            var nodeTag = panel.Tag as NodeTag;
            var inputTag = nodeTag != null ? nodeTag.Payload as InputTag : null;
            if (inputTag == null)
            {
                return;
            }

            string label = UiNodeReader.GetString(dict, "label") ?? UiNodeReader.GetString(dict, "name") ?? "input";
            string name = UiNodeReader.GetString(dict, "name") ?? label;
            bool required = UiNodeReader.GetBool(dict, "required");
            string helper = UiNodeReader.GetString(dict, "helperText");
            string placeholder = UiNodeReader.GetString(dict, "placeholder");
            string value = UiNodeReader.GetString(dict, "value") ?? string.Empty;
            string onChangeAction = UiNodeReader.GetString(dict, "onChangeAction");

            inputTag.Name = name;
            inputTag.ActionId = onChangeAction;
            inputTag.Label.Text = required ? label + " *" : label;

            if (!_inputState.ContainsKey(name))
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    inputTag.Input.Text = value;
                    inputTag.Input.ForeColor = Color.Gainsboro;
                }
                else if (!string.IsNullOrWhiteSpace(placeholder))
                {
                    inputTag.Input.Text = placeholder;
                    inputTag.Input.ForeColor = Color.DimGray;
                }
            }

            if (!string.IsNullOrWhiteSpace(helper))
            {
                if (inputTag.Helper == null)
                {
                    var helperLabel = new Label
                    {
                        AutoSize = true,
                        ForeColor = Color.DimGray,
                        Font = new Font("Consolas", 8f),
                        Text = helper,
                        Top = inputTag.Input.Bottom + 4
                    };
                    panel.Controls.Add(helperLabel);
                    inputTag.Helper = helperLabel;
                }
                else
                {
                    inputTag.Helper.Text = helper;
                }
            }
            else if (inputTag.Helper != null)
            {
                panel.Controls.Remove(inputTag.Helper);
                inputTag.Helper.Dispose();
                inputTag.Helper = null;
            }
        }

        private void UpdateSelect(
            Panel panel,
            Dictionary<string, object> dict,
            Action rerender,
            Action<UiPayload> setPayload)
        {
            if (panel == null)
            {
                return;
            }

            var nodeTag = panel.Tag as NodeTag;
            var selectTag = nodeTag != null ? nodeTag.Payload as SelectTag : null;
            if (selectTag == null)
            {
                return;
            }

            string label = UiNodeReader.GetString(dict, "label") ?? UiNodeReader.GetString(dict, "name") ?? "select";
            string name = UiNodeReader.GetString(dict, "name") ?? label;
            bool required = UiNodeReader.GetBool(dict, "required");
            string helper = UiNodeReader.GetString(dict, "helperText");
            var optionsDict = dict.ContainsKey("options") ? dict["options"] as Dictionary<string, object> : null;
            string value = UiNodeReader.GetString(dict, "value") ?? string.Empty;
            string onChangeAction = UiNodeReader.GetString(dict, "onChangeAction");

            selectTag.Name = name;
            selectTag.ActionId = onChangeAction;
            selectTag.Label.Text = required ? label + " *" : label;

            if (optionsDict != null)
            {
                selectTag.Combo.Items.Clear();
                foreach (var entry in optionsDict)
                {
                    selectTag.Combo.Items.Add(new ComboItem(entry.Key, entry.Value != null ? entry.Value.ToString() : entry.Key));
                }
            }

            string cachedValue;
            if (_inputState.TryGetValue(name, out cachedValue))
            {
                foreach (ComboItem item in selectTag.Combo.Items)
                {
                    if (item.Value == cachedValue || item.Key == cachedValue)
                    {
                        selectTag.Combo.SelectedItem = item;
                        break;
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(value))
            {
                foreach (ComboItem item in selectTag.Combo.Items)
                {
                    if (item.Value == value || item.Key == value)
                    {
                        selectTag.Combo.SelectedItem = item;
                        break;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(helper))
            {
                if (selectTag.Helper == null)
                {
                    var helperLabel = new Label
                    {
                        AutoSize = true,
                        ForeColor = Color.DimGray,
                        Font = new Font("Consolas", 8f),
                        Text = helper,
                        Top = selectTag.Combo.Bottom + 4
                    };
                    panel.Controls.Add(helperLabel);
                    selectTag.Helper = helperLabel;
                }
                else
                {
                    selectTag.Helper.Text = helper;
                }
            }
            else if (selectTag.Helper != null)
            {
                panel.Controls.Remove(selectTag.Helper);
                selectTag.Helper.Dispose();
                selectTag.Helper = null;
            }
        }

        private static string GetComboValue(ComboItem item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            return item.Key ?? item.Value ?? string.Empty;
        }

        private static string[] BuildArgsForAction(string[] baseArgs, string[] actionArgs)
        {
            var flags = baseArgs.Where(arg => arg.StartsWith("-", StringComparison.Ordinal)).ToList();
            var args = new List<string>();
            if (actionArgs != null)
            {
                args.AddRange(actionArgs);
            }
            args.AddRange(flags);
            return args.ToArray();
        }

        private sealed class ComboItem
        {
            public string Key { get; private set; }
            public string Value { get; private set; }

            public ComboItem(string key, string value)
            {
                Key = key;
                Value = value;
            }

            public override string ToString()
            {
                return Value;
            }
        }

        private sealed class NodeTag
        {
            public NodeTag(string id, string type, FlowLayoutPanel inner = null, Label header = null, object payload = null)
            {
                Id = id;
                Type = type;
                Inner = inner;
                Header = header;
                Payload = payload;
            }

            public string Id { get; private set; }
            public string Type { get; private set; }
            public FlowLayoutPanel Inner { get; private set; }
            public Label Header { get; private set; }
            public object Payload { get; set; }
        }

        private sealed class InputTag
        {
            public InputTag(string name, string actionId, Label label, TextBox input, Label helper)
            {
                Name = name;
                ActionId = actionId;
                Label = label;
                Input = input;
                Helper = helper;
            }

            public string Name { get; set; }
            public string ActionId { get; set; }
            public Label Label { get; private set; }
            public TextBox Input { get; private set; }
            public Label Helper { get; set; }
        }

        private sealed class SelectTag
        {
            public SelectTag(string name, string actionId, Label label, ComboBox combo, Label helper)
            {
                Name = name;
                ActionId = actionId;
                Label = label;
                Combo = combo;
                Helper = helper;
            }

            public string Name { get; set; }
            public string ActionId { get; set; }
            public Label Label { get; private set; }
            public ComboBox Combo { get; private set; }
            public Label Helper { get; set; }
        }
    }
}
