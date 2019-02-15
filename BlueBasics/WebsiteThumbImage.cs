using System.Drawing;


namespace BlueBasics
{
    public static class WebsiteThumbImage

    {
        public static Bitmap GenerateScreenshot(string url, int width, int height)
        {
            var wb = new System.Windows.Forms.WebBrowser();
            wb.ScrollBarsEnabled = false;
            wb.ScriptErrorsSuppressed = true;
            wb.Navigate(url);
            while (wb.ReadyState != System.Windows.Forms.WebBrowserReadyState.Complete) { System.Windows.Forms.Application.DoEvents(); }
            // Set the size of the WebBrowser control
            wb.Width = width;
            wb.Height = height;
            if (width == -1)
            {
                wb.Width = wb.Document.Body.ScrollRectangle.Width;
            }
            if (height == -1)
            {
                wb.Height = wb.Document.Body.ScrollRectangle.Height;
            }
            var bitmap = new Bitmap(wb.Width, wb.Height);
            wb.DrawToBitmap(bitmap, new Rectangle(0, 0, wb.Width, wb.Height));
            wb.Dispose();
            return bitmap;
        }
    }
}
