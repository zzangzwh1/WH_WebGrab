//**************************************************************************************
// Name : Wonhyuk Cho
// Instructor : Simon Walker
// Date : 2022-March-14
// CMPE2800 – WebGrabber – 1212 Version (In Person)

// you will implement insert any website url in text box and download every images using regex pattenrn
// when different link same images are there name will be changed to ex) image(1).jpg 
// when result button clicked through message box result will show up and when user press image location
// it will show if images downloaded or name has been changed
// when images clicked images will be installed on listview and when clear pressed images that in list will be cleared

//**************************************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace CMPE2800___WebGrabber
{
    public partial class Form1 : Form
    {
        //global variable regex url , total image count, duplicateimage count, image tpye count and error count
        private int regexUrlCnt = 0;
        private int totalImg = 0;
        private int duplicateImgCnt = 0;
        int cnt = 0;
        int errorCount = 0;
       // C:\Users\zzang\source\repos\CMPE2800 _ WebGrabber(4)\CMPE2800 _ WebGrabber
        string imgFilePath = @"C:\CMPE2800\CMPE2800 _ WebGrabber\CMPE2800 _ WebGrabber\images\";
        //regex that matches with link
        Regex linkUrl = new Regex(@"(http(s?):)([\/|.|\w|\s|-])*");
        //Test webstite 
        //https://spaceflightnow.com/
        /*       https://thor.net.nait.ca/~demo/cmpe2000/ica01Demo/async_one.html
                 https://thor.net.nait.ca/~demo/cmpe2000/ica01Demo/async_two.html
                 https://thor.net.nait.ca/~demo/cmpe2000/ica01Demo/async_dups.html
                 https://www.google.com/
         */
        //  string images = "*.jpg , *.gif , *.png , *.jpeg , *.svg , *.webp , *.tiff , *.ico , *.apng";
        Stopwatch sw = new Stopwatch();

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            //button event and keyevent
            UI_tBox_input.KeyDown += UI_tBox_input_KeyDown;
            UI_btn_status.Click += ButtonEvent;
            UI_Btn_clear.Click += ButtonEvent;
            UI_btn_result.Click += ButtonEvent;
            UI_btn_openfile.Click += ButtonEvent;
           
        }
       /// <summary>
       /// opendialog image when button imagbutton pressed this function will be callback
       /// purpose of this method is to show user that images downloaded or not
       /// </summary>
        private void OpenFileDialogImage()
        {   // opendialog initial directory is image folder path
            // all files allowed when user choose all files it will show every images if it is downloaded
            using(OpenFileDialog ofg = new OpenFileDialog())
            {
                ofg.InitialDirectory = imgFilePath;
                ofg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if(ofg.ShowDialog() == DialogResult.OK)
                {
                    string filePath = ofg.FileName;
                }
            }
        }
        /// <summary>
        /// when any button pressed using switch and each functions will be installed
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">event</param>
        private void ButtonEvent(object sender, EventArgs e)
        {
            ImageList _ilist = new ImageList();
            
            //using switch , each button function will be opened when button is clicked
            switch (((Button)sender).Name)
            {
                case "UI_btn_status":   WebsiteResult();        break;
                case "UI_btn_result":
                                        AddImage(_ilist);
                                        PopulateImage(_ilist);
                                                                break;
                case "UI_Btn_clear":    ImageClear();           break;
                case "UI_btn_openfile": OpenFileDialogImage();  break;
            }
        }

        // website will be on output will be installed when Result button clicked
        private void WebsiteResult()
        {
            MessageBox.Show($"The number of links processed from the website : {regexUrlCnt}\n" +
                             $"The number of links that represent images of any sort : { totalImg} \n" +
                             $"The number of duplicate image links found : {duplicateImgCnt}\n" +
                             $"The number of different image types processed : {cnt}\n" +
                             $"The number of Errors: {errorCount}\n" +
                             $"The total processing time :{sw.Elapsed.TotalSeconds} S");
            Console.WriteLine($"The number of links processed from the website : {regexUrlCnt}\n" +
                             $"The number of links that represent images of any sort : { totalImg} \n" +
                             $"The number of duplicate image links found : {duplicateImgCnt}\n" +
                             $"The number of different image types processed : {cnt}\n" +
                             $"The number of Errors: {errorCount}\n"+
                             $"The total processing time :{sw.Elapsed.TotalSeconds} S");
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "CMPE2800_WebGrabber";

        }
        /// <summary>
        /// key down event when url is inserted and press enter then it will in proccessed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UI_tBox_input_KeyDown(object sender, KeyEventArgs e)
        {
            string n = UI_tBox_input.Text;

            //when key code is enter and link url matches with textbox input then it will read images
            if (e.KeyCode == Keys.Enter)
            {
                if (linkUrl.IsMatch(n))
                {
                    ImageRead(n);
                }

                //when failed
                else
                {
                    MessageBox.Show("Must input URL link");
                }

            }
        }
        /// <summary>
        /// when it matches with rexgeximgurl then it will download images that matches with regex pattenr
        /// when there are same name of images from other link then name will be added as imagename(1).jpg
        /// 
        /// </summary>
        /// <param name="n">textbox input</param>
        async private void ImageRead(string n)
        {
            List<string> extensionList = new List<string>();
            //regex pattern for image - start with http or https and \w means [a-zA-Z0-9_] are word characters. \s Matches any white-space character.
            // or ~ or - . and any image type
            Regex regexImgUrl = new Regex(@"(http(s?):)([\/|.|\w|\s|~|-])*\.(?:jpg|gif|png|jpeg|svg|webp|tiff|ico|apng)");
            // Regex regexImgUrl = new Regex(@"(http(s?):)([\/|.|\w|\s|-])*\.(?:jpg|gif|png|jpeg|svg|webp|tiff|ico|apng)");
            // Regex linkUrl = new Regex(@"(http(s?):)([\/|.|\w|\s|-])*");
            List<Task> _Lst = new List<Task>();
            string extension = "";
            Console.WriteLine(n);
            WebClient client = new WebClient();
            // openread task
            //stop watch start
            sw.Start();
            var x = await client.OpenReadTaskAsync(n);
            StreamReader _sr = new StreamReader(x);
            string resultString = _sr.ReadToEnd();

            Console.WriteLine(resultString);
            // link check if url is available
            LinkCheck(resultString, linkUrl);

            //when it matches with regeximg url pattenr
            if (regexImgUrl.IsMatch(resultString))
            {
               
                //  https://cloudinary.com/blog/top_10_mistakes_in_handling_website_images_and_how_to_solve_them

                // MatchCollection mC = regexImgUrl.Matches(resultString);
                foreach (Match gP in regexImgUrl.Matches(resultString))
                {
                    //total image count increment
                    ++totalImg;
                    Console.WriteLine("gp~" + gP.ToString());
                    //  Console.WriteLine(Path.GetFileName(resultString));
                    //duplicate image count increment
                    ++duplicateImgCnt;

                    
                    string fileWithoutExtension = "";
                    string name = "";
                    try
                    {
                        //filename path 
                        name = Path.GetFileName(gP.ToString());
                        //GetFileNameWithoutExtension
                        fileWithoutExtension = Path.GetFileNameWithoutExtension(gP.ToString());
                        //Extension
                        extension = Path.GetExtension(gP.ToString());
                        string compareExtension = Path.GetExtension(gP.ToString());                       
                        extensionList.Add(extension);

                    }
                    //catch exception                    
                    catch (Exception ex)
                    {
                        Console.WriteLine("Extra Exception happend! : " + ex.Message);
                    }

                    Task t = null;
                    byte[] Arr = null;
                    byte[] temp = null;
                    int cntImage = 0;
                    Console.WriteLine(imgFilePath + fileWithoutExtension + extension);

                    string linkImgName = "";
                    Console.WriteLine("fsdfds" + fileWithoutExtension + extension);
                    foreach (Task d in _Lst)
                    {
                        //when images name matches
                        if ((fileWithoutExtension + extension) == Path.GetFileName(d.AsyncState.ToString()))
                        {
                            //link image nmae will be  d.AsyncState.ToString();
                            linkImgName = d.AsyncState.ToString();
                        }

                    }
                    //when images name isnt empty
                    if (linkImgName != "")
                    {
                        try
                        {
                            client = new WebClient();
                            //downloaddata
                            Arr = client.DownloadData(gP.ToString());
                            client = new WebClient();
                            //downloaddata
                            temp = client.DownloadData(linkImgName);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        //when byte count isnt matches
                        if (Arr.Count() != temp.Count())
                        {
                            // ++regexDifferentImgCnt;
                            client = new WebClient();
                            //same name image file will have imagename(number). image type
                            t = client.DownloadFileTaskAsync(gP.ToString(), imgFilePath + fileWithoutExtension + '(' + ++cntImage + ')' + extension);
                            _Lst.Add(t);
                        }

                    }
                    else
                    {
                        //duplicate image count decrement
                        --duplicateImgCnt;
                        client = new WebClient();
                        //downloadfile task
                        t = client.DownloadFileTaskAsync(gP.ToString(), imgFilePath + fileWithoutExtension + extension);
                        _Lst.Add(t);
                    }

                }

            }

            try
            {
                //count image type 
                CountExtension(extensionList);
                //async await task when all
                await Task.WhenAll(_Lst);
                //stop watch stop
                sw.Stop();


                // CountExtension(extensionList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //iterate list and when status is rantocompletion then completed
            // when it doenst match then errocount increase 
            foreach (Task f in _Lst)
            {
                if (f.Status.ToString() == "RanToCompletion")
                    Console.WriteLine("Completed");
                else
                {
                    ++errorCount;
                    Console.WriteLine("Error count : " + errorCount);
                }
            }
       
            //reuslt on output 
            Console.WriteLine("The number of links processed from the website : " + regexUrlCnt);
            Console.WriteLine("The number of links that represent images of any sort :" + totalImg);
            Console.WriteLine("The number of duplicate image links found : " + duplicateImgCnt);
            Console.WriteLine("The number of different image types processed : " + cnt);
            Console.WriteLine($"The total processing time : {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Done! {x} ");

        }
        /// <summary>
        /// private void LinkCheck(string n, Regex linkUrl)
        /// check if link url matches and count increment for regexurl count
        /// </summary>
        /// <param name="n">string inserted from text box</param>
        /// <param name="linkUrl"> link url regex</param>
        private void LinkCheck(string n, Regex linkUrl)
        {
            //when linkmatches 
            if (linkUrl.IsMatch(n))
            {
                Console.WriteLine("what is 1 : " + linkUrl.ToString());
                //iterate that matches with link url and increment regexurlCnt
                foreach (Match gP in linkUrl.Matches(n))
                    ++regexUrlCnt;
            }
        }
        /// <summary>
        ///    private void CountExtension(IEnumerable<string> _list)
        /// count image type 
        /// </summary>
        /// <param name="_list">collection </param>
        private void CountExtension(IEnumerable<string> _list)
        {
            // using hash because we are only looking for distinted images type
            HashSet<string> hash = new HashSet<string>(_list);
            foreach (string s in hash)
            {
                //count images
                if (s == ".png" || s == ".jpg" || s == ".jpeg" || s == ".tiff" || s == ".gif" || s == ".webp" || s == ".apng" || s == ".ico" || s == ".svg")
                    ++cnt;
            }
        }
        /// <summary>
        ///  private void ImageClear()
        /// clear listview
        /// </summary>
        private void ImageClear() => listView1.Items.Clear();
        /// <summary>
        ///   private void AddImage(ImageList i)
        /// add image into image list
        /// </summary>
        /// <param name="i">imagelist</param>
        private void AddImage(ImageList i)
        {

            listView1.View = View.LargeIcon;
            i.ImageSize = new Size(200, 200);
            listView1.LargeImageList = i;
            //get file directory and add image file name into imagelist
            foreach (string d in Directory.GetFiles(imgFilePath))
            {
                try
                {
                    i.Images.Add(Image.FromFile(d));

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Console.WriteLine("from add image" + d);

            }
        }
        /// <summary>
        ///  private void PopulateImage(ImageList _i)
        ///  pouplate images in listview
        /// </summary>
        /// <param name="_i"></param>
        private void PopulateImage(ImageList _i)
        {
            //count imags that in folder and add it into listview
            for (int i = 0; i <= _i.Images.Count; ++i)
            {
                ListViewItem item = new ListViewItem();
                item.ImageIndex = i;
                listView1.Items.Add(item);
                Console.WriteLine(i.ToString());

            }
        }
    }
   }











