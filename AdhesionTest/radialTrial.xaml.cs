using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AdhesionTest
{
    /// <summary>
    ///     Interaction logic for radialTrial.xaml
    /// </summary>
    public partial class radialTrial : Page
    {
        private const double gridHeight = 60;
        private int numTests = 1;
        private readonly radialTrialManager trial;

        public radialTrial(radialTrialManager trial)
        {
            this.trial = trial;
            InitializeComponent();
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
            trial.numStrokes = Convert.ToInt32(numStrokesBox.Text);
            trial.desiredPreload = Convert.ToDouble(preloadBox.Text);
            trial.dragDistance = Convert.ToDouble(dragDistanceBox.Text);
            trial.randomizeOrder = Convert.ToBoolean(randomizeTrial.IsChecked);
            trial.overrideTrialDefaults = Convert.ToBoolean(multipleTrials.IsChecked);
            if (Convert.ToBoolean(multipleTrials.IsChecked))
            {
                trial.trials = new radialTrialManager.radialTrialIteration[numTests];
                for (var i = 0; i < numTests; i++)
                {
                    trial.trials[i].incomingAngle =
                        Convert.ToInt32(FindChild<TextBox>(Application.Current.MainWindow, "incomingAngleBox" + i).Text)*
                        Math.PI/180;
                    trial.trials[i].outgoingAngle =
                        Convert.ToDouble(FindChild<TextBox>(Application.Current.MainWindow, "outgoingAngleBox" + i).Text)*
                        Math.PI/180;
                    trial.trials[i].dragDistance =
                        Convert.ToDouble(FindChild<TextBox>(Application.Current.MainWindow, "dragDistanceBox" + i).Text);
                }
            }
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
            var col = new ColumnDefinition();
            leftCol.HorizontalAlignment = HorizontalAlignment.Center;
            leftCol.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(leftCol, 0);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            leftCol.ColumnDefinitions.Add(col);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            leftCol.ColumnDefinitions.Add(col);
            Grid.SetRow(leftCol, numTests);

            var incomingAngle = new Label();
            incomingAngle.Content = "Incoming Angle:    ";
            incomingAngle.Padding = new Thickness(4);
            incomingAngle.FontWeight = FontWeights.Thin;
            incomingAngle.FontSize = 18;
            incomingAngle.Opacity = 0.8;
            Grid.SetColumn(incomingAngle, 0);

            var innerGrid = new Grid();
            innerGrid.HorizontalAlignment = HorizontalAlignment.Center;
            Grid.SetColumn(innerGrid, 1);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            innerGrid.ColumnDefinitions.Add(col);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            innerGrid.ColumnDefinitions.Add(col);

            var incomingAngleBox = new TextBox();
            ;
            incomingAngleBox.Padding = new Thickness(3);
            incomingAngleBox.FontWeight = FontWeights.Thin;
            incomingAngleBox.FontSize = 18;
            incomingAngleBox.Opacity = 0.8;
            incomingAngleBox.Width = 100;
            incomingAngleBox.Height = 40;
            incomingAngleBox.HorizontalAlignment = HorizontalAlignment.Center;
            incomingAngleBox.Text = "90";
            incomingAngleBox.VerticalAlignment = VerticalAlignment.Top;
            incomingAngleBox.MaxLines = 1;
            incomingAngleBox.TextWrapping = TextWrapping.Wrap;
            incomingAngleBox.PreviewTextInput += TextBoxPreviewTextInput; //add event
            incomingAngleBox.Name = "incomingAngleBox" + numTests;

            var incomingAngleUnits = new Label();
            incomingAngleUnits.Content = "degrees";
            incomingAngleUnits.Padding = new Thickness(4);
            incomingAngleUnits.FontWeight = FontWeights.Thin;
            incomingAngleUnits.FontSize = 18;
            incomingAngleUnits.Opacity = 0.8;
            Grid.SetColumn(incomingAngleUnits, 1);

            innerGrid.Children.Add(incomingAngleBox);
            innerGrid.Children.Add(incomingAngleUnits);
            leftCol.Children.Add(incomingAngle);
            leftCol.Children.Add(innerGrid);
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

            var outgoingAngle = new Label();
            outgoingAngle.Content = "Outgoing angle:    ";
            outgoingAngle.Padding = new Thickness(4);
            outgoingAngle.FontWeight = FontWeights.Thin;
            outgoingAngle.FontSize = 18;
            outgoingAngle.Opacity = 0.8;
            Grid.SetColumn(outgoingAngle, 0);

            innerGrid = new Grid();
            innerGrid.HorizontalAlignment = HorizontalAlignment.Center;
            Grid.SetColumn(innerGrid, 1);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            innerGrid.ColumnDefinitions.Add(col);
            col = new ColumnDefinition();
            col.Width = GridLength.Auto;
            innerGrid.ColumnDefinitions.Add(col);

            var outgoingAngleBox = new TextBox();
            ;
            outgoingAngleBox.Padding = new Thickness(3);
            outgoingAngleBox.FontWeight = FontWeights.Thin;
            outgoingAngleBox.FontSize = 18;
            outgoingAngleBox.Opacity = 0.8;
            outgoingAngleBox.Width = 100;
            outgoingAngleBox.Height = 40;
            outgoingAngleBox.HorizontalAlignment = HorizontalAlignment.Center;
            outgoingAngleBox.Text = "90";
            outgoingAngleBox.VerticalAlignment = VerticalAlignment.Top;
            outgoingAngleBox.MaxLines = 1;
            outgoingAngleBox.TextWrapping = TextWrapping.Wrap;
            outgoingAngleBox.PreviewTextInput += TextBoxPreviewTextInput; //add event
            outgoingAngleBox.Name = "outgoingAngleBox" + numTests;

            var outgoingAngleUnits = new Label();
            outgoingAngleUnits.Content = "degrees";
            outgoingAngleUnits.Padding = new Thickness(4);
            outgoingAngleUnits.FontWeight = FontWeights.Thin;
            outgoingAngleUnits.FontSize = 18;
            outgoingAngleUnits.Opacity = 0.8;
            Grid.SetColumn(outgoingAngleUnits, 1);

            innerGrid.Children.Add(outgoingAngleBox);
            innerGrid.Children.Add(outgoingAngleUnits);
            midCol.Children.Add(outgoingAngle);
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

        /// <summary>
        ///     Updates the visibility of the UI elements when the multiple trials check box is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void multipleTrials_Clicked(object sender, RoutedEventArgs e)
        {
            if (!Convert.ToBoolean(multipleTrials.IsChecked))
            {
                addTest.Visibility = Visibility.Hidden;
                scrollView.Visibility = Visibility.Hidden;
                dragDistanceGrid.Visibility = Visibility.Visible;
            }
            else
            {
                addTest.Visibility = Visibility.Visible;
                scrollView.Visibility = Visibility.Visible;
                dragDistanceGrid.Visibility = Visibility.Hidden;
            }
        }
    }
}