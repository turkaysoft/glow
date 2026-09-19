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
        private volatile bool DNSTest_isRunning = false;
        public GlowDNSTestTool(){
            InitializeComponent();
            //
            DNSTable.Columns.Add("DNSProvider", "Provider");
            DNSTable.Columns.Add("DNSValue", "Value");
            //
            DNSTable.Columns[0].Width = (int)(175 * this.DeviceDpi / 96f);
            DNSTable.RowTemplate.Height = (int)(32 * this.DeviceDpi / 96f);
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
        // PRE-LOAD
        // ======================================================================================================
        public void GTool_DNSTest_Preloader(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                //
                DNSTable.BackgroundColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                DNSTable.GridColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                DNSTable.DefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                DNSTable.DefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                DNSTable.AlternatingRowsDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                DNSTable.ColumnHeadersDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                DNSTable.ColumnHeadersDefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                DNSTable.ColumnHeadersDefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                DNSTable.DefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                DNSTable.DefaultCellStyle.SelectionForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                //
                DNS_PerfectResultLabel.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                DNS_PerfectResultLabel.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                //
                DNS_TestStartBtn.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                DNS_TestStartBtn.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                DNS_TestStartBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                DNS_TestStartBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                DNS_TestStartBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                //
                DNS_TestExportBtn.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                DNS_TestExportBtn.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                DNS_TestExportBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                DNS_TestExportBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                DNS_TestExportBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                //
                TSImageRenderer(DNS_TestStartBtn, GlowMain.theme == 1 ? Properties.Resources.ct_test_start_light : Properties.Resources.ct_test_start_dark, 18, ContentAlignment.MiddleRight);
                TSImageRenderer(DNS_TestExportBtn, GlowMain.theme == 1 ? Properties.Resources.ct_export_light : Properties.Resources.ct_export_dark, 17, ContentAlignment.MiddleRight);
                // TEXT
                // ----------------------
                Text = string.Format(software_lang.TSReadLangs("DNSTestTool", "dtt_title"), Application.ProductName);
                DNSTest_pingSendText = software_lang.TSReadLangs("DNSTestTool", "dtt_success");
                DNSTest_pingSendError = software_lang.TSReadLangs("DNSTestTool", "dtt_error");
                //
                DNSTable.Columns[0].HeaderText = software_lang.TSReadLangs("DNSTestTool", "dtt_column_server");
                DNSTable.Columns[1].HeaderText = software_lang.TSReadLangs("DNSTestTool", "dtt_column_server_response");
                //
                DNS_TestStartBtn.Text = " " + software_lang.TSReadLangs("DNSTestTool", "dtt_start");
                DNS_TestExportBtn.Text = " " + software_lang.TSReadLangs("DNSTestTool", "dtt_export");
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GTool_DNSTest_Preloader()"); }
            }
        }
        // LOAD
        // ======================================================================================================
        private void GlowDNSTestTool_Load(object sender, EventArgs e){
            try{
                GTool_DNSTest_Preloader();
                foreach (var item in DNSTest_dnsProviders){
                    DNSTable.Rows.Add(item.Provider.Name, software_lang.TSReadLangs("DNSTestTool", "dtt_start_await"));
                }
                DNSTable.ClearSelection();
                this.FormClosing += new FormClosingEventHandler(GlowDNSTestTool_FormClosing);
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GlowDNSTestTool_Load()"); }
            }
        }
        private void GlowDNSTestTool_FormClosing(object sender, FormClosingEventArgs e){
            if (DNSTest_isRunning){
                e.Cancel = true;
            }
        }
        // Async DNS Check
        // ======================================================================================================
        private async Task CheckDnsAsync(DnsTestItem item){
            string resultText = null;
            long? best = null;
            try{
                var sb = new StringBuilder();
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
                resultText = sb.ToString();
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "CheckDnsAsync()"); }
                resultText = DNSTest_pingSendError;
                best = null;
            }
            item.BestPing = best;
            item.IsCompleted = true;
            try{
                if (IsDisposed || !IsHandleCreated) return;
                if (item.RowIndex < 0 || item.RowIndex >= DNSTable.Rows.Count) return;
                string text = string.IsNullOrEmpty(resultText) ? DNSTest_pingSendError : resultText;
                if (InvokeRequired){
                    BeginInvoke(new Action(() => {
                        try{
                            if (IsDisposed || !IsHandleCreated) return;
                            if (item.RowIndex < 0 || item.RowIndex >= DNSTable.Rows.Count) return;
                            DNSTable.Rows[item.RowIndex].Cells[1].Value = text;
                        }catch { }
                    }));
                }else{
                    DNSTable.Rows[item.RowIndex].Cells[1].Value = text;
                }
            }catch { }
        }
        // START ENGINE
        // ======================================================================================================
        private async void DNS_TestStartBtn_Click(object sender, EventArgs e){
            if (DNSTest_isRunning) return;
            DNS_TestStartBtn.Enabled = false;
            DNS_TestExportBtn.Enabled = false;
            try{
                // Check network connection
                if (!await IsNetworkAvailable()){
                    DNS_PerfectResultLabel.Text = software_lang.TSReadLangs("DNSTestTool", "dtt_no_net");
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("DNSTestTool", "dtt_no_net_no_test"));
                    return;
                }
                DNSTest_isRunning = true;
                string BestResultPrefix = software_lang.TSReadLangs("DNSTestTool", "dtt_best_result") + " ";
                string awaitText = software_lang.TSReadLangs("DNSTestTool", "dtt_start_await");
                foreach (var item in DNSTest_dnsProviders){
                    item.BestPing = null;
                    item.IsCompleted = false;
                }
                if (!IsDisposed && IsHandleCreated){
                    foreach (var item in DNSTest_dnsProviders){
                        if (item.RowIndex >= 0 && item.RowIndex < DNSTable.Rows.Count)
                            DNSTable.Rows[item.RowIndex].Cells[1].Value = awaitText;
                    }
                    DNS_PerfectResultLabel.Text = "-";
                }
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
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "DNS_TestStartBtn_Click()"); }
            }finally{
                //
                Text = string.Format(software_lang.TSReadLangs("DNSTestTool", "dtt_title"), Application.ProductName);
                DNSTest_isRunning = false;
                try{
                    if (!IsDisposed && IsHandleCreated){
                        DNS_TestExportBtn.Enabled = true;
                        DNS_TestStartBtn.Enabled = true;
                    }
                }catch { }
            }
        }
        // COPY RESULT
        // ======================================================================================================
        private void DNSTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e){
            try{
                if (e.RowIndex < 0 || e.RowIndex >= DNSTable.Rows.Count) return;
                if (DNSTest_dnsProviders.All(p => p.IsCompleted)){
                    if (DNSTable.SelectedRows.Count > 0){
                        string name = Convert.ToString(DNSTable.Rows[e.RowIndex].Cells[0].Value);
                        string val = Convert.ToString(DNSTable.Rows[e.RowIndex].Cells[1].Value);
                        Clipboard.SetText(name + ": " + val);
                        TS_MessageBoxEngine.TS_MessageBox(this, 1, string.Format(software_lang.TSReadLangs("DNSTestTool", "dtt_copy_success"), name));
                    }
                }else{
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("DNSTestTool", "dtt_copy_info"));
                }
            }catch (Exception){
                try{
                    string name = e.RowIndex >= 0 && e.RowIndex < DNSTable.Rows.Count ? Convert.ToString(DNSTable.Rows[e.RowIndex].Cells[0].Value) : string.Empty;
                    TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("DNSTestTool", "dtt_copy_failed"), name, "\n"));
                }catch { }
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
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "DNS_TestExportBtn_Click()"); }
            }
        }
    }
}