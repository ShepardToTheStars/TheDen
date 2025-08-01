// SPDX-FileCopyrightText: 2022 theashtronaut
// SPDX-FileCopyrightText: 2023 Nemanja
// SPDX-FileCopyrightText: 2023 metalgearsloth
// SPDX-FileCopyrightText: 2023 qwerltaz
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT
// SPDX-FileCopyrightText: 2025 ArtisticRoomba
// SPDX-FileCopyrightText: 2025 chromiumboy
// SPDX-FileCopyrightText: 2025 sleepyyapril
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared.Atmos;
using Content.Shared.Temperature;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using static Content.Shared.Atmos.Components.GasAnalyzerComponent;
using Direction = Robust.Shared.Maths.Direction;

namespace Content.Client.Atmos.UI
{
    [GenerateTypedNameReferences]
    public sealed partial class GasAnalyzerWindow : DefaultWindow
    {
        public GasAnalyzerWindow()
        {
            RobustXamlLoader.Load(this);
        }

        public void Populate(GasAnalyzerUserMessage msg)
        {
            if (msg.Error != null)
            {
                CTopBox.AddChild(new Label
                {
                    Text = Loc.GetString("gas-analyzer-window-error-text", ("errorText", msg.Error)),
                    FontColorOverride = Color.Red
                });
                return;
            }

            if (msg.NodeGasMixes.Length == 0)
            {
                CTopBox.AddChild(new Label
                {
                    Text = Loc.GetString("gas-analyzer-window-no-data")
                });
                MinSize = new Vector2(CTopBox.DesiredSize.X + 40, MinSize.Y);
                return;
            }

            Vector2 minSize;

            // Environment Tab
            var envMix = msg.NodeGasMixes[0];

            CTabContainer.SetTabTitle(1, envMix.Name);
            CEnvironmentMix.RemoveAllChildren();
            GenerateGasDisplay(envMix, CEnvironmentMix);

            // Device Tab
            if (msg.NodeGasMixes.Length > 1)
            {
                CTabContainer.SetTabVisible(0, true);
                CTabContainer.SetTabTitle(0, Loc.GetString("gas-analyzer-window-tab-title-capitalized", ("title", msg.DeviceName)));
                // Set up Grid
                GridIcon.OverrideDirection = msg.NodeGasMixes.Length switch
                {
                    // Unary layout
                    2 => Direction.South,
                    // Binary layout
                    3 => Direction.East,
                    // Trinary layout
                    4 => Direction.East,
                    _ => GridIcon.OverrideDirection
                };

                GridIcon.SetEntity(IoCManager.Resolve<IEntityManager>().GetEntity(msg.DeviceUid));
                LeftPanel.RemoveAllChildren();
                MiddlePanel.RemoveAllChildren();
                RightPanel.RemoveAllChildren();
                if (msg.NodeGasMixes.Length == 2)
                {
                    // Unary, use middle
                    LeftPanelLabel.Text = string.Empty;
                    MiddlePanelLabel.Text = Loc.GetString("gas-analyzer-window-tab-title-capitalized", ("title", msg.NodeGasMixes[1].Name));
                    RightPanelLabel.Text = string.Empty;

                    LeftPanel.Visible = false;
                    MiddlePanel.Visible = true;
                    RightPanel.Visible = false;

                    GenerateGasDisplay(msg.NodeGasMixes[1], MiddlePanel);

                    minSize = new Vector2(CDeviceGrid.DesiredSize.X + 40, MinSize.Y);
                }
                else if (msg.NodeGasMixes.Length == 3)
                {
                    // Binary, use left and right
                    LeftPanelLabel.Text = Loc.GetString("gas-analyzer-window-tab-title-capitalized", ("title", msg.NodeGasMixes[1].Name));
                    MiddlePanelLabel.Text = string.Empty;
                    RightPanelLabel.Text = Loc.GetString("gas-analyzer-window-tab-title-capitalized", ("title", msg.NodeGasMixes[2].Name));

                    LeftPanel.Visible = true;
                    MiddlePanel.Visible = false;
                    RightPanel.Visible = true;

                    GenerateGasDisplay(msg.NodeGasMixes[1], LeftPanel);
                    GenerateGasDisplay(msg.NodeGasMixes[2], RightPanel);

                    minSize = new Vector2(CDeviceGrid.DesiredSize.X + 40, MinSize.Y);
                }
                else if (msg.NodeGasMixes.Length == 4)
                {
                    // Trinary, use all three
                    // Trinary can be flippable, which complicates how to display things currently
                    LeftPanelLabel.Text = Loc.GetString("gas-analyzer-window-tab-title-capitalized",
                        ("title", msg.DeviceFlipped ? msg.NodeGasMixes[1].Name : msg.NodeGasMixes[3].Name));
                    MiddlePanelLabel.Text = Loc.GetString("gas-analyzer-window-tab-title-capitalized", ("title", msg.NodeGasMixes[2].Name));
                    RightPanelLabel.Text = Loc.GetString("gas-analyzer-window-tab-title-capitalized",
                        ("title", msg.DeviceFlipped ? msg.NodeGasMixes[3].Name : msg.NodeGasMixes[1].Name));

                    LeftPanel.Visible = true;
                    MiddlePanel.Visible = true;
                    RightPanel.Visible = true;

                    GenerateGasDisplay(msg.DeviceFlipped ? msg.NodeGasMixes[1] : msg.NodeGasMixes[3], LeftPanel);
                    GenerateGasDisplay(msg.NodeGasMixes[2], MiddlePanel);
                    GenerateGasDisplay(msg.DeviceFlipped ? msg.NodeGasMixes[3] : msg.NodeGasMixes[1], RightPanel);

                    minSize = new Vector2(CDeviceGrid.DesiredSize.X + 40, MinSize.Y);
                }
                else
                {
                    // oh shit of fuck its more than 4 this ui isn't gonna look pretty anymore
                    CDeviceMixes.RemoveAllChildren();
                    for (var i = 1; i < msg.NodeGasMixes.Length; i++)
                    {
                        GenerateGasDisplay(msg.NodeGasMixes[i], CDeviceMixes);
                    }
                    LeftPanel.Visible = false;
                    MiddlePanel.Visible = false;
                    RightPanel.Visible = false;
                    minSize = new Vector2(CDeviceMixes.DesiredSize.X + 40, MinSize.Y);
                }
            }
            else
            {
                // Hide device tab, no device selected
                CTabContainer.SetTabVisible(0, false);
                CTabContainer.CurrentTab = 1;
                minSize = new Vector2(CEnvironmentMix.DesiredSize.X + 40, MinSize.Y);
            }

            MinSize = minSize;
        }

