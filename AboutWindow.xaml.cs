// Copyright © 2026, HB9DUT
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Windows;

namespace TciMeter;

public partial class AboutWindow : Window
{
    public AboutWindow(string callsign)
    {
        InitializeComponent();
        DarkMode.Apply(this);
        NameText.Text = string.IsNullOrWhiteSpace(callsign) ? AppInfo.ProductName : $"{callsign} · {AppInfo.ProductName}";
        VersionText.Text = Loc.T("about.version", AppInfo.Version);
        CopyrightText.Text = "Copyright © 2026, HB9DUT";
        LicenseLink.Inlines.Add(AppInfo.LicenseName);
        WarrantyText.Text = AppInfo.WarrantyNotice;
    }

    private void OnLicenseClick(object sender, RoutedEventArgs e)
    {
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(AppInfo.LicenseUrl) { UseShellExecute = true }); }
        catch { /* no browser available */ }
    }
}
