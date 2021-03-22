#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
// https://github.com/cromagan/BlueElements
// 
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER  
// DEALINGS IN THE SOFTWARE. 
#endregion


using BlueBasics;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueControls {
    public static class Allgemein {

        public static IContextMenu ParentControlWithCommands(this object o) {
            IContextMenu par = o.ParentControl<IContextMenu>();

            if (par == null) { return null; }


            ItemCollectionList ThisContextMenu = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu);
            ItemCollectionList UserMenu = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu);
            List<string> tags = new List<string>();
            bool Cancel = false;
            bool Translate = true;

            par.GetContextMenuItems(null, ThisContextMenu, out object HotItem, tags, ref Cancel, ref Translate);

            if (Cancel) { return null; }



            ContextMenuInitEventArgs ec = new ContextMenuInitEventArgs(HotItem, tags, UserMenu);
            par.OnContextMenuInit(ec);
            if (ec.Cancel) { return null; }

            if (ThisContextMenu != null && ThisContextMenu.Count > 0) { return par; }
            if (UserMenu.Count > 0) { return par; }

            return null;
        }

        public static t ParentControl<t>(this object o) {

            if (o is System.Windows.Forms.Control co) {



                do {
                    co = co.Parent;
                    switch (co) {
                        case null:
                            return default;
                        case t ctr:
                            return ctr;
                        default:
                            break;
                    }
                } while (true);

            }

            return default;

        }
        public static List<string> SplitByWidth(this string Text, float MaxWidth, int MaxLines, enDesign design, enStates state) {

            List<string> _broken = new List<string>();

            int pos = 0;
            int FoundCut = 0;
            string Rest = Text;

            if (MaxLines < 1) { MaxLines = 100; }

            BlueFont F = Skin.GetBlueFont(design, state);




            do {
                pos++;
                string ToTEst = Rest.Substring(0, pos);
                System.Drawing.SizeF s = BlueFont.MeasureString(ToTEst, F.Font());


                if (pos < Rest.Length && Convert.ToChar(Rest.Substring(pos, 1)).isPossibleLineBreak()) { FoundCut = pos; }


                if (s.Width > MaxWidth || pos == Rest.Length) {

                    if (pos == Rest.Length) {
                        _broken.Add(Rest);
                        return _broken;
                    } // Alles untergebracht

                    if (_broken.Count == MaxLines - 1) {
                        // Ok, werden zu viele Zeilen. Also diese Kürzen.
                        _broken.Add(Rest.TrimByWidth(MaxWidth, F));
                        return _broken;
                    }

                    if (FoundCut > 1) {
                        pos = FoundCut + 1;
                        ToTEst = Rest.Substring(0, pos);
                        FoundCut = 0;
                    }


                    _broken.Add(ToTEst);
                    Rest = Rest.Substring(pos);
                    pos = -1; // wird gleich erhöht

                }




            } while (true);


            //foreach (string ThisCap in _captiontmp)
            //{

            //    SizeF s = GenericControl.Skin.FormatedText_NeededSize(ThisCap, null, GenericControl.Skin.BlueFont(enDesign.Item_Listbox, vState));
            //    Rectangle r = new Rectangle((int)(DCoordinates.Left + DCoordinates.Width / 2.0 - s.Width / 2.0), (int)(DCoordinates.Bottom - s.Height) - 3, (int)s.Width, (int)s.Height);

            //    r = new Rectangle(r.Left - trp.X, r.Top - trp.Y, r.Width, r.Height);
            //    //GenericControl.Skin.Draw_Back(GR, enDesign.Item_Listbox_Unterschrift, vState, r, null, false);
            //    //GenericControl.Skin.Draw_Border(GR, enDesign.Item_Listbox_Unterschrift, vState, r);
            //    GenericControl.Skin.Draw_FormatedText(GR, ThisCap, enDesign.Item_Listbox, vState, null, enAlignment.Horizontal_Vertical_Center, r, null, false);

            //}




        }

        public static string TrimByWidth(this string TXT, float MaxWidth, BlueFont F) {
            if (F == null) { return TXT; }
            System.Drawing.SizeF tSize = BlueFont.MeasureString(TXT, F.Font());

            if (tSize.Width - 1 > MaxWidth && TXT.Length > 1) {
                int Min = 0;
                int Max = TXT.Length;
                int Middle = 0;

                do {
                    Middle = (int)(Min + (Max - Min) / 2.0);
                    tSize = BlueFont.MeasureString(TXT.Substring(0, Middle) + "...", F.Font());

                    if (tSize.Width + 3 > MaxWidth) {
                        Max = Middle;
                    } else {
                        Min = Middle;
                    }

                } while (Max - Min > 1);


                if (Middle == 1 && tSize.Width - 2 > MaxWidth) {
                    return string.Empty;  // ACHTUNG: 5 Pixel breiter (Beachte oben +4 und hier +2)
                }

                return TXT.Substring(0, Middle) + "...";

            }

            return TXT;
        }

        public static List<string> ToListOfString(this List<BasicListItem> Items) {
            List<string> w = new List<string>();
            if (Items == null) { return w; }

            foreach (BasicListItem ThisItem in Items) {
                if (ThisItem != null) {
                    if (!string.IsNullOrEmpty(ThisItem.Internal)) {
                        w.Add(ThisItem.Internal);
                    }
                }
            }

            return w;
        }
    }
}