        private void GenerateGasDisplay(GasMixEntry gasMix, Control parent)
        {
            var panel = new PanelContainer
            {
                VerticalExpand = true,
                HorizontalExpand = true,
                Margin = new Thickness(4),
                PanelOverride = new StyleBoxFlat{BorderColor = Color.FromHex("#4f4f4f"), BorderThickness = new Thickness(1)}
            };
            var dataContainer = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Vertical, VerticalExpand = true, Margin = new Thickness(4)};


            parent.AddChild(panel);
            panel.AddChild(dataContainer);

            // Volume label
            var volBox = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal };

            volBox.AddChild(new Label
            {
                Text = Loc.GetString("gas-analyzer-window-volume-text")
            });
            volBox.AddChild(new Control
            {
                MinSize = new Vector2(10, 0),
                HorizontalExpand = true
            });
            volBox.AddChild(new Label
            {
                Text = Loc.GetString("gas-analyzer-window-volume-val-text", ("volume", $"{gasMix.Volume:0.##}")),
                Align = Label.AlignMode.Right,
                HorizontalExpand = true
            });
            dataContainer.AddChild(volBox);

            // Pressure label
            var presBox = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal };

            presBox.AddChild(new Label
            {
                Text = Loc.GetString("gas-analyzer-window-pressure-text")
            });
            presBox.AddChild(new Control
            {
                MinSize = new Vector2(10, 0),
                HorizontalExpand = true
            });
            presBox.AddChild(new Label
            {
                Text = Loc.GetString("gas-analyzer-window-pressure-val-text", ("pressure", $"{gasMix.Pressure:0.##}")),
                Align = Label.AlignMode.Right,
                HorizontalExpand = true
            });
            dataContainer.AddChild(presBox);

            // If there is no gas, it doesn't really have a temperature, so skip displaying it
            if (gasMix.Pressure > Atmospherics.GasMinMoles)
            {
                // Temperature label
                var tempBox = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal };

                tempBox.AddChild(new Label
                {
                    Text = Loc.GetString("gas-analyzer-window-temperature-text")
                });
                tempBox.AddChild(new Control
                {
                    MinSize = new Vector2(10, 0),
                    HorizontalExpand = true
                });
                tempBox.AddChild(new Label
                {
                    Text = Loc.GetString("gas-analyzer-window-temperature-val-text",
                        ("tempK", $"{gasMix.Temperature:0.#}"),
                        ("tempC", $"{TemperatureHelpers.KelvinToCelsius(gasMix.Temperature):0.#}")),
                    Align = Label.AlignMode.Right,
                    HorizontalExpand = true
                });
                dataContainer.AddChild(tempBox);
            }

            if (gasMix.Gases == null || gasMix.Gases?.Length == 0)
            {
                // Separator
                dataContainer.AddChild(new Control
                {
                    MinSize = new Vector2(0, 10)
                });

                // Add a label that there are no gases so it's less confusing
                dataContainer.AddChild(new Label
                {
                    Text = Loc.GetString("gas-analyzer-window-no-gas-text"),
                    FontColorOverride = Color.Gray
                });
                // return, everything below is for the fancy gas display stuff
                return;
            }
            // Separator
            dataContainer.AddChild(new Control
            {
                MinSize = new Vector2(0, 10)
            });

            // Add a table with all the gases
            var tableKey = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical
            };
            var tableVal = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical
            };
            var tablePercent = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical
            };
            dataContainer.AddChild(new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                Children =
                {
                    tableKey,
                    new Control
                    {
                        MinSize = new Vector2(10, 0),
                        HorizontalExpand = true
                    },
                    tableVal,
                    new Control
                    {
                        MinSize = new Vector2(10, 0),
                        HorizontalExpand = true
                    },
                    tablePercent
                }
            });
            // This is the gas bar thingy
            var height = 30;
            var gasBar = new SplitBar
            {
                MinHeight = height,
                MinBarSize = new Vector2(12, 0)
            };
            // Separator
            dataContainer.AddChild(new Control
            {
                MinSize = new Vector2(0, 10),
                VerticalExpand = true
            });

            var totalGasAmount = 0f;
            foreach (var gas in gasMix.Gases!)
            {
                totalGasAmount += gas.Amount;
            }

            tableKey.AddChild(new Label
                { Text = Loc.GetString("gas-analyzer-window-gas-column-name"), Align = Label.AlignMode.Center });
            tableVal.AddChild(new Label
                { Text = Loc.GetString("gas-analyzer-window-molarity-column-name"), Align = Label.AlignMode.Center });
            tablePercent.AddChild(new Label
                { Text = Loc.GetString("gas-analyzer-window-percentage-column-name"), Align = Label.AlignMode.Center });

            tableKey.AddChild(new StripeBack());
            tableVal.AddChild(new StripeBack());
            tablePercent.AddChild(new StripeBack());

            for (var j = 0; j < gasMix.Gases.Length; j++)
            {
                var gas = gasMix.Gases[j];
                var color = Color.FromHex($"#{gas.Color}", Color.White);
                // Add to the table
                tableKey.AddChild(new Label
                {
                    Text = Loc.GetString(gas.Name)
                });
                tableVal.AddChild(new Label
                {
                    Text = Loc.GetString("gas-analyzer-window-molarity-text",
                        ("mol", $"{gas.Amount:0.00}")),
                    Align = Label.AlignMode.Right,
                });
                tablePercent.AddChild(new Label
                {
                    Text = Loc.GetString("gas-analyzer-window-percentage-text",
                        ("percentage", $"{(gas.Amount / totalGasAmount * 100):0.0}")),
                    Align = Label.AlignMode.Right
                });

                // Add to the gas bar //TODO: highlight the currently hover one
                gasBar.AddEntry(gas.Amount, color, tooltip: Loc.GetString("gas-analyzer-window-molarity-percentage-text",
                    ("gasName", gas.Name),
                    ("amount", $"{gas.Amount:0.##}"),
                    ("percentage", $"{(gas.Amount / totalGasAmount * 100):0.#}")));
            }

            dataContainer.AddChild(gasBar);
        }
    }
}
