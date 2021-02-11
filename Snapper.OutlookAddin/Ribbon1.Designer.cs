namespace OutlookAddIn1
{
    partial class Ribbon1 : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public Ribbon1()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Ribbon1));
            this.TabCalendar = this.Factory.CreateRibbonTab();
            this.timeReportGroup = this.Factory.CreateRibbonGroup();
            this.PlayerButton = this.Factory.CreateRibbonButton();
            this.SettingsButton = this.Factory.CreateRibbonButton();
            this.TabCalendar.SuspendLayout();
            this.timeReportGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabCalendar
            // 
            this.TabCalendar.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.TabCalendar.ControlId.OfficeId = "TabCalendar";
            this.TabCalendar.Groups.Add(this.timeReportGroup);
            this.TabCalendar.Label = "TabCalendar";
            this.TabCalendar.Name = "TabCalendar";
            // 
            // timeReportGroup
            // 
            this.timeReportGroup.Items.Add(this.PlayerButton);
            this.timeReportGroup.Items.Add(this.SettingsButton);
            this.timeReportGroup.Label = "TIM Snapper";
            this.timeReportGroup.Name = "timeReportGroup";
            // 
            // PlayerButton
            // 
            this.PlayerButton.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.PlayerButton.Image = ((System.Drawing.Image)(resources.GetObject("PlayerButton.Image")));
            this.PlayerButton.Label = "Vad gjorde jag då?";
            this.PlayerButton.Name = "PlayerButton";
            this.PlayerButton.ScreenTip = "Visa vad du gjorde på detta datum";
            this.PlayerButton.ShowImage = true;
            this.PlayerButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.PlayerButtonClick);
            // 
            // SettingsButton
            // 
            this.SettingsButton.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.SettingsButton.Label = "Inställningar";
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.OfficeImageId = "ControlsGallery";
            this.SettingsButton.ScreenTip = "Inställningar för Tim Snapper";
            this.SettingsButton.ShowImage = true;
            this.SettingsButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.SettingsButtonClick);
            // 
            // Ribbon1
            // 
            this.Name = "Ribbon1";
            this.RibbonType = "Microsoft.Outlook.Explorer";
            this.Tabs.Add(this.TabCalendar);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Ribbon1_Load);
            this.TabCalendar.ResumeLayout(false);
            this.TabCalendar.PerformLayout();
            this.timeReportGroup.ResumeLayout(false);
            this.timeReportGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab TabCalendar;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup timeReportGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton PlayerButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton SettingsButton;
    }

    partial class ThisRibbonCollection
    {
        internal Ribbon1 Ribbon1
        {
            get { return this.GetRibbon<Ribbon1>(); }
        }
    }
}
