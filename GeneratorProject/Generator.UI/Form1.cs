using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.IO;
using System.Diagnostics;

namespace Generator.UI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            fiilSqlInstances();
        }
        private void fiilSqlInstances()
        {
            var sqlInstances = Helpers.SqlHelper.ListLocalSqlInstances().ToList();
            for (int i = 0; i < sqlInstances.Count; i++)
            {
                cmbSqlInstances.Items.Add(sqlInstances[i]);
            }
        }

        public static Configuration OpenConfigFile(string configPath)
        {
            var configFile = new FileInfo(configPath);
            var vdm = new VirtualDirectoryMapping(configFile.DirectoryName, true, configFile.Name);
            var wcfm = new WebConfigurationFileMap();
            wcfm.VirtualDirectories.Add("/", vdm);
            return WebConfigurationManager.OpenMappedWebConfiguration(wcfm, "/");
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
          
            try
            {
                var appPath = Application.StartupPath;
                var serverName = cmbSqlInstances.SelectedItem.ToString();
                var userName = txtUserName.Text;
                var password = txtPassword.Text;
                var databaseName = txtDatabaseName.Text;

                var bakFilePath = appPath + "\\db.bak";
                string sql = $@"RESTORE DATABASE {databaseName} FROM DISK = '{bakFilePath}'
                    WITH FILE = 1,
                    MOVE 'AdventureWorks2014_Data' 
	                TO '{appPath}\{databaseName}.mdf', 
                    MOVE 'AdventureWorks2014_Log' 
	                TO '{appPath}\{databaseName}.ldf';";
                SqlConnection con = new SqlConnection($@"Server={serverName};Database=master;User Id={userName};
Password={password};Integrated Security=True");
                SqlCommand command = new SqlCommand(sql, con);
                con.Open();
                command.ExecuteNonQuery();
                MessageBox.Show("Veri Tabanı Yüklendi. Tamama tıkladığınızda web sitesi açılacaktır.");
                con.Close();
                con.Dispose();

                Microsoft.Web.Administration.ServerManager iisManager = new Microsoft.Web.Administration.ServerManager();
                var check=iisManager.Sites.Any(x => x.Name == "TestSite");
                if (!check) { 
                    iisManager.Sites.Add("TestSite", "http", "*:4545:", appPath+"\\Publish");
                    iisManager.CommitChanges();
                }

                Configuration config = OpenConfigFile(appPath + "/Publish/web.config");
                ConnectionStringsSection sec = (ConnectionStringsSection)config.GetSection("connectionStrings");
                sec.ConnectionStrings["Context"].ConnectionString = $@"metadata=res://*/Models.AdventureWorksModel.csdl|res://*/Models.AdventureWorksModel.ssdl|res://*/Models.AdventureWorksModel.msl;provider=System.Data.SqlClient;provider connection string="+HttpUtility.HtmlDecode("&quot;")+ $"data source={serverName};initial catalog={databaseName};persist security info=True;user id={userName};password={password};MultipleActiveResultSets=True;App=EntityFramework" + HttpUtility.HtmlDecode("&quot;") + ";";
                config.Save();

                ProcessStartInfo sInfo = new ProcessStartInfo("http://localhost:4545");
                Process.Start(sInfo);
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
