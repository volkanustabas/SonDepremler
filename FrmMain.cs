using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using RestSharp;
using Sunny.UI;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace SonDepremler
{
    public partial class FrmMain : UIForm
    {
        public FrmMain()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            _bwGetData = new BackgroundWorker();
            _bwGetData.DoWork += BwGetData_DoWork;
            _bwGetData.RunWorkerCompleted += BwGetData_RunWorkerCompleted;
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            Sui_ComboBox_HtmlAgilityPack_Secim.SelectedIndex = 0;
        }

        #region HtmlAgilityPackTab

        private readonly BackgroundWorker _bwGetData;

        private void Sui_Button_Getir_Click(object sender, EventArgs e)
        {
            try
            {
                if (Sui_DataGridView_HtmlAgilityPack_Data.Rows.Count > 0)
                {
                    Sui_DataGridView_HtmlAgilityPack_Data.ClearRows();
                    Sui_DataGridView_HtmlAgilityPack_Data.DataSource = null;
                }

                if (!_bwGetData.IsBusy)
                {
                    DataGridViewClose(Sui_DataGridView_HtmlAgilityPack_Data);
                    _bwGetData.RunWorkerAsync();
                }
            }
            catch (Exception)
            {
                //
            }
        }

        private void BwGetData_DoWork(object sender, DoWorkEventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate { });

            switch (Sui_ComboBox_HtmlAgilityPack_Secim.SelectedIndex)
            {
                case 0:
                    var listeAfad = SonVerilerAfad();
                    var dt = new DataTable();
                    //dt.Columns.Add(@"ID", typeof(string));
                    dt.Columns.Add(@"TarihSaat", typeof(string));
                    dt.Columns.Add(@"Enlem", typeof(string));
                    dt.Columns.Add(@"Boylam", typeof(string));
                    dt.Columns.Add(@"DerinlikKM", typeof(string));
                    dt.Columns.Add(@"Tip", typeof(string));
                    dt.Columns.Add(@"Buyukluk", typeof(string));
                    dt.Columns.Add(@"Yer", typeof(string));


                    foreach (var itemAfad in listeAfad)
                        dt.Rows.Add(itemAfad.TarihSaat, itemAfad.Enlem, itemAfad.Boylam, itemAfad.DerinlikKm,
                            itemAfad.Tip,
                            itemAfad.Buyukluk, itemAfad.Yer);
                    Sui_DataGridView_HtmlAgilityPack_Data.DataSource = dt;
                    break;

                case 1:
                    var listeKandilli = SonVerilerKandilli();

                    var dtKandilli = new DataTable();
                    //dtKandilli.Columns.Add(@"ID", typeof(string));
                    dtKandilli.Columns.Add(@"TarihSaat", typeof(string));
                    dtKandilli.Columns.Add(@"Enlem", typeof(string));
                    dtKandilli.Columns.Add(@"Boylam", typeof(string));
                    dtKandilli.Columns.Add(@"DerinlikKM", typeof(string));
                    dtKandilli.Columns.Add(@"Tip", typeof(string));
                    dtKandilli.Columns.Add(@"Buyukluk", typeof(string));
                    dtKandilli.Columns.Add(@"Yer", typeof(string));

                    foreach (var itemKandilli in listeKandilli)
                        dtKandilli.Rows.Add(itemKandilli.TarihSaat, itemKandilli.Enlem,
                            itemKandilli.Boylam, itemKandilli.DerinlikKm, itemKandilli.Tip, itemKandilli.Buyukluk,
                            itemKandilli.Yer);
                    Sui_DataGridView_HtmlAgilityPack_Data.DataSource = dtKandilli;

                    break;

                default:
                    Sui_DataGridView_HtmlAgilityPack_Data.DataSource = null;
                    break;
            }

            DataGridStretchLastColumn(Sui_DataGridView_HtmlAgilityPack_Data);
        }

        private void BwGetData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DataGridViewRefresh(Sui_DataGridView_HtmlAgilityPack_Data);
        }

        private void Sui_DataGridView_Data_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var dd = new DepremData
                {
                    Enlem = Sui_DataGridView_HtmlAgilityPack_Data.Rows[e.RowIndex].Cells["Enlem"].Value.ToString(),
                    Boylam = Sui_DataGridView_HtmlAgilityPack_Data.Rows[e.RowIndex].Cells["Boylam"].Value.ToString()
                };

                if (!string.IsNullOrEmpty(dd.Enlem) && !string.IsNullOrEmpty(dd.Boylam))
                {
                    var googleHarita = @"https://www.google.com/maps/place/";
                    var link = $"{googleHarita}{dd.Enlem}+{dd.Boylam}/@{dd.Enlem},{dd.Boylam},7z";
                    Process.Start(link);
                }
            }
        }

        private static List<DepremData> SonVerilerAfad()
        {
            var listAfad = new List<DepremData>();

            var wc = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            var webData = wc.DownloadString("https://deprem.afad.gov.tr/last-earthquakes.html");

            //string webData = System.Text.Encoding.UTF8.GetString(raw);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(webData);

            var htmlNodes =
                htmlDocument.DocumentNode.SelectNodes("//table[@class='content-table']/tbody/tr");

            if (htmlNodes != null)
                foreach (var item in htmlNodes)
                {
                    var subDocument = new HtmlDocument();
                    subDocument.LoadHtml(item.InnerHtml);

                    var linkNodes = subDocument.DocumentNode.SelectNodes("//td");
                    if (linkNodes != null)
                    {
                        var depremData = new DepremData();

                        var tdcount = 0;
                        foreach (var subitem in linkNodes)
                        {
                            tdcount++;
                            switch (tdcount)
                            {
                                case 1:
                                    depremData.TarihSaat = Convert.ToDateTime(subitem.InnerText);
                                    break;
                                case 2:
                                    depremData.Enlem = subitem.InnerText;
                                    break;
                                case 3:
                                    depremData.Boylam = subitem.InnerText;
                                    break;
                                case 4:
                                    depremData.DerinlikKm = subitem.InnerText;
                                    break;
                                case 5:
                                    depremData.Tip = subitem.InnerText;
                                    break;
                                case 6:
                                    depremData.Buyukluk = subitem.InnerText;
                                    break;
                                case 7:
                                    depremData.Yer = subitem.InnerText;
                                    break;
                                case 8:
                                    depremData.Id = subitem.InnerText;
                                    break;
                            }
                        }

                        listAfad.Add(depremData);
                    }
                }

            return listAfad;
        }

        private static List<DepremData> SonVerilerKandilli()
        {
            var list = new List<DepremData>();

            var wc = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            var webData = wc.DownloadString("http://www.koeri.boun.edu.tr/scripts/sondepremler.asp");

            //string webData = System.Text.Encoding.UTF8.GetString(raw);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(webData);

            var htmlNode = htmlDocument.DocumentNode.SelectSingleNode("//pre");
            var icerik = htmlNode.InnerText;

            var icerikler = icerik.Split("--------------");
            var depremler = icerikler.Last().Trim();

            var depremList = depremler.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var item in depremList)
            {
                var depremData = new DepremData();

                var m1 = Regex.Match(item, @"(\d{4}).(\d{2}).(\d{2}) (\d{2}):(\d{2}):(\d{2})");
                var dt = m1.Value;
                depremData.TarihSaat = Convert.ToDateTime(dt);

                var m2 = Regex.Match(item, @"[0-9][0-9].[0-9][0-9][0-9][0-9]   [0-9][0-9].[0-9][0-9][0-9][0-9]");
                var coor = m2.Value;
                var coors = coor.Split("   ");
                string enlem = coors.First().Trim(), boylam = coors.Last().Trim();
                depremData.Enlem = enlem;
                depremData.Boylam = boylam;

                var m3 = Regex.Match(item, @"   [A-Z]\w+        ");
                var m4 = Regex.Match(item, @"   [A-Z]\w+.\w+ .\w+.        ");
                var m5 = Regex.Match(item, @"   [A-Z]\w+..\w+.        ");
                var yer = m3.Value;
                if (string.IsNullOrEmpty(yer)) yer = m4.Value;
                if (string.IsNullOrEmpty(yer)) yer = m5.Value;
                depremData.Yer = yer.Trim();

                var m6 = Regex.Match(item, @"[0-9]+\.[0-9]      ");
                var derinlik = m6.Value;
                depremData.DerinlikKm = derinlik.Trim();

                var m7 = Regex.Match(item, @"\-.-  [0-9].[0-9]  ");
                var ml = m7.Value.Replace("-.-", "").Trim();
                depremData.Buyukluk = ml;
                depremData.Tip = "ML";

                depremData.Id = depremData.TarihSaat.ToString("yyyy-MM-dd HH:mm:ss");

                list.Add(depremData);
            }

            return list;
        }

        public class DepremData
        {
            public string Id { get; set; }
            public DateTime TarihSaat { get; set; }
            public string Enlem { get; set; }
            public string Boylam { get; set; }
            public string DerinlikKm { get; set; }
            public string Tip { get; set; }
            public string Buyukluk { get; set; }
            public string Yer { get; set; }
        }

        #endregion

        #region RestSharpTab

        public string BaslangicZamani;
        public string BitisZamani;
        public DataTable DtDeprem;

        private async void Sui_SymbolButton_RestSharp_Getir_Click(object sender, EventArgs e)
        {
            DataGridViewClose(Sui_DataGridView_RestSharp_Data);

            await VeriGetir(Sui_DataGridView_RestSharp_Data);

            Thread.Sleep(1000);

            DataGridViewRefresh(Sui_DataGridView_RestSharp_Data);

            Sui_LedLabel_RestSharp_Count.Text = Sui_DataGridView_RestSharp_Data.Rows.Count.ToString();
        }

        private async Task VeriGetir(DataGridView dgv)
        {
            dgv.Visible = false;


            // "https://deprem.afad.gov.tr/apiv2/event/filter?start=2024-01-12 0:00:00&end=2024-01-16 0:00:00&minmag=2.9&country=Türkiye"

            BitisZamani = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss");
            BaslangicZamani = Convert.ToDateTime(BitisZamani).AddDays(-5).ToString("yyyy-MM-dd HH:mm");


            if (Sui_DataGridView_RestSharp_Data.Rows.Count > 0)
                try
                {
                    Sui_LedLabel_RestSharp_Count.Text = @"-";
                    DtDeprem.Clear();
                    dgv.DataSource = null;
                }
                catch (Exception)
                {
                    //
                }


            var options =
                new RestClientOptions(
                    $@"https://deprem.afad.gov.tr/apiv2/event/filter?start={BaslangicZamani}&end={BitisZamani}&minmag=2.9&&orderby=timedesc")
                {
                    MaxTimeout = -1
                };
            var client = new RestClient(options);
            var request =
                new RestRequest(
                    $@"https://deprem.afad.gov.tr/apiv2/event/filter?start={BaslangicZamani}&end={BitisZamani}&minmag=2.9&&orderby=timedesc");
            var response = await client.ExecuteAsync(request);

            DtDeprem = new DataTable();

            DtDeprem.Columns.Add(@"Tarih", typeof(DateTime));
            DtDeprem.Columns.Add(@"Konum", typeof(string));
            DtDeprem.Columns.Add(@"Enlem", typeof(string));
            DtDeprem.Columns.Add(@"Boylam", typeof(string));
            DtDeprem.Columns.Add(@"Buyukluk", typeof(string));
            DtDeprem.Columns.Add(@"Ulke", typeof(string));
            DtDeprem.Columns.Add(@"Sehir", typeof(string));
            DtDeprem.Columns.Add(@"Bolge", typeof(string));


            if (response.Content != null)
            {
                var myDeserializedClass = JsonConvert.DeserializeObject<List<DepremJson>>(response.Content);

                var sadeceTurkiyeDepremListesi = from s in myDeserializedClass
                    where s.Country != null && s.Country.Contains("Türkiye")
                    select s;

                foreach (var item in sadeceTurkiyeDepremListesi)
                    DtDeprem.Rows.Add(item.Date, item.Location, item.Latitude, item.Longitude, item.Magnitude,
                        item.Country,
                        item.Province,
                        item.District);

                //DtDeprem.DefaultView.Sort = @"date desc";
                Sui_DataGridView_RestSharp_Data.DataSource = DtDeprem;
                DataGridViewColumnColor(dgv);
                dgv.Refresh();
                dgv.PerformLayout();
                Sui_DataGridView_RestSharp_Data.ClearSelection();
                dgv.Visible = true;
            }
        }

        public void DataGridViewColumnColor(DataGridView dgv)
        {
            var rowscount = dgv.Rows.Count;

            for (var i = 0; i < rowscount; i++)
            {
                if (dgv.Rows[i].Cells[4].Value != null)
                {
                    var richterOlcegi = Convert.ToDouble(dgv.Rows[i].Cells[4].Value);
                    if (richterOlcegi <= 29)
                    {
                        dgv.Rows[i].Cells[4].Style.BackColor = Color.GreenYellow;
                        dgv.Rows[i].Cells[4].Style.ForeColor = Color.Black;
                    }
                    else if (richterOlcegi >= 30 && richterOlcegi <= 41)
                    {
                        dgv.Rows[i].Cells[4].Style.BackColor = Color.DarkGreen;
                        dgv.Rows[i].Cells[4].Style.ForeColor = Color.White;
                    }
                    else if (richterOlcegi >= 42 && richterOlcegi <= 60)
                    {
                        dgv.Rows[i].Cells[4].Style.BackColor = Color.Orange;
                        dgv.Rows[i].Cells[4].Style.ForeColor = Color.Black;
                    }
                    else if (richterOlcegi >= 61 && richterOlcegi <= 73)
                    {
                        dgv.Rows[i].Cells[4].Style.BackColor = Color.OrangeRed;
                        dgv.Rows[i].Cells[4].Style.ForeColor = Color.White;
                    }
                    else
                    {
                        dgv.Rows[i].Cells[4].Style.BackColor = Color.DarkRed;
                        dgv.Rows[i].Cells[4].Style.ForeColor = Color.White;
                    }
                }

                var dt = Convert.ToDateTime(dgv.Rows[i].Cells[0].Value.ToString());
                dgv.Rows[i].Cells[0].Value = dt.AddHours(3);
            }
        }

        public class DepremJson
        {
            [JsonProperty("country")] public string Country;

            [JsonProperty("date")] public DateTime Date;

            [JsonProperty("depth")] public string Depth;

            [JsonProperty("district")] public string District;

            [JsonProperty("eventID")] public string EventId;

            [JsonProperty("isEventUpdate")] public bool IsEventUpdate;

            [JsonProperty("lastUpdateDate")] public DateTime? LastUpdateDate;

            [JsonProperty("latitude")] public string Latitude;

            [JsonProperty("location")] public string Location;

            [JsonProperty("longitude")] public string Longitude;

            [JsonProperty("magnitude")] public string Magnitude;

            [JsonProperty("neighborhood")] public string Neighborhood;

            [JsonProperty("province")] public string Province;

            [JsonProperty("rms")] public string Rms;

            [JsonProperty("type")] public string Type;
        }

        private void Sui_DataGridView_RestSharp_Data_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var enlemGoogle = Sui_DataGridView_RestSharp_Data.Rows[e.RowIndex].Cells["Enlem"].Value.ToString();
                var boylamGoogle = Sui_DataGridView_RestSharp_Data.Rows[e.RowIndex].Cells["Boylam"].Value.ToString();

                if (!string.IsNullOrEmpty(enlemGoogle) && !string.IsNullOrEmpty(boylamGoogle))
                {
                    var googleHarita = @"https://www.google.com/maps/place/";
                    var link = $"{googleHarita}{enlemGoogle}+{boylamGoogle}/@{enlemGoogle},{boylamGoogle},7z";
                    Process.Start(link);
                }
            }
        }

        #endregion

        #region DataGridViewHelper

        private void DataGridStretchLastColumn(UIDataGridView dataGridView)
        {
            if (Sui_DataGridView_HtmlAgilityPack_Data.Rows.Count > 0)
            {
                var lastColIndex = dataGridView.Columns.Count - 1;
                var lastCol = dataGridView.Columns[lastColIndex];
                lastCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void DataGridViewRefresh(UIDataGridView uidgv)
        {
            uidgv.ScrollBars = ScrollBars.Both;
            uidgv.PerformLayout();
            uidgv.Refresh();
            uidgv.Visible = true;
        }

        private void DataGridViewClose(UIDataGridView uidgv)
        {
            uidgv.Visible = false;
            uidgv.ScrollBars = ScrollBars.None;
        }

        #endregion
    }
}