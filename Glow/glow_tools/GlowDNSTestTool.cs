using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//
using static Glow.TSModules;

namespace Glow.glow_tools{
    public partial class GlowDNSTestTool : Form{
        // VARIABLES
        // ======================================================================================================
        private readonly TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
        private string DNSTest_pingSendText, DNSTest_pingSendError;
        private List<DnsTestItem> DNSTest_dnsProviders;
        public GlowDNSTestTool(){
            InitializeComponent();
            //
            DNSTable.Columns.Add("DNSProvider", "Provider");
            DNSTable.Columns.Add("DNSValue", "Value");
            //
            DNSTable.Columns[0].Width = (int)(175 * this.DeviceDpi / 96f);
            DNSTable.RowTemplate.Height = (int)(26 * this.DeviceDpi / 96f);
            foreach (DataGridViewColumn col in DNSTable.Columns){
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            foreach (DataGridViewColumn columnPadding in DNSTable.Columns){
                int scaledPadding = (int)(3 * this.DeviceDpi / 96f);
                columnPadding.DefaultCellStyle.Padding = new Padding(scaledPadding, 0, 0, 0);
            }
            //
            DNS_TestStartBtn.Width = Btn_FLP.Width;
            DNS_TestExportBtn.Width = Btn_FLP.Width;
            //
            InitDnsProviders();
        }
        // DNS Provider class
        // ======================================================================================================
        private class DnsTestItem{
            public GlowMain.DnsProvider Provider { get; }
            public int RowIndex { get; }
            public bool IsCompleted { get; set; }
            public long? BestPing { get; set; }
            public DnsTestItem(GlowMain.DnsProvider provider, int rowIndex){
                Provider = provider;
                RowIndex = rowIndex;
            }
        }
        private void InitDnsProviders(){
            DNSTest_dnsProviders = new List<DnsTestItem>();
            int row = 0;
            foreach (var provider in GlowMain.DnsProviders){
                DNSTest_dnsProviders.Add(new DnsTestItem(provider, row));
                row++;
            }
        }
        // LOAD SETTINGS
        // ======================================================================================================
        public void Dns_test_settings(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "ContentPanelBGColor");
                //
                DNSTable.BackgroundColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "DataGridBGColor");
                DNSTable.GridColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "DataGridColor");
                DNSTable.DefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "DataGridBGColor");
                DNSTable.DefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "DataGridFEColor");
                DNSTable.AlternatingRowsDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "DataGridAlternatingColor");
                DNSTable.ColumnHeadersDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "OSDAndServicesPageBG");
                DNSTable.ColumnHeadersDefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "OSDAndServicesPageBG");
                DNSTable.ColumnHeadersDefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "OSDAndServicesPageFE");
                DNSTable.DefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "OSDAndServicesPageBG");
                DNSTable.DefaultCellStyle.SelectionForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "OSDAndServicesPageFE");
                //
                DNS_PerfectResultLabel.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "PageContainerBGAndPageContentTotalColors");
                DNS_PerfectResultLabel.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "ContentLabelLeft");
                //
                DNS_TestStartBtn.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColor");
                DNS_TestStartBtn.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "DynamicThemeActiveBtnBG");
                DNS_TestStartBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColor");
                DNS_TestStartBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColor");
                DNS_TestStartBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                //
                DNS_TestExportBtn.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColor");
                DNS_TestExportBtn.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "DynamicThemeActiveBtnBG");
                DNS_TestExportBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColor");
                DNS_TestExportBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColor");
                DNS_TestExportBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                //
                TSImageRenderer(DNS_TestStartBtn, GlowMain.theme == 1 ? Properties.Resources.ct_test_start_light : Properties.Resources.ct_test_start_dark, 18, ContentAlignment.MiddleRight);
                TSImageRenderer(DNS_TestExportBtn, GlowMain.theme == 1 ? Properties.Resources.ct_export_light : Properties.Resources.ct_export_dark, 17, ContentAlignment.MiddleRight);
                //
                Text = string.Format(software_lang.TSReadLangs("DNSTestTool", "dtt_title"), Application.ProductName);
                DNSTest_pingSendText = software_lang.TSReadLangs("DNSTestTool", "dtt_success");
                DNSTest_pingSendError = software_lang.TSReadLangs("DNSTestTool", "dtt_error");
                //
                DNSTable.Columns[0].HeaderText = software_lang.TSReadLangs("DNSTestTool", "dtt_column_server");
                DNSTable.Columns[1].HeaderText = software_lang.TSReadLangs("DNSTestTool", "dtt_column_server_response");
                //
                DNS_TestStartBtn.Text = " " + software_lang.TSReadLangs("DNSTestTool", "dtt_start");
                DNS_TestExportBtn.Text = " " + software_lang.TSReadLangs("DNSTestTool", "dtt_export");
            }catch (Exception){ }
        }
        // LOAD
        // ======================================================================================================
        private void GlowDNSTestTool_Load(object sender, EventArgs e){
            Dns_test_settings();
            foreach (var item in DNSTest_dnsProviders){
                DNSTable.Rows.Add(item.Provider.Name, software_lang.TSReadLangs("DNSTestTool", "dtt_start_await"));
            }
            DNSTable.ClearSelection();
        }
        // Async DNS Check
        // ======================================================================================================
        private async Task CheckDnsAsync(DnsTestItem item){
            try{
                var sb = new StringBuilder();
                long? best = null;
                using (var ping = new Ping()){
                    // ONLY IPv4
                    var ipv4List = item.Provider.IPv4;
                    for (int i = 0; i < ipv4List.Count; i++){
                        string ip = ipv4List[i];
                        PingReply reply = await ping.SendPingAsync(ip, 3000);
                        if (reply.Status == IPStatus.Success){
                            sb.Append(string.Format("{0} - {1} {2} ms", ip, DNSTest_pingSendText, reply.RoundtripTime));
                            if (!best.HasValue || reply.RoundtripTime < best.Value)
                                best = reply.RoundtripTime;
                        }else{
                            sb.Append(string.Format("{0} - {1} ({2})", ip, DNSTest_pingSendError, reply.Status));
                        }
                        if (i < ipv4List.Count - 1)
                            sb.Append("   |   ");
                    }
                }
                Invoke(new Action(() =>{
                    DNSTable.Rows[item.RowIndex].Cells[1].Value = sb.ToString();
                    item.BestPing = best;
                    item.IsCompleted = true;
                }));
            }catch{
                Invoke(new Action(() =>{
                    item.IsCompleted = true;
                }));
            }
        }
        // START ENGINE
        // ======================================================================================================
        private async void DNS_TestStartBtn_Click(object sender, EventArgs e){
            DNS_TestStartBtn.Enabled = false;
            DNS_TestExportBtn.Enabled = false;
            // Check network connection
            if (!await IsNetworkAvailable()){
                DNS_PerfectResultLabel.Text = software_lang.TSReadLangs("DNSTestTool", "dtt_no_net");
                TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("DNSTestTool", "dtt_no_net_no_test"));
                DNS_TestExportBtn.Enabled = true;
                DNS_TestStartBtn.Enabled = true;
                return;
            }
            string BestResultPrefix = software_lang.TSReadLangs("DNSTestTool", "dtt_best_result") + " ";
            //
            Text = string.Format(software_lang.TSReadLangs("DNSTestTool", "dtt_title"), Application.ProductName) + " | " + software_lang.TSReadLangs("DNSTestTool", "dtt_title_test_ruining");
            var tasks = DNSTest_dnsProviders.Select(p => CheckDnsAsync(p)).ToArray();
            await Task.WhenAll(tasks);
            var bestTwo = DNSTest_dnsProviders.Where(x => x.BestPing.HasValue).OrderBy(x => x.BestPing.Value).Take(2).ToList();
            if (bestTwo.Count == 0){
                DNS_PerfectResultLabel.Text = "-";
            }else if (bestTwo.Count == 1){
                DNS_PerfectResultLabel.Text = BestResultPrefix +  string.Format("{0} ({1} ms)", bestTwo[0].Provider.Name, bestTwo[0].BestPing.Value);
            }else{
                DNS_PerfectResultLabel.Text = BestResultPrefix + string.Format("{0} ({1} ms)  |  {2} ({3} ms)", bestTwo[0].Provider.Name, bestTwo[0].BestPing.Value, bestTwo[1].Provider.Name, bestTwo[1].BestPing.Value);
            }
            //
            Text = string.Format(software_lang.TSReadLangs("DNSTestTool", "dtt_title"), Application.ProductName);
            DNS_TestExportBtn.Enabled = true;
            DNS_TestStartBtn.Enabled = true;
        }
        // COPY RESULT
        // ======================================================================================================
        private void DNSTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e){
            try{
                if (DNSTest_dnsProviders.All(p => p.IsCompleted)){
                    if (DNSTable.SelectedRows.Count > 0){
                        Clipboard.SetText(DNSTable.Rows[e.RowIndex].Cells[0].Value + ": " + DNSTable.Rows[e.RowIndex].Cells[1].Value);
                        TS_MessageBoxEngine.TS_MessageBox(this, 1, string.Format(software_lang.TSReadLangs("DNSTestTool", "dtt_copy_success"), DNSTable.Rows[e.RowIndex].Cells[0].Value));
                    }
                }else{
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("DNSTestTool", "dtt_copy_info"));
                }
            }catch (Exception){
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("DNSTestTool", "dtt_copy_failed"), DNSTable.Rows[e.RowIndex].Cells[0].Value, "\n"));
            }
        }
        // PRINT ENGINE
        // ======================================================================================================
        readonly List<string> PrintDNSList = new List<string>();
        private void DNS_TestExportBtn_Click(object sender, EventArgs e){
            try{
                PrintDNSList.Clear();
                var providers = GlowMain.DnsProviders;
                if (providers == null || providers.Count == 0)
                    return;
                PrintDNSList.Add(Application.ProductName + " - " + string.Format(software_lang.TSReadLangs("PrintEngine", "pe_save_name"), software_lang.TSReadLangs("DNSTestTool", "dtt_export_name")));
                PrintDNSList.Add(Environment.NewLine + new string('-', 100) + Environment.NewLine);
                int maxNameLength = providers.Max(p => p.Name.Length);
                int maxLeftLength = 0;
                for (int i = 0; i < providers.Count; i++){
                    var cellValue = DNSTable.Rows[i].Cells[1].Value?.ToString();
                    if (string.IsNullOrWhiteSpace(cellValue))
                        continue;
                    var parts = cellValue.Split('|');
                    if (parts.Length == 2){
                        int len = parts[0].Trim().Length;
                        if (len > maxLeftLength)
                            maxLeftLength = len;
                    }
                }
                for (int i = 0; i < providers.Count; i++){
                    string name = providers[i].Name.PadRight(maxNameLength);
                    var cellValue = DNSTable.Rows[i].Cells[1].Value?.ToString();
                    if (string.IsNullOrWhiteSpace(cellValue)){
                        PrintDNSList.Add($"{name}: -");
                        continue;
                    }
                    var parts = cellValue.Split('|');
                    if (parts.Length == 2){
                        string left = parts[0].Trim().PadRight(maxLeftLength);
                        string right = parts[1].Trim();
                        PrintDNSList.Add($"{name}: {left}\t|\t{right}");
                    }else{
                        PrintDNSList.Add($"{name}: {cellValue.Trim()}");
                    }
                }
                PrintDNSList.Add(Environment.NewLine + new string('-', 100) + Environment.NewLine);
                PrintDNSList.Add(DNS_PerfectResultLabel.Text);
                using (SaveFileDialog saveDlg = new SaveFileDialog{
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Title = Application.ProductName + " - " + software_lang.TSReadLangs("PrintEngine", "pe_save_directory"),
                    DefaultExt = "txt",
                    FileName = Application.ProductName + " - " + string.Format(software_lang.TSReadLangs("PrintEngine", "pe_save_name"), software_lang.TSReadLangs("DNSTestTool", "dtt_export_name")),
                    Filter = software_lang.TSReadLangs("PrintEngine", "pe_save_txt") + " (*.txt)|*.txt"
                }){
                    if (saveDlg.ShowDialog() == DialogResult.OK){
                        File.WriteAllText(saveDlg.FileName, string.Join(Environment.NewLine, PrintDNSList));
                        var res = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("PrintEngine", "pe_save_success") + Environment.NewLine + Environment.NewLine + software_lang.TSReadLangs("PrintEngine", "pe_save_info_open"), Application.ProductName, saveDlg.FileName));
                        if (res == DialogResult.Yes)
                            Process.Start(saveDlg.FileName);
                    }
                }
            }catch (Exception){ }
        }
    }
}