using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AdhesionTest
{
    /// <summary>
    ///     Interaction logic for linearTrial.xaml
    /// </summary>
    public partial class linearTrial : Page
    {
        private const double gridHeight = 60;
        private int numTests = 1;
        private readonly linearTrialManager trial;

        public linearTrial(linearTrialManager trial)
        {
            this.trial = trial;
            InitializeComponent();
        }

        /// <summary>
        ///     Adds a row when the add trial button is clicked, this has to be dynamically
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addTestClick(object sender, RoutedEventArgs e)
        {
            //Add a row
            var row = new RowDefinition();
            row.Height = new GridLength(gridHeight);
            trialsGrid.RowDefinitions.Add(row);

            var leftCol = new Grid();
            //Add two columns
            var col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            leftCol.ColumnDefinitions.Add(col);
            leftCol.HorizontalAlignment = HorizontalAlignment.Center;
            leftCol.VerticalAlignment = VerticalAlignment.Center;
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            leftCol.ColumnDefinitions.Add(col);
            Grid.SetRow(leftCol, numTests);
            Grid.SetColumn(leftCol, 0);

            var numCyclesLabel = new Label();
            numCyclesLabel.Padding = new Thickness(4);
            numCyclesLabel.FontWeight = FontWeights.Thin;
            numCyclesLabel.HorizontalAlignment = HorizontalAlignment.Center;
            numCyclesLabel.VerticalAlignment = VerticalAlignment.Center;
            numCyclesLabel.FontSize = 18;
            numCyclesLabel.Opacity = 0.8;
            numCyclesLabel.Content = "Number of cycles    ";
            Grid.SetColumn(numCyclesLabel, 0);

            var cycleNumBox = new TextBox();
            cycleNumBox.Padding = new Thickness(3);
            cycleNumBox.Height = 40;
            cycleNumBox.Width = 100;
            cycleNumBox.Text = "1";
            cycleNumBox.FontSize = 18;
            cycleNumBox.FontWeight = FontWeights.Thin;
            cycleNumBox.Opacity = 0.8;
            cycleNumBox.MaxLines = 1;
            cycleNumBox.HorizontalAlignment = HorizontalAlignment.Center;
            cycleNumBox.VerticalAlignment = VerticalAlignment.Center;
            cycleNumBox.TextWrapping = TextWrapping.Wrap;
            cycleNumBox.Name = "cycleNumBox" + numTests;
            Console.WriteLine("cycleNumBox" + numTests);
            cycleNumBox.PreviewTextInput += TextBoxPreviewTextInput; //add event
            Grid.SetColumn(cycleNumBox, 1);

            leftCol.Children.Add(numCyclesLabel);
            leftCol.Children.Add(cycleNumBox);

            trialsGrid.Children.Add(leftCol);

            var midCol = new Grid();
            midCol.HorizontalAlignment = HorizontalAlignment.Center;
            midCol.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(midCol, 1);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            midCol.ColumnDefinitions.Add(col);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            midCol.ColumnDefinitions.Add(col);
            Grid.SetRow(midCol, numTests);

            var preloadForce = new Label();
            preloadForce.Content = "Preload force:    ";
            preloadForce.Padding = new Thickness(4);
            preloadForce.FontWeight = FontWeights.Thin;
            preloadForce.FontSize = 18;
            preloadForce.Opacity = 0.8;
            Grid.SetColumn(preloadForce, 0);

            var innerGrid = new Grid();
            innerGrid.HorizontalAlignment = HorizontalAlignment.Center;
            Grid.SetColumn(innerGrid, 1);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            innerGrid.ColumnDefinitions.Add(col);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            innerGrid.ColumnDefinitions.Add(col);

            var preloadForceBox = new TextBox();
            ;
            preloadForceBox.Padding = new Thickness(3);
            preloadForceBox.FontWeight = FontWeights.Thin;
            preloadForceBox.FontSize = 18;
            preloadForceBox.Opacity = 0.8;
            preloadForceBox.Width = 100;
            preloadForceBox.Height = 40;
            preloadForceBox.HorizontalAlignment = HorizontalAlignment.Center;
            preloadForceBox.Text = "3";
            preloadForceBox.VerticalAlignment = VerticalAlignment.Top;
            preloadForceBox.MaxLines = 1;
            preloadForceBox.TextWrapping = TextWrapping.Wrap;
            preloadForceBox.PreviewTextInput += TextBoxPreviewTextInput; //add event
            preloadForceBox.Name = "preloadForceBox" + numTests;

            var preloadUnits = new Label();
            preloadUnits.Content = "mN";
            preloadUnits.Padding = new Thickness(4);
            preloadUnits.FontWeight = FontWeights.Thin;
            preloadUnits.FontSize = 18;
            preloadUnits.Opacity = 0.8;
            Grid.SetColumn(preloadUnits, 1);

            innerGrid.Children.Add(preloadForceBox);
            innerGrid.Children.Add(preloadUnits);
            midCol.Children.Add(preloadForce);
            midCol.Children.Add(innerGrid);
            trialsGrid.Children.Add(midCol);

            var rightCol = new Grid();
            rightCol.HorizontalAlignment = HorizontalAlignment.Center;
            rightCol.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(rightCol, 2);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            rightCol.ColumnDefinitions.Add(col);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            rightCol.ColumnDefinitions.Add(col);
            Grid.SetRow(rightCol, numTests);

            var dragDistance = new Label();
            dragDistance.Content = "Drag distance:    ";
            dragDistance.Padding = new Thickness(4);
            dragDistance.FontWeight = FontWeights.Thin;
            dragDistance.FontSize = 18;
            dragDistance.Opacity = 0.8;

            innerGrid = new Grid();
            innerGrid.HorizontalAlignment = HorizontalAlignment.Center;
            Grid.SetColumn(innerGrid, 1);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            innerGrid.ColumnDefinitions.Add(col);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            innerGrid.ColumnDefinitions.Add(col);

            var dragDistanceBox = new TextBox();
            dragDistanceBox.Padding = new Thickness(3);
            dragDistanceBox.Height = 40;
            dragDistanceBox.Width = 100;
            dragDistanceBox.FontSize = 18;
            dragDistanceBox.FontWeight = FontWeights.Thin;
            dragDistanceBox.Opacity = 0.8;
            dragDistanceBox.MaxLines = 1;
            dragDistanceBox.HorizontalAlignment = HorizontalAlignment.Center;
            dragDistanceBox.Text = "0";
            dragDistanceBox.TextWrapping = TextWrapping.Wrap;
            dragDistanceBox.PreviewTextInput += TextBoxPreviewTextInput; //add event
            dragDistanceBox.Name = "dragDistanceBox" + numTests;
            dragDistanceBox.VerticalAlignment = VerticalAlignment.Top;

            var dragDistanceUnits = new Label();
            dragDistanceUnits.Content = "μm";
            dragDistanceUnits.Padding = new Thickness(4);
            dragDistanceUnits.FontWeight = FontWeights.Thin;
            dragDistanceUnits.FontSize = 18;
            dragDistanceUnits.Opacity = 0.8;
            Grid.SetColumn(dragDistanceUnits, 1);

            innerGrid.Children.Add(dragDistanceBox);
            innerGrid.Children.Add(dragDistanceUnits);
            rightCol.Children.Add(dragDistance);
            rightCol.Children.Add(innerGrid);
            trialsGrid.Children.Add(rightCol);

            numTests++;
        }


        private void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!sanitizeInput(e.Text))
            {
                //Invalid input
                e.Handled = true;
            }
        }

        /// <summary>
        ///     Ensures that only numbers/decimals are input
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>Returns true if valid</returns>
        private bool sanitizeInput(string inputString)
        {
            if (!char.IsDigit(inputString, inputString.Length - 1) &&
                !(inputString.ElementAt(inputString.Length - 1) == '.') &&
                !(inputString.ElementAt(inputString.Length - 1) == '-'))
            {
                return false;
            }
            return true;
        }

        private void runTrialClick(object sender, RoutedEventArgs e)
        {
            trial.trials = new linearTrialManager.linearTrialIteration[numTests];
            for (var i = 0; i < numTests; i++)
            {
                trial.trials[i].cycles =
                    Convert.ToInt32(FindChild<TextBox>(Application.Current.MainWindow, "cycleNumBox" + i).Text);
                trial.trials[i].dragDistance =
                    Convert.ToDouble(FindChild<TextBox>(Application.Current.MainWindow, "dragDistanceBox" + i).Text);
                trial.trials[i].preload =
                    Convert.ToDouble(FindChild<TextBox>(Application.Current.MainWindow, "preloadForceBox" + i).Text);
            }
            trial.yAxisShift = Convert.ToDouble(yAxisShiftBox.Text);
            trial.incomingLateralAngle = Convert.ToDouble(incomingAngleBox.Text);
            trial.outgoingLateralAngle = Convert.ToDouble(incomingAngleBox.Text);
            MainWindow.mainFrame.Navigate(new trialViewer(trial));
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.Navigate(new runSelection());
        }

        /// <summary>
        ///     Finds a Child of a given item in the visual tree.
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>
        ///     The first parent item that matches the submitted type parameter.
        ///     If not matching item can be found,
        ///     a null parent is being returned.
        /// </returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T) child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T) child;
                    break;
                }
            }

            return foundChild;
        }
    }
}