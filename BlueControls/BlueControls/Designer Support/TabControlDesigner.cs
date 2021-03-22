using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;

namespace BlueControls.Designer_Support {
    internal sealed class TabControlDesigner : ParentControlDesigner {
        #region  Private Instance Variables 

        private readonly DesignerVerbCollection m_verbs = new DesignerVerbCollection();
        private IDesignerHost m_DesignerHost;
        private ISelectionService m_SelectionService;

        #endregion

        public TabControlDesigner() {

            DesignerVerb verb1 = new DesignerVerb("Add Tab", OnAddPage);
            DesignerVerb verb2 = new DesignerVerb("Insert Tab", OnInsertPage);
            DesignerVerb verb3 = new DesignerVerb("Remove Tab", OnRemovePage);
            m_verbs.AddRange(new[] { verb1, verb2, verb3 });
        }

        #region  Properties 

        public override DesignerVerbCollection Verbs {
            get {
                if (m_verbs.Count == 3) {
                    AbstractTabControl MyControl = (AbstractTabControl)Control;
                    if (MyControl.TabCount > 0) {
                        m_verbs[1].Enabled = true;
                        m_verbs[2].Enabled = true;
                    } else {
                        m_verbs[1].Enabled = false;
                        m_verbs[2].Enabled = false;
                    }
                }
                return m_verbs;
            }
        }

        public IDesignerHost DesignerHost {
            get {
                if (m_DesignerHost == null) {
                    m_DesignerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
                }
                return m_DesignerHost;
            }
        }

        public ISelectionService SelectionService {
            get {
                if (m_SelectionService == null) {
                    m_SelectionService = (ISelectionService)GetService(typeof(ISelectionService));
                }
                return m_SelectionService;
            }
        }

        #endregion

        public void OnAddPage(object sender, System.EventArgs e) {

            AbstractTabControl ParentControl = (AbstractTabControl)Control;
            System.Windows.Forms.Control.ControlCollection oldTabs = ParentControl.Controls;

            RaiseComponentChanging(TypeDescriptor.GetProperties(ParentControl)["TabPages"]);

            TabPage P = (TabPage)DesignerHost.CreateComponent(typeof(TabPage));
            P.Text = P.Name;
            ParentControl.TabPages.Add(P);

            RaiseComponentChanged(TypeDescriptor.GetProperties(ParentControl)["TabPages"], oldTabs, ParentControl.TabPages);
            ParentControl.SelectedTab = P;

            SetVerbs();
        }

        protected override void OnPaintAdornments(System.Windows.Forms.PaintEventArgs pe) {
            //Don't want DrawGrid dots.
        }


        public void OnInsertPage(object sender, System.EventArgs e) {

            AbstractTabControl ParentControl = (AbstractTabControl)Control;
            System.Windows.Forms.Control.ControlCollection oldTabs = ParentControl.Controls;
            int Index = ParentControl.SelectedIndex;

            RaiseComponentChanging(TypeDescriptor.GetProperties(ParentControl)["TabPages"]);

            TabPage P = (TabPage)DesignerHost.CreateComponent(typeof(TabPage));
            P.Text = P.Name;

            System.Windows.Forms.TabPage[] tpc = new System.Windows.Forms.TabPage[ParentControl.TabCount + 1];
            //Starting at our Insert Position, store and remove all the tabpages.
            for (int i = Index; i < ParentControl.TabCount; i++) {
                tpc[i] = ParentControl.TabPages[Index];
                ParentControl.TabPages.Remove(ParentControl.TabPages[Index]);
            }
            //add the tabpage to be inserted.
            ParentControl.TabPages.Add(P);
            //then re-add the original tabpages.
            for (int i = Index; i < tpc.GetUpperBound(0); i++) {
                ParentControl.TabPages.Add(tpc[i]);
            }

            RaiseComponentChanged(TypeDescriptor.GetProperties(ParentControl)["TabPages"], oldTabs, ParentControl.TabPages);
            ParentControl.SelectedTab = P;

            SetVerbs();
        }

        public void OnRemovePage(object sender, System.EventArgs e) {

            AbstractTabControl ParentControl = (AbstractTabControl)Control;
            System.Windows.Forms.Control.ControlCollection oldTabs = ParentControl.Controls;

            if (ParentControl.SelectedIndex < 0) { return; }

            RaiseComponentChanging(TypeDescriptor.GetProperties(ParentControl)["TabPages"]);

            DesignerHost.DestroyComponent(ParentControl.TabPages[ParentControl.SelectedIndex]);

            RaiseComponentChanged(TypeDescriptor.GetProperties(ParentControl)["TabPages"], oldTabs, ParentControl.TabPages);

            SelectionService.SetSelectedComponents(new IComponent[] { ParentControl }, SelectionTypes.Auto);

            SetVerbs();
        }

        private void SetVerbs() {

            AbstractTabControl ParentControl = (AbstractTabControl)Control;

            switch (ParentControl.TabPages.Count) {
                case 0:
                    Verbs[1].Enabled = false;
                    Verbs[2].Enabled = false;
                    break;

                case 1:
                    Verbs[1].Enabled = false;
                    Verbs[2].Enabled = true;
                    break;

                default:
                    Verbs[1].Enabled = true;
                    Verbs[2].Enabled = true;
                    break;
            }
        }

        private const int WM_NCHITTEST = 0x84;

        private const int HTTRANSPARENT = -1;
        private const int HTCLIENT = 1;

        protected override void WndProc(ref System.Windows.Forms.Message m) {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST) {
                //select tabcontrol when Tabcontrol clicked outside of TabItem.
                if (m.Result.ToInt32() == HTTRANSPARENT) {
                    m.Result = new IntPtr(HTCLIENT);
                }
            }
        }


        private const int TCM_HITTEST = 0x130D;

        private struct TCHITTESTINFO {
            public Point pt;
            public TabControlHitTest flags;
        }

        protected override bool GetHitTest(Point point) {

            if ((System.Windows.Forms.Control)SelectionService.PrimarySelection == Control) {
                TCHITTESTINFO hti = new TCHITTESTINFO {
                    pt = Control.PointToClient(point)
                };

                System.Windows.Forms.Message m = new System.Windows.Forms.Message {
                    HWnd = Control.Handle,
                    Msg = TCM_HITTEST
                };

                IntPtr lparam = Marshal.AllocHGlobal(Marshal.SizeOf(hti));
                Marshal.StructureToPtr(hti, lparam, false);
                m.LParam = lparam;

                base.WndProc(ref m);
                Marshal.FreeHGlobal(lparam);

                if (m.Result.ToInt32() != -1) { return hti.flags != TabControlHitTest.TCHT_NOWHERE; }

            }

            return false;
        }


        //Fix the AllSizable selectiorule on System.Windows.Forms.DockStyle.Fill
        public override SelectionRules SelectionRules {
            get {
                if (Control.Dock == System.Windows.Forms.DockStyle.Fill) { return SelectionRules.Visible; }
                return base.SelectionRules;
            }
        }
    }
}

