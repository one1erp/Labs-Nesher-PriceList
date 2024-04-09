using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Common;
using DAL;
using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using Telerik.WinControls.UI;
using Telerik.WinControls;

namespace PriceList
{

    [ComVisible(true)]
    [ProgId("PriceList.PriceList")]
    public partial class PriceList : UserControl, IExtensionWindow
    {
        #region Ctor
        public PriceList()
        {
            InitializeComponent();
            BackColor = Color.FromName("Control");
        }
        #endregion

        #region private members


        private IExtensionWindowSite2 _ntlsSite;
        private INautilusDBConnection _ntlsCon;
        private List<TestTemplateEx> _TestTemplatelist;
        private IDataLayer dal;

        #endregion

        #region Implementation of IExtensionWindow

        public bool CloseQuery()
        {
            return true;
        }

        public void Internationalise() { }

        public void SetSite(object site)
        {
            _ntlsSite = (IExtensionWindowSite2)site;
            _ntlsSite.SetWindowInternalName("");
            _ntlsSite.SetWindowRegistryName("");
            _ntlsSite.SetWindowTitle("מחירון");
        }


        public void PreDisplay()
        {
            Utils.CreateConstring(_ntlsCon);
            dal = new DataLayer();
            dal.Connect();
            PopulateTabs();
        }

        public WindowButtonsType GetButtons()
        {
            return LSExtensionWindowLib.WindowButtonsType.windowButtonsNone;
        }

        public bool SaveData()
        {
            return false; 
        }

        public void SetServiceProvider(object serviceProvider)
        {
            NautilusServiceProvider sp = serviceProvider as NautilusServiceProvider;
            _ntlsCon = Utils.GetNtlsCon(sp);

        }

        public void SetParameters(string parameters)
        {

        }

        public void Setup()
        {

        }

        public WindowRefreshType DataChange()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public WindowRefreshType ViewRefresh()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public void refresh() { }

        public void SaveSettings(int hKey) { }

        public void RestoreSettings(int hKey) { }
        public void Close()
        {

        }


        #endregion

        #region Init controls

        private void PopulateTabs()
        {
            //get Test templates
            _TestTemplatelist = dal.GetTestTemplatesForPriceList();

            foreach (var tab in radPageView1.Pages)
            {
                //Create a Grid View for each TAB
                CreateGridView(tab);

                var radDataGrid = (RadGridView)tab.Controls[0];

                //Add coulmns
                AddColumns(radDataGrid);

                DesignColumns(radDataGrid);

                //get lab of tab
                var labTab = tab.Tag.ToString();


                if (labTab != "ALL")
                {
                    var newList = new List<TestTemplateEx>();

                    foreach (var item in _TestTemplatelist)
                    {
                        if (item == null || item.RelevantLabs == null) continue;
                        var labs = item.RelevantLabs.Split(',');

                        bool containsThisLab = labs.Contains(labTab);
                        if (containsThisLab)
                        {
                            newList.Add(item);
                        }
                    }

                   
                    radDataGrid.DataSource = newList;

                }
                else
                {
                    radDataGrid.DataSource = _TestTemplatelist;
                }
            }



        }

      

        private void CreateGridView(RadPageViewPage tab)
        {
            var rgv = new RadGridView
                          {
                              AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill,
                              EnableAlternatingRowColor = true,
                              ShowRowErrors = true,
                              Dock = DockStyle.Fill,
                              AllowAddNewRow = false,
                              RightToLeft = RightToLeft.Yes,
                              ShowRowHeaderColumn = true,
                              AllowDeleteRow = false,
                              Width = 800,
                              Height = 800,
                              ReadOnly = false,
                              AutoGenerateColumns = true
                          };
            rgv.TableElement.Padding = new Padding(0);
            rgv.TableElement.DrawBorder = true;
            rgv.TableElement.CellSpacing = -1;
            rgv.TableElement.Text = "";

     
            rgv.RowFormatting += (rgv_RowFormatting);

            tab.Controls.Add(rgv);
        }

