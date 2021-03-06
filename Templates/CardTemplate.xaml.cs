﻿using Windows.UI.Xaml.Controls;

namespace AutismCommunicationApp
{
    public sealed partial class CardTemplate : UserControl
    {

        //  Adapted from https://github.com/Windows-Readiness/AbsoluteBeginnersWin10/blob/master/UWP-042/UWP-042/UserControlDataTemplate/UserControlDataTemplate/ContactTemplate.xaml.cs

        public DataModel.Picture Picture { get { return this.DataContext as DataModel.Picture; } }

        public CardTemplate()
        {
            this.InitializeComponent();

            this.DataContextChanged += (s, e) => Bindings.Update();
        }
    }
}
