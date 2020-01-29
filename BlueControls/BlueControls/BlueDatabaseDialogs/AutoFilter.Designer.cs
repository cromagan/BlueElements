using BlueControls.Controls;
using BlueControls.Enums;
using System.ComponentModel;
using System.Diagnostics;

namespace BlueControls.BlueDatabaseDialogs
{


    public partial class AutoFilter 
		{
			//Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
			[DebuggerNonUserCode()]
			protected override void Dispose(bool disposing)
			{
				try
				{
					if (disposing && components != null)
					{
						components.Dispose();
					}
				}
				finally
				{
					base.Dispose(disposing);
				}
			}

			//Wird vom Windows Form-Designer benötigt.
			private IContainer components;

			//Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
			//Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
			//Das Bearbeiten mit dem Code-Editor ist nicht möglich.
			[DebuggerStepThrough()]
			private void InitializeComponent()
			{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoFilter));
            this.TXTBox = new BlueControls.Controls.TextBox();
            this.Timer1x = new System.Windows.Forms.Timer(this.components);
            this.BlueLine1 = new BlueControls.Controls.Line();
            this.FiltItems = new BlueControls.Controls.ListBox();
            this.sFilter = new BlueControls.Controls.ListBox();
            this.Was = new BlueControls.Controls.Caption();
            this.SuspendLayout();
            // 
            // TXTBox
            // 
            this.TXTBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TXTBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.TXTBox.Location = new System.Drawing.Point(8, 100);
            this.TXTBox.Name = "TXTBox";
            this.TXTBox.Size = new System.Drawing.Size(150, 24);
            this.TXTBox.TabIndex = 2;
            this.TXTBox.Enter += new System.EventHandler(this.TXTBox_Enter);
            this.TXTBox.LostFocus += new System.EventHandler(this.Something_LostFocus);
            // 
            // Timer1x
            // 
            this.Timer1x.Enabled = true;
            this.Timer1x.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // BlueLine1
            // 
            this.BlueLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BlueLine1.Location = new System.Drawing.Point(8, 132);
            this.BlueLine1.Name = "BlueLine1";
            this.BlueLine1.Size = new System.Drawing.Size(152, 2);
            this.BlueLine1.Text = "BlueLine1";
            // 
            // FiltItems
            // 
            this.FiltItems.AddAllowed = enAddType.None;
            this.FiltItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FiltItems.Appearance = enBlueListBoxAppearance.Autofilter;
            this.FiltItems.Location = new System.Drawing.Point(8, 140);
            this.FiltItems.Name = "FiltItems";
            this.FiltItems.QuickInfo = "";
            this.FiltItems.Size = new System.Drawing.Size(150, 12);
            this.FiltItems.TabIndex = 5;
            this.FiltItems.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.FiltItems_ItemClicked);
            this.FiltItems.LostFocus += new System.EventHandler(this.Something_LostFocus);
            // 
            // sFilter
            // 
            this.sFilter.AddAllowed = enAddType.None;
            this.sFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sFilter.Appearance = enBlueListBoxAppearance.Autofilter;
            this.sFilter.Location = new System.Drawing.Point(8, 8);
            this.sFilter.Name = "sFilter";
            this.sFilter.QuickInfo = "";
            this.sFilter.Size = new System.Drawing.Size(150, 72);
            this.sFilter.TabIndex = 1;
            this.sFilter.Text = "Standard";
            this.sFilter.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.sFilter_ItemClicked);
            this.sFilter.LostFocus += new System.EventHandler(this.Something_LostFocus);
            // 
            // Was
            // 
            this.Was.Location = new System.Drawing.Point(8, 84);
            this.Was.Name = "Was";
            this.Was.Size = new System.Drawing.Size(112, 16);
            this.Was.Text = "...oder Text:";
            // 
            // AutoFilter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(165, 159);
            this.Controls.Add(this.Was);
            this.Controls.Add(this.sFilter);
            this.Controls.Add(this.FiltItems);
            this.Controls.Add(this.BlueLine1);
            this.Controls.Add(this.TXTBox);
            this.Design = enDesign.Form_AutoFilter;
            this.Name = "AutoFilter";
            this.Text = "AutoFilter";
            this.TopMost = true;
            this.ResumeLayout(false);

			}

			internal TextBox TXTBox;
			internal System.Windows.Forms.Timer Timer1x;
			internal Line BlueLine1;
			internal ListBox FiltItems;
			internal ListBox sFilter;
			internal Caption Was;
		}
	}