        void rgv_RowFormatting(object sender, RowFormattingEventArgs e)
        {

            var tt = (TestTemplateEx)e.RowElement.RowInfo.DataBoundItem;
            if (tt!=null&&tt.Price<=0)
            {
         
            
                e.RowElement.DrawFill = true;
                e.RowElement.GradientStyle = GradientStyles.Solid;
                e.RowElement.BackColor = Color.Red;
            }
            else
            {
                e.RowElement.ResetValue(LightVisualElement.BackColorProperty, ValueResetFlags.Local);
                e.RowElement.ResetValue(LightVisualElement.GradientStyleProperty, ValueResetFlags.Local);
                e.RowElement.ResetValue(LightVisualElement.DrawFillProperty, ValueResetFlags.Local);
            }
            

            
        }









        private void AddColumns(RadGridView radDataGrid)
        {
            if (radDataGrid.ColumnCount == 0)
            {


                radDataGrid.Columns.Add(new GridViewTextBoxColumn("שם הבדיקה", "Workflow.Description"));

                radDataGrid.Columns.Add(new SelectedPhraeGridViewTextBoxColumn("שם הבדיקה",
                                                                                "Completion",
                                                                                "Completion",
                                                                                dal));

                radDataGrid.Columns.Add(new GridViewTextBoxColumn("תקן", "Standard"));
                radDataGrid.Columns.Add(new GridViewTextBoxColumn("זמן קבלה", "Completion"));
                radDataGrid.Columns.Add(new GridViewCheckBoxColumn("הסמכה", "Authorization"));
                radDataGrid.Columns.Add(new GridViewCheckBoxColumn("הכרה", "Recognition"));
                radDataGrid.Columns.Add(new GridViewCheckBoxColumn("קבלן משנה", "Outsource"));
                radDataGrid.Columns.Add(new GridViewDecimalColumn("מחיר", "Price"));

                

            }
        }



        private void DesignColumns(RadGridView radDataGrid)
        {
            foreach (var item in radDataGrid.Columns)
            {
                item.Width = 100;
                item.TextAlignment = ContentAlignment.MiddleCenter;
                item.ReadOnly = true;

                if (item.Name == "מחיר")
                    item.ReadOnly = false;
            }
        }


        
        #endregion

        private void OKBtn_Click(object sender, EventArgs e)
        {

            dal.SaveChanges();
            _ntlsSite.CloseWindow();

        }

        private void Close_Click(object sender, EventArgs e)
        {

            if (HasChanges())
            {
                var dr = MessageBox.Show("האם ברצונך לשמור את השינויים?", "", MessageBoxButtons.YesNoCancel);

                if (dr == DialogResult.Yes)
                {
                    dal.SaveChanges();
                }
                else if (dr == DialogResult.Cancel)
                {
                    return;
                }
            }
            dal.Close();
            _ntlsSite.CloseWindow();
        }

        private void WindowExtension_Resize(object sender, EventArgs e)
        {
            lblHeader.Location = new Point(Width / 2 - lblHeader.Width / 2, lblHeader.Location.Y);
        }

        private bool HasChanges()
        {
            return _TestTemplatelist.Any(item => item.EntityState == EntityState.Modified);
        }
    }

    public class SelectedPhraeGridViewTextBoxColumn : GridViewTextBoxColumn
    {
        private string selectedPhraseName;
        private string selectedPhraseDescription;
        public SelectedPhraeGridViewTextBoxColumn() {}

        public SelectedPhraeGridViewTextBoxColumn(string fieldName) : base(fieldName){}

        public SelectedPhraeGridViewTextBoxColumn(string uniqueName, string fieldName) : base(uniqueName, fieldName){}

        public SelectedPhraeGridViewTextBoxColumn(string uniqueName, string phraseHeader, string phraseName, IDataLayer dal) : base(uniqueName)
        {
            selectedPhraseName = dal.GetPhraseByName(phraseHeader).PhraseEntries.Where(pe => pe.PhraseName == phraseName).FirstOrDefault().PhraseName;
            selectedPhraseDescription = dal.GetPhraseByName(phraseHeader).PhraseEntries.Where(pe => pe.PhraseName == phraseName).FirstOrDefault().PhraseDescription;
        }

        public string SelectedPhraseName
        {
            get { return selectedPhraseName; }
        }

        public string SelectedPhraseDescription
        {
            get { return selectedPhraseDescription; }
        }


    }



}




