using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.Specialized;
using System.IO;
using Granicus.MediaManager.SDK;

namespace SIREDocumentImporter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker worker = new BackgroundWorker() { WorkerReportsProgress = true };
        private MediaManager mm;
        private int eventId;

        public MainWindow()
        {
            InitializeComponent();

            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Import completed.");
            progressBar.Value = 0;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            MetaDataData[] metadata = mm.GetEventMetaData(eventId);
            for(int i = 0; i < metadata.Length; i++)
            {
                MetaDataData meta = metadata[i];
                if (meta.Payload.GetType().Name == "Document")
                {
                    Document doc = (Document)meta.Payload;
                    if (doc.Location.StartsWith("http://dockets.sandiego.gov/sirepub"))
                    {
                        using (CustomWebClient webclient = new CustomWebClient())
                        {
                            byte[] data = webclient.DownloadData(doc.Location);

                            string url = string.Format("http://granicus.sandiego.gov/panes/EditEventMetaData.php?root_type=event&root_id={0}&meta_id={1}", eventId,meta.ID);
                            webclient.Headers.Add("Cookie", mm.CookieContainer.GetCookieHeader(new Uri(url)));

                            MultipartHelper helper = new MultipartHelper();
                            NameValueCollection props = new NameValueCollection();
                            props.Add("form_panel1_submit", "Save Changes");
                            props.Add("form_panel1_payload1", meta.Name);
                            props.Add("form_panel1_payload2", "");
                            helper.Add(new NameValuePart(props));
                            MemoryStream stream = new MemoryStream(data);
                            FilePart pdf = new FilePart(stream, "form_panel1_file", "application/pdf");
                            pdf.FileName = "fromsire.pdf";
                            helper.Add(pdf);

                            helper.Upload(webclient, url, "POST");
                        }
                    }
                }
                worker.ReportProgress((Int32) (((Double)(i + 1) / (Double)metadata.Length) * 100));
            }
            worker.ReportProgress(100);
        }

        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mm = new MediaManager(granicusHostInput.Text, granicusUserInput.Text, granicusPasswordInput.Password);
                eventId = int.Parse(eventIdInput.Text);
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Failed to import documents: {0}", ex.Message));
            }
        }
    }
}
